# 設計ドキュメント

## 設計方針

新しいゲームを作り始めるときの「土台」として機能するテンプレート。以下を目標としている。

- **DI・リアクティブを標準** とし、`Find()` や static は使わない
- **非同期を UniTask** で統一し、キャンセル処理を明示的に行う
- **アセットは Addressables** で遅延ロードし、`Resources.Load` は使わない
- **UI は UI Toolkit（UXML）** で構築し、uGUI は使わない

---

## シーン構成

```
Common (常駐)
  ├── BootLoader              ビルド起動時に Title をロードする
  ├── SoundPlayer
  ├── SceneTransitioner
  ├── TransitionPresenter
  ├── OptionPresenter / OptionModel（OptionModalPresenter は OptionPresenter が生成する plain C#）
  └── Store 群（SoundStore / ModalStore ← AssetStoreBase を継承）

Title (アディティブ) → Home (アディティブ) → Main (アディティブ)
```

- `Common` シーンは起動時にロードされ、以降アンロードされない
- 他シーンは `Common` の上にアディティブでロード・アンロードされる
- シーン遷移は `SceneTransitioner.Transit(Scenes next)` を呼ぶだけでよい
- 遷移時は `TransitionPresenter` が画面をフェードアウト→ロード→フェードインの演出を行う
- `Scenes` enum の値は Build Settings の buildIndex と一致させる（`Common=0 / Title=1 / Home=2 / Main=3`）。シーンを追加・並べ替えるときは enum と Build Settings の両方を揃えること

### 画面遷移

| From | To | トリガー |
|---|---|---|
| 起動 | Title | ビルド起動時に `BootLoader` が Common から自動ロード |
| Title | Home | 「PRESS START」クリック（遷移先は `GameStartButtonPresenter._nextScene` で Inspector 指定） |
| Home | Main | 「ゲーム開始」クリック |
| Main | Home | オプションモーダルの「ゲームをやめる」 |
| Main 以外の任意のシーン | Title | オプションモーダルの「タイトルに戻る」 |

Home の「クレジット」はシーン遷移ではなく、同一シーン内のオーバーレイ表示。

### なぜアディティブか

シーン単位で DontDestroyOnLoad を使わず、Common シーンを「永続レイヤー」として扱うことで
サウンド・オプション・シーン遷移を全シーンで共有できる。

---

## 依存性注入（VContainer）

```
CommonLifetimeScope   全シーン共通のシングルトンを登録
  ├── ModalStore
  ├── OptionExitRouter
  ├── OptionPresenter
  ├── OptionModel
  ├── SoundPlayer
  ├── SoundStore
  ├── TransitionPresenter
  └── SceneTransitioner

TitleLifetimeScope    GameStartButtonPresenter / AudioManager
HomeLifetimeScope     CreditModel / HomePresenter
MainLifetimeScope     （シーン固有の登録はまだ無い）
```

- 各シーンの `Injector/` フォルダに `*LifetimeScope.cs` を置く
- 新しいサービスは LifetimeScope に登録してコンストラクタでインジェクト
- シーンロード後の LifetimeScope 構築は `SceneExtensions.BuildLifetimeScopes()` 拡張メソッドが担う（BootLoader / CommonSceneLoader / SceneTransitioner から呼ばれる）

---

## 状態管理（R3）

Model → Presenter の単方向データフロー + 双方向バインディング。

```
OptionModel
  内部: ReactiveProperty<float>          （書き込みは SetBGMVolume() / SetSEVolume() 経由のみ）
  公開: BGMVolume / SEVolume
        ReadOnlyReactiveProperty<float>  （読み取りは .CurrentValue）

OptionModalPresenter
  → BGMVolume.Subscribe で Slider を更新
  → Slider の ValueChanged で SetBGMVolume() を呼ぶ
```

- Model の状態は `ReactiveProperty<T>` を private フィールドで持ち、外部へは `ReadOnlyReactiveProperty<T>` として公開する。書き込みは Model のメソッド経由に限定し、外から `.Value` に代入できないようにする
- 公開側の現在値は `.CurrentValue` で読む（`ReadOnlyReactiveProperty<T>` に `.Value` は無い）
- サブスクリプションは `AddTo(_disposables)` または `AddTo(destroyCancellationToken)` で管理
- Model は PlayerPrefs を通じて永続化する

---

## 非同期処理（UniTask）

- `IAsyncStartable` を実装したクラスは VContainer が StartAsync を呼ぶ
- 非同期ロードの完了待ちは `await _store.Loaded` の形で行う（`Store` 系の仕組みは後述「アセット管理（Addressables）」を参照）

```csharp
// 例: AudioManager (Title シーン)
public async UniTask StartAsync(CancellationToken cancellation = default)
{
    await _soundStore.Loaded;
    _soundPlayer.PlayBGM(_soundStore.TitleBGM);
}
```

### MonoBehaviour のインジェクションタイミング

`CommonSceneLoader.Awake()` は `async void` で、シーンのスコープ構築（`BuildLifetimeScopes()`）を担う。Common シーンが未ロードなら `LoadSceneAsync` と `UniTask.NextFrame()` を挟んでから、ロード済みなら同フレームで呼ぶ。いずれの場合も **MonoBehaviour の `Awake/OnEnable/Start` が呼ばれる時点ではインジェクションが完了していない**（`BuildLifetimeScopes` は最速でも自身の `Awake` と同じタイミング以降）。

| コールバック | インジェクト済みフィールドを使えるか |
|---|---|
| `Awake` / `OnEnable` / `Start` | **不可**（injection 前） |
| `[Inject] Construct(...)` | 可（injection と同時に呼ばれる） |
| `IAsyncStartable.StartAsync()` | 可（Build 完了後に VContainer が呼ぶ） |
| ユーザー操作イベントコールバック | 可（injection 完了後に発火） |

「シーン起動時にインジェクト済みフィールドを使って初期化したい」場合は、`Start()` ではなく `[Inject] Construct(...)` メソッド内で行うか、`IAsyncStartable` を実装した純粋 C# サービスを `RegisterEntryPoint` で登録してそこから MonoBehaviour の public メソッドを呼ぶ。

### シーン遷移のキャンセル処理

`SceneTransitioner` は `SemaphoreSlim` で同時遷移を防ぎ、
連打された場合は最後のリクエストのみ実行する（前の遷移は CancellationToken でキャンセル）。

### ISceneReady — シーン準備完了の通知

`RevealAsync`（フェードイン）の前に、`SceneTransitioner` は次シーンの root GameObject を検索し、`ISceneReady` を実装した**全ての**コンポーネントの `ReadyAsync(ct)` を `UniTask.WhenAll` で並行待機する。

これにより、Addressables の非同期ロードなど「表示前に完了させたい初期化」がフェードイン前に終わり、背景や要素が空白のまま画面が現れるのを防ぐ。

新しいシーンで表示前に待ちたい非同期処理がある場合は、そのシーンの Presenter に `ISceneReady` を実装し、準備完了時に `ReadyAsync` を完了させるだけでよい（実装が無いシーンは素通りする任意フック）。

`ReadyAsync` がキャンセル以外の例外を投げても、暗幕が残り続けないよう `SceneTransitioner` 側で例外をログ出力して握りつぶし、フェードインは必ず実行する（`WaitReadySafelyAsync`）。実装側で初期化失敗を扱いたい場合は `ReadyAsync` 内で完結させること。

---

## サウンド設計

- BGM: `AudioSource.loop = true`、`PlayBGM()` で差し替え
- SE: `PlayOneShot()` で重ね再生
- 音量は `OptionModel` が 0–1 で保持し、`BGMVolume` / `SEVolume`（`ReadOnlyReactiveProperty<float>`）で公開する
- `SoundPlayer` は音量変化を Subscribe して AudioSource に即時反映

> `_bgmAudioSource.volume = v / 2` としているのは、
> OptionModel の値 1.0 がデフォルトの AudioSource 最大音量の半分に相当するようにしているため。

---

## UI 設計（UI Toolkit）

### ファイル配置

```
Assets/Scripts/<Scene>/<Feature>/
  ├── *Presenter.cs   （UI ロジック）
  └── *.uxml          （見た目 / Addressables 経由でロードするものは AddressableAssets/ に配置）
```

### PanelSettings

`Assets/Scripts/Panel Settings.asset` の Scale Mode を **Scale With Screen Size** に設定済み。
基準解像度に対して UI 全体がスケールするため、固定 px 値で指定したサイズが解像度によらず適切な物理サイズになる。

### オプションモーダル

- アイコンクリックで表示、Close ボタンで非表示
- 左側の退出ボタン（`ExitButton`）は現在シーンでラベルと遷移先が変わる。判定と遷移は `OptionExitRouter` が担い、`OptionPresenter` はモーダルの開閉と SE に専念する
  - Main 以外: **「タイトルに戻る」** → Title シーンへ遷移
  - Main: **「ゲームをやめる」** → Home シーンへ遷移
  - モーダルは Common で一度だけ生成して使い回すので、ラベルの差し替えは開くたびに `OptionExitRouter.CurrentLabel` を読んで `OptionModalPresenter.SetExitLabel()` で行う
- オーバーレイ（`rgba(0,0,0,0.55)`）がゲーム画面を暗幕。`picking-mode` は既定（`Position`）にして背後へのクリックを遮る（Title の全画面「PRESS START」などを暗幕越しに押させないため）
- モーダルカードは画面中央に配置（`align-items: center; justify-content: center`）
- UIDocument の SortingOrder は `1000`（重ね順の一覧は [design-system.md](design-system.md)「UIDocument の設定」を参照）
- モーダル内 UI バインド（スライダー・ボタン）は `OptionModalPresenter`（plain C# クラス）が担い、`OptionPresenter.SetupAsync()` 内で `new` して使う

---

## アセンブリ構成

スクリプトは4つの Assembly Definition に分割されている。

| アセンブリ | パス | asmdef の references |
|---|---|---|
| `Common` | `Assets/Scripts/Common/` | VContainer / UniTask / UniTask.Addressables / Unity.Addressables / Unity.ResourceManager |
| `Title` | `Assets/Scripts/Title/` | Common / VContainer / UniTask |
| `Home` | `Assets/Scripts/Home/` | Common / VContainer / UniTask |
| `Main` | `Assets/Scripts/Main/` | Common / VContainer / UniTask / UniTask.Addressables / Unity.Addressables / Unity.ResourceManager |

- `Title` / `Home` / `Main` は `Common` に依存し、逆方向の依存は禁止
- **R3 / DOTween は asmdef の references に現れない**。DLL 側が `autoReferenced` なため、参照を書かなくても全アセンブリから使える（EditMode テストの asmdef だけは `precompiledReferences` に `R3.dll` を明示する必要がある）
- 依存を追加したら asmdef の references を更新する。推移的参照は自動解決されないため、直接 `using` するアセンブリはすべて列挙すること

---

## アセット管理（Addressables）

```
Assets/AddressableAssets/
  ├── Icon/        SVG アイコン
  ├── Modal/       Modal.uxml / Modal.uss
  └── Sound/       AudioClip
```

- `SoundStore` / `ModalStore` はともに `AssetStoreBase` を継承し、ボイラープレート（`UniTask Loaded`・`Start()`・try-catch）を共有
- `AssetStoreBase` は `IStartable` を実装し、`LoadAssetsCore()` をサブクラスに委譲する
- 起動時に Addressables ロードを行い、完了を `UniTask Loaded` プロパティで通知する。使う側は `await _store.Loaded` で待機してから使用する

# 設計ドキュメント

## 設計方針

新しいゲームを作り始めるときの「土台」として機能するテンプレート。以下を目標としている。

- **DI・リアクティブを標準** とし、`Find()` や static は使わない
- **非同期を UniTask** で統一し、キャンセル処理を明示的に行う
- **アセットは Addressables** で遅延ロードし、`Resources.Load` は使わない
- **UI は UI Toolkit（UXML）** で構築し、uGUI は使わない（例外は VFX を UI Toolkit より手前に合成する場合のみ。[effects.md](effects.md) を参照）

---

## シーン構成

```
Common (常駐)
  ├── BootLoader              ビルド起動時に Title をロードする
  ├── SoundPlayer / UiSoundPlayer（UI 操作の効果音を「開く・閉じる・決定」で鳴らす窓口）
  │                           AudioListener も Sound Player に 1 つだけ置く（各シーンのカメラには付けない。遷移中に 2 つにならないように）
  ├── SceneTransitioner
  ├── TransitionPresenter
  ├── OptionPresenter / OptionModel
  └── Store 群（SoundStore / ModalStore ← AssetStoreBase を継承）

Title (アディティブ) → Home (アディティブ) → Main (アディティブ)
```

- `Common` シーンは起動時にロードされ、以降アンロードされない
- 他シーンは `Common` の上にアディティブでロード・アンロードされる
- シーン遷移は `SceneTransitioner.Transit(Scenes next)` を呼ぶだけでよい
- 遷移時は `TransitionPresenter` が画面をフェードアウト→ロード→フェードインの演出を行う。暗幕が出ている間はクリックを遮るため、遷移中に別の遷移（オプションの「タイトルに戻る」など）は始められない
- `Scenes` enum の値は Build Profiles のシーンリストの buildIndex と一致させる（`Common=0 / Title=1 / Home=2 / Main=3`）。シーンを追加・並べ替えるときは enum とシーンリストの両方を揃えること

### 画面遷移

| From | To | トリガー |
|---|---|---|
| 起動 | Title | ビルド起動時に `BootLoader` が Common から自動ロード |
| Title | Home | 「PRESS START」クリック（遷移先は `GameStartButtonPresenter._nextScene` で Inspector 指定） |
| Home | Main | 「ゲーム開始」クリック |
| Main | Home | オプションモーダルの「ゲームをやめる」 |
| Home | Title | オプションモーダルの「タイトルに戻る」（Title では退出ボタン自体を表示しない） |

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
  ├── UiSoundPlayer
  ├── TransitionPresenter
  └── SceneTransitioner

TitleLifetimeScope    GameStartButtonPresenter / SceneBgmPlayer（Bgm.Title）
HomeLifetimeScope     CreditModel / HomePresenter / SceneBgmPlayer（Bgm.Title）
MainLifetimeScope     SceneBgmPlayer（Bgm.Main）
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
- 非同期ロードの完了待ちは `await _store.Loaded` の形で行う。シーン破棄で止まるよう `AttachExternalCancellation(ct)` でキャンセルを伝え、`OperationCanceledException` は正常系として catch する（`Store` 系の仕組みは後述「アセット管理（Addressables）」を参照）

```csharp
// 例: SceneBgmPlayer（Common。各シーンの BGM を流す）
public async UniTask StartAsync(CancellationToken ct = default)
{
    try
    {
        await _soundStore.Loaded.AttachExternalCancellation(ct);
        _soundPlayer.PlayBGM(_soundStore.GetBgm(_bgm));
    }
    catch (OperationCanceledException)
    {
        // シーン破棄によるキャンセルは正常系なので何もしない
    }
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

> この制約は `autoRun` が無効なシーン（Title / Home / Main）の話。Common シーンの `CommonLifetimeScope` は `autoRun` が有効で自身の `Awake` でビルドされるため、Common 内の MonoBehaviour（`OptionPresenter` / `SoundPlayer` など）は `Start()` でインジェクト済みフィールドを使える。

「シーン起動時にインジェクト済みフィールドを使って初期化したい」場合は、`Start()` ではなく `[Inject] Construct(...)` メソッド内で行うか、`IAsyncStartable` を実装した純粋 C# サービスを `RegisterEntryPoint` で登録してそこから MonoBehaviour の public メソッドを呼ぶ。

### シーン遷移のキャンセル処理

`SceneTransitioner` は `SemaphoreSlim` で同時遷移を防ぎ、
連打された場合は最後のリクエストのみ実行する（前の遷移は CancellationToken でキャンセル）。

### ISceneReady — シーン準備完了の通知

`RevealAsync`（フェードイン）の前に、`SceneTransitioner` は次シーンの root GameObject を検索し、`ISceneReady` を実装した**全ての**コンポーネントの `ReadyAsync(ct)` を `UniTask.WhenAll` で並行待機する。

これにより、Addressables の非同期ロードなど「表示前に完了させたい初期化」がフェードイン前に終わり、背景や要素が空白のまま画面が現れるのを防ぐ。

新しいシーンで表示前に待ちたい非同期処理がある場合は、そのシーンの GameObject にアタッチした MonoBehaviour（Presenter など）に `ISceneReady` を実装し、準備完了時に `ReadyAsync` を完了させるだけでよい（実装が無いシーンは素通りする任意フック）。DI に登録しただけの純粋 C# クラスは検索対象外なので注意（実装例は [patterns.md](patterns.md)「4. シーン表示前に非同期初期化を待つ（ISceneReady）」）。

ISceneReady を待つのは `SceneTransitioner.Transit` による遷移だけで、起動直後（`BootLoader`）や Editor でシーンを直接 Play したとき（`CommonSceneLoader`）の初回ロードは対象外（暗幕も出さない）。初回表示のシーンで重い初期化が必要になったら、待機処理を共通化して初回ロードにも入れる。

`ReadyAsync` がキャンセル以外の例外を投げても、暗幕が残り続けないよう `SceneTransitioner` 側で例外をログ出力して握りつぶし、フェードインは必ず実行する（`WaitReadySafelyAsync`）。実装側で初期化失敗を扱いたい場合は `ReadyAsync` 内で完結させること。

---

## サウンド設計

- BGM: `AudioSource.loop = true`、`PlayBGM()` で差し替え。同じ曲がすでに流れていれば頭から再生し直さない
- シーンごとの BGM は、各シーンの LifetimeScope に `SceneBgmPlayer` を流す曲（`Bgm` enum）付きで登録して決める: `builder.RegisterEntryPoint<SceneBgmPlayer>(Lifetime.Scoped).WithParameter(Bgm.Main);`。Title と Home は同じ曲なので、Title → Home では途切れずに流れ続ける
- UI 操作の効果音は `UiSoundPlayer` の `PlayOpen()`（開く）/ `PlayClose()`（閉じる）/ `PlayDecide()`（決定）で鳴らす。どの操作にどの SE を使うかの対応は `UiSoundPlayer` にだけ書く
- SE: `PlayOneShot()` で重ね再生。SoundStore のロード前など clip が null のときは鳴らさない（例外にしない）
- 音量は `OptionModel` が 0–1 で保持し、`BGMVolume` / `SEVolume`（`ReadOnlyReactiveProperty<float>`）で公開する
- `SoundPlayer` は音量変化を Subscribe して AudioSource に即時反映（`BindVolume`）

> `BindVolume` で BGM / SE の AudioSource に `v * VolumeScale`（`VolumeScale = 0.5f`）を反映しているのは、
> OptionModel の値 1.0 がデフォルトの AudioSource 最大音量の半分に相当するようにしているため。

---

## UI 設計（UI Toolkit）

### ファイル配置

```
Assets/Scripts/<Scene>/[<Feature>/]
  ├── *Presenter.cs   （UI ロジック。Title/GameStartButton のように Presenter/ サブフォルダに置く場合もある）
  └── View/
        ├── *.uxml    （見た目 / Addressables 経由でロードするものは AddressableAssets/ に配置）
        └── *.uss     （UXML と同じディレクトリに置く）
```

Common の `Option/` は Presenter と `Option.uxml` / `Option.uss` を同じフォルダに置いている。

### PanelSettings

`Assets/Scripts/Panel Settings.asset` の Scale Mode を **Scale With Screen Size** に設定済み。
基準解像度に対して UI 全体がスケールするため、固定 px 値で指定したサイズが解像度によらず適切な物理サイズになる。

### オプションモーダル

- アイコンクリックで表示、Close ボタンで非表示
- 左側の退出ボタン（`ExitButton`）は現在シーンでラベルと遷移先が変わる。判定と遷移は `OptionExitRouter` が担い、`OptionPresenter` はモーダルの開閉と SE に専念する
  - Home: **「タイトルに戻る」** → Title シーンへ遷移
  - Main: **「ゲームをやめる」** → Home シーンへ遷移
  - Title: 戻る先が無いので退出ボタンを表示しない（`OptionExitRouter.IsExitAvailable`）
  - モーダルは Common で一度だけ生成して使い回すので、ラベルと表示の差し替えは開くたびに `OptionExitRouter.CurrentLabel` / `IsExitAvailable` を読んで `OptionModalPresenter.SetExit()` で行う
- オーバーレイは Option.uss で初期状態を非表示にしてあり、モーダルと効果音（ModalStore / SoundStore）のロードが終わるまではアイコンを押しても開かない。どちらかのロードに失敗した場合はエラーログを出し、オーバーレイとアイコンを非表示にしてオプションを使えなくする（閉じられないオーバーレイを残さないため）
- オーバーレイがゲーム画面を暗幕（色と配置は [design-system.md](design-system.md)「オーバーレイ」を参照）。`picking-mode` は既定（`Position`）にして背後へのクリックを遮る（Title の全画面「PRESS START」などを暗幕越しに押させないため）
- UIDocument の SortingOrder は `1000`（重ね順の一覧は [design-system.md](design-system.md)「UIDocument の設定」を参照）
- モーダル内 UI バインド（スライダー・ボタン）は `OptionModalPresenter`（plain C# クラス）が担い、`OptionPresenter.SetupAsync(ct)` 内で `new` して使う（`ct` は Destroy で止まるトークンで、ロード待ちの間に破棄されたら何もしない）

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
- `AssetStoreBase` は `IStartable` を実装し、`LoadAssetsCore()` をサブクラスに委譲する。サブクラスは `LoadAssetsCore()` とエラーログ用の `AssetCategory` を実装し、各アセットは `LoadAsync<T>(address)`（`Addressables.LoadAssetAsync<T>(...).ToUniTask()` のラッパー）でロードする
- 起動時に Addressables ロードを行い、完了を `UniTask Loaded` プロパティで通知する。使う側は `await _store.Loaded` で待機してから使用する

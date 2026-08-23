# 実装パターン集

よく触る実装パターンのレシピ。新機能を追加するときはここを起点にする。

---

## 1. 新しい Presenter を追加する（シーン単位）

### 手順

**① Presenter クラスを作る**

```csharp
// IAsyncStartable を実装してエントリポイントにする場合
public sealed class YourPresenter : IAsyncStartable, IDisposable
{
    public async UniTask StartAsync(CancellationToken ct)
    {
        try { /* 初期化・購読 */ }
        catch (OperationCanceledException) { }
    }

    public void Dispose() { /* 購読解除など */ }
}
```

MonoBehaviour として配置する場合は `RegisterComponentInHierarchy<YourPresenter>()` を使う。

純粋 C# クラスには `destroyCancellationToken` が無いので、R3 の購読はフィールドの `CancellationTokenSource` に紐づけて `Dispose` で切る（実装例は [DisconnectionHandler](../Assets/Scripts/Main/DisconnectionHandler.cs)）:

```csharp
private readonly CancellationTokenSource _cts = new();

void IStartable.Start()
{
    _model.State.Subscribe(OnChanged).AddTo(_cts.Token);
}

public void Dispose()
{
    _cts.Cancel();
    _cts.Dispose();
}
```

**② LifetimeScope に登録する**

対象シーンの `LifetimeScope`（例: `Assets/Scripts/Main/Injector/MainLifetimeScope.cs`）の `Configure` に追加:

```csharp
// 純粋 C# クラス（エントリポイント）
builder.RegisterEntryPoint<YourPresenter>().AsSelf();

// MonoBehaviour（シーン内に配置済み）
builder.RegisterComponentInHierarchy<YourPresenter>().AsSelf().AsImplementedInterfaces();

// 依存を注入するだけで自動起動不要な場合
builder.Register<YourService>(Lifetime.Scoped);
```

> シーン起動時の初期化を MonoBehaviour の `Start()` で書かないこと。インジェクション完了前に呼ばれるため。`IAsyncStartable.StartAsync()` か `[Inject] Construct(...)` を使う（[architecture.md](architecture.md)「MonoBehaviour のインジェクションタイミング」）。

**③ UI 要素の取得は `Awake()` ではなく `IStartable.Start()` で行う**

`UIDocument` の visualTree は `UIDocument.OnEnable()` で構築される。シーンロード時は「全コンポーネントの `Awake` → 全コンポーネントの `OnEnable`」の順で呼ばれるため、
**`Awake()` の時点では `rootVisualElement` にツリーが入っていない可能性がある**（`root.Q<Button>(...)` が null を返す）。

VContainer の `IStartable.Start()` は `LifetimeScope.Build()` 完了後に呼ばれ、`OnEnable` より確実に後なので、ここで View を組み立てる。

```csharp
public sealed class YourPresenter : MonoBehaviour, IStartable
{
    private YourView _view;

    void IStartable.Start()   // 明示的実装にすると Unity の Start() メッセージとして呼ばれない
    {
        UIDocument uiDocument = GetComponent<UIDocument>();
        _view = new YourView(uiDocument.rootVisualElement);
        _view.SomethingClicked += OnClickSomething;
    }
}
```

実装例は [HomePresenter](../Assets/Scripts/Home/HomePresenter.cs)。

---

## 2. async MonoBehaviour での destroyCancellationToken の扱い（Unity 6）

Unity 6 では `destroyCancellationToken` を **一度も参照しないまま MonoBehaviour が破棄される** と
`MissingReferenceException` が発生する（"DestroyCancellation token should be called atleast once before destroying the monobehaviour object"）。

### 対処パターン

async メソッド内で最初の `await` の後に `destroyCancellationToken` を参照する場合、
`await` 中に MonoBehaviour が破棄されると例外が出る。以下の2点を必ず守る:

**① `await` の直後に `this == null` ガードを入れる**

```csharp
private async UniTaskVoid BuildAsync()
{
    try
    {
        await _someTask;

        if (this == null) { return; }   // ← await 後は必ずガード

        CancellationToken ct = destroyCancellationToken;  // ← ガード後に一度だけキャプチャ
        // 以降は ct を使う
    }
    catch (OperationCanceledException) { }
}
```

**② キャプチャした `ct` を以降のすべての箇所で使う**

メソッド内で `destroyCancellationToken` を直接参照するのは最初のキャプチャ時のみ。
`CancellationTokenSource.CreateLinkedTokenSource` や他のメソッドへの引数も `ct` を渡す。

---

## 3. DOTween + UI Toolkit でのスタイル値ゲッター（フリーズ対策）

UI Toolkit のスタイルプロパティを DOTween ゲッターに直接渡すと、シーケンス開始フレームでの
値読み取りが不定になり `OnComplete` が発火しないケースがある。

### NG パターン

```csharp
DOTween.To(() => _overlay.style.opacity.value, v => _overlay.style.opacity = v, 1f, 0.25f)
```

スタイルプロパティの `.value` を毎フレーム読み取るため、前フレームの状態に依存して初期値が不正になることがある。

### OK パターン（ローカル float 変数）

```csharp
float opacity = 0f;
DOTween.To(
    () => opacity,
    v => { opacity = v; _overlay.style.opacity = v; },
    1f, 0.25f
)
```

ローカル float 変数を「仲介」として使うことで初期値が確定し、`OnComplete` が確実に発火する。
`TransitionPresenter`（フェード演出）はこのパターンで実装済み。同様の Tween を新たに書く場合も必ずこの形式を使う。

> あわせて、フェードの Tween には `.OnKill(() => tcs.TrySetResult())` を付ける。途中で `Kill()` されたとき `OnComplete` は呼ばれないため、`await` している `UniTaskCompletionSource` を `OnKill` でも完了させないとデッドロックする（シーン破棄・連続遷移で発生）。

### ループ Tween（点滅などの繰り返し演出）

点滅・脈動のように終わりのない演出は `SetLoops(-1, LoopType.Yoyo)` で往復させる。ループ Tween は自動で終了しないため、
**開始を `OnEnable`、破棄を `OnDisable` に対応させ、停止時にスタイルを既定値へ戻す**（半透明のまま固まるのを防ぐ）。

```csharp
private void OnEnable()  { StartBlink(); }
private void OnDisable() { StopBlink(); }

private void StartBlink()
{
    StopBlink();
    float opacity = 1f;                       // ローカル float を仲介（上記 OK パターン）
    _label.style.opacity = opacity;
    _blinkTween = DOTween.To(
        () => opacity,
        v => { opacity = v; _label.style.opacity = v; },
        _blinkMinOpacity, _blinkDuration
    )
    .SetEase(Ease.InOutSine)
    .SetLoops(-1, LoopType.Yoyo);
}

private void StopBlink()
{
    _blinkTween?.Kill();
    _blinkTween = null;
    if (_label != null) { _label.style.opacity = 1f; }
}
```

シーン遷移のトリガーになる要素（タイトルの `PRESS START` など）は、**クリックハンドラの先頭でも `StopBlink()` を呼ぶ**。
フェードアウト中に点滅が続くと演出が濁るため。実装例は [GameStartButtonPresenter](../Assets/Scripts/Title/GameStartButton/Presenter/GameStartButtonPresenter.cs)。

---

## 4. シーン表示前に非同期初期化を待つ（ISceneReady）

Addressables ロードやネットワーク初期化など「フェードイン前に終わらせたい処理」がある場合、
そのシーンの Presenter（や任意の MonoBehaviour）に `ISceneReady` を実装する。
`SceneTransitioner` がフェードイン前に、次シーン内の **全** `ISceneReady` 実装の `ReadyAsync` を
`UniTask.WhenAll` で待機する（実装が無いシーンは素通り）。

```csharp
public sealed class YourPresenter : IAsyncStartable, ISceneReady
{
    private readonly UniTaskCompletionSource _ready = new();

    public async UniTask StartAsync(CancellationToken ct)
    {
        try
        {
            await LoadAssetsAsync(ct);   // 表示前に終わらせたい初期化
            _ready.TrySetResult();        // 完了を通知 → フェードイン開始
        }
        catch (OperationCanceledException) { }
    }

    // SceneTransitioner がフェードイン前にこれを await する
    public UniTask ReadyAsync(CancellationToken ct) => _ready.Task.AttachExternalCancellation(ct);
}
```

> `ReadyAsync` がキャンセル以外の例外を投げても、`SceneTransitioner` 側でログ出力して握りつぶしフェードインは継続する（暗幕が残らない）。初期化失敗の扱いは `ReadyAsync` 内で完結させること。

---

## 5. 新しいシーンを追加する

シーンを1つ増やすときに触る箇所は決まっている。抜けやすいのは **Build Settings と `Scenes` enum の対応** と **`.meta` の追従**。

**① シーンを作る**

`Assets/Scenes/<Name>.unity` を作り、既存シーン（[Title.unity](../Assets/Scenes/Title.unity) など）と同じルート構成にする。

```
[BOOT]      ├── LifeTime Scope        <Name>LifetimeScope（parentReference = CommonLifetimeScope / autoRun = 0）
            └── Common Scene Loader   CommonSceneLoader プレハブのインスタンス
[LOGIC]     （純粋ロジック用。空でよい）
[CONTENTS]  ├── UI                    UIDocument + <Name>Presenter
            └── MainCamera            Camera + AudioListener + UniversalAdditionalCameraData
```

`Common Scene Loader` を置いておくと、そのシーンから直接 Play しても Common シーンがロードされる。

**② Build Settings に登録し、`Scenes` enum と番号を揃える**

File → Build Settings にシーンを追加し、**遷移順に並べ替える**。そのうえで [SceneTransitioner.cs](../Assets/Scripts/Common/SceneManagement/SceneTransitioner.cs) の `Scenes` enum を Build Settings の buildIndex と一致させる。

```csharp
public enum Scenes
{
    Common = 0,
    Title = 1,
    Home = 2,
    Main = 3
}
```

> 途中に挿入すると後続の番号がずれる。`Scenes` を `[SerializeField]` で持つコンポーネント（[GameStartButtonPresenter](../Assets/Scripts/Title/GameStartButton/Presenter/GameStartButtonPresenter.cs) の `_nextScene` など）は **シーンファイルに数値で保存されている** ため、意図した遷移先のままか確認すること。

**③ アセンブリを分ける**

`Assets/Scripts/<Name>/<Name>.asmdef` を作り、`Common` と必要なライブラリを参照する（`Common` から新シーンへの逆依存は禁止）。既存の [Title.asmdef](../Assets/Scripts/Title/Title.asmdef) をコピーするのが早い。

**④ LifetimeScope を作る**

`Assets/Scripts/<Name>/Injector/<Name>LifetimeScope.cs` を作り、シーン固有のサービスと Presenter を登録する（上記「1. 新しい Presenter を追加する」参照）。

**⑤ 遷移を繋ぐ**

遷移元の Presenter から `_sceneTransitioner.Transit(Scenes.<Name>)` を呼ぶ。フェード・ロード・アンロードは `SceneTransitioner` が面倒を見る。

**⑥ テストを追加する**

PlayMode テストはシーン名で読む（`SceneManager.LoadSceneAsync("<Name>", LoadSceneMode.Single)`）。`CommonSceneLoader._loaded` の reflection リセットを `[UnityTearDown]` に入れること。実装例は [HomeSceneTests](../Assets/Tests/PlayMode/HomeSceneTests.cs)。

---

## 共通ルール

命名・型・アーキテクチャの規約は [CLAUDE.md](../CLAUDE.md) の「コーディング規約」「アーキテクチャ」節を正とする。ここでは UI 実装で忘れやすいものだけ挙げる:

- USS では `gap` 禁止 → 子要素の `margin` で代替
- USS には `:last-child` / `:nth-child` などの構造擬似クラスが無い → 末尾判定は要素を生成する C# 側で modifier クラスを付けて表現する（[design-system.md](design-system.md)「明細行」参照）

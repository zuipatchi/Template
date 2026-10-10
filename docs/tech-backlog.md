# 技術バックログ（判断待ち・未確認・改善候補）

**Claude 用。** チェック系スキル（`/feature`・`/ship`・`/code-check`・`/docs-check`）が**見送った項目**と、**Unity での確認が済んでいない項目**の一覧。運用ルールは [.claude/shared/check-policy.md](../.claude/shared/check-policy.md)「6. 技術バックログ」を参照。作りたいゲームの仕様は [game-backlog.md](game-backlog.md)（ユーザー用）に書く。

- 解決したら項目ごと削除する（「解決済み」として残さない。経緯は git のコミットに残る）
- 各項目の「推奨」は Claude の案と理由。ユーザーが方針を決めたら、その項目に「決定: 推奨どおり」または「決定: 〜」と書き足すだけでよい。次の作業で Claude が対応して項目を消す
- 各項目の ID は再利用しない

---

## 判断待ち

方針が決まれば直せるもの。

### B-11 遷移が異常終了すると暗幕が残り、操作できなくなる
- 場所: [SceneTransitioner.cs](../Assets/Scripts/Common/SceneManagement/SceneTransitioner.cs) `Transit`
- 内容: 次の 2 つの場合に、クリックを遮る暗幕が出たまま `RevealAsync` が呼ばれず、ゲームが進められなくなる
  - **キャンセル以外の例外**: 暗幕を出した後に `BuildLifetimeScopes`（DI の登録漏れ）や `LoadSceneAsync`（Build Profiles に無いシーン）が例外を投げると、`OperationCanceledException` しか catch していないため暗幕が残る。Title / Home のボタンも無効のまま戻らない
  - **遷移中に元のシーンへの遷移が来た場合**: 実行中の遷移をキャンセルしてから「同じシーンなら何もしない」判定をするため、暗幕を出した後に元のシーンへの `Transit` が来ると、どちらの遷移も暗幕を戻さない。今は暗幕がクリックを遮るので UI からは起きず、コードから呼んだときだけ起きる
- 選択肢: (a) `catch (Exception e)` で `Debug.LogException` してから `RevealAsync` で暗幕を戻す（オプションの「タイトルに戻る」で復帰できる）、(b) 暗幕を残したままエラー画面を出す、(c) 今のまま（開発中にログで気付ければよい）。2 つ目は、同じシーンの判定を実行中の遷移のキャンセルより前に移す
- 推奨: **(a) と、同じシーンの判定の前倒しの両方を行う**。同じクラスの `WaitReadySafelyAsync` が「例外で暗幕を残さない」方針を取っており、それに揃えられる。暗幕を戻せば、Common のオプションから「タイトルに戻る」で復帰できる。エラー時の復帰方法は仕様の判断になるため、今回は見送った
- 出典: /code-check（2026-10-10）

---

## 未確認

Unity Editor での確認が済んでいないもの。確認できたら削除する。

### U-04 エフェクトの Canvas と UIDocument の重なり順
- 場所: [effects.md](effects.md)（Canvas の `sortingOrder = 100`）、[design-system.md](design-system.md)「UIDocument の設定」
- 内容: uGUI の Canvas と UIDocument の SortingOrder が同じ基準で並ぶか（エフェクトがシーンの UI より手前・オプションと遷移の暗幕より奥に出るか）が未検証
- 推奨: **エフェクトを実装するときに Unity MCP で重なりを確認し、違っていれば effects.md の値と説明を直す**。今はエフェクトの実装が無く確かめられないため
- 出典: B-09 の対応（2026-10-10）

---

## 改善候補

急がないもの。手が空いたときや関連する箇所を触るときに対応する。

### I-08 Addressables のハンドルを解放していない
- 場所: [AssetStoreBase.cs](../Assets/Scripts/Common/Store/AssetStoreBase.cs) `LoadAsync<T>`
- 内容: `LoadAsync<T>` はロード結果だけを返してハンドルを捨てているため、`Addressables.Release` を呼べない。SoundStore / ModalStore は Common に常駐し 1 回しかロードしないので通常のプレイでは実害はほぼ無いが、CommonLifetimeScope を作り直す状況（PlayMode テストなど）では参照カウントが積み上がる
- 直すなら: `AssetStoreBase` でハンドルを保持し、`IDisposable` を実装して `Dispose` で `Addressables.Release` する
- 推奨: **今は保留し、シーンごとにアセットを読み込んで捨てる Store を作るときに対応する**。今の Store はアプリ終了まで使い続けるアセットしか持たないため、解放する場面が無い
- 出典: I-04 の保留分（2026-10-10）

### I-09 BGM をすべてメモリに常駐させている
- 場所: [SoundStore.cs](../Assets/Scripts/Common/Store/SoundStore.cs)、BGM の AudioClip のインポート設定
- 内容: 起動時に全 BGM（タイトル曲・メイン曲）を読み込み、アプリ終了まで保持している。メモリへの影響は AudioClip の Load Type（Decompress On Load か Streaming か）で大きく変わる
- 直すなら: BGM の Load Type を Streaming にする。または BGM を使うシーンでだけロードし、離れるときに解放する（I-08 が前提）
- 推奨: **今は保留し、BGM の曲数が増えたときか、モバイル・WebGL でメモリが問題になったときに、まず Load Type を Streaming にする**。今は 2 曲だけで、計測なしに効果を判断できないため
- 出典: I-04 の保留分（2026-10-10）

### I-10 EditMode テストの asmdef が直接使っていないアセンブリを参照している
- 場所: [Tests.EditMode.asmdef](../Assets/Tests/EditMode/Tests.EditMode.asmdef)（R3.Unity・UniTask の GUID）
- 内容: CLAUDE.md「EditMode テストの注意点」は「`references` にはテストコードが直接使う型のアセンブリだけを入れる」としているが、今のテスト（CreditModelTests / OptionModelTests）は R3.Unity と UniTask の型を直接使っていない（R3 は `precompiledReferences` の R3.dll から来ている）
- 直し方: 2 つの参照を外して EditMode テストを実行し、コンパイルと実行が通ることを確かめる
- 推奨: **次に EditMode テストを追加・変更するときに、参照を外してテストを実行する**。外してもコンパイルが通るかは Unity で実行しないと確かめられないため
- 出典: /docs-check（2026-10-10）

---

## 検討済み・対応しない

チェックで指摘されても再調査しない項目（前提が変わったときだけ見直す）。

- **DOTween で opacity を動かす処理の共通化**（TransitionPresenter / GameStartButtonPresenter）: docs/patterns.md に載せた定型パターンとして意図的に同じ書き方をしている。共通化にはアセンブリをまたぐ公開の拡張メソッドが要る
- **GameStartButtonPresenter の点滅処理の切り出し**: 約 30 行で、105 行のファイルを分けるとかえって細かくなりすぎる
- **BootLoader と Transit の「ロード → スコープ構築 → アクティブ化」の共通化**: BootLoader はフェードも DI も持たない起動専用の処理で、寄せると起動時の挙動が変わる
- **「アクティブシーンが X か」判定のヘルパー化**: 2 か所だけで、OptionExitRouter 側はすでに `IsInMainScene()` にまとまっている
- **SceneTransitioner の `_runningCts` の Cancel / Dispose 2 行の共通化**: 効果が小さい
- **CLAUDE.md と architecture.md に R3 の注意点・Scenes enum の値・AssetStore の説明が両方ある**: CLAUDE.md は要約として持っているので許容する
- **Main.asmdef が使っていない UniTask.Addressables / Addressables / ResourceManager を参照している**: Main はゲーム本体を実装するシーンで、Store を作るときに必要になる。architecture.md の表もこの構成で書いてある
- **CommonSceneLoader が `gameObject.scene` ではなくアクティブシーンのスコープを構築している**: 今の起動・遷移の流れではどちらも同じシーンを指す。`static _loaded` で最初の 1 つしか動かないため、`gameObject.scene` に変えても複数シーンを開いて Play したときの問題は解決しない
- **SceneTransitioner.Transit の長さ（約 95 行）・OptionPresenter の依存 4 つ**: どちらも 1 つの責務に収まっている。150 行・141 行で分割の基準（200 行）を下回る
- **Store のアドレス文字列の `const` 化・既定値と同じ初期化子（`= null` / `= false`）・既存フィールドへの代入での `new()`**: .editorconfig の対象外で、好みの範囲
- **ドキュメント間で重なる短い説明**（ISceneReady は MonoBehaviour だけが検出される・オプション退出ボタンの分岐・PlayMode テストの `_loaded` リセット）: CLAUDE.md の表と注意点は要約として持ち、詳細は architecture.md / patterns.md に置く
- **購読解除・注入タイミング・Destroy トークンの書き方が architecture.md と patterns.md で異なる**: 純粋 C# と MonoBehaviour、`destroyCancellationToken` と `GetCancellationTokenOnDestroy()` はどちらも正しく動き、場面に応じた書き分けになっている
- **docs のコード例の一部が断片になっている**（未定義の変数・`await` の無い `async`）: 説明したい箇所だけを抜き出した例として許容する
- **新規シーンの階層図の GameObject 名と実シーンの名前の違い**（`Lifetime Scope` / `LifeTime Scope`、`MainCamera` / `Main Camera`）: 名前は動作に影響しない

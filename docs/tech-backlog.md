# 技術バックログ（判断待ち・未確認・改善候補）

**Claude 用。** チェック系スキル（`/feature`・`/ship`・`/code-check`・`/docs-check`）が**見送った項目**と、**Unity での確認が済んでいない項目**の一覧。運用ルールは [.claude/shared/check-policy.md](../.claude/shared/check-policy.md)「6. 技術バックログ」を参照。作りたいゲームの仕様は [game-backlog.md](game-backlog.md)（ユーザー用）に書く。

- 解決したら項目ごと削除する（「解決済み」として残さない。経緯は git のコミットに残る）
- ユーザーが方針を決めたら、その項目に「決定: 〜」と書き足すだけでよい。次の作業で Claude が対応して項目を消す
- 各項目の ID は再利用しない

---

## 判断待ち

方針が決まれば直せるもの。

### B-01 Title で「タイトルに戻る」を押しても何も起きない
- 場所: [OptionExitRouter.cs](../Assets/Scripts/Common/Option/OptionExitRouter.cs)、[SceneTransitioner.cs](../Assets/Scripts/Common/SceneManagement/SceneTransitioner.cs)（同じシーンへの遷移は何もしない）
- 内容: Title でオプションを開くと「タイトルに戻る」が表示されるが、押しても SE が鳴ってモーダルが閉じるだけ
- 選択肢: Title ではボタンを非表示／無効にする、または仕様として docs に明記する
- 出典: code-check（2026-10-10）

### B-02 アセットのロード失敗時にオプションのオーバーレイが残る
- 場所: [OptionPresenter.cs](../Assets/Scripts/Common/Option/OptionPresenter.cs) `SetupAsync`
- 内容: ModalStore / SoundStore のロードに失敗すると半透明オーバーレイが全画面を覆ったまま残る。単に隠すだけだと、後でアイコンを押したときに閉じるボタンの無いオーバーレイが開く
- 選択肢: 失敗時はアイコンを無効化する、エラー表示を出す、など失敗時の振る舞いを決める
- 出典: code-check（2026-10-10）

### B-03 効果音再生の窓口クラスを作るか
- 場所: [OptionPresenter.cs](../Assets/Scripts/Common/Option/OptionPresenter.cs)（依存 5 つ）、[HomePresenter.cs](../Assets/Scripts/Home/HomePresenter.cs)（依存 4 つ）、[GameStartButtonPresenter.cs](../Assets/Scripts/Title/GameStartButton/Presenter/GameStartButtonPresenter.cs)
- 内容: 各 Presenter が `SoundStore` と `SoundPlayer` を両方受け取り、「開く＝Enter2、閉じる＝Cancel1、決定＝Enter1」の対応を個別に書いている
- 直すなら: Common に `PlayEnter1()` / `PlayEnter2()` / `PlayCancel()` を持つ窓口クラスを新設し、CommonLifetimeScope に登録する（公開 API の新設なので設計判断）
- 出典: duplicate-check・split-check（2026-10-10）

### B-04 BGM の再生が Title にしかない
- 場所: [AudioManager.cs](../Assets/Scripts/Title/Sound/AudioManager.cs)、[SoundStore.cs](../Assets/Scripts/Common/Store/SoundStore.cs)
- 内容: Home / Main でも Title の BGM が流れ続ける。`SoundStore.MainBGM` と `ResultSE` は読み込んでいるがどこからも使われていない
- 選択肢: Main に BGM を切り替える EntryPoint を置く、使わないなら Store から外す、テンプレートの使用例として残す
- 出典: code-check（2026-10-10）

### B-05 初回ロードでは ISceneReady が待たれない
- 場所: [BootLoader.cs](../Assets/Scripts/Common/SceneManagement/BootLoader.cs)、[CommonSceneLoader.cs](../Assets/Scripts/Common/SceneManagement/CommonSceneLoader.cs)
- 内容: ISceneReady を待つのは `SceneTransitioner.Transit` だけ。起動直後や Editor でシーンを直接 Play したときは暗幕も出さず、初期化前の画面が見える
- 選択肢: 初回ロードでも暗幕を出して待つ（待機処理を SceneExtensions に共通化）、または「初回ロードは対象外」と docs に明記する
- 出典: code-check（2026-10-10）

### B-06 UI 要素の取得タイミングが patterns.md の推奨と違う
- 場所: [OptionPresenter.cs](../Assets/Scripts/Common/Option/OptionPresenter.cs)・[TransitionPresenter.cs](../Assets/Scripts/Common/Transition/TransitionPresenter.cs)（Awake で取得）、[GameStartButtonPresenter.cs](../Assets/Scripts/Title/GameStartButton/Presenter/GameStartButtonPresenter.cs)（OnEnable で取得）
- 内容: [patterns.md](patterns.md) は「UI 要素の取得は `IStartable.Start()` で」と定めているが、既存コードは Awake / OnEnable で取得している（現状は動いている）。patterns.md の「全コンポーネントの Awake → 全コンポーネントの OnEnable」という順序説明も、Unity の実際の挙動と違う可能性がある（未検証）
- 選択肢: コードを推奨の形に寄せる、または「同じ GameObject の UIDocument を使う場合は例外」と docs に書く
- 出典: code-check・sync-docs（2026-10-10）

### B-07 使われていないコンポーネントの説明が design-system.md に残っている
- 場所: [design-system.md](design-system.md) の `.action-button` と `AddButtonHoverEffect`
- 内容: どの USS / C# にも定義・使用がない。削除済み機能の名残かどうかは不明
- 選択肢: 削除する、または「テンプレート未使用の参考例」と明記する
- 出典: docs-review・sync-docs（2026-10-10）

### B-08 uGUI 禁止と effects.md の uGUI 使用
- 場所: [CLAUDE.md](../CLAUDE.md)「アーキテクチャ」、[effects.md](effects.md)
- 内容: CLAUDE.md は「uGUI 禁止」だが、effects.md は VFX の合成に uGUI の Canvas / RawImage を使っている
- 選択肢: CLAUDE.md に「VFX を UI Toolkit より手前に合成する場合のみ例外（effects.md 参照）」と書き足す
- 出典: docs-review（2026-10-10）

### B-09 effects.md の Canvas の sortingOrder が重ね順と合わない
- 場所: [effects.md](effects.md)、[design-system.md](design-system.md)「UIDocument の設定」
- 内容: effects.md は Canvas の `sortingOrder = 100` で「UI Toolkit より上」としているが、このテンプレートでは Option が 1000、Transition が 2000 なので、100 はそれらより下になる
- 選択肢: エフェクトをどの層に置くべきか決めて、effects.md の値と説明を直す
- 出典: docs-review（2026-10-10）

### B-10 カラーパレットに無い文字色
- 場所: [design-system.md](design-system.md)「カラーパレット」
- 内容: `.btn-secondary` の文字色 `rgb(230, 230, 245)` がパレットに無い
- 選択肢: パレットに「テキスト（セカンダリボタン）」の行を足す
- 出典: docs-review（2026-10-10）

---

## 未確認

Unity Editor での確認が済んでいないもの。確認できたら削除する。

（なし）

---

## 改善候補

急がないもの。手が空いたときや関連する箇所を触るときに対応する。

### I-01 CommonLifeTimeScope.cs のファイル名
- [CommonLifeTimeScope.cs](../Assets/Scripts/Common/Injector/CommonLifeTimeScope.cs) のファイル名（`LifeTime`）とクラス名（`CommonLifetimeScope`）の大文字小文字が違う。他シーンの `*LifetimeScope.cs` にも揃っていない。直すなら Unity Editor でリネームし（.meta の GUID を保つ）、CLAUDE.md のリンクも直す

### I-02 注入前のクリックで NullReferenceException の可能性
- [GameStartButtonPresenter.cs](../Assets/Scripts/Title/GameStartButton/Presenter/GameStartButtonPresenter.cs) はクリック登録を OnEnable で行うため、Editor で Title から直接起動した直後の数フレームにクリックされると注入前のフィールドを参照する可能性がある（人が操作できる猶予はごくわずか）

### I-03 PlayerPrefs.Save を呼んでいない
- [OptionModel.cs](../Assets/Scripts/Common/Option/OptionModel.cs) は `SetFloat` のみ。強制終了やモバイルのプロセス kill で音量設定が保存されない可能性がある。モーダルを閉じるときに `Save()` を呼ぶ案

### I-04 サウンド関連のパフォーマンス（計測なしでは効果が判断できない）
- [SoundStore.cs](../Assets/Scripts/Common/Store/SoundStore.cs) の 6 つのロードが直列（`UniTask.WhenAll` で並列化できる）
- Addressables のハンドルを保持しておらず `Release` できない（`AssetStoreBase.LoadAsync` でハンドルを保持し、`IDisposable` で解放する案）
- 全 BGM を常駐させている（BGM の Load Type を Streaming にする案）
- 音量スライダーのドラッグ中に `PlayerPrefs.SetFloat` を連続で呼んでいる

### I-05 ドキュメントの表記ゆれ
- `LifeTime Scope`（GameObject 名）と `LifetimeScope` の混在、`StartAsync` の引数名（`cancellation` / `ct`）、半角英字前後のスペース、`scale` の値の書き方（1 値 / 2 値）。統一の基準がドキュメントに無い
- [patterns.md](patterns.md) 1 節で、前の例は `IAsyncStartable` なのに後の例は `void IStartable.Start()` を使っている

### I-06 遷移中に AudioListener が 2 つになる
- Title と Home の両方のカメラに AudioListener があり、遷移中に両シーンが同時に読み込まれている間だけ「There are 2 audio listeners in the scene」のログが出る（エラーではない。遷移完了で解消）。直すなら AudioListener を Common シーンに 1 つだけ置き、各シーンのカメラから外す
- 出典: Unity MCP での動作確認（2026-10-10）

---

## 検討済み・対応しない

チェックで指摘されても再調査しない項目（前提が変わったときだけ見直す）。

- **DOTween で opacity を動かす処理の共通化**（TransitionPresenter / GameStartButtonPresenter）: docs/patterns.md に載せた定型パターンとして意図的に同じ書き方をしている。共通化にはアセンブリをまたぐ公開の拡張メソッドが要る
- **GameStartButtonPresenter の点滅処理の切り出し**: 約 30 行で、105 行のファイルを分けるとかえって細かくなりすぎる
- **BootLoader と Transit の「ロード → スコープ構築 → アクティブ化」の共通化**: BootLoader はフェードも DI も持たない起動専用の処理で、寄せると起動時の挙動が変わる
- **「アクティブシーンが X か」判定のヘルパー化**: 2 か所だけで、OptionExitRouter 側はすでに `IsInMainScene()` にまとまっている
- **SceneTransitioner の `_runningCts` の Cancel / Dispose 2 行の共通化**: 効果が小さい
- **CLAUDE.md と architecture.md に R3 の注意点・Scenes enum の値・AssetStore の説明が両方ある**: CLAUDE.md は要約として持っているので許容する

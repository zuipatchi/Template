# 技術バックログ（判断待ち・未確認・改善候補）

**Claude 用。** チェック系スキル（`/feature`・`/ship`・`/code-check`・`/docs-check`）が**見送った項目**と、**Unity での確認が済んでいない項目**の一覧。運用ルールは [.claude/shared/check-policy.md](../.claude/shared/check-policy.md)「6. 技術バックログ」を参照。作りたいゲームの仕様は [game-backlog.md](game-backlog.md)（ユーザー用）に書く。

- 解決したら項目ごと削除する（「解決済み」として残さない。経緯は git のコミットに残る）
- 各項目の「推奨」は Claude の案と理由。ユーザーが方針を決めたら、その項目に「決定: 推奨どおり」または「決定: 〜」と書き足すだけでよい。次の作業で Claude が対応して項目を消す
- 各項目の ID は再利用しない

---

## 判断待ち

方針が決まれば直せるもの。

（なし）

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

### I-01 CommonLifeTimeScope.cs のファイル名
- [CommonLifeTimeScope.cs](../Assets/Scripts/Common/Injector/CommonLifeTimeScope.cs) のファイル名（`LifeTime`）とクラス名（`CommonLifetimeScope`）の大文字小文字が違う。他シーンの `*LifetimeScope.cs` にも揃っていない。直すなら Unity Editor でリネームし（.meta の GUID を保つ）、CLAUDE.md のリンクも直す
- 推奨: **直す**。Unity MCP で Editor からリネームすれば .meta の GUID が保たれ、シーンの参照も壊れない。次に Common を触る作業のついでに行う

### I-02 注入前のクリックで NullReferenceException の可能性
- [GameStartButtonPresenter.cs](../Assets/Scripts/Title/GameStartButton/Presenter/GameStartButtonPresenter.cs) はクリック登録を OnEnable で行うため、Editor で Title から直接起動した直後の数フレームにクリックされると注入前のフィールドを参照する可能性がある（人が操作できる猶予はごくわずか）
- 推奨: **クリックのハンドラの先頭で、注入前なら何もせず抜ける null ガードを入れる**。人が実際に踏む可能性は低いが、1 行で防げるため

### I-03 PlayerPrefs.Save を呼んでいない
- [OptionModel.cs](../Assets/Scripts/Common/Option/OptionModel.cs) は `SetFloat` のみ。強制終了やモバイルのプロセス kill で音量設定が保存されない可能性がある。モーダルを閉じるときに `Save()` を呼ぶ案
- 推奨: **直す**。モバイルでは OS にプロセスを終了されることが普通にあり、設定が戻ると不具合として報告されやすい。モーダルを閉じるときに 1 回だけ `Save()` を呼べばよく、ドラッグ中の連続書き込み（I-04）の対策にもなる

### I-04 サウンド関連のパフォーマンス（計測なしでは効果が判断できない）
- [SoundStore.cs](../Assets/Scripts/Common/Store/SoundStore.cs) の 5 つのロードが直列（`UniTask.WhenAll` で並列化できる）
- Addressables のハンドルを保持しておらず `Release` できない（`AssetStoreBase.LoadAsync` でハンドルを保持し、`IDisposable` で解放する案）
- 全 BGM を常駐させている（BGM の Load Type を Streaming にする案）
- 音量スライダーのドラッグ中に `PlayerPrefs.SetFloat` を連続で呼んでいる
- 推奨: **並列化（`UniTask.WhenAll`）だけ今やり、残りは保留する**。並列化は数行で起動時間が縮み、挙動も変わらない。ハンドルの解放と BGM の Streaming 化は、音声が増えてメモリが問題になってから。スライダーの連続書き込みは I-03 で解消する

### I-05 ドキュメントの表記ゆれ
- `LifeTime Scope`（GameObject 名）と `LifetimeScope` の混在、`StartAsync` の引数名（`cancellation` / `ct`）、半角英字前後のスペース、`scale` の値の書き方（1 値 / 2 値）。統一の基準がドキュメントに無い
- [patterns.md](patterns.md) 1 節で、前の例は `IAsyncStartable` なのに後の例は `void IStartable.Start()` を使っている
- 推奨: **`LifetimeScope` と、コード例の引数名 `ct` だけ統一し、半角スペースと `scale` の書き方は放置する**。前の 2 つは検索や写経の邪魔になるが、後の 2 つは読む上で困らない。patterns.md 1 節の例の食い違いは、後の例を `IAsyncStartable` にそろえる

### I-06 遷移中に AudioListener が 2 つになる
- Title と Home の両方のカメラに AudioListener があり、遷移中に両シーンが同時に読み込まれている間だけ「There are 2 audio listeners in the scene」のログが出る（エラーではない。遷移完了で解消）。直すなら AudioListener を Common シーンに 1 つだけ置き、各シーンのカメラから外す
- 推奨: **直す**。今は害の無いログだけだが、遷移中に BGM の聞こえ方が一瞬変わる原因にもなりうる。AudioListener を Common シーンの SoundPlayer と同じ GameObject に 1 つ置き、各シーンのカメラからは外す（シーンの変更は Unity MCP で行う）
- 出典: Unity MCP での動作確認（2026-10-10）

### I-07 Play するとフォントアセットに差分が出る
- [NotoSansJP-Bold SDF.asset](../Assets/Font/NotoSansJP-Bold%20SDF.asset) は Atlas Population Mode が Dynamic のため、Play 中に文字のアトラス（1024×1024）が作られてアセットに書き込まれ、Git に差分が出る（中身は実行時のキャッシュでコミット不要）
- 推奨: **フォントアセットの「Clear Dynamic Data On Build」と、Play 終了時に動的データを消す設定を確認し、差分が出ないようにする**。設定で防げない場合は、Claude がコミット前にこのファイルの差分を `git restore` で捨てる運用にする
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

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

---

## 検討済み・対応しない

チェックで指摘されても再調査しない項目（前提が変わったときだけ見直す）。

- **DOTween で opacity を動かす処理の共通化**（TransitionPresenter / GameStartButtonPresenter）: docs/patterns.md に載せた定型パターンとして意図的に同じ書き方をしている。共通化にはアセンブリをまたぐ公開の拡張メソッドが要る
- **GameStartButtonPresenter の点滅処理の切り出し**: 約 30 行で、105 行のファイルを分けるとかえって細かくなりすぎる
- **BootLoader と Transit の「ロード → スコープ構築 → アクティブ化」の共通化**: BootLoader はフェードも DI も持たない起動専用の処理で、寄せると起動時の挙動が変わる
- **「アクティブシーンが X か」判定のヘルパー化**: 2 か所だけで、OptionExitRouter 側はすでに `IsInMainScene()` にまとまっている
- **SceneTransitioner の `_runningCts` の Cancel / Dispose 2 行の共通化**: 効果が小さい
- **CLAUDE.md と architecture.md に R3 の注意点・Scenes enum の値・AssetStore の説明が両方ある**: CLAUDE.md は要約として持っているので許容する

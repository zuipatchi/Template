# Template
Unityで新しくゲームを作るときに利用するテンプレート
ここには開発者向けのメモを記載する

## 完成作例 おやつ屋さん

Unity Hub でこのプロジェクトを開き、Unity 6000.3.18f1 で `Assets/Scenes/Title.unity` を再生する。タイトルをクリックして「ゲーム開始」を押し、注文されたおやつを 3 つの中から選ぶ。5 問のあとに正解数が出て、「もう一度」で最初から遊べる。

入門書を読みながら自分で作る場合は、テンプレートのコミット `429fe7541ca350342794b87ef7238d8cc008c784` から `MySnackShop` ブランチを作る。

## 開発の進め方

Claude Code と Codex のどちらでも同じ開発手順を使えます。プロジェクトの指示は Claude Code が [CLAUDE.md](CLAUDE.md)、Codex が [AGENTS.md](AGENTS.md) から読み込みます。

| 用途 | Claude Code | Codex |
|---|---|---|
| バックログの機能を実装 | `/feature` | `$source-command-feature` |
| 手動変更を検証してコミット | `/ship` | `$source-command-ship` |
| コードを点検 | `/code-check` | `$source-command-code-check` |
| ドキュメントを点検 | `/docs-check` | `$source-command-docs-check` |
| コピーしたテンプレートを初期化 | `/new-project` | `$source-command-new-project` |

0. 作りたいもののアイデアは [docs/game-backlog.md](docs/game-backlog.md) に箇条書きで溜めておく
1. 使用中のツールで feature を実行する。既定ではいちばん上の項目を作り、`G-03` のような ID を添えると項目を指定できる。提示された「何を作るか・完了条件」を承認する
2. エージェントが実装 → 検証ループ（規約チェック・セルフレビュー・テストコード実行・Unity MCP での動作確認）→ ドキュメント更新 → コミットまで進める
3. 報告のチェックリストに沿って Unity Editor で確認する（Editor は開いたままになっている）
4. 気になる点があれば「〇〇が変」と伝えるだけで、2 の流れをもう一度回す

feature を使わずに手で変更した場合は、ship で同じ検証・ドキュメント更新・コミットを行う。

リポジトリ全体の定期点検には次の 2 つを使う（大きな機能を入れた後や週 1 回など）。

- code-check はコード全体のバグ・規約・設計・パフォーマンス・重複・ファイル分割を点検する
- docs-check はドキュメント内部の品質と、コードとの食い違いを点検する

チェック系スキルの共通方針（途中で確認を求めない・直す／見送るの基準）は [.claude/shared/check-policy.md](.claude/shared/check-policy.md) にまとめてある。見送った項目と Unity での確認待ちは [docs/tech-backlog.md](docs/tech-backlog.md) に溜まるので、時間があるときに目を通して「決定: 〜」と書き足せば、次の作業で対応される。

Unity MCP での動作確認には `com.unity.pipeline` パッケージ（[Packages/manifest.json](Packages/manifest.json)）と Unity CLI（`unity` コマンド）が必要。

Codex で C# の自動規約チェックを使うには、プロジェクトと [.codex/hooks.json](.codex/hooks.json) の Stop hook を信頼済みにします。実行には Git、Bash（Windows では Git for Windows に同梱）、.NET SDK、Unity が生成した `.csproj` が必要です。

## 使用 Package
- Addressables
- R3
- UniTask
- VContainer
- DOTween

バージョンは [Packages/manifest.json](Packages/manifest.json) を参照（DOTween は manifest.json ではなく `Assets/Plugins/` に同梱）。

## プラットフォーム
Windows / Mac / WebGL のいずれでもビルドできる。

## 日本語フォント
- ゲーム全体の既定フォントは NotoSansJP Bold (SDF) をテーマで全 UI に適用済み。詳細・差し替え方法・WebGL 固有の制約は [docs/design-system.md](docs/design-system.md)「日本語フォント」を参照

## Asset Store のアセットと Git
- Asset Store からダウンロードしたものは `Assets/AssetStore/` に入れる。このディレクトリも Git で管理する
- **このテンプレートから作るゲームのリポジトリは必ず Private にする。** Asset Store のアセットの多くはライセンスで再配布が禁止されており、Public リポジトリに push すると誰でもダウンロードできる状態で公開してしまう
- このテンプレート自体のリポジトリは Public なので、テンプレートには Asset Store のアセットをコミットしない

## このテンプレートから新規プロジェクトを作る手順

フォルダをコピーした後、Claude Code で `/new-project`、または Codex で `$source-command-new-project` を実行する。

プロジェクト名を聞かれるので答えると、必要な箇所を自動で書き換えてくれる。

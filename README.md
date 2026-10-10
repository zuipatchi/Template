# Template
Unityで新しくゲームを作るときに利用するテンプレート
ここには開発者向けのメモを記載する

## 開発の進め方
1. `/feature` で作りたいものを伝え、提示された「何を作るか・完了条件」を承認する
2. あとは Claude が実装 → 検証ループ（規約チェック・セルフレビュー・テストコード実行・Unity MCP での動作確認）→ ドキュメント更新 → コミットまで一度に進める
3. 報告のチェックリストに沿って Unity Editor で確認する（Editor は開いたままになっている）
4. 気になる点があれば「〇〇が変」と伝えるだけで、2 の流れをもう一度回す

`/feature` を使わずに手で変更した場合は、`/ship` でレビュー・コミット・ドキュメント更新を行う。リポジトリ全体の定期点検には `/code-check`・`/duplicate-check`・`/split-check`・`/docs-review`・`/sync-docs` を使う。

Unity MCP での動作確認には `com.unity.pipeline` パッケージ（[Packages/manifest.json](Packages/manifest.json)）と Unity CLI（`unity` コマンド）が必要。

## 使用 Package
- Addressables
- R3
- UniTask
- VContainer
- DOTween

バージョンは [Packages/manifest.json](Packages/manifest.json) を参照。

## プラットフォーム
Windows / Mac / WebGL のいずれでもビルドできる。

## 日本語フォント
- ゲーム全体の既定フォントは NotoSansJP Bold (SDF) をテーマで全 UI に適用済み。詳細・差し替え方法・WebGL 固有の制約は [docs/design-system.md](docs/design-system.md)「日本語フォント」を参照

## gitignore
- Asset Storeからダウンロードした物は AssetStore ディレクトリに入れるとGitに管理されない

## このテンプレートから新規プロジェクトを作る手順

フォルダをコピーした後、Claude Code で以下を実行する:

```
/new-project
```

プロジェクト名を聞かれるので答えると、必要な箇所を自動で書き換えてくれる。

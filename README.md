# Template
Unityで新しくゲームを作るときに利用するテンプレート
ここには開発者向けのメモを記載する

## 開発の進め方
1. `/feature` を実行して新機能を実装する（ヒアリング→実装→レビュー→改善→動作確認まで自動で進む）
2. 自動テスト（EditMode / PlayMode）を実行する（手順は [CLAUDE.md](CLAUDE.md)「Unity 開発」を参照）
3. Unity Editor で Play して動作確認する
4. 問題なければ `/ship` を実行してコミット・ドキュメント更新まで行う

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

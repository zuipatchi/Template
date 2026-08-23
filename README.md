# Template
Unityで新しくゲームを作るときに利用するテンプレート
ここには開発者向けのメモを記載する

## 開発の進め方
1. `/feature` を実行して新機能を実装する（ヒアリング→実装→テストまで自動で進む）
2. PlayMode: Window → General → Test Runner → PlayMode タブ → Run All で自動テストを実行する
3. Unity Editor で Play して動作確認する
4. 問題なければ `/ship` を実行してコミット・ドキュメント更新まで行う

## テストプレイの仕方
- 自動テスト（EditMode / PlayMode）の実行手順は [CLAUDE.md](CLAUDE.md)「Unity 開発」を参照

## 使用 Package
- Addressables
- R3
- UniTask
- VContainer
- DOTween

バージョンは [Packages/manifest.json](Packages/manifest.json) を参照。

## プラットフォーム
Windows / Mac / WebGL のいずれでもビルドできる。
WebGL 固有のフォント制約は [docs/design-system.md](docs/design-system.md)「日本語フォント」を参照。

## 日本語フォント
- ゲーム全体の既定フォントは NotoSansJP Bold (SDF) をテーマで全 UI に適用済み。詳細・差し替え方法は [docs/design-system.md](docs/design-system.md) を参照

## gitignore
- Asset Storeからダウンロードした物は AssetStore ディレクトリに入れるとGitに管理されない

## このテンプレートから新規プロジェクトを作る手順

フォルダをコピーした後、Claude Code で以下を実行する:

```
/new-project
```

プロジェクト名を聞かれるので答えると、必要な箇所を自動で書き換えてくれる。

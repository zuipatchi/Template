---
description: テンプレートから新規プロジェクトをセットアップする
---

# new-project: 新規プロジェクトセットアップ

このリポジトリを新しいゲームプロジェクトとして初期化する。

## 手順

1. ユーザーに**新しいプロジェクト名（例: MyGame）**を聞く

2. 以下のファイルを更新する:

   ### ProjectSettings/ProjectSettings.asset
   `productName: Template` を `productName: <新プロジェクト名>` に変更
   （`metroPackageName` / `metroApplicationDescription` も同様に変更する）

   ### Template.slnx
   ファイル名を `<新プロジェクト名>.slnx` にリネーム（`git mv Template.slnx <新プロジェクト名>.slnx`）

   ### .vscode/settings.json
   `dotnet.defaultSolution` を `<新プロジェクト名>.slnx` に変更

   ### README.md
   - 1行目の `# Template` を `# <新プロジェクト名>` に変更
   - 2行目のプロジェクト説明をユーザーに確認して書き換える

   ### CLAUDE.md
   - 「プロジェクト概要」セクションの説明を新しいゲームに合わせて書き換える

3. Library フォルダを削除してもらう。VS Code には表示されないので注意。Unity / Unity Hub を一度閉じないと削除できない

4. オンライン機能（UGS マッチメイキング / NGO）はこのテンプレートには含まれていない。
   必要な場合: `Packages/manifest.json` に `com.unity.netcode.gameobjects` / `com.unity.services.multiplayer` / `com.unity.services.authentication` を追加し、`ProjectSettings/ProjectSettings.asset` の `cloudProjectId` と `organizationId` を Unity Editor → Edit → Project Settings → Services で紐づける必要があることを案内する。

5. 完了後、ユーザーに以下を伝える:
   - 変更した内容の一覧
   - .gitを再生成してもらう

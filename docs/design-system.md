# デザインシステム

UI Toolkit（UXML + USS）を使用したUIの設計ルールをまとめる。

## USS ファイルの使い方

スタイルはインライン記述（UXML の `style="..."` 属性）せず、シーンごとの USS ファイルに定義してクラスで適用する。

```xml
<!-- UXML の先頭で USS を読み込む -->
<ui:UXML ...>
    <Style src="MyScene.uss" />
    ...
    <ui:Button name="StartButton" class="btn-accent" />
</ui:UXML>
```

USS ファイルは対応する UXML と同じディレクトリに配置する（例: `View/Home.uss`）。

C# 側から `element.style.*` を書き換えてよいのは、**表示/非表示・アニメーション中の値**のように実行時に変化する状態だけ（例: `_overlay.style.display`、DOTween が動かす `opacity`）。見た目の定義そのものは USS に置く。

---

## オプションアイコンとの重なり防止

Common シーンのオプションアイコン（`right:0 / top:0`、60×60px）は全シーンに `position:absolute` で重なる。
画面の右上角にぴったり張り付ける配置なので、新しいシーンを作成する際は、**右上エリアに UI 要素を配置しない**ように設計すること。

- メインコンテンツは中央寄せ・左寄せで配置する
- 右上に要素を置く場合は `right` を `80px` 以上、`top` を `80px` 以上あける（アイコン 60px + 余白 20px）
- フルスクリーンのオーバーレイ（ローディング・モーダル暗幕など）は `position:absolute` で全画面を覆うため重なりは問題なし
- 全画面をクリック領域にする要素（Title の `.press-start` など）も問題なし。オプションアイコンはシーン側より手前の UIDocument にあり、覆われてもクリックを受け取る（重ね順は後述「UIDocument の設定」を参照）。禁止しているのは**目に見える UI をアイコンの真下に置くこと**

---

## カラーパレット

| 用途 | 値 |
|---|---|
| カード背景 | `rgb(22, 22, 35)` |
| オーバーレイ暗幕 | `rgba(0, 0, 0, 0.55)` |
| ボーダー | `rgba(255, 255, 255, 0.15)` |
| 区切り線 | `rgba(255, 255, 255, 0.1)` |
| テキスト（見出し） | `rgb(240, 240, 255)` |
| テキスト（本文・ラベル） | `rgb(180, 180, 210)` |
| テキスト（ボタン） | `rgb(255, 255, 255)` |
| アクセント（ボタン背景） | `rgb(70, 90, 180)` |

---

## タイポグラフィ

| 用途 | font-size | font-style |
|---|---|---|
| タイトル画面のプロンプト（`PRESS START`） | `28px` | normal（`letter-spacing: 8px`） |
| モーダルタイトル | `20px` | normal（既定フォントが Bold のため指定不要） |
| ラベル（項目名） | `13px` | normal |
| ボタンテキスト | `14px` | normal |

> 既定フォントが NotoSansJP **Bold** なので、`-unity-font-style: bold` を重ねると faux bold で太くなりすぎる。原則 normal のまま使う。

### 文字色とコントラスト

SDF フォントはエッジをアンチエイリアスで描画するため、**低コントラストの文字（暗い背景に対して暗めの色）は細く見え、高コントラストの文字（暗い背景に対して白）は太く見える**（同じウェイトでも錯視で太さが変わって見える）。複数のボタンを並べて太さを揃えたいときは、文字色のコントラストを揃える。例として `btn-secondary` の文字色は `rgb(230, 230, 245)` とし、白文字の `btn-accent` と並べても太さが揃って見えるようにしている。

### 日本語フォント

ゲーム全体の既定フォントは **NotoSansJP Bold (SDF)** で、テーマ [UnityDefaultRuntimeTheme.tss](../Assets/UI%20Toolkit/UnityThemes/UnityDefaultRuntimeTheme.tss) の `.unity-text-element` に `-unity-font-definition` を設定して全テキスト要素（Label / Button 等）へ継承させている。**Label ごとにインラインで `-unity-font` を指定する必要はない**。

- フォントアセット: [Assets/Font/NotoSansJP-Bold SDF.asset](../Assets/Font/NotoSansJP-Bold%20SDF.asset)（Atlas Population Mode = Dynamic。未収録グリフは実行時にソース TTF [NotoSansJP-Bold.ttf](../Assets/Font/NotoSansJP-Bold.ttf) から補完される）
- 太さは Bold (700) で焼いてあるため、`-unity-font-style: bold` を重ねると faux bold で過剰に太くなる場合がある。見出しをさらに強調したいときのみ使う。
- 別の太さ・別フォントに差し替える場合は、新しい SDF を作って TSS の url を差し替える（または PanelSettings の Text Settings に PanelTextSettings を割り当てる）。

> **WebGL では NotoSansJP に無い記号は豆腐（□）になる**。絵文字・ディングバット系（鉛筆 `✎`、`✕`、小三角 `▾` など）はこのフォントに未収録で、**エディタでは OS フォントへフォールバックして見えても WebGL ビルドでは豆腐になる**。閉じる＝`×`(U+00D7)、ドロップダウン矢印＝`▼`(U+25BC) のように収録済みグリフを使うか、画像アイコン（USS `background-image`）／テキストに置き換える。矢印（→←↑↓）・三点リーダ（…）・星（★☆）・●■・引用符は収録済み。

---

## スペーシング

| 用途 | 値 |
|---|---|
| カード内パディング（`.card`） | `32px` 全周 |
| モーダルカード内パディング（`.modal-card`） | `28px 32px` |
| セクション間マージン | `18px` |
| 最終セクション下マージン | `24px` |
| カード見出し下マージン（`.card__title`） | `12px` |
| モーダルタイトル下マージン（`.modal-title`） | `16px` |
| 区切り線下マージン（`.divider`） | `16px`（Modal.uss のみ `20px`） |
| ラベル〜スライダー間 | `4px` |

---

## コンポーネント

USS クラス定義の実装例は以下を参照。

- [Assets/Scripts/Title/GameStartButton/View/Title.uss](../Assets/Scripts/Title/GameStartButton/View/Title.uss) — タイトルシーン
- [Assets/Scripts/Home/View/Home.uss](../Assets/Scripts/Home/View/Home.uss) — ホームシーン
- [Assets/Scripts/Common/Option/Option.uss](../Assets/Scripts/Common/Option/Option.uss) — オプションアイコンと暗幕（Common 常駐）
- [Assets/AddressableAssets/Modal/Modal.uss](../Assets/AddressableAssets/Modal/Modal.uss) — オプションモーダルの中身

USS はスタイルシート単位で読み込まれるため、同じクラス名を複数の USS に定義してよい（`.card` / `.btn-accent` などは各シーンの USS に複製されている）。値を変えるときは**全ファイルを揃えて更新すること**。

### カード（`.card`）

```css
.card {
    background-color: rgb(22, 22, 35);
    border-top-left-radius: 16px; border-top-right-radius: 16px;
    border-bottom-left-radius: 16px; border-bottom-right-radius: 16px;
    border-left-width: 1px; border-right-width: 1px;
    border-top-width: 1px; border-bottom-width: 1px;
    border-left-color: rgba(255, 255, 255, 0.15); /* 他3辺も同じ */
    padding-top: 32px; padding-right: 32px; padding-bottom: 32px; padding-left: 32px;
}
```

### カード見出し（`.card__title`）

```css
.card__title {
    font-size: 20px;
    color: rgb(240, 240, 255);
    -unity-text-align: upper-left;
    margin-bottom: 12px;
}
```

モーダルの見出しは中央寄せの別クラス `.modal-title`（`font-size: 20px; -unity-text-align: middle-center; margin-bottom: 16px;`）を使う。

### 区切り線（`.divider`）

```css
.divider {
    height: 1px;
    background-color: rgba(255, 255, 255, 0.1);
    margin-bottom: 16px;
}
```

Modal.uss の `.divider` だけは `margin-bottom: 20px`（モーダル内は行間を広めに取るため）。

### ボタン

```css
/* アクセントボタン（主要アクション） */
.btn-accent {
    background-color: rgb(70, 90, 180);
    color: rgb(255, 255, 255);
    border-top-left-radius: 8px; /* 他3角も同じ */
    border-left-width: 0; /* 他3辺も同じ */
    padding-top: 10px; padding-right: 10px; padding-bottom: 10px; padding-left: 10px;
    font-size: 14px;
    -unity-text-align: middle-center;
}

/* セカンダリボタン（補助アクション） */
.btn-secondary {
    background-color: rgba(255, 255, 255, 0.07);
    color: rgb(230, 230, 245); /* btn-accent と見た目の太さを揃えるため明るめ。下記「文字色とコントラスト」参照 */
    border-top-left-radius: 8px; /* 他3角も同じ */
    border-left-width: 1px; /* 他3辺も同じ */
    border-left-color: rgba(255, 255, 255, 0.15); /* 他3辺も同じ */
    padding-top: 10px; padding-right: 10px; padding-bottom: 10px; padding-left: 10px;
    font-size: 14px;
    -unity-text-align: middle-center;
}
```

**C# コードで生成したボタンのホバー・押下効果**

`Button` をコードで生成してインラインスタイル（`button.style.backgroundColor = ...`）を設定している場合、USS の `:hover` / `:active` 擬似クラスは**インラインスタイルに上書きされて効かない**（インラインスタイルが優先される）。この場合は PointerEvent コールバックで対応する。

```csharp
private static void AddButtonHoverEffect(Button button, Color baseColor)
{
    Color hoverColor = new Color(
        Mathf.Clamp01(baseColor.r + 0.12f),
        Mathf.Clamp01(baseColor.g + 0.12f),
        Mathf.Clamp01(baseColor.b + 0.12f), baseColor.a);
    Color activeColor = new Color(
        Mathf.Clamp01(baseColor.r - 0.1f),
        Mathf.Clamp01(baseColor.g - 0.1f),
        Mathf.Clamp01(baseColor.b - 0.1f), baseColor.a);
    button.RegisterCallback<PointerEnterEvent>(_ => button.style.backgroundColor = new StyleColor(hoverColor));
    button.RegisterCallback<PointerLeaveEvent>(_ => button.style.backgroundColor = new StyleColor(baseColor));
    button.RegisterCallback<PointerDownEvent>(_ => button.style.backgroundColor = new StyleColor(activeColor));
    button.RegisterCallback<PointerUpEvent>(_ => button.style.backgroundColor = new StyleColor(hoverColor));
}
```

可能なら、インラインスタイルを使わず USS クラスのみでスタイルを管理して `:hover` / `:active` を効かせるほうが簡潔。

**背景画像ボタンの押下フィードバック（scale 変化）**

背景が PNG 画像で背景色変化が見えないボタンには、`scale` の transition でホバー拡大・押下縮小のフィードバックを付ける。

```css
.action-button {
    transition-property: scale;
    transition-duration: 0.1s;
}
.action-button:hover  { scale: 1.06 1.06; }
.action-button:active { scale: 0.94 0.94; }
```

### リスト項目（`.room-item`）

```css
.room-item {
    background-color: rgba(255, 255, 255, 0.05);
    border-top-left-radius: 8px; /* 他3角も同じ */
    border-left-width: 1px; /* 他3辺も同じ */
    border-left-color: rgba(255, 255, 255, 0.1); /* 他3辺も同じ */
    padding-top: 14px; padding-right: 16px; padding-bottom: 14px; padding-left: 16px;
    margin-bottom: 8px;
    color: rgb(180, 180, 210);
    font-size: 14px;
    -unity-text-align: middle-left;
}
.room-item:hover {
    background-color: rgba(70, 90, 180, 0.2);
    border-left-color: rgba(70, 90, 180, 0.5); /* 他3辺も同じ */
}
```

### 空状態（`.empty-state`）

リストが 0 件のときに代わりに出す一行メッセージ（「ルームがありません」など）。本文色よりさらに落とした色にする。

```css
.empty-state {
    color: rgb(120, 120, 150);
    font-size: 14px;
    -unity-text-align: middle-center;
    margin-top: 32px;
    margin-bottom: 32px;
}
```

### モーダル（`.modal-card` / `.modal-title` / `.option-row` / `.modal-actions`）

オプションモーダル（[Modal.uss](../Assets/AddressableAssets/Modal/Modal.uss)）の構成クラス。`.card` とは別定義で、パディングだけ `28px 32px` と縦を詰めている。

```css
.modal-card    { /* .card と同じ配色・角丸・枠線 */ padding: 28px 32px; min-width: 360px; }
.modal-title   { font-size: 20px; color: rgb(240, 240, 255); -unity-text-align: middle-center; margin-bottom: 16px; }

.option-row       { margin-bottom: 18px; }   /* ラベル＋スライダーの1組 */
.option-row--last { margin-bottom: 24px; }   /* 最終行だけ広めに空ける */
.option-label     { font-size: 13px; color: rgb(180, 180, 210); margin-bottom: 4px; }
.option-slider    { flex-grow: 1; }

.modal-actions         { flex-direction: row; justify-content: space-between; }
.modal-actions__exit   { flex-grow: 1; margin-right: 8px; }  /* 左: 退出ボタン（.btn-secondary と併用） */
.modal-actions__close  { flex-grow: 1; }                     /* 右: 閉じるボタン（.btn-accent と併用） */
```

### 明細行（`.credit-row`）

「項目名 ─ 値」を左右に振り分けて並べる行。クレジット表示などで使う。

```css
.credit-row {
    flex-direction: row;
    justify-content: space-between;
    align-items: center;
    padding-top: 8px; padding-bottom: 8px;
    border-bottom-width: 1px;
    border-bottom-color: rgba(255, 255, 255, 0.1);
}
.credit-row--last { border-bottom-width: 0; }   /* 末尾の区切り線を消す（下記参照） */

.credit-row__role { font-size: 13px; color: rgb(180, 180, 210); -unity-text-align: middle-left; }
.credit-row__name { font-size: 14px; color: rgb(240, 240, 255); -unity-text-align: middle-right; }
```

> **USS には `:last-child` / `:nth-child` などの構造擬似クラスが無い**（対応しているのは `:hover` / `:focus` / `:active` / `:disabled` など状態系のみ）。
> 「最後の行だけ区切り線を消す」といった処理は、要素を生成する C# 側で末尾判定して modifier クラスを付ける。実装例は [HomeView.BuildCredits](../Assets/Scripts/Home/HomeView.cs)。

### タイトル画面のプロンプト（`.press-start`）

タイトルシーンの `PRESS START` は、ボタンでありながら見た目は「文字だけ」にする。
背景・枠線を透明にし、`:hover` / `:focus` / `:active` でも背景を出さないよう上書きする（上書きしないと既定テーマのグレー背景が出る）。

さらに **画面のどこをクリックしてもスタートできる**よう、ボタン自体を `flex-grow: 1` で全画面に広げる。
背景が透明なので見た目は中央の文字だけ（`-unity-text-align: middle-center`）で変わらない。
既定テーマのボタンには余白が付くため `margin` を 0 にして端まで届かせる。

```css
.press-start {
    flex-grow: 1;                 /* 全画面をクリック領域にする */
    margin-top: 0;                /* 他3辺も同じ（既定テーマの余白を消す） */
    background-color: rgba(0, 0, 0, 0);
    border-left-width: 0; /* 他3辺も同じ */
    color: rgb(240, 240, 255);
    font-size: 28px;
    letter-spacing: 8px;
    -unity-text-align: middle-center;
    padding-top: 8px; padding-right: 16px; padding-bottom: 8px; padding-left: 16px;
}
.press-start:hover  { background-color: rgba(0, 0, 0, 0); color: rgb(255, 255, 255); }
.press-start:focus  { background-color: rgba(0, 0, 0, 0); color: rgb(255, 255, 255); }
.press-start:active { background-color: rgba(0, 0, 0, 0); scale: 0.97; }
```

右上のオプションアイコンは Common シーンの UIDocument（`SortingOrder: 1000`）にあり、Title の UIDocument（`0`）より前面なので、全画面ボタンに覆われてもクリックできる。
全画面のクリック領域を持つシーンでは、オプションモーダルの暗幕（`ModalOverlay`）が `picking-mode` を既定（`Position`）にしてクリックを遮ることが前提になる。`Ignore` にすると暗幕越しに背後のボタンが押せてしまう。

なお全画面ボタンはカーソルが画面上にある限り常に `:hover` 状態になるため、`:hover` の色替えは実質的に効かなくなる。それでも `:hover` / `:focus` の上書きは既定テーマのグレー背景を消す役割があるので残す。

点滅（チカチカ）は USS ではなく DOTween のループ Tween で実装する。UI Toolkit には CSS の `@keyframes` に相当する仕組みがないため。
手順は [patterns.md「ループ Tween」](patterns.md#ループ-tween点滅などの繰り返し演出) を参照。

---

## インタラクション演出ルール

### 擬似クラスと役割

| 擬似クラス | トリガー | 演出 |
|---|---|---|
| `:hover` | マウスカーソルが乗った | 背景を少し明るくする |
| `:focus` | キーボード・ゲームパッドで選択中 | `:hover` より少し明るくする |
| `:active` | 押下中（マウスボタンを押している間） | 背景を暗くし、`scale` で縮小 |

擬似クラスは **`:hover` → `:focus` → `:active`** の順で定義する（特異度が同じため、後に書いたものが優先される）。

### ボタンの値の目安

**`btn-accent`**（ベース: `rgb(70, 90, 180)`）

| 擬似クラス | 背景色 | scale |
|---|---|---|
| `:hover` | `rgb(85, 105, 198)` | — |
| `:focus` | `rgb(95, 115, 210)` | — |
| `:active` | `rgb(50, 65, 145)` | `0.96` |

**`btn-secondary`**（ベース: `rgba(255, 255, 255, 0.07)`）

| 擬似クラス | 背景色 | scale |
|---|---|---|
| `:hover` | `rgba(255, 255, 255, 0.13)` | — |
| `:focus` | `rgba(255, 255, 255, 0.18)` | — |
| `:active` | `rgba(255, 255, 255, 0.03)` | `0.96` |

**リスト項目（`.room-item` など）**

`:active` はボタンより控えめに `scale: 0.98` とし、背景のアクセントカラー透明度を上げる。

### USS スニペット

```css
/* ボタン擬似クラスの記述順（常にこの順番で書く） */
.btn-accent:hover  { background-color: ...; }
.btn-accent:focus  { background-color: ...; }
.btn-accent:active { background-color: ...; scale: 0.96; }
```

---

## オーバーレイ（`.overlay-bg` / `.overlay-label` / `.modal-host`）

モーダル表示・ローディング表示ではゲーム画面を暗幕で覆う。暗幕は `position: absolute` で全画面を覆い、中身を中央配置する。

```css
.overlay-bg {
    position: absolute;
    width: 100%;
    height: 100%;
    align-items: center;
    justify-content: center;
    background-color: rgba(0, 0, 0, 0.55);
}

/* 暗幕の上に出す見出し（「相手を待っています…」など） */
.overlay-label {
    font-size: 18px;
    color: rgb(240, 240, 255);
    -unity-text-align: middle-center;
    margin-bottom: 24px;
}

/* 暗幕の中でモーダルを受け取るコンテナ（Option.uss。C# から中身を Add する） */
.modal-host {
    flex-grow: 1;
    align-items: center;
    justify-content: center;
}
```

```xml
<ui:VisualElement name="ModalOverlay" class="overlay-bg">
    <ui:VisualElement name="ModalHost" picking-mode="Ignore" class="modal-host"/>
</ui:VisualElement>
```

表示/非表示の切り替えは USS ではなく C# の `element.style.display` で行う（実行時に変化する状態のため）。

---

## アイコン

アイコンは SVG を `Assets/AddressableAssets/Icon/` に配置し、USS の `background-image` で参照する。

| ファイル | 用途 |
|---|---|
| `sliders-solid-full.svg` | オプション設定アイコン（Option.uss の `.option-icon` が参照） |
| `align-justify-solid-full.svg` | 未使用（アドレス `Icon/align-justify-solid`） |

常に表示するアイコン（オプションアイコンなど）は `position: absolute` で配置する。
**サイズは `%` ではなく `px` で正方形に指定する**（`width: 5%; height: 5%;` のように `%` を使うと、幅と高さで基準の辺が変わるため画面のアスペクト比に応じて縦横比が崩れる）。

```css
/* 右上角にぴったり固定表示する例（オプションアイコン / Option.uss） */
.option-icon {
    background-image: url("project://database/Assets/AddressableAssets/Icon/sliders-solid-full.svg?fileID=7388822144124034973&guid=ff539d642a1a86d4aac70aba745fb6cb&type=3#sliders-solid-full");
    position: absolute;
    right: 0;
    top: 0;
    width: 60px;
    height: 60px;
}
```

```xml
<ui:Image name="OptionSliders" class="option-icon"/>
```

---

## UIDocument の設定

SortingOrder は「シーンの UI → オプション → フェード暗幕」の順に手前へ重なるよう決めてある。

| UIDocument | SortingOrder | 理由 |
|---|---|---|
| 各シーンの UI | `0` | 通常のゲーム画面 |
| Common の `Option` | `1000` | オプションアイコンとモーダルをシーン UI より手前に出すため |
| Common の `Transition` | `2000` | シーン遷移のフェード暗幕。オプションモーダルも覆い隠す最前面 |

新しいシーンの UIDocument は `0` のままにする。シーン内で重ね順を作りたい場合は UIDocument を増やさず、同一ツリー内の要素順で解決する（`1000` 以上を使うとオプションアイコンより手前に出てしまう）。

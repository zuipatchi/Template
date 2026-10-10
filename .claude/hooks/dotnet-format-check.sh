#!/usr/bin/env bash
# Stop hook: 変更された .cs ファイルを dotnet format --verify-no-changes でチェックする。
# 違反があれば exit 2 で Claude に差し戻して修正させる（最大 MaxAttempts 回）。
# 上限回数まで修正しても違反が残る場合は、修正をやめてユーザーへ報告するよう指示する。
# 対象は git 上で HEAD から変更された／未追跡の .cs ファイルのみ（Assets/AssetStore など ignore 対象は含まない）。

MaxAttempts=3

input="$(cat)"

cd "$(git rev-parse --show-toplevel 2>/dev/null)" || exit 0

# 試行回数はセッションごとの一時ファイルに保存する
sessionId="$(printf '%s' "$input" | sed -n 's/.*"session_id"[[:space:]]*:[[:space:]]*"\([^"]*\)".*/\1/p')"
stateFile="${TMPDIR:-/tmp}/claude-dotnet-format-${sessionId:-default}"

# stop_hook_active が false なら、この hook による差し戻し中ではない新しい停止なので回数をリセットする
if ! printf '%s' "$input" | grep -q '"stop_hook_active"[[:space:]]*:[[:space:]]*true'; then
    rm -f "$stateFile"
fi

files="$( { git diff --name-only HEAD -- '*.cs'; git ls-files --others --exclude-standard -- '*.cs'; } 2>/dev/null | sort -u )"
if [ -z "$files" ]; then
    rm -f "$stateFile"
    exit 0
fi

command -v dotnet >/dev/null 2>&1 || exit 0

# 各ファイルを Compile Include で含んでいる csproj を探し、csproj ごとにファイルをまとめる
declare -A projectFiles
while IFS= read -r f; do
    [ -f "$f" ] || continue
    winPath="$(printf '%s' "$f" | sed 's#/#\\#g')"
    proj="$(grep -lF "Compile Include=\"$winPath\"" ./*.csproj 2>/dev/null | head -n 1)"
    [ -z "$proj" ] && continue
    projectFiles["$proj"]+="$f"$'\n'
done <<< "$files"

report=""
for proj in "${!projectFiles[@]}"; do
    mapfile -t includes < <(printf '%s' "${projectFiles[$proj]}" | sed '/^$/d')
    out="$(dotnet format "$proj" --verify-no-changes --no-restore -v q --include "${includes[@]}" 2>&1)"
    if [ $? -ne 0 ]; then
        report+="$out"$'\n'
    fi
done

if [ -z "$report" ]; then
    rm -f "$stateFile"
    exit 0
fi

attempts="$(cat "$stateFile" 2>/dev/null)"
attempts="${attempts:-0}"

# 上限到達後の報告ターンが終わったら、それ以上は止めない
if [ "$attempts" = "reported" ]; then
    rm -f "$stateFile"
    exit 0
fi

if [ "$attempts" -ge "$MaxAttempts" ]; then
    echo "reported" > "$stateFile"
    {
        echo "dotnet format の違反を ${MaxAttempts} 回修正しましたが解消しませんでした。これ以上修正せず、残っている違反（ファイル・行・内容）と解消できなかった理由をユーザーに報告して作業を終了してください:"
        printf '%s' "$report" | head -n 50
    } >&2
    exit 2
fi

attempts=$((attempts + 1))
echo "$attempts" > "$stateFile"
{
    echo "dotnet format --verify-no-changes で規約違反が見つかりました。修正してください（修正 ${attempts}/${MaxAttempts} 回目。dotnet format <csproj> --no-restore --include <file> で自動修正できます）:"
    printf '%s' "$report" | head -n 50
} >&2
exit 2

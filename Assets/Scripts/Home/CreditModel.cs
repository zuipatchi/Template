using System.Collections.Generic;

namespace Home
{
    /// <summary>
    /// クレジット画面に表示する項目を保持するクラス
    /// 担当者名はテンプレートのプレースホルダなので、プロジェクトごとに書き換える
    /// </summary>
    public sealed class CreditModel
    {
        private readonly IReadOnlyList<CreditEntry> _entries = new[]
        {
            new CreditEntry("企画", "-"),
            new CreditEntry("プログラム", "-"),
            new CreditEntry("イラスト", "-"),
            new CreditEntry("サウンド", "-"),
            new CreditEntry("アセット", "-")
        };

        public IReadOnlyList<CreditEntry> Entries => _entries;
    }
}

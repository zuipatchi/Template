using System.Collections.Generic;
using System.Linq;
using Home;
using NUnit.Framework;

namespace Tests.EditMode
{
    public class CreditModelTests
    {
        private CreditModel _model;

        [SetUp]
        public void SetUp()
        {
            _model = new CreditModel();
        }

        [Test]
        public void クレジット項目が5件返る()
        {
            Assert.AreEqual(5, _model.Entries.Count);
        }

        [Test]
        public void クレジット項目が企画からアセットまでの順で並ぶ()
        {
            string[] expected = { "企画", "プログラム", "イラスト", "サウンド", "アセット" };
            IReadOnlyList<CreditEntry> entries = _model.Entries;

            CollectionAssert.AreEqual(expected, entries.Select(entry => entry.Role).ToArray());
        }

        [Test]
        public void すべてのクレジット項目に担当者名が設定されている()
        {
            foreach (CreditEntry entry in _model.Entries)
            {
                Assert.IsFalse(string.IsNullOrEmpty(entry.Name), $"{entry.Role} の担当者名が空です");
            }
        }
    }
}

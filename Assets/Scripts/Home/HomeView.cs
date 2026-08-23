using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Home
{
    /// <summary>
    /// Home シーンの UI 要素を保持し、操作をイベントとして公開するクラス
    /// </summary>
    public sealed class HomeView
    {
        private readonly Button _gameStartButton;
        private readonly Button _creditButton;
        private readonly VisualElement _creditOverlay;
        private readonly VisualElement _creditList;
        private readonly Button _closeCreditButton;

        public event Action GameStartClicked;
        public event Action CreditClicked;
        public event Action CloseCreditClicked;

        public HomeView(VisualElement root)
        {
            _gameStartButton = root.Q<Button>("GameStartButton");
            _creditButton = root.Q<Button>("CreditButton");
            _creditOverlay = root.Q<VisualElement>("CreditOverlay");
            _creditList = root.Q<VisualElement>("CreditList");
            _closeCreditButton = root.Q<Button>("CloseCreditButton");

            _gameStartButton.clicked += () => GameStartClicked?.Invoke();
            _creditButton.clicked += () => CreditClicked?.Invoke();
            _closeCreditButton.clicked += () => CloseCreditClicked?.Invoke();
        }

        /// <summary>
        /// クレジット項目の行を生成して一覧に並べる
        /// </summary>
        public void BuildCredits(IReadOnlyList<CreditEntry> entries)
        {
            _creditList.Clear();

            for (int i = 0; i < entries.Count; i++)
            {
                CreditEntry entry = entries[i];

                VisualElement row = new VisualElement();
                row.AddToClassList("credit-row");
                // USS に :last-child が無いため、末尾の区切り線はクラスで打ち消す
                if (i == entries.Count - 1)
                {
                    row.AddToClassList("credit-row--last");
                }

                Label role = new Label(entry.Role);
                role.AddToClassList("credit-row__role");
                row.Add(role);

                Label name = new Label(entry.Name);
                name.AddToClassList("credit-row__name");
                row.Add(name);

                _creditList.Add(row);
            }
        }

        public void SetCreditVisible(bool visible)
        {
            _creditOverlay.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        /// <summary>
        /// シーン遷移中の連打を防ぐためにカード内のボタンをまとめて無効化する
        /// </summary>
        public void SetInteractable(bool interactable)
        {
            _gameStartButton.SetEnabled(interactable);
            _creditButton.SetEnabled(interactable);
        }
    }
}

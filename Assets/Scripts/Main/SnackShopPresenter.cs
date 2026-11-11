using System;
using R3;
using UnityEngine.UIElements;
using VContainer.Unity;

namespace Main
{
    public sealed class SnackShopPresenter : IStartable, IDisposable
    {
        private readonly SnackRoundModel _model;
        private readonly UIDocument _document;
        private VisualElement _choices;
        private Label _progress;
        private Label _order;
        private Label _feedback;
        private Button _pudding;
        private Button _donut;
        private Button _shortcake;
        private Button _next;
        private Button _restart;
        private IDisposable _subscription;

        public SnackShopPresenter(SnackRoundModel model, UIDocument document)
        {
            _model = model;
            _document = document;
        }

        public void Start()
        {
            VisualElement root = _document.rootVisualElement;
            _choices = root.Q("Choices");
            _progress = root.Q<Label>("Progress");
            _order = root.Q<Label>("Order");
            _feedback = root.Q<Label>("Feedback");
            _pudding = root.Q<Button>("Pudding");
            _donut = root.Q<Button>("Donut");
            _shortcake = root.Q<Button>("Shortcake");
            _next = root.Q<Button>("NextOrder");
            _restart = root.Q<Button>("Restart");

            _pudding.clicked += ChoosePudding;
            _donut.clicked += ChooseDonut;
            _shortcake.clicked += ChooseShortcake;
            _next.clicked += _model.Next;
            _restart.clicked += _model.Restart;
            _subscription = _model.Revision.Subscribe(_ => Render());
        }

        public void Dispose()
        {
            _subscription?.Dispose();
            if (_pudding == null)
            {
                return;
            }

            _pudding.clicked -= ChoosePudding;
            _donut.clicked -= ChooseDonut;
            _shortcake.clicked -= ChooseShortcake;
            _next.clicked -= _model.Next;
            _restart.clicked -= _model.Restart;
        }

        private void ChoosePudding() => _model.Choose(Snack.Pudding);
        private void ChooseDonut() => _model.Choose(Snack.Donut);
        private void ChooseShortcake() => _model.Choose(Snack.Shortcake);

        private void Render()
        {
            bool answered = _model.SelectedSnack.HasValue;
            bool finished = _model.Finished;
            string snack = NameOf(_model.CurrentOrder);

            _progress.text = finished ? "本日の結果" : $"注文 {_model.OrderNumber} / {_model.OrderCount}  ・  正解 {_model.Score}";
            _order.text = finished ? $"{_model.OrderCount}問中 {_model.Score}問 正解！" : $"「{snack}ください」";
            _feedback.text = finished ? "ご来店ありがとうございました。" : "注文されたおやつを選んでね。";
            if (answered && !finished)
            {
                _feedback.text = _model.SelectedSnack == _model.CurrentOrder
                    ? $"正解！ {snack}をお渡ししました。"
                    : $"おしい！ 注文されたのは{snack}です。";
            }

            _choices.EnableInClassList("is-hidden", finished);
            _next.EnableInClassList("is-hidden", !answered || finished);
            _restart.EnableInClassList("is-hidden", !finished);
            _pudding.SetEnabled(!answered);
            _donut.SetEnabled(!answered);
            _shortcake.SetEnabled(!answered);
            _pudding.EnableInClassList("is-selected", _model.SelectedSnack == Snack.Pudding);
            _donut.EnableInClassList("is-selected", _model.SelectedSnack == Snack.Donut);
            _shortcake.EnableInClassList("is-selected", _model.SelectedSnack == Snack.Shortcake);
        }

        private static string NameOf(Snack snack) => snack switch
        {
            Snack.Pudding => "プリン",
            Snack.Donut => "ドーナツ",
            _ => "ショートケーキ"
        };
    }
}

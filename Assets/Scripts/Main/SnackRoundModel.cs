using R3;

namespace Main
{
    public enum Snack
    {
        Pudding,
        Donut,
        Shortcake
    }

    public sealed class SnackRoundModel
    {
        private static readonly Snack[] Orders =
        {
            Snack.Pudding, Snack.Donut, Snack.Shortcake, Snack.Pudding, Snack.Donut
        };

        private readonly ReactiveProperty<int> _revision = new(0);

        public ReadOnlyReactiveProperty<int> Revision => _revision;
        public int OrderNumber { get; private set; } = 1;
        public int OrderCount => Orders.Length;
        public int Score { get; private set; }
        public Snack CurrentOrder => Orders[OrderNumber - 1];
        public Snack? SelectedSnack { get; private set; }
        public bool Finished { get; private set; }

        public void Choose(Snack snack)
        {
            if (Finished || SelectedSnack.HasValue)
            {
                return;
            }

            SelectedSnack = snack;
            if (snack == CurrentOrder)
            {
                Score++;
            }

            _revision.Value++;
        }

        public void Next()
        {
            if (Finished || !SelectedSnack.HasValue)
            {
                return;
            }

            if (OrderNumber == OrderCount)
            {
                Finished = true;
            }
            else
            {
                OrderNumber++;
                SelectedSnack = null;
            }

            _revision.Value++;
        }

        public void Restart()
        {
            OrderNumber = 1;
            Score = 0;
            SelectedSnack = null;
            Finished = false;
            _revision.Value++;
        }
    }
}

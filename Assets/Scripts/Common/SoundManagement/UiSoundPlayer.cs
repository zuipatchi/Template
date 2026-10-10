using Common.Store;
using Cysharp.Threading.Tasks;

namespace Common.SoundManagement
{
    /// <summary>
    /// UI 操作の効果音を「開く・閉じる・決定」の意味で鳴らす窓口。
    /// どの操作にどの SE を使うかの対応はここにだけ書く
    /// </summary>
    public sealed class UiSoundPlayer
    {
        private readonly SoundPlayer _soundPlayer;
        private readonly SoundStore _soundStore;

        public UiSoundPlayer(SoundPlayer soundPlayer, SoundStore soundStore)
        {
            _soundPlayer = soundPlayer;
            _soundStore = soundStore;
        }

        /// <summary>効果音のロード完了</summary>
        public UniTask Loaded => _soundStore.Loaded;

        /// <summary>モーダル・オーバーレイなどを開くとき</summary>
        public void PlayOpen()
        {
            _soundPlayer.PlaySE(_soundStore.Enter2SE);
        }

        /// <summary>モーダル・オーバーレイなどを閉じるとき</summary>
        public void PlayClose()
        {
            _soundPlayer.PlaySE(_soundStore.Cancel1SE);
        }

        /// <summary>画面遷移などを決定するとき</summary>
        public void PlayDecide()
        {
            _soundPlayer.PlaySE(_soundStore.Enter1SE);
        }
    }
}

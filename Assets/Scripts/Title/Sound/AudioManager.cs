using System;
using System.Threading;
using Common.SoundManagement;
using Common.Store;
using Cysharp.Threading.Tasks;
using VContainer;
using VContainer.Unity;

namespace Title.Sound
{

    public class AudioManager : IAsyncStartable
    {
        private readonly SoundPlayer _soundPlayer;
        private readonly SoundStore _soundStore;

        [Inject]
        public AudioManager(SoundPlayer soundPlayer, SoundStore soundStore)
        {
            _soundPlayer = soundPlayer;
            _soundStore = soundStore;
        }

        public async UniTask StartAsync(CancellationToken cancellation = default)
        {
            try
            {
                await _soundStore.Loaded.AttachExternalCancellation(cancellation);
                _soundPlayer.PlayBGM(_soundStore.TitleBGM);
            }
            catch (OperationCanceledException)
            {
                // シーン破棄によるキャンセルは正常系なので何もしない
            }
        }
    }
}

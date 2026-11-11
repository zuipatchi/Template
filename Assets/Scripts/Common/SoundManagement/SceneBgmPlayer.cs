using System;
using System.Threading;
using Common.Store;
using Cysharp.Threading.Tasks;
using VContainer.Unity;

namespace Common.SoundManagement
{
    /// <summary>Common シーンで全画面共通の BGM を開始する。</summary>
    public sealed class SceneBgmPlayer : IAsyncStartable
    {
        private readonly SoundPlayer _soundPlayer;
        private readonly SoundStore _soundStore;

        public SceneBgmPlayer(SoundPlayer soundPlayer, SoundStore soundStore)
        {
            _soundPlayer = soundPlayer;
            _soundStore = soundStore;
        }

        public async UniTask StartAsync(CancellationToken ct = default)
        {
            try
            {
                await _soundStore.Loaded.AttachExternalCancellation(ct);
                _soundPlayer.PlayBGM(_soundStore.BackgroundMusic);
            }
            catch (OperationCanceledException)
            {
                // Common シーンの破棄によるキャンセルは正常系なので何もしない
            }
        }
    }
}

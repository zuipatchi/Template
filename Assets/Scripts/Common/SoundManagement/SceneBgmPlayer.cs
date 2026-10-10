using System;
using System.Threading;
using Common.Store;
using Cysharp.Threading.Tasks;
using VContainer.Unity;

namespace Common.SoundManagement
{
    /// <summary>
    /// シーンの開始時に、そのシーンの BGM を流すエントリポイント。
    /// 各シーンの LifetimeScope で流す曲を指定して登録する:
    /// <c>builder.RegisterEntryPoint&lt;SceneBgmPlayer&gt;(Lifetime.Scoped).WithParameter(Bgm.Title);</c>
    /// </summary>
    public sealed class SceneBgmPlayer : IAsyncStartable
    {
        private readonly SoundPlayer _soundPlayer;
        private readonly SoundStore _soundStore;
        private readonly Bgm _bgm;

        public SceneBgmPlayer(SoundPlayer soundPlayer, SoundStore soundStore, Bgm bgm)
        {
            _soundPlayer = soundPlayer;
            _soundStore = soundStore;
            _bgm = bgm;
        }

        public async UniTask StartAsync(CancellationToken cancellation = default)
        {
            try
            {
                await _soundStore.Loaded.AttachExternalCancellation(cancellation);
                _soundPlayer.PlayBGM(_soundStore.GetBgm(_bgm));
            }
            catch (OperationCanceledException)
            {
                // シーン破棄によるキャンセルは正常系なので何もしない
            }
        }
    }
}

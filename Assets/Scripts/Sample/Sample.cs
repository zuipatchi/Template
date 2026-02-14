using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using Cysharp.Threading.Tasks;
using Common.SoundManagement;
using Common.Store;
using System.Threading.Tasks;

namespace SampleScene
{
    public class Sample : MonoBehaviour
    {
        private SoundPlayer _soundPlayer;
        private SoundStore _soundStore;
        private AudioClip _bgm;
        private AudioClip _se;

        [Inject]
        public void Construct(SoundPlayer soundPlayer, SoundStore soundStore)
        {
            _soundPlayer = soundPlayer;
            _soundStore = soundStore;
        }

        private void Start()
        {
            StartAsync().Forget();
        }
        private async UniTask StartAsync()
        {
            await _soundStore.Loaded;
            _bgm = _soundStore.MainBGM;
            _soundPlayer.PlayBGM(_bgm);
            _se = _soundStore.Sole;
        }

        private void Update()
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                _soundPlayer.PlaySE(_se);
            }
        }
    }
}

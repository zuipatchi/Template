using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using Cysharp.Threading.Tasks;
using R3;
using System.Threading;
using Scripts.Common;

namespace Scripts.Sample
{


    public class Sample : MonoBehaviour
    {
        private SoundPlayer _soundPlayer;
        private Store _store;
        private readonly Subject<Unit> _subject = new();

        [Inject]
        public void Construct(SoundPlayer soundPlayer, Store store)
        {
            _soundPlayer = soundPlayer;
            _store = store;
        }

        private void Start()
        {
            PatiAsync().Forget();

            CancellationToken ct = this.GetCancellationTokenOnDestroy();

            _subject
                .Subscribe(_ => Debug.Log("イベントが発行された"))
                .AddTo(ct);

            _subject.OnNext(Unit.Default);
        }

        private void Update()
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                var se = _store.Sole;
                _soundPlayer.PlaySE(se);
            }
        }

        private async UniTask PatiAsync()
        {
            Debug.Log("3秒待つ");
            await UniTask.Delay(3000);
            Debug.Log("3秒経った");

            var bgm = _store.BGM;
            _soundPlayer.PlayBGM(bgm);
        }
    }
}

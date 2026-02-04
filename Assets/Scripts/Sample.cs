using UnityEngine;
using VContainer;
using Cysharp.Threading.Tasks;
using R3;
using System.Threading;
using Scripts.Common;

public class Sample : MonoBehaviour
{
    private Store _store;
    private readonly Subject<Unit> _subject = new();

    [Inject]
    public void Construct(Store store)
    {
        _store = store;
    }

    public async UniTask PatiAsync()
    {
        Debug.Log("3秒待つ");
        await UniTask.Delay(3000);
        Debug.Log("3秒経った");

        var cube = _store.Cube;
        Instantiate(cube);
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
}

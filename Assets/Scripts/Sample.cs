using UnityEngine;
using VContainer;
using Cysharp.Threading.Tasks;
using R3;
using System.Threading;

public class Sample : MonoBehaviour
{
    private readonly Subject<Unit> _subject = new();

    [Inject]
    public void Construct()
    {
        Debug.Log("DIされることがあれば呼ばれる");
    }

    public async UniTask PatiAsync()
    {
        Debug.Log("3秒待つ");
        await UniTask.Delay(3000);
        Debug.Log("3秒経った");
    }

    private void Start()
    {
        PatiAsync().Forget();

        CancellationToken ct = this.GetCancellationTokenOnDestroy();

        _subject
            .Subscribe(_ => Debug.Log("Start"))
            .AddTo(ct);

        _subject.OnNext(Unit.Default);
    }
}

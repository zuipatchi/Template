using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class LoadAsset : MonoBehaviour
{
    private readonly string _cubeAddressable = "Cube";
    private GameObject _cube;
    public GameObject Cube => _cube;

    private void Start()
    {
        LoadCube().Forget();
    }

    private async UniTask LoadCube()
    {
        _cube = await Addressables.LoadAssetAsync<GameObject>(_cubeAddressable).ToUniTask();
    }
}

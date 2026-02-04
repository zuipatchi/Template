using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using VContainer.Unity;

namespace Scripts.Common
{
    /// <summary>
    /// AddressableAsset をロードしておくクラス
    /// </summary>
    public class Store : IStartable
    {
        private readonly string _cubeAddressable = "Cube";
        private GameObject _cube;
        public GameObject Cube => _cube;

        public void Start()
        {
            LoadCube().Forget();
        }

        private async UniTask LoadCube()
        {
            _cube = await Addressables.LoadAssetAsync<GameObject>(_cubeAddressable).ToUniTask();
            Debug.Log("Addressables のロードが完了");
        }
    }
}

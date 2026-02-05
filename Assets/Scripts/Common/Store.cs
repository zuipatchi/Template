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
        private readonly string _bgmAddressable = "BGM";
        public GameObject Cube => _cube;
        public AudioClip BGM => _bgm;
        private GameObject _cube = null;
        private AudioClip _bgm = null;

        public void Start()
        {
            LoadAssets().Forget();
        }

        private async UniTask LoadAssets()
        {
            _cube = await Addressables.LoadAssetAsync<GameObject>(_cubeAddressable).ToUniTask();
            _bgm = await Addressables.LoadAssetAsync<AudioClip>(_bgmAddressable).ToUniTask();
            Debug.Log("Addressables のロードが完了");
        }
    }
}

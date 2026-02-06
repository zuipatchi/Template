using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using VContainer.Unity;

namespace Common.Store
{
    /// <summary>
    /// AddressableAsset をロードしておくクラス
    /// </summary>
    public class SoundStore : IStartable
    {
        // アドレス
        private readonly string _bgmAddressable = "BGM";
        private readonly string _soleAddressable = "Sole";

        // プロパティ
        public AudioClip BGM => _bgm;
        public AudioClip Sole => _sole;

        // メンバー
        private AudioClip _bgm = null;
        private AudioClip _sole = null;

        public void Start()
        {
            LoadAssets().Forget();
        }

        private async UniTask LoadAssets()
        {
            _bgm = await Addressables.LoadAssetAsync<AudioClip>(_bgmAddressable).ToUniTask();
            _sole = await Addressables.LoadAssetAsync<AudioClip>(_soleAddressable).ToUniTask();
            Debug.Log("Addressables のロードが完了");
        }
    }
}

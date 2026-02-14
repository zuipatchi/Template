using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.AddressableAssets;
using VContainer.Unity;

namespace Common.Store
{
    public class SoundStore : IStartable
    {
        private UniTaskCompletionSource _loadedTcs = new();
        public UniTask Loaded => _loadedTcs.Task;

        // アドレス
        private readonly string _mainBgmAddressable = "Sound/BGM/CatInPalmBeach";
        private readonly string _soleAddressable = "Sole";
        private readonly string _titleBGMAddressable = "Sound/BGM/Title";

        // プロパティ
        public AudioClip MainBGM => _mainBGM;
        public AudioClip Sole => _sole;
        public AudioClip TitleBGM => _titleBGM;

        // メンバー
        private AudioClip _mainBGM = null;
        private AudioClip _sole = null;
        private AudioClip _titleBGM = null;

        public void Start()
        {
            LoadAssets().Forget();
        }

        private async UniTask LoadAssets()
        {
            _mainBGM = await Addressables.LoadAssetAsync<AudioClip>(_mainBgmAddressable).ToUniTask();
            _sole = await Addressables.LoadAssetAsync<AudioClip>(_soleAddressable).ToUniTask();
            _titleBGM = await Addressables.LoadAssetAsync<AudioClip>(_titleBGMAddressable).ToUniTask();
            _loadedTcs.TrySetResult();
        }
    }
}

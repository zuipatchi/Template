using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Common.Store
{
    public class SoundStore : AssetStoreBase
    {
        private readonly string _backgroundMusicAddress = "Sound/BGM/CatInPalmBeach";
        private readonly string _cancel1SEAddressable = "Sound/SE/Cancel1";
        private readonly string _enter1SEAddressable = "Sound/SE/Enter1";
        private readonly string _enter2SEAddressable = "Sound/SE/Enter2";

        public AudioClip BackgroundMusic => _backgroundMusic;
        public AudioClip Cancel1SE => _cancel1SE;
        public AudioClip Enter1SE => _enter1SE;
        public AudioClip Enter2SE => _enter2SE;

        private AudioClip _backgroundMusic = null;
        private AudioClip _cancel1SE = null;
        private AudioClip _enter1SE = null;
        private AudioClip _enter2SE = null;

        protected override string AssetCategory => "サウンド";

        protected override async UniTask LoadAssetsCore()
        {
            // 互いに依存しないので並列にロードする
            (_backgroundMusic, _cancel1SE, _enter1SE, _enter2SE) = await UniTask.WhenAll(
                LoadAsync<AudioClip>(_backgroundMusicAddress),
                LoadAsync<AudioClip>(_cancel1SEAddressable),
                LoadAsync<AudioClip>(_enter1SEAddressable),
                LoadAsync<AudioClip>(_enter2SEAddressable));
        }
    }
}

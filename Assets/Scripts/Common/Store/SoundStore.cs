using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Common.Store
{
    public class SoundStore : AssetStoreBase
    {
        private readonly string _mainBgmAddressable = "Sound/BGM/CatInPalmBeach";
        private readonly string _titleBGMAddressable = "Sound/BGM/Title";
        private readonly string _cancel1SEAddressable = "Sound/SE/Cancel1";
        private readonly string _enter1SEAddressable = "Sound/SE/Enter1";
        private readonly string _enter2SEAddressable = "Sound/SE/Enter2";
        private readonly string _resultSEAddressable = "Sound/SE/Result";

        public AudioClip MainBGM => _mainBGM;
        public AudioClip TitleBGM => _titleBGM;
        public AudioClip Cancel1SE => _cancel1SE;
        public AudioClip Enter1SE => _enter1SE;
        public AudioClip Enter2SE => _enter2SE;
        public AudioClip ResultSE => _resultSE;

        private AudioClip _mainBGM = null;
        private AudioClip _titleBGM = null;
        private AudioClip _cancel1SE = null;
        private AudioClip _enter1SE = null;
        private AudioClip _enter2SE = null;
        private AudioClip _resultSE = null;

        protected override string AssetCategory => "サウンド";

        protected override async UniTask LoadAssetsCore()
        {
            _mainBGM = await LoadAsync<AudioClip>(_mainBgmAddressable);
            _titleBGM = await LoadAsync<AudioClip>(_titleBGMAddressable);
            _cancel1SE = await LoadAsync<AudioClip>(_cancel1SEAddressable);
            _enter1SE = await LoadAsync<AudioClip>(_enter1SEAddressable);
            _enter2SE = await LoadAsync<AudioClip>(_enter2SEAddressable);
            _resultSE = await LoadAsync<AudioClip>(_resultSEAddressable);
        }
    }
}

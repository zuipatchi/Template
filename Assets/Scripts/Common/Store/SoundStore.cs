using System;
using Common.SoundManagement;
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

        public AudioClip MainBGM => _mainBGM;
        public AudioClip TitleBGM => _titleBGM;
        public AudioClip Cancel1SE => _cancel1SE;
        public AudioClip Enter1SE => _enter1SE;
        public AudioClip Enter2SE => _enter2SE;

        private AudioClip _mainBGM = null;
        private AudioClip _titleBGM = null;
        private AudioClip _cancel1SE = null;
        private AudioClip _enter1SE = null;
        private AudioClip _enter2SE = null;

        protected override string AssetCategory => "サウンド";

        public AudioClip GetBgm(Bgm bgm)
        {
            switch (bgm)
            {
                case Bgm.Title:
                    return _titleBGM;
                case Bgm.Main:
                    return _mainBGM;
                default:
                    throw new ArgumentOutOfRangeException(nameof(bgm), bgm, null);
            }
        }

        protected override async UniTask LoadAssetsCore()
        {
            // 互いに依存しないので並列にロードする
            (_mainBGM, _titleBGM, _cancel1SE, _enter1SE, _enter2SE) = await UniTask.WhenAll(
                LoadAsync<AudioClip>(_mainBgmAddressable),
                LoadAsync<AudioClip>(_titleBGMAddressable),
                LoadAsync<AudioClip>(_cancel1SEAddressable),
                LoadAsync<AudioClip>(_enter1SEAddressable),
                LoadAsync<AudioClip>(_enter2SEAddressable));
        }
    }
}

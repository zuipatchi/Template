using System;
using R3;
using UnityEngine;
using VContainer.Unity;

namespace Common.Option
{
    public class OptionModel : IStartable
    {
        private const string BGMVolumeKey = "bgmVolume";
        private const string SEVolumeKey = "seVolume";
        private const float DefaultVolume = 0.5f;

        private readonly ReactiveProperty<float> _bgmVolume = new();
        public ReadOnlyReactiveProperty<float> BGMVolume => _bgmVolume;

        private readonly ReactiveProperty<float> _seVolume = new();
        public ReadOnlyReactiveProperty<float> SEVolume => _seVolume;

        // セーブデータから読み込み
        public void Start()
        {
            _bgmVolume.Value = PlayerPrefs.GetFloat(BGMVolumeKey, DefaultVolume);
            _seVolume.Value = PlayerPrefs.GetFloat(SEVolumeKey, DefaultVolume);
        }

        public void SetBGMVolume(float value)
        {
            SetVolume(_bgmVolume, BGMVolumeKey, value);
        }

        public void SetSEVolume(float value)
        {
            SetVolume(_seVolume, SEVolumeKey, value);
        }

        // PlayerPrefs.SetFloat はメモリ上の値を変えるだけなので、強制終了やモバイルのプロセス終了に備えてディスクへ書き出す。
        // スライダーのドラッグ中に毎回書き出さないよう、モーダルを閉じるときに 1 回だけ呼ぶ
        public void Save()
        {
            PlayerPrefs.Save();
        }

        private static void SetVolume(ReactiveProperty<float> volume, string key, float value)
        {
            volume.Value = Math.Clamp(value, 0, 1);
            PlayerPrefs.SetFloat(key, volume.Value);
        }
    }
}

using Common.Option;
using R3;
using UnityEngine;
using VContainer;

namespace Common.SoundManagement
{
    /// <summary>
    /// AudioClip を再生するクラス
    /// OptionModel の volume で音量を管理している
    /// </summary>
    public class SoundPlayer : MonoBehaviour
    {
        // OptionModel の値 1.0 が AudioSource の最大音量の半分に相当するようにするための係数。
        private const float VolumeScale = 0.5f;

        private AudioSource _bgmAudioSource;
        private AudioSource _seAudioSource;
        private OptionModel _optionModel;
        private readonly CompositeDisposable _disposables = new();

        [Inject]
        public void Construct(OptionModel optionModel)
        {
            _optionModel = optionModel;
        }

        private void Start()
        {
            _bgmAudioSource = gameObject.AddComponent<AudioSource>();
            _bgmAudioSource.loop = true;

            BindVolume(_bgmAudioSource, _optionModel.BGMVolume);

            _seAudioSource = gameObject.AddComponent<AudioSource>();
            _seAudioSource.playOnAwake = false;
            _seAudioSource.loop = false;

            BindVolume(_seAudioSource, _optionModel.SEVolume);
        }

        // OptionModel の音量を AudioSource に反映し続ける
        private void BindVolume(AudioSource audioSource, ReadOnlyReactiveProperty<float> volume)
        {
            volume
                .Subscribe(v => audioSource.volume = v * VolumeScale)
                .AddTo(_disposables);
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
        }

        // 同じ曲がすでに流れていれば頭から再生し直さない（Title → Home のように同じ BGM のシーンへ移るとき）
        public void PlayBGM(AudioClip clip)
        {
            if (_bgmAudioSource.clip == clip && _bgmAudioSource.isPlaying)
            {
                return;
            }
            _bgmAudioSource.clip = clip;
            _bgmAudioSource.Play();
        }

        // SoundStore のロード完了前（起動直後のボタン操作など）は clip が null なので鳴らさない
        public void PlaySE(AudioClip clip)
        {
            if (clip == null)
            {
                return;
            }
            _seAudioSource.PlayOneShot(clip);
        }
    }
}

using UnityEngine;

namespace Scripts.Common
{
    /// <summary>
    /// AudioClip を再生するクラス
    /// </summary>
    public class SoundPlayer : MonoBehaviour
    {
        private AudioSource _bgmAudioSource;
        private AudioSource _seAudioSource;


        private void Start()
        {
            _bgmAudioSource = gameObject.AddComponent<AudioSource>();
            _bgmAudioSource.loop = true;

            _seAudioSource = gameObject.AddComponent<AudioSource>();
            _seAudioSource.playOnAwake = false;
            _seAudioSource.loop = false;
        }

        public void PlayBGM(AudioClip clip)
        {
            _bgmAudioSource.clip = clip;
            _bgmAudioSource.Play();
        }

        public void PlaySE(AudioClip clip)
        {
            _seAudioSource.PlayOneShot(clip);
        }
    }
}

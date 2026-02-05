using UnityEngine;

namespace Scripts.Common
{
    public class SoundPlayer : MonoBehaviour
    {
        private AudioSource _bgmAudioSource;
        private AudioSource _seAudioSource;


        private void Start()
        {
            _bgmAudioSource = gameObject.AddComponent<AudioSource>();
            _seAudioSource = gameObject.AddComponent<AudioSource>();
        }

        public void PlayBGM(AudioClip clip)
        {
            if (clip == null) Debug.Log("clipがnull");
            if (_bgmAudioSource == null) Debug.Log("Audiosourceがnull");
            _bgmAudioSource.clip = clip;
            _bgmAudioSource.loop = true;
            _bgmAudioSource.Play();
            Debug.Log("BGMを鳴らした");
        }

        public void PlaySE(AudioClip se)
        {
            
        }
    }
}

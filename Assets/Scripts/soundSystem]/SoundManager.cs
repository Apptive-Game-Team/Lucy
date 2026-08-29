using System;
using UnityEngine;

namespace soundSystem_
{
    public class SoundManager : SingletonObject<SoundManager>
    {

        protected override void Awake()
        {
            base.Awake();
            if (audioSource == null)
            {
                // GetComponent returns null instead of throwing, so the old try/catch never fired.
                audioSource = gameObject.GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    throw new Exception("AudioSource is not found");
                }
            }
        }

        [SerializeField]
        public SoundSources soundSources;

        [SerializeField]
        private AudioSource audioSource;

        private void Start()
        {
            PlayBackgroundMusic("HorrorStrange");
        }

        public void PlayBackgroundMusic(string name)
        {
            AudioClip clip = soundSources.GetClipByName(name);
            if (clip == null)
            {
                return;
            }
            audioSource.clip = clip;
            audioSource.Play();
        }

        public void StopBackgroundMusic()
        {
            audioSource.Stop();
        }
    }
}

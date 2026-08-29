using UnityEngine;

namespace soundSystem_
{
    public class ToggleableObjectSoundController : SoundController
    {
        private readonly AudioClip[] audioClips = new AudioClip[2];

        protected override void Awake()
        {
            base.Awake();
        }

        public void SetType(string type)
        {
            audioClips[0] = SoundManager.Instance.soundSources.GetClipByName(type + "Close");
            audioClips[1] = SoundManager.Instance.soundSources.GetClipByName(type + "Open");
        }

        public void PlaySound(bool isOpen)
        {
            int i = isOpen ? 1 : 0;
            audioSource.clip = audioClips[i];
            audioSource.Play();
        }

    }
}

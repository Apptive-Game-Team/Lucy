using System.Collections.Generic;
using InputSystem;
using UnityEngine;

namespace soundSystem_
{
    public enum UISound{
        BUTTON_CLICK=0,
    }

    public class UISoundController : SoundController, IKeyInputListener
    {
        private readonly List<AudioClip> clips = new List<AudioClip>();

        protected override void Awake()
        {
            base.Awake();
        }

        private void Start()
        {
            InitSoundSources();
            InputManager.Instance.SetKeyListener(this);
        }

        public void PlayButton(UISound uISound)
        {
            if ((int)uISound >= clips.Count || clips[(int)uISound] == null)
            {
                return;
            }
            audioSource.clip = clips[(int)uISound];
            audioSource.Play();
        }

        private void InitSoundSources()
        {
            clips.Add(SoundManager.Instance.soundSources.GetClipByName("ButtonSound"));
        }

        void IKeyInputListener.OnKeyDown(ActionCode action)
        {
            if (action == ActionCode.SelectClick || action == ActionCode.OpenInventory)
            {
                PlayButton(UISound.BUTTON_CLICK);
            }
        }
    }
}
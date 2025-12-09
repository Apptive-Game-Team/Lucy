using System.Collections.Generic;
using InputSystem;

namespace soundSystem_
{
    public enum UISound{
        BUTTON_CLICK=0,
    }

    public class UISoundController : SoundController, IKeyInputListener
    {
        private readonly List<SoundSource> soundSources = new List<SoundSource>();

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
            audioSource.clip = soundSources[(int)uISound].sound;
            audioSource.Play();
        }

        private void InitSoundSources()
        {
            soundSources.Add(SoundManager.Instance.soundSources.GetByName("ButtonSound").Value);
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
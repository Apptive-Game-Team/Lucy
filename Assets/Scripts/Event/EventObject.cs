using Portal;
using soundSystem_;
using UnityEngine;

namespace Event
{
    public class EventObject : MonoBehaviour, ISceneChangeListener
    {
        private EventSoundController eventSoundController;

        void Start()
        {
            PortalManager.Instance.SetSceneChangeListener(this);
            eventSoundController = GetComponent<EventSoundController>();
            EventScheduler.Instance.eventObjects.Add("FirstMeetNpcEventObject", this);
        }

        public void PlaySound()
        {
            eventSoundController.PlaySound(SoundManager.Instance.soundSources.GetByName("OpenTheDoor2").Value.sound, true);
        }
        public void StopSound()
        {
            eventSoundController.StopSound();
        }

        public void OnSceneChange()
        {
            EventScheduler.Instance.UpdateGameEvent();
        }
    }
}

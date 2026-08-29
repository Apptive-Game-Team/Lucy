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
            // Indexer, not Add: a second EventObject (or a scene reload) threw on a duplicate key.
            EventScheduler.Instance.eventObjects["FirstMeetNpcEventObject"] = this;
        }

        public void PlaySound()
        {
            eventSoundController.PlaySound(SoundManager.Instance.soundSources.GetClipByName("OpenTheDoor2"), true);
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

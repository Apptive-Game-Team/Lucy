using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Portal
{
    /// <summary>
    /// Manages scene transitions through portals and notifies registered listeners of scene changes.
    /// Singleton that persists across scene loads to maintain state.
    /// </summary>
    public class PortalManager : SingletonObject<PortalManager>
    {
        [SerializeField] SceneDataList sceneDataList;
        [SerializeField] PortalDataList portalDataList;

        private const float CALL_LISTENER_DELAY = 0.1f;

        private readonly List<ISceneChangeListener> listeners = new List<ISceneChangeListener>();

        private bool isTransiting;

        private void Start()
        {
            DontDestroyOnLoad(this);
            StartCoroutine(CallOnSceneChange());
        }

        /// <summary>
        /// Registers a listener to be notified when scene changes occur.
        /// </summary>
        /// <param name="listener">The listener to register</param>
        public void SetSceneChangeListener(ISceneChangeListener listener)
        {
            if (!listeners.Contains(listener))
            {
                listeners.Add(listener);
            }
        }

        /// <summary>
        /// Notifies all registered listeners of a scene change after a short delay.
        /// Catches and logs any exceptions from listeners to prevent cascade failures.
        /// </summary>
        private IEnumerator CallOnSceneChange()
        {
            yield return new WaitForSeconds(CALL_LISTENER_DELAY);
            // Backwards: listeners destroyed by the scene load are dropped here, and a listener
            // registering another one during the callback cannot invalidate the iteration.
            for (int i = listeners.Count - 1; i >= 0; i--)
            {
                ISceneChangeListener listener = listeners[i];
                if (listener == null || (listener is MonoBehaviour behaviour && behaviour == null))
                {
                    listeners.RemoveAt(i);
                    continue;
                }

                try
                {
                    listener.OnSceneChange();
                }
                catch(Exception e)
                {
                    Debug.LogWarning(e.Message);
                }

            }
        }

        /// <summary>
        /// Transitions to a new scene through the specified portal.
        /// Handles fade effects, scene loading, and player repositioning.
        /// </summary>
        /// <param name="portalID">The ID of the portal to use for transition</param>
        public IEnumerator TransitScene(PortalID portalID)
        {
            PortalData portalData = portalDataList.GetPortalDataByID(portalID);
            SceneData sceneData = portalData == null ? null : sceneDataList.GetSceneDataByID(portalData.sceneID);
            if (sceneData == null || isTransiting)
            {
                yield break;
            }
            isTransiting = true;

            yield return CameraEffector.Instance.FadeOut();
            SceneManager.LoadScene(sceneData.sceneName);
            yield return null; // LoadScene only takes effect at the end of the frame
            StartCoroutine(CallOnSceneChange());
            Character.Instance.transform.position = portalData.destination;
            yield return CameraEffector.Instance.FadeIn();
            isTransiting = false;
        }

    }
}

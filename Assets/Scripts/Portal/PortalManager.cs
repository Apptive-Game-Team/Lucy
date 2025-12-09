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
            listeners.Add(listener);
        }

        /// <summary>
        /// Notifies all registered listeners of a scene change after a short delay.
        /// Catches and logs any exceptions from listeners to prevent cascade failures.
        /// </summary>
        private IEnumerator CallOnSceneChange()
        {
            yield return new WaitForSeconds(CALL_LISTENER_DELAY);
            foreach (ISceneChangeListener listener in listeners)
            {
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
            yield return CameraEffector.Instance.FadeOut();
            PortalData portalData = portalDataList.GetPortalDataByID(portalID);
            SceneData sceneData = sceneDataList.GetSceneDataByID(portalData.sceneID);
            SceneManager.LoadScene(sceneData.sceneName);
            StartCoroutine(CallOnSceneChange());
            Character.Instance.transform.position = portalData.destination;
            StartCoroutine(CameraEffector.Instance.FadeIn());
        }

    }
}

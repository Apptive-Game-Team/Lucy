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
        public bool IsTransitioning { get; private set; }

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
            if (listener != null && !listeners.Contains(listener)) listeners.Add(listener);
        }

        /// <summary>
        /// Notifies all registered listeners of a scene change after a short delay.
        /// Catches and logs any exceptions from listeners to prevent cascade failures.
        /// </summary>
        private IEnumerator CallOnSceneChange()
        {
            yield return new WaitForSecondsRealtime(CALL_LISTENER_DELAY);
            listeners.RemoveAll(listener => listener == null || (listener is UnityEngine.Object obj && obj == null));
            foreach (ISceneChangeListener listener in listeners.ToArray())
            {
                try
                {
                    listener.OnSceneChange();
                }
                catch(Exception e)
                {
                    Debug.LogException(e);
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
            if (IsTransitioning || Lucy.Presentation.GameSession.BlocksGameplayInput) yield break;
            if (portalDataList == null || sceneDataList == null) yield break;
            PortalData portalData = portalDataList.GetPortalDataByID(portalID);
            if (portalData == null) yield break;
            SceneData sceneData = sceneDataList.GetSceneDataByID(portalData.sceneID);
            if (sceneData == null || !Application.CanStreamedLevelBeLoaded(sceneData.sceneName))
            {
                Debug.LogError("Portal destination is not in Build Settings: " + portalID);
                yield break;
            }
            IsTransitioning = true;
            try
            {
                if (InputSystem.InputManager.Instance != null) InputSystem.InputManager.Instance.SetMovementState(false);
                if (Character.Instance != null && Character.Instance.TryGetComponent<Rigidbody2D>(out var body)) body.velocity = Vector2.zero;
                if (CameraEffector.Instance != null) yield return CameraEffector.Instance.FadeOut();
                if (Lucy.Presentation.GameSession.BlocksGameplayInput)
                {
                    if (CameraEffector.Instance != null) yield return CameraEffector.Instance.FadeIn();
                    yield break;
                }
                yield return SceneManager.LoadSceneAsync(sceneData.sceneName);
                if (Character.Instance != null) Character.Instance.transform.position = portalData.destination;
                yield return CallOnSceneChange();
                if (CameraEffector.Instance != null) yield return CameraEffector.Instance.FadeIn();
            }
            finally
            {
                if (InputSystem.InputManager.Instance != null) InputSystem.InputManager.Instance.SetMovementState(true);
                IsTransitioning = false;
            }
        }

    }
}

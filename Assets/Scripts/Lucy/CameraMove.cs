using System;
using Portal;
using ReferenceSystem;
using UnityEngine;

namespace Lucy
{
    public class CameraMove : SingletonObject<CameraMove>, ISceneChangeListener
    {
        public GameObject player;
        public GameObject square;
        float height;
        float width;
        float diffX;
        float diffY;
        public float CameraMoveSpeed = 150f;
        public GameObject hallucination;

        void OnEnable()
        {
            if (Instance != this) return;
            ReferenceManager.Instance.SetReferableObject("MainCamera",this,false);
            var overlay = transform.Find("hallucination");
            hallucination = overlay != null ? overlay.gameObject : null;
            if (hallucination != null) hallucination.SetActive(false);
        }

        protected override void Awake()
        {
            base.Awake();
            if (Instance != this) return;
            PortalManager.Instance.SetSceneChangeListener(this);
        }

        void ISceneChangeListener.OnSceneChange()
        {
            square = ReferenceManager.Instance.FindGameObjectByName("Square");
            player = Character.Instance != null ? Character.Instance.gameObject : null;
            if (square == null || Camera.main == null) return;
            Renderer BackgroundRenderer = square.GetComponent<Renderer>();
            Vector2 MapSize = new Vector2(BackgroundRenderer.bounds.size.x, BackgroundRenderer.bounds.size.y);
            height = Camera.main.orthographicSize;
            width = height * Screen.width / Screen.height;
            diffX = Mathf.Max(0, MapSize.x/2 - width);
            diffY = Mathf.Max(0, MapSize.y/2 - height);
        }

        void Update()
        {
            if (Lucy.Presentation.GameSession.BlocksGameplayInput || player == null || square == null) return;
            try
            {
                CameraPosition();
            }
            catch (MissingReferenceException)
            {
#if UNITY_EDITOR
                print("Square or Player is not Attached yet");
#endif
            }
        }

        void CameraPosition()
        {
            try
            {
                transform.position = Vector3.Lerp(transform.position, player.transform.position + new Vector3(0, 0, -10), Time.deltaTime * CameraMoveSpeed);
                //transform.LookAt(Character.transform);

                float ClampX = Mathf.Clamp(transform.position.x, -diffX + square.transform.position.x, diffX + square.transform.position.x);
                float ClampY = Mathf.Clamp(transform.position.y, -diffY + square.transform.position.y, diffY + square.transform.position.y);
                transform.position = new Vector3(ClampX, ClampY, -10);
            }
            catch (MissingReferenceException)
            {
                player = Character.Instance.gameObject;
            } catch (NullReferenceException)
            {
                player = Character.Instance.gameObject;
            }
            
        }
    }
}


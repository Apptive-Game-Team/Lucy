using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace CharacterCamera
{
    public class CameraMove : SingletonObject<CameraMove>
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
            ReferenceManager.Instance.SetReferableObject("MainCamera",this,false);
            hallucination = transform.Find("hallucination").gameObject;
            hallucination.SetActive(false);
        }

        protected override void Awake()
        {
            base.Awake();
        }

        void Start()
        {
            square = ReferenceManager.Instance.FindGameObjectByName("Square");
            player = Character.Instance.gameObject;
            Renderer BackgroundRenderer = square.GetComponent<Renderer>();
            Vector2 MapSize = new Vector2(BackgroundRenderer.bounds.size.x, BackgroundRenderer.bounds.size.y);
            height = Camera.main.orthographicSize;
            width = height * Screen.width / Screen.height;
            diffX = MapSize.x/2 - width;
            diffY = MapSize.y/2 - height;
            
        }

        void Update()
        {
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


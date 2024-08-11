using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CharacterCamera
{
    public class CameraMove : MonoBehaviour
    {
        public GameObject Character;
        public GameObject BackGround;
        float height;
        float width;
        float diffX;
        float diffY;
        public float CameraMoveSpeed = 150f;
        
        void Awake()
        {
            Renderer BackgroundRenderer = BackGround.GetComponent<Renderer>();
            Vector2 MapSize = new Vector2(BackgroundRenderer.bounds.size.x, BackgroundRenderer.bounds.size.y);
            height = Camera.main.orthographicSize;
            width = height * Screen.width / Screen.height;
            diffX = MapSize.x/2 - width;
            diffY = MapSize.y/2 - height;
        }

        void Update()
        {
            CameraPosition();
        }

        void CameraPosition()
        {
            transform.position = Vector3.Lerp(transform.position,Character.transform.position + new Vector3(0,0,-10),Time.deltaTime * CameraMoveSpeed);
            //transform.LookAt(Character.transform);

            float ClampX = Mathf.Clamp(transform.position.x,-diffX,diffX);
            float ClampY = Mathf.Clamp(transform.position.y,-diffY,diffY);
            transform.position = new Vector3(ClampX,ClampY,-10);
        }
    }
}


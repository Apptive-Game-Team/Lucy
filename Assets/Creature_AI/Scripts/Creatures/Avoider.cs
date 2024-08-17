using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Creature
{

    public class CreatureAvoidingAction : CreatureAction
    {
        Avoider avoider;
        public CreatureAvoidingAction(Avoider avoider)
        {
            this.avoider = avoider;
        }

        public override void Play()
        {
            avoider.StartCoroutine(avoider.AvoidingAction());
        }
    }

    public class Avoider : Creature
    {

        Vector3 tempVelocity = new Vector2();
        Vector3 handLightPosition;
        void Start()
        {
            base.Start();
            minSpeed = 1;
            maxSpeed = 2;

            actions.Add(new CreatureAvoidingAction(this));

            AddLightOnMap();
            pathFinder = new PathFinder(map, mapOffset);
            actions[(int)status].Play();
            StartCoroutine(MoveOnPath());
        }

        public IEnumerator AvoidingAction()
        {
            //Rigidbody2D rigidbody = gameObject.GetComponent<Rigidbody2D>();
            tempVelocity.Set(transform.position.x - handLightPosition.x, transform.position.y - handLightPosition.y, 0);
            tempVelocity.Normalize();
            //tempVelocity *= maxSpeed;
            //rigidbody.velocity = tempVelocity;
            speed = 4;
            path = pathFinder.GetRandomPath(2, tempVelocity, Vector3ToVector3Int(transform.position));

            yield return new WaitForSeconds(0.1f);
            status = CreatureStatus.PATROL;
            actions[(int)status].Play();
        }

        private void AddLightOnMap()
        {
            GameObject[] spotLights = GameObject.FindGameObjectsWithTag("Light");

            foreach (GameObject spotLight in spotLights)
            {
                Light2D light = spotLight.GetComponentInChildren<Light2D>();
                List<(int, int)> points = PointsInCircle(
                    (int)spotLight.transform.position.x,
                    (int)spotLight.transform.position.y,
                    (int)light.pointLightOuterRadius);

                foreach ((int, int) point in points)
                {
                    try
                    {
                        map[point.Item1 - mapOffset.x, point.Item2 - mapOffset.y] = 0;
                    }
                    catch { }
                    
                }
            }
        }

        public override IEnumerator PatrolAction()
        {
            speed = minSpeed;
            SetRandomPath();
            DetectPlayer();
            yield return new WaitForSeconds(0.1f);
            actions[(int)status].Play();
        }

        protected override void OnTriggerEnter2D(Collider2D collision)
        {
            base.OnTriggerEnter2D(collision);
        }

        public static List<(int, int)> PointsInCircle(int cx, int cy, int radius)
        {
            List<(int, int)> points = new List<(int, int)>();

            int xMin = (int)Math.Ceiling((decimal)cx - radius);
            int xMax = (int)Math.Floor((decimal)cx + radius);
            int yMin = (int)Math.Ceiling((decimal)cy - radius);
            int yMax = (int)Math.Floor((decimal)cy + radius);

            for (int x = xMin; x <= xMax; x++)
            {
                for (int y = yMin; y <= yMax; y++)
                {
                    if ((x - cx) * (x - cx) + (y - cy) * (y - cy) <= radius * radius)
                    {
                        points.Add((x, y));
                    }
                }
            }

            return points;
        }

        public void OnDetectedByHandLight(Vector3 handLightPosition)
        {
            status = CreatureStatus.AVOIDING;
            this.handLightPosition = handLightPosition;
        }
    }
}
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

        protected Vector3 tempVelocity = new Vector2();
        protected Vector3 handLightPosition;
        protected int[,] lightAppliedMap;

        protected void Start()
        {
            base.Start();
            minSpeed = 1;
            maxSpeed = 2;

            actions.Add(new CreatureAvoidingAction(this));

            
            pathFinder = new PathFinder(map, mapOffset);
            AddLightOnMap();
            actions[(int)status].Play();
            StartCoroutine(MoveOnPath());
        }


        protected override List<Node> FindPath(float x, float y)
        {
            AddLightOnMap();
            return base.FindPath(x, y);
        }

        protected override void SetRandomPath()
        {
            AddLightOnMap();
            base.SetRandomPath();
        }


        public IEnumerator AvoidingAction()
        {
            tempVelocity.Set(transform.position.x - handLightPosition.x, transform.position.y - handLightPosition.y, 0);
            tempVelocity.Normalize();
            speed = 4;
            path = pathFinder.GetRandomPath(20, tempVelocity, Vector3ToVector3Int(transform.position));

            yield return new WaitForSeconds(4);
            status = CreatureStatus.PATROL;
            actions[(int)status].Play();
        }
        GameObject[] lastSpotLights = new GameObject[0];

        protected void AddLightOnMap()
        {
            if (debugMode)
            {
                Debug.Log(gameObject.name + " | " + this.name + " : Apply Light On Map...");
            }

            lightAppliedMap = DeepCopy(map);
            
            GameObject[] spotLights = GameObject.FindGameObjectsWithTag("Light");

            if (CompareGameObjectArrays.AreGameObjectArraysEqual(lastSpotLights, spotLights))
            {
                return;
            }
            else
            { 
                lastSpotLights = spotLights;
            } 

            foreach (GameObject spotLight in spotLights)
            {
                Light2D light = spotLight.GetComponentInChildren<Light2D>();
                if (!light.gameObject.active)
                {
                    continue;
                }
                List<(int, int)> points;
                try
                {
                    points = PointsInCircle(
                    (int)spotLight.transform.position.x,
                    (int)spotLight.transform.position.y,
                    (int)light.pointLightOuterRadius);
                }
                catch
                {
                    continue;
                }
                

                foreach ((int, int) point in points)
                {
                    try
                    {
                        lightAppliedMap[point.Item1 - mapOffset.x, point.Item2 - mapOffset.y] = 0;
                    }
                    catch { }
                    
                }

                pathFinder.SetMap(lightAppliedMap);
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

        public List<(int, int)> PointsInCircle(int cx, int cy, int radius)
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

        protected int[,] DeepCopy(int[,] originalArray)
        {
            int rows = originalArray.GetLength(0);
            int cols = originalArray.GetLength(1);

            int[,] newArray = new int[rows, cols];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    newArray[i, j] = originalArray[i, j];
                }
            }

            return newArray;
        }

        protected void Update()
        {
            base.Update();
        }
    }
}
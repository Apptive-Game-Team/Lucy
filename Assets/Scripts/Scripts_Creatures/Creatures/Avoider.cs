using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Team6203.Util;

namespace Creature
{
    public class Avoider : Creature
    {

        protected Vector3 tempVelocity = new Vector2();
        protected Vector3 handLightPosition;

        protected override void Start()
        {
            pathFinderType = PathFinderType.AVOIDER;
            base.Start();

            minSpeed = 1;
            maxSpeed = 2;

            actions.Add(CreatureStatus.AVOIDING, (AvoidingStart, AvoidingUpdate));
            actions[status].Start();
        }


        protected override List<Node> FindPath(float x, float y)
        {
            return base.FindPath(x, y);
        }

        public IEnumerator AvoidingCounter()
        {
            yield return new WaitForSeconds(4);
            status = CreatureStatus.PATROL;
            actions[status].Start();
        }

        protected override void OnTriggerEnter2D(Collider2D collision)
        {
            base.OnTriggerEnter2D(collision);
        }

        public void OnDetectedByHandLight(Vector3 handLightPosition)
        {
            if (status != CreatureStatus.AVOIDING)
            {
                status = CreatureStatus.AVOIDING;
                actions[status].Start();
                this.handLightPosition = handLightPosition;

            }
        }

        protected override void Update()
        {
            base.Update();
        }

        protected override void PatrolUpdate()
        {
            base.PursuitUpdate();
            if (path == null || path.Count == 0)
            {
                SetRandomPath();
            }
        }

        protected virtual void AvoidingStart()
        {
            tempVelocity.Set(transform.position.x - handLightPosition.x, transform.position.y - handLightPosition.y, 0);
            tempVelocity.Normalize();
            speed = maxSpeed;
            startNode.SetPosition(transform.position.x, transform.position.y);
            path = creatureManager.pathFinders[(int)pathFinderType].FindDirectionPath(startNode, tempVelocity, 5f);
            StartCoroutine(AvoidingCounter());
        }

        protected virtual void AvoidingUpdate()
        {
            
        }
    }
}
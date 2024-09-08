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

        protected void Start()
        {
            pathFinderType = PathFinderType.AVOIDER;
            base.Start();

            minSpeed = 1;
            maxSpeed = 2;

            actions.Add(new CreatureAvoidingAction(this));
            actions[(int)status].Play();
            StartCoroutine(MoveOnPath());
        }


        protected override List<Node> FindPath(float x, float y)
        {
            return base.FindPath(x, y);
        }

        protected override void SetRandomPath()
        {
            base.SetRandomPath();
        }


        public IEnumerator AvoidingAction()
        {
            tempVelocity.Set(transform.position.x - handLightPosition.x, transform.position.y - handLightPosition.y, 0);
            tempVelocity.Normalize();
            speed = 4;
            path = CreatureManager.Instance.pathFinders[(int)pathFinderType].GetRandomPath(20, tempVelocity, Vector3ToVector3Int(transform.position));

            yield return new WaitForSeconds(4);
            status = CreatureStatus.PATROL;
            actions[(int)status].Play();
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

        public void OnDetectedByHandLight(Vector3 handLightPosition)
        {
            status = CreatureStatus.AVOIDING;
            this.handLightPosition = handLightPosition;
        }

        protected void Update()
        {
            base.Update();
        }
    }
}
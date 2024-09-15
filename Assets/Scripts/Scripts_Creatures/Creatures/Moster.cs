using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Creature
{
    public class Moster : Creature
    {
        protected override void Start()
        {
            base.Start();
            minSpeed = 1;
            maxSpeed = 2;
            actions[(int)status].Play();
            StartCoroutine(MoveOnPath());
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
    }
}


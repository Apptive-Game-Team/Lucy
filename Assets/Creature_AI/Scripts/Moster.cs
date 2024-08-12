using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Creature
{
    public class Moster : Creature
    {
        void Start()
        {
            base.Start();
        }

        public override IEnumerator PatrolAction()
        {
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


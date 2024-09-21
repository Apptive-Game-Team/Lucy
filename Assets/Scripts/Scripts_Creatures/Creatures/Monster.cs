using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Creature
{
    public class Monster : Creature
    {
        protected override void Start()
        {
            base.Start();
            minSpeed = 1;
            maxSpeed = 2;
            actions[status].Start();
            StartCoroutine(MoveOnPath());
        }

        protected override void PatrolUpdate()
        {
            base.PatrolUpdate();
            if (path == null || path.Count == 0)
            {
                SetRandomPath();
            }
        }

        protected override void Update()
        {
            base.Update();
        }

        protected override void OnTriggerEnter2D(Collider2D collision)
        {
            base.OnTriggerEnter2D(collision);
        }
    }
}


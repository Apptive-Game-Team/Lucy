using UnityEngine;

namespace Scripts_Creatures.Creatures
{
    public class Monster : Creature
    {
        protected override void Start()
        {
            base.Start();
            if (!enabled) return;
            minSpeed = 1;
            maxSpeed = 2;
            actions[status].Start();
        }

        protected override void PatrolUpdate()
        {
            base.PatrolUpdate();
            if (status != CreatureStatus.PATROL) return;
            if (path == null || path.Count == 0)
            {
                SetDirectionPath();
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


using System.Collections;
using UnityEngine;

namespace Creature
{
    public class Stunnee : Avoider
    {

        protected override void Start()
        {
            base.Start();
            actions[CreatureStatus.AVOIDING] = (StunStart, StunUpdate);
        }


        public IEnumerator StunCounter()
        {
            yield return new WaitForSeconds(4);
            status = CreatureStatus.PATROL;
            actions[status].Start();
        }



        protected virtual void StunStart()
        {
            path = null;
            StartCoroutine(StunCounter());
        }

        protected virtual void StunUpdate()
        {

        }

        protected override void OnTriggerEnter2D(Collider2D collision)
        {
            base.OnTriggerEnter2D(collision);
        }

#if UNITY_EDITOR
        protected override void Update()
        {
            base.Update();
        }
#endif
    }
}
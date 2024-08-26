using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Rendering.Universal;

namespace Creature
{

    public class CreatureStunAction : CreatureAction
    {
        Stunnee stunnee;
        public CreatureStunAction(Stunnee stunnee)
        {
            this.stunnee = stunnee;
        }

        public override void Play()
        {
            stunnee.StartCoroutine(stunnee.StunnedAction());
        }
    }

    public class Stunnee : Avoider
    {

        protected void Start()
        {
            base.Start();
            actions[(int)CreatureStatus.AVOIDING] = new CreatureStunAction(this);
        }


        public IEnumerator StunnedAction()
        {
            path = null;
            yield return new WaitForSeconds(4);
            status = CreatureStatus.PATROL;
            actions[(int)status].Play();
        }


        protected override void OnTriggerEnter2D(Collider2D collision)
        {
            base.OnTriggerEnter2D(collision);
        }
    }
}
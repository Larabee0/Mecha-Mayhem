using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedButton.Mech.Examples
{
    public class ShotgunProjectiles : ExampleBasicProjectile
    {
        [SerializeField]
        private GameObject projectileExplosion;

        public override void Initilise(CentralMechComponent origin, int damage, float destroyDelay = 20f)
        {
            this.origin = origin;
            this.damage = damage;
            Destroy(gameObject, destroyDelay);
        }
        protected override void OnCollisionEnter(Collision collision)
        {
            base.OnCollisionEnter(collision);
            Instantiate(projectileExplosion, transform.position, Quaternion.identity);
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedButton.Mech.Examples
{
    public class ShotgunProjectiles : ExampleBasicProjectile
    {
        [SerializeField]
        private GameObject projectileExplosion;

        public override void Initilise(CentralMechComponent origin, int damage)
        {
            this.damage = damage;
            Destroy(gameObject, 0.5f);
        }
        protected override void OnCollisionEnter(Collision collision)
        {
            base.OnCollisionEnter(collision);
            Instantiate(projectileExplosion, transform.position, Quaternion.identity);
            CentralMechComponent mech = collision.gameObject.GetComponentInParent<CentralMechComponent>();
            if (mech && mech != origin)
            {
                mech.UpdateHealth(damage);
            }
        }
    }
}
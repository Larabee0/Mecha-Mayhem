using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedButton.Mech.Examples
{
    public class ExampleBasicProjectile : ProjectileCore
    {
        [SerializeField] private CentralMechComponent origin;
        private int damage;
        public override void Initilise(CentralMechComponent origin,int damage)
        {
            this.origin = origin;
            this.damage = damage;
            Destroy(gameObject, 20f);
        }

        protected override void OnCollisionEnter(Collision collision)
        {
            Destroy(gameObject);
            if (collision.gameObject.TryGetComponent(out CentralMechComponent mech) && mech != origin)
            {
                mech.UpdateHealth(damage);
            }
        }
    }
}
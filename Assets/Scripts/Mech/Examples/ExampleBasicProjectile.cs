using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedButton.Mech.Examples
{
    /// <summary>
    /// example projecitle used by the BasicPhysicsWeapon
    /// </summary>
    public class ExampleBasicProjectile : ProjectileCore
    {
        [SerializeField] private CentralMechComponent origin;
        private int damage;

        /// <summary>
        /// set the damage nad origin mech properties.
        /// also if this object has not hit anything after 20 seconds, destroys itself
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="damage"></param>
        public override void Initilise(CentralMechComponent origin,int damage)
        {
            this.origin = origin;
            this.damage = damage;
            Destroy(gameObject, 20f);
        }

        /// <summary>
        /// Destroy the projecitle.
        /// When we hit another object, try get a CMC in its hierarchy
        /// assuming we have not hit ourselves, apply the damge.
        /// </summary>
        /// <param name="collision"></param>
        protected override void OnCollisionEnter(Collision collision)
        {
            Destroy(gameObject);
            CentralMechComponent mech = collision.gameObject.GetComponentInParent<CentralMechComponent>();
            if (mech && mech != origin)
            {
                mech.UpdateHealth(damage);
            }
        }
    }
}
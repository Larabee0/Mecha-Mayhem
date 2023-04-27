using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedButton.Mech.Examples
{

    public class BouncingLaserProjectile : ProjectileCore
    {
        public override void Initilise(CentralMechComponent origin, int damage, float destroyDelay = 20)
        {
            throw new System.NotImplementedException();
        }

        public void Initilise(BouncingLaserProjectile origin, int damage)
        {

        }

        protected override void OnCollisionEnter(Collision collision)
        {
            throw new System.NotImplementedException();
        }
    }
}
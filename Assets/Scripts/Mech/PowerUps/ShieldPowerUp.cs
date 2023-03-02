using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace RedButton.Mech
{
    public class ShieldPowerUp : PowerUpCore
    {
        [SerializeField] private int shieldExtraHealth = 3;

        public override void AddTo(CentralMechComponent target)
        {
            target.shield.currentShieldHealth = shieldExtraHealth;
        }
    }
}
using RedButton.Mech;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace RedButton.GamePlay
{
    public class ShieldPowerUp : PowerUpCore
    {
        [SerializeField] private int shieldExtraHealth = 3;
        public override void Copy(PowerUpCore source)
        {
            base.Copy(source);
            shieldExtraHealth = (source as ShieldPowerUp).shieldExtraHealth;
        }
        public override void AddTo(CentralMechComponent target)
        {
            target.shield.currentShieldHealth = shieldExtraHealth;
        }
    }
}
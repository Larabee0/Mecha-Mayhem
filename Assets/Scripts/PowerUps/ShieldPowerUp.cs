using RedButton.Mech;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace RedButton.GamePlay
{
    public class ShieldPowerUp : PowerUpCore
    {
        [SerializeField] private int shieldExtraHealth = 3;
        [SerializeField] private bool RechargeShieldNowIfDischarged = false;
        public override void Copy(PowerUpCore source)
        {
            shieldExtraHealth = (source as ShieldPowerUp).shieldExtraHealth;
        }
        
        public override void AddTo(CentralMechComponent target)
        {
            int curHealth = target.shield.currentShieldHealth;

            switch (curHealth)
            {
                case 0:
                    curHealth = shieldExtraHealth;
                    break;
                case > 0 when (curHealth <= target.shield.MaxShieldHealth && target.ShieldActive)||(curHealth <= shieldExtraHealth && !target.ShieldActive):
                    curHealth += shieldExtraHealth;
                    break;
            }

            target.shield.currentShieldHealth = curHealth;
            target.shield.ShieldColour();
            if(RechargeShieldNowIfDischarged)
            {
                target.shield.RechargeNow();
            }
        }
    }
}
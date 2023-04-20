using RedButton.Mech;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace RedButton.GamePlay
{
    public class ShieldPowerUp : PowerUpCore
    {
        [SerializeField] private int shieldExtraHealth = 3;
        private ShieldScript shieldTarget;
        public override void Copy(PowerUpCore source)
        {
            shieldExtraHealth = (source as ShieldPowerUp).shieldExtraHealth;
        }
        
        public override void AddTo(CentralMechComponent target)
        {
            shieldTarget = target.shield;
            shieldTarget.healthOffset = shieldExtraHealth;
            shieldTarget.RechargeNow();
            shieldTarget.OnShieldDamaged += OnTargetShieldDamaged;
        }

        private void OnTargetShieldDamaged()
        {
            if(shieldTarget.CurrentShieldHealth < shieldTarget.MaxShieldHealth)
            {
                shieldTarget.healthOffset = 0;
                OnDisable();
            }
        }

        private void OnDisable()
        {
            if(shieldTarget != null)
            {
                shieldTarget.OnShieldDamaged -= OnTargetShieldDamaged;
                Destroy(this);
            }
        }
    }
}
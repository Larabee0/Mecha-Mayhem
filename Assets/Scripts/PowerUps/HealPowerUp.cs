using RedButton.Mech;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace RedButton.GamePlay
{
    public class HealPowerUp : PowerUpCore
    {
        [SerializeField] private int healAmount = 50;

        public override void Copy(PowerUpCore source)
        {
            base.Copy(source);
            healAmount = (source as HealPowerUp).healAmount;
        }

        public override void AddTo(CentralMechComponent target)
        {
            if(target.Health< target.MaxHealth)
            {
                base.AddTo(target);
                target.UpdateHealth(-healAmount);
            }
        }
    }
}

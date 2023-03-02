using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace RedButton.Mech
{
    public class HealPowerUp : PowerUpCore
    {
        [SerializeField] private int healAmount = 50;
       
        public override void AddTo(CentralMechComponent target)
        {
            target.UpdateHealth(-healAmount);
        }
    }
}

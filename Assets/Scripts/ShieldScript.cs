using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace RedButton.Mech
{
    public class ShieldScript : WeaponCore
    {
        [SerializeField] private GameObject shield;

        protected override void BindtoControls()
        {
            ButtonEventContainer buttonEventContainer = controlBinding switch
            {
                ControlBinding.Fire1 => CMC.MechInputController.fireOneButton,
                ControlBinding.Fire2 => CMC.MechInputController.fireTwoButton,
                _ => null,
            };

            buttonEventContainer.OnButtonPressed += Fire;
            buttonEventContainer.OnButtonReleased += UnFire;
        }

        protected override void UnBindControls()
        {
            ButtonEventContainer buttonEventContainer = controlBinding switch
            {
                ControlBinding.Fire1 => CMC.MechInputController.fireOneButton,
                ControlBinding.Fire2 => CMC.MechInputController.fireTwoButton,
                _ => null,
            };

            buttonEventContainer.OnButtonPressed -= Fire;
            buttonEventContainer.OnButtonReleased -= UnFire;
        }

        private void UnFire()
        {
            Debug.Log("unfier");
            shield.SetActive(false);
        }

        public override void Fire()
        {
            shield.SetActive(true);
            Debug.Log("fier");
        }

        public override void GroupFire()
        {
            
        }

        protected override void Update()
        {
            
        }
    }
}

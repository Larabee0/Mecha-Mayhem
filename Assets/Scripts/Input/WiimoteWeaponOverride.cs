using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RedButton.Mech;
using RedButton.Mech.Examples;

namespace RedButton.Wiimote
{
    public class WiimoteWeaponOverride : ExampleBasicRaycasterWeapon
    {
        [SerializeField] private WiimoteTesting wiimoteInput;

        protected override void Awake()
        {
            wiimoteInput = FindObjectOfType<WiimoteTesting>();
            CMC = GetComponentInParent<CentralMechComponent>();

            if (CMC == null)
            {
                Debug.LogErrorFormat("Weapon attached to {0}, was unable to find the CMC! of its mech", gameObject.name);
                enabled = false;
                return;
            }
        }

        protected override void Update()
        {
            base.Update();
            if(wiimoteInput!= null)
            {
                if (wiimoteInput.ButtonB)
                {
                    Fire();
                }
            }
            else if( Input.GetMouseButton(0) )
            {
                Fire();
            }
        }
    }
}
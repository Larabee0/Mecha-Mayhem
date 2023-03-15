using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace RedButton.Mech
{
    public class ShieldScript : WeaponCore
    {
        [SerializeField] int shieldCD;
        [SerializeField] private int maxShieldHealth;
        [HideInInspector] public int currentShieldHealth;
        [SerializeField] bool shieldReady;
        [SerializeField] private GameObject shieldObject;
        [SerializeField] private MeshRenderer shieldColour;

        public CentralMechComponent ShieldOwner => CMC;
        public int MaxShieldHealth => maxShieldHealth;
        
        public bool ShieldActive => shieldObject.activeSelf;
        protected override void Awake()
        {
            base.Awake();
            StartCoroutine(ShieldRecharge(shieldCD));
            shieldColour = GetComponentInChildren<MeshRenderer>();
            shieldObject.transform.SetParent(null, true);
            UnFire();
            shieldReady = true;
        }

        protected override void Start()
        {
            targetObject = CMC.MechMovementCore.TargetPoint;
        }

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
            shieldObject.SetActive(false);
            currentShieldHealth -= 3;
            currentShieldHealth = currentShieldHealth < 0 ? 0 : currentShieldHealth;
        }

        public override void Fire()
        {
            if (shieldReady && !shieldObject.activeSelf)
            {
                shieldObject.SetActive(true);
                currentShieldHealth += maxShieldHealth;
                ShieldColour();
            }

        }
        IEnumerator ShieldRecharge(float time)
        {
            yield return new WaitForSeconds(time);
            shieldReady = true;
        }

        public void RechargeNow()
        {
            if (!shieldReady)
            {
                StopAllCoroutines();
                shieldReady = true;
            }
        }

        public void DamageShield()
        {
            currentShieldHealth--;

            ShieldColour();
            if (currentShieldHealth <= 0)
            {
                shieldObject.SetActive(false);
                shieldReady = false;
                StartCoroutine(ShieldRecharge(shieldCD * 2));
            }
        }

        

        public override void GroupFire()
        {
            
        }

        protected override void Update()
        {
            shieldObject.transform.position = transform.position;
            shieldObject.transform.forward = transform.forward;
        }

        public void ShieldColour()
        {
            Color shieldColour = currentShieldHealth switch
            {
                > 3 => Color.blue,
                3 => Color.green,
                2 => Color.yellow,
                1 => Color.red,
                _ => Color.green,
            };
            this.shieldColour.material.SetColor("_ObjectColor", shieldColour);
        }
    }
}

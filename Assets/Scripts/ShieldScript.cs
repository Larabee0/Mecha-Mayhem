using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RedButton.Mech
{
    public class ShieldScript : WeaponCore
    {
        [SerializeField] int shieldCD;
        [SerializeField] private int maxShieldHealth;
        private int currentShieldHealth;
        public int healthOffset;
        [SerializeField] bool shieldReady;
        [SerializeField] private GameObject shieldObject;
        [SerializeField] private MeshRenderer shieldColour;
        public Pluse OnShieldDamaged;
        [SerializeField] private Image shieldRechargeIcon;
        public CentralMechComponent ShieldOwner => CMC;
        public int MaxShieldHealth => maxShieldHealth;
        private bool overrideToWhite;

        public int CurrentShieldHealth => currentShieldHealth;
        
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
            currentShieldHealth = maxShieldHealth;
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
            UnboundFromControls = false;
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
            UnboundFromControls = true;
        }

        private void UnFire()
        {
            shieldObject.SetActive(false);
        }

        public override void Fire()
        {
            if (shieldReady && !shieldObject.activeSelf)
            {
                shieldObject.SetActive(true);
                ShieldColour();
            }

        }
        IEnumerator ShieldRecharge(float time)
        {
            shieldRechargeIcon.fillAmount = 0f;
            int lastState = 0;
            for (float t = 0; t < time; t+=Time.deltaTime)
            {
                float progress = Mathf.InverseLerp(0, time, t);
                int state;
                switch (progress)
                {
                    case < 0.4f:
                        state = 0;
                        shieldRechargeIcon.color = Color.Lerp(Color.white, Color.red, Mathf.InverseLerp(0, 0.4f, progress));
                        break;
                    case < 0.7f:
                        state = 1;
                        shieldRechargeIcon.color = Color.Lerp(Color.red, Color.yellow, Mathf.InverseLerp(0.4f, 0.7f, progress));
                        break;
                    default:
                        state = 2;
                        shieldRechargeIcon.color = Color.Lerp(Color.yellow, Color.green, Mathf.InverseLerp(0.7f, 1f, progress));
                        break;
                }
                if (lastState != state)
                {
                    StartCoroutine(FlashIcon());
                    // flash
                }
                if (overrideToWhite)
                {
                    shieldRechargeIcon.color = Color.white;
                }
                lastState = state;
                shieldRechargeIcon.fillAmount = progress;
                yield return null;
            }
            StartCoroutine(FlashIcon());
            shieldReady = true;
            currentShieldHealth = maxShieldHealth;
        }

        private IEnumerator FlashIcon()
        {
            shieldRechargeIcon.color = Color.white;
            overrideToWhite = true;
            shieldRechargeIcon.gameObject.SetActive(false);
            yield return new WaitForSeconds(0.1f);
            shieldRechargeIcon.gameObject.SetActive(true);
            yield return new WaitForSeconds(0.1f);
            shieldRechargeIcon.gameObject.SetActive(false);
            yield return new WaitForSeconds(0.1f);
            shieldRechargeIcon.gameObject.SetActive(true);
            overrideToWhite = false;
            ShieldColour();
        }

        public void RechargeNow()
        {
            if (!shieldReady)
            {
                StopAllCoroutines();
                shieldReady = true;
            }
            currentShieldHealth = maxShieldHealth + healthOffset;
            ShieldColour();
        }

        public void DamageShield()
        {
            currentShieldHealth--;
            ShieldColour();
            OnShieldDamaged?.Invoke();
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
            shieldRechargeIcon.transform.LookAt(Camera.main.transform.position, -Vector3.up);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            healthOffset = 0;
            RechargeNow();
        }

        public void ShieldColour()
        {
            if(this.shieldColour == null)
            {
                return;
            }
            Color shieldColour = currentShieldHealth switch
            {
                > 3 => Color.blue,
                3 => Color.green,
                2 => Color.yellow,
                1 => Color.red,
                _ => Color.green,
            };

            float weight = Mathf.InverseLerp(0, MaxShieldHealth, currentShieldHealth);
            shieldRechargeIcon.color = shieldColour;
            shieldRechargeIcon.fillAmount = weight;

            this.shieldColour.material.SetColor("_FrontColour", shieldColour);
            this.shieldColour.material.SetColor("_BackColour", shieldColour);
        }
    }
}

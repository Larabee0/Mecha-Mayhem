using RedButton.Mech;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedButton.GamePlay
{
    public class MapPowerUp : MonoBehaviour
    {
        [SerializeField] private AudioSource source;
        [SerializeField] private PowerUpCore powerUpCore;
        [HideInInspector] public PowerUpsManager manager;
        public bool mechInTrigger;
        [SerializeField] private float spinSpeed = 1;
        [SerializeField] private float brightness = 1f;
        private float SpinAngle => Time.deltaTime * UnityEngine.Random.value * spinSpeed;

        MeshRenderer capsuleRenderer;
        Transform powerUpCapsule;

        private void Awake()
        {
            capsuleRenderer = GetComponentInChildren<MeshRenderer>();
            capsuleRenderer.enabled = false;
            powerUpCapsule = capsuleRenderer.transform;
            powerUpCore = GetComponent<PowerUpCore>();
            source.transform.SetParent(null);
            
        }

        private void Update()
        {
            powerUpCapsule.Rotate(SpinAngle, SpinAngle, SpinAngle, Space.Self);
        }

        private void OnValidate()
        {
            if(powerUpCore != null)
            {
                capsuleRenderer.material.SetColor("_EmissiveColor", powerUpCore.powerUpColour * brightness);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if(powerUpCore == null)
            {
                return;
            }
            CentralMechComponent mech = other.gameObject.GetComponentInParent<CentralMechComponent>();

            if(mech != null)
            {
                if(powerUpCore.particleEffect != null)
                {
                    Instantiate(powerUpCore.particleEffect, mech.transform);
                }

                source.Play();
                
                powerUpCore.AddTo(mech);
                ConsumePowerUp();
            }

        }

        private void OnTriggerStay(Collider other)
        {
            mechInTrigger = true;
        }

        private void OnTriggerExit(Collider other)
        {
            mechInTrigger = false;
        }

        public void ConsumePowerUp()
        {
            capsuleRenderer.enabled = false;
            if(powerUpCore != null)
            {
                StopAllCoroutines();
                manager.activePowerUps.Remove(this);
                manager.inactivePowerUps.Add(this);
                manager.ReleasePowerUp(powerUpCore.GetType());
                Destroy(powerUpCore);
            }
        }

        public void SetPowerUp(PowerUpCore powerUpCore)
        {
            if (powerUpCore != null)
            {
                this.powerUpCore = (PowerUpCore)gameObject.AddComponent(powerUpCore.GetType());
                this.powerUpCore.Copy(powerUpCore);
                manager.activePowerUps.Add(this);
                manager.inactivePowerUps.Remove(this);
                this.powerUpCore.powerUpColour = capsuleRenderer.material.color = powerUpCore.powerUpColour;
                this.powerUpCore.particleEffect = powerUpCore.particleEffect;
                capsuleRenderer.material.SetColor("_EmissiveColor", powerUpCore.powerUpColour * brightness);
                capsuleRenderer.enabled = true;
                StartCoroutine(PowerUpTimeOut());
            }
            else
            {
                manager.activePowerUps.Remove(this);
                manager.inactivePowerUps.Add(this);
            }
        }

        private IEnumerator PowerUpTimeOut()
        {
            yield return new WaitForSeconds(manager.powerUpTimeOut);
            ConsumePowerUp();
        }
    }
}

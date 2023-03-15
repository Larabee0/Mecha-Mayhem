using RedButton.Mech;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace RedButton.GamePlay
{
    public class MapPowerUp : MonoBehaviour
    {
        [SerializeField] private PowerUpCore powerUpCore;
        [HideInInspector] public PowerUpsManager manager;
        [SerializeField] private bool mechInTrigger;
        [SerializeField] private bool powerUpActive;
        [SerializeField] private float spinSpeed = 1;

        private float SpinAngle => Time.deltaTime * UnityEngine.Random.value * spinSpeed;

        float powerUpTimeOut;
        float countUpTimer;

        MeshRenderer capsuleRenderer;
        Transform powerUpCapsule;

        private void Awake()
        {
            capsuleRenderer = GetComponentInChildren<MeshRenderer>();
            capsuleRenderer.enabled = false;
            powerUpCapsule = capsuleRenderer.transform;
            powerUpCore = GetComponent<PowerUpCore>();
            
        }

        private void Update()
        {
            powerUpCapsule.Rotate(SpinAngle, SpinAngle, SpinAngle, Space.Self);
        }

        

        private void OnTriggerEnter(Collider other)
        {
            if(countUpTimer > 0 || powerUpCore == null)
            {
                return;
            }
            CentralMechComponent mech = other.gameObject.GetComponentInParent<CentralMechComponent>();

            if(mech != null)
            {
                powerUpCore.AddTo(mech);
                ForceRespawn();
            }

        }

        private void OnTriggerStay(Collider other)
        {
            mechInTrigger = true;
        }

        private void OnTriggerExit(Collider other)
        {
            if (countUpTimer > 0)
            {
                countUpTimer = 0;
                powerUpTimeOut = manager.GetRespawnTime();
            }
            mechInTrigger = false;
        }

        public void ForceRespawn()
        {
            capsuleRenderer.enabled = false;
            if(powerUpCore != null)
            {
                manager.activePowerUps.Remove(this);
                manager.inactivePowerUps.Add(this);
                manager.ReleasePowerUp(powerUpCore.GetType());
                Destroy(powerUpCore);
            }
            StartCoroutine(ReSpawner());
        }

        private IEnumerator ReSpawner()
        {
            powerUpTimeOut = manager.GetRespawnTime();
            countUpTimer = 0;
            while(countUpTimer < powerUpTimeOut)
            {
                while (mechInTrigger)
                {
                    yield return null;
                }
                countUpTimer += Time.deltaTime;
                yield return null;
            }

            countUpTimer = 0;
            PowerUpCore powerUpCore = manager.GetPowerUp();
            if (powerUpCore != null)
            {
                this.powerUpCore = (PowerUpCore)gameObject.AddComponent(powerUpCore.GetType());
                this.powerUpCore.Copy(powerUpCore);
                manager.activePowerUps.Add(this);
                manager.inactivePowerUps.Remove(this);
                capsuleRenderer.material.color = powerUpCore.powerUpColour;
                capsuleRenderer.enabled = true;

            }
            else
            {
                manager.activePowerUps.Remove(this);
                manager.inactivePowerUps.Add(this);
            }
            
        }
    }
}

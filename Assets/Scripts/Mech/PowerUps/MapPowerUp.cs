using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace RedButton.Mech
{
    public class MapPowerUp : MonoBehaviour
    {
        [SerializeField] private PowerUpCore powerUpCore;

        private void OnTriggerEnter(Collider other)
        {

            CentralMechComponent mech = other.gameObject.GetComponentInParent<CentralMechComponent>();

            if(mech != null)
            {
                powerUpCore.AddTo(mech);
                Destroy(gameObject);
            }

        }
        private void Awake()
        {
            powerUpCore = GetComponent<PowerUpCore>();
        }
    }
}

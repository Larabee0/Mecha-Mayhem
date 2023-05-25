using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedButton.GamePlay
{
    public class EjectaScript : MonoBehaviour
    {
        [Header("Ejecta Charecteristics")]
        private float velocity;
        public Vector3 TargetPoint;
        [Header("\nImpact")]
        [SerializeField] private GameObject impact;
        [Header("\nHazard Zone")]
        [SerializeField] private GameObject HazardZone;

        private GameObject hazardzoneInstance;

        private void Start()
        {
            GameObject Caldera = GameObject.Find("TargetPlane");
            velocity = Caldera.GetComponent<VolcanoGimmick>().Velocity;
            Rigidbody rigidbody = gameObject.GetComponent<Rigidbody>();
            rigidbody.AddForce(0,-velocity,0, ForceMode.VelocityChange);
            hazardzoneInstance = Instantiate(HazardZone,TargetPoint,Quaternion.identity);
            Destroy(gameObject, 5);
            Destroy(hazardzoneInstance, 5);
        }

        protected void OnCollisionEnter(Collision collision)
        {
            Destroy(gameObject);
            Destroy(hazardzoneInstance);
            Instantiate(impact, TargetPoint, Quaternion.identity);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedButton.GamePlay
{
    public class EjectaScript : MonoBehaviour
    {
        [Header("Ejecta Charecteristics")]
        private float velocity;
        [SerializeField] private Vector3 TargetPoint;
        [Header("\nImpact")]
        [SerializeField] GameObject impact;
        [Header("\nHazard Zone")]
        [SerializeField] GameObject HazardZone; 

        private void Awake()
        {
            GameObject Caldera = GameObject.Find("TargetPlane");
            velocity = Caldera.GetComponent<VolcanoGimmick>().Velocity;
            TargetPoint = Caldera.GetComponent<VolcanoGimmick>().TargetPoint;
            Rigidbody rigidbody = gameObject.GetComponent<Rigidbody>();
            rigidbody.AddForce(0,-velocity,0, ForceMode.VelocityChange);
            GameObject Haz = GameObject.Instantiate(HazardZone);
            Haz.transform.position = TargetPoint;
        }
        // Update is called once per frame

        protected void OnCollisionEnter(Collision collision)
        {
            Destroy(gameObject);
            GameObject Haz = GameObject.Find("Hazard Zone(Clone)");
            Destroy(Haz);
            GameObject Impact = Instantiate(impact);
            Impact.transform.position = TargetPoint;
        }
    }
}

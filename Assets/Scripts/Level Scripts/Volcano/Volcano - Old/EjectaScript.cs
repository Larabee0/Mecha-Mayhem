using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedButton.GamePlay
{
    public class EjectaScript : VolcanoGimmick
    {
        private void Awake()
        {
            Rigidbody rigidbody = gameObject.GetComponent<Rigidbody>();
            rigidbody.AddForce(TargetPoint * Velocity, ForceMode.Impulse);
        }
        // Update is called once per frame

        protected void OnCollisionEnter(Collision collision)
        {
            Destroy(gameObject);
            Mech.CentralMechComponent mech = collision.gameObject.GetComponentInParent<Mech.CentralMechComponent>();
            mech.UpdateHealth(damage);
        }
    }
}

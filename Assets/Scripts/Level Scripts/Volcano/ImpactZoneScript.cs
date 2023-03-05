using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedButton.GamePlay
{

    public class ImpactZoneScript : MonoBehaviour
    {
        private int damage;

        private void Awake()
        {
            GameObject Caldera = GameObject.Find("TargetPlane");
            damage = Caldera.GetComponent<VolcanoGimmick>().damage;
            Destroy(gameObject, 3f);
        }
        // Update is called once per frame

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.name == "TestMechVelocityChangeMovement(Clone)")
            {
                Mech.CentralMechComponent mech = collision.gameObject.GetComponentInParent<Mech.CentralMechComponent>();
                mech.UpdateHealth(damage);
            }
        }
    }
}


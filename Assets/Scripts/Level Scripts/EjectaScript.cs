using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedButton.GamePlay
{
    public class EjectaScript : MonoBehaviour
    {
        [SerializeField] public int damage = 50;

        private Vector3 bulletVector = VolcanoGimmick.bulletVector;

        public Vector3 BulletVector { get => bulletVector; set => bulletVector = value; }

        // Update is called once per frame
        private void Awake()
        {
            gameObject.Rigidbody.AddForce(VolcanoGimmick.bulletVector * Velocity, ForceMode.Impulse);
        }

        public void CheckImpact(Collision collision)
        {
            Destroy(gameObject);
            Mech.CentralMechComponent mech = collision.gameObject.GetComponentInParent<Mech.CentralMechComponent>();
            mech.UpdateHealth(damage);
        }
    }
}

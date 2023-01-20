using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedButton.Mech
{
    public abstract class ProjectileCore : MonoBehaviour
    {
        [SerializeField] protected Rigidbody rigid;
        [SerializeField] protected Collider[] projectileColliders;
        public Rigidbody Rigidbody => rigid;
        public Collider[] ProjectileColliders => projectileColliders;

        protected virtual void Awake()
        {
            projectileColliders= GetComponentsInChildren<Collider>();
        }

        protected virtual void Start()
        {

        }

        protected virtual void Update()
        {

        }

        protected abstract void OnCollisionEnter(Collision collision);

        public abstract void Initilise(CentralMechComponent origin,int damage);
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedButton.Mech
{
    /// <summary>
    /// Base class all Phyiscal projectiles should be dervied from.
    /// This expects there to be some collider(s) within the hierarchy of the projecitle object
    /// and a rigidbody.
    /// </summary>
    public abstract class ProjectileCore : MonoBehaviour
    {
        [SerializeField] protected Rigidbody rigid;
        [SerializeField] protected Collider[] projectileColliders;
        public Rigidbody Rigidbody => rigid;
        public Collider[] ProjectileColliders => projectileColliders;

        protected virtual void Awake()
        {
            projectileColliders = GetComponentsInChildren<Collider>();
        }

        protected virtual void Start() { }

        protected virtual void Update() { }

        /// <summary>
        /// OnCollisionEnter should be implemented.
        /// </summary>
        /// <param name="collision"></param>
        protected abstract void OnCollisionEnter(Collision collision);

        /// <summary>
        /// initilise should be implmented (even if it does nothing)
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="damage"></param>
        public abstract void Initilise(CentralMechComponent origin, int damage);
    }
}
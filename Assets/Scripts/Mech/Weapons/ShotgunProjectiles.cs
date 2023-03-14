using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedButton.Mech.Examples
{
    public class ShotgunProjectiles : ProjectileDeathScript
    {
        [SerializeField] Vector3 finalScale;
        [SerializeField, Range(0, 1)] float shotgunRange;
        [SerializeField] private Transform shotgunCollider;

        public override void Initilise(CentralMechComponent origin, int damage, float destroyDelay = 20f)
        {
            this.origin = origin;
            this.damage = damage;
            StartCoroutine(GoBig());
        }
        protected override void OnCollisionEnter(Collision collision)
        {
            base.OnCollisionEnter(collision);
            Instantiate(projectileExplosion, transform.position, Quaternion.identity);
            CentralMechComponent mech = collision.gameObject.GetComponentInParent<CentralMechComponent>();
            if (mech && mech != origin)
            {
                mech.UpdateHealth(damage);
            }
        }

        IEnumerator GoBig()
        {
            Vector3 initialSize = shotgunCollider.transform.localScale;
            for (float i = 0; i <= shotgunRange; i += Time.deltaTime)
            {
                shotgunCollider.transform.localScale = Vector3.Lerp(initialSize, finalScale, i);
                yield return null;
            }
            Destroy(gameObject);
        }
    }
}
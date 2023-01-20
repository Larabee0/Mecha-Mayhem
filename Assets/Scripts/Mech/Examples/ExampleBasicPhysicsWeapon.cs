using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedButton.Mech.Examples
{
    public class ExampleBasicPhysicsWeapon : WeaponCore
    {
        [SerializeField] float shootForce;

        [SerializeField][Range(0f, 1f)] float fireIntervalMin = 0.01f;
        [SerializeField][Range(0f, 1f)] float fireIntervalMax = 0.05f;
        private float fireInterval = 0f;

        public override void Fire()
        {
            fireInterval -= Time.deltaTime;

            switch (fireInterval)
            {
                case <= 0f:
                    fireInterval = Random.Range(fireIntervalMin, fireIntervalMax);
                    PhysicsShoot();
                    break;
            }
        }

        public override void GroupFire()
        {
            PhysicsShoot();
        }

        private void PhysicsShoot()
        {
            if (projectileSpawnPoint == null || targetObject == null) return;

            Vector3 bulletVector = (TargetPos - projectileSpawnPoint.position).normalized;
            Quaternion rotation = Quaternion.LookRotation(bulletVector);

            ProjectileCore projectile = Instantiate(projectilePrefab, projectileSpawnPoint.position, rotation);

            for (int p = 0; p < projectile.ProjectileColliders.Length; p++)
            {
                for (int m = 0; m < CMC.MechColliders.Length; m++)
                {
                    Physics.IgnoreCollision(projectile.ProjectileColliders[p], CMC.MechColliders[m]);
                }
            }

            projectile.Initilise(CMC, damage);

            projectile.Rigidbody.AddForce(bulletVector * shootForce, ForceMode.Impulse);
        }
    }
}
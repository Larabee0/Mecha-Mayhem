using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedButton.Mech.Examples
{
    /// <summary>
    /// Example physics wepaon
    /// </summary>
    public class ExampleBasicPhysicsWeapon : WeaponCore
    {
        private float fireInterval = 0f;

        [Header("Physics Weapon Settings")]
        [SerializeField] float shootForce;
        [SerializeField][Range(0f, 1f)] float fireIntervalMin = 0.01f;
        [SerializeField][Range(0f, 1f)] float fireIntervalMax = 0.05f;

        /// <summary>
        /// Fire is one of the methods that requires implmentation.
        /// Here whenever Fire() is called, it simply counts down a timer called FireInterval.
        /// When this values is less than zero it fires the weapon by calling PhysicsShoot().
        /// It also then resets the fireInteral to a random time between a min and max value which you can set in the editor.
        /// </summary>
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

        /// <summary>
        /// GroupFire is a pass through to call PhysicsShoot() directly as timing is handled by the WeaponGroup.
        /// </summary>
        public override void GroupFire()
        {
            PhysicsShoot();
        }

        /// <summary>
        /// This is basically a copy of the weapon from the demo game. only it has been modified to use the ProjecitleCore script.
        /// </summary>
        private void PhysicsShoot()
        {
            // safety measure.
            if (muzzleOriginPoint == null || targetObject == null)
            {
                Debug.LogWarningFormat("MuzzleOrigin or TargetObject for weapon {0} was not set, aborting weapon firing", gameObject.name);
                return;
            }

            // calculate the direction to fire the bullet along, this can also be used to set the rotation of it.
            Vector3 bulletVector = (TargetPos - muzzleOriginPoint.position).normalized;
            Quaternion rotation = Quaternion.LookRotation(bulletVector);
            
            // spawn the projectile in the correct orientaiton and position
            ProjectileCore projectile = Instantiate(projectilePrefab, muzzleOriginPoint.position, rotation);

            // Because awake is called immidately for the projectile, its colliders property will now be set (assuming it has any)
            // so we can safely ignore collisions between them and the colliders in the origin mech.
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
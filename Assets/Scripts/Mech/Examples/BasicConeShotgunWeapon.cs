using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace RedButton.Mech.Examples
{
    /// <summary>
    /// Example cone wepaon
    /// </summary>
    public class BasicConeShotgunWeapon : WeaponCore
    {
        private float fireInterval = 0f;

        [Header("Physics Cone Weapon Settings")]
        [SerializeField] float angleOfBullets;
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
            if (!CMC.ShieldActive)
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
        }

        /// <summary>
        /// GroupFire is a pass through to call PhysicsShoot() directly as timing is handled by the WeaponGroup.
        /// </summary>
        public override void GroupFire()
        {
            if (!CMC.ShieldActive)
            {
                PhysicsShoot();
            }
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
            Vector3 leftBulletVector = Quaternion.AngleAxis(-angleOfBullets, Vector3.up) * bulletVector;// (TargetPos +  - muzzleOriginPoint.position).normalized;
            Vector3 rightBulletVector = Quaternion.AngleAxis(angleOfBullets, Vector3.up) * bulletVector;
            Quaternion leftRotation = Quaternion.LookRotation(leftBulletVector);
            Quaternion rightRotation = Quaternion.LookRotation(rightBulletVector);

            // spawn the projectile in the correct orientaiton and position
            ProjectileCore projectile = Instantiate(projectilePrefab, muzzleOriginPoint.position, rotation);
            ProjectileCore leftProjectile = Instantiate(projectilePrefab, muzzleOriginPoint.position, leftRotation);
            ProjectileCore rightProjectile = Instantiate(projectilePrefab, muzzleOriginPoint.position, rightRotation);

            // Because awake is called immidately for the projectile, its colliders property will now be set (assuming it has any)
            // so we can safely ignore collisions between them and the colliders in the origin mech.

            IgnoreCollisions(projectile);
            IgnoreCollisions(leftProjectile);
            IgnoreCollisions(rightProjectile);

            IgnoreCollisions(projectile, leftProjectile);
            IgnoreCollisions(projectile, rightProjectile);
            IgnoreCollisions(leftProjectile, rightProjectile);

            projectile.Initilise(CMC, damage, 0.5f);

            leftProjectile.Initilise(CMC, damage, 0.5f);

            rightProjectile.Initilise(CMC, damage,0.5f);

            projectile.Rigidbody.AddForce(bulletVector * shootForce, ForceMode.Impulse);

            leftProjectile.Rigidbody.AddForce(leftBulletVector * shootForce, ForceMode.Impulse);

            rightProjectile.Rigidbody.AddForce(rightBulletVector * shootForce, ForceMode.Impulse);
        }

        private void IgnoreCollisions(ProjectileCore projectile)
        {
            // Because awake is called immidately for the projectile, its colliders property will now be set (assuming it has any)
            // so we can safely ignore collisions between them and the colliders in the origin mech.
            for (int p = 0; p < projectile.ProjectileColliders.Length; p++)
            {
                for (int m = 0; m < CMC.MechColliders.Length; m++)
                {
                    Physics.IgnoreCollision(projectile.ProjectileColliders[p], CMC.MechColliders[m]);
                }
            }
        }


        private void IgnoreCollisions(ProjectileCore projectile, ProjectileCore other)
        {
            // Because awake is called immidately for the projectile, its colliders property will now be set (assuming it has any)
            // so we can safely ignore collisions between them and the colliders in the origin mech.
            for (int p = 0; p < projectile.ProjectileColliders.Length; p++)
            {
                for (int m = 0; m < other.ProjectileColliders.Length; m++)
                {
                    Physics.IgnoreCollision(projectile.ProjectileColliders[p], other.ProjectileColliders[m]);
                }
            }
        }
    }
}
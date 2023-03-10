using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace RedButton.Mech.Examples
{
    /// <summary>
    /// Example cone wepaon
    /// </summary>
    public class BasicConeShotgunWeapon : ExampleBasicPhysicsWeapon
    {
        [Header("Physics Cone Weapon Settings")]
        [SerializeField] float angleOfBullets;

        /// <summary>
        /// This is basically a copy of the weapon from the demo game. only it has been modified to use the ProjecitleCore script.
        /// </summary>
        protected override void PhysicsShoot()
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
            //Vector3 leftBulletVector = Quaternion.AngleAxis(-angleOfBullets, Vector3.up) * bulletVector;// (TargetPos +  - muzzleOriginPoint.position).normalized;
            //Vector3 rightBulletVector = Quaternion.AngleAxis(angleOfBullets/2, Vector3.up) * bulletVector;
            //Vector3 leftMidBulletVector = Quaternion.AngleAxis(-angleOfBullets/2, Vector3.up) * bulletVector;
            //Vector3 rightMidBulletVector = Quaternion.AngleAxis(angleOfBullets, Vector3.up) * bulletVector;
            //Quaternion leftRotation = Quaternion.LookRotation(leftBulletVector);
            //Quaternion rightRotation = Quaternion.LookRotation(rightBulletVector);
            //Quaternion leftMidRotation = Quaternion.LookRotation(leftMidBulletVector);
            //Quaternion rightMidRotation = Quaternion.LookRotation(rightMidBulletVector);

            
            
            // spawn the projectile in the correct orientaiton and position
            ProjectileCore projectile = Instantiate(projectilePrefab, muzzleOriginPoint.position, rotation);
            //ProjectileCore leftProjectile = Instantiate(projectilePrefab, muzzleOriginPoint.position, leftRotation);
            //ProjectileCore rightProjectile = Instantiate(projectilePrefab, muzzleOriginPoint.position, rightRotation);
            //ProjectileCore leftMidProjectile = Instantiate(projectilePrefab, muzzleOriginPoint.position, leftMidRotation);
            //ProjectileCore rightMidProjectile = Instantiate(projectilePrefab, muzzleOriginPoint.position, rightMidRotation);

            // Because awake is called immidately for the projectile, its colliders property will now be set (assuming it has any)
            // so we can safely ignore collisions between them and the colliders in the origin mech.

            IgnoreCollisions(projectile);
            //IgnoreCollisions(leftProjectile);
            //IgnoreCollisions(rightProjectile);
            //IgnoreCollisions(leftMidProjectile);
            //IgnoreCollisions(rightMidProjectile);

            //IgnoreCollisions(projectile, leftProjectile);
            //IgnoreCollisions(projectile, rightProjectile);
            //IgnoreCollisions(projectile, leftMidProjectile);
            //IgnoreCollisions(projectile, rightMidProjectile);
            //IgnoreCollisions(leftProjectile, rightProjectile);
            //IgnoreCollisions(leftProjectile, leftMidProjectile);
            //IgnoreCollisions(leftProjectile, rightMidProjectile);
            //IgnoreCollisions(rightProjectile, leftMidProjectile);
            //IgnoreCollisions(rightProjectile, rightMidProjectile);
            //IgnoreCollisions(leftMidProjectile, rightMidProjectile);

            projectile.Initilise(CMC, damage, 0.5f);

            //leftProjectile.Initilise(CMC, damage);

            //rightProjectile.Initilise(CMC, damage);

            //leftMidProjectile.Initilise(CMC, damage);

            //rightMidProjectile.Initilise(CMC, damage);

            projectile.Rigidbody.AddForce(bulletVector * shootForce, ForceMode.Impulse);

            //leftProjectile.Rigidbody.AddForce(leftBulletVector * shootForce, ForceMode.Impulse);

            //rightProjectile.Rigidbody.AddForce(rightBulletVector * shootForce, ForceMode.Impulse);

            //leftMidProjectile.Rigidbody.AddForce(leftMidBulletVector * shootForce, ForceMode.Impulse);

            //rightMidProjectile.Rigidbody.AddForce(rightMidBulletVector * shootForce, ForceMode.Impulse);
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
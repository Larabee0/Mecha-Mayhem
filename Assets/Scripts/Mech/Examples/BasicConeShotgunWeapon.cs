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
            
            // spawn the projectile in the correct orientaiton and position
            ProjectileCore projectile = Instantiate(projectilePrefab, muzzleOriginPoint.position, rotation);
            
            // Because awake is called immidately for the projectile, its colliders property will now be set (assuming it has any)
            // so we can safely ignore collisions between them and the colliders in the origin mech.

            IgnoreCollisions(projectile);
            
            projectile.Initilise(CMC, damage, 0.5f);

            projectile.Rigidbody.AddForce(bulletVector * shootForce, ForceMode.Impulse);
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
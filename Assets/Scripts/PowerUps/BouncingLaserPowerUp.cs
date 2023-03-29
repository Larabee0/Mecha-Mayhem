using RedButton.Mech;
using RedButton.Mech.Examples;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedButton.GamePlay
{
    public class BouncingLaserPowerUp : WeaponPowerUpBase
    {
        [SerializeField] private BoucningLaserWeapon boucningLaserPrefab;

        public override void Copy(PowerUpCore source)
        {
            base.Copy(source);
            boucningLaserPrefab = (source as BouncingLaserPowerUp).boucningLaserPrefab;
        }

        protected override bool AddWeapon(WeaponCore weapon)
        {
            GameObject spawnedWeapon = Instantiate(boucningLaserPrefab.gameObject,weapon.transform.parent);
            addedWeapons.Add(spawnedWeapon);
            return true;
        }

        protected override bool AddWeapon(WeaponGroup weapon)
        {
            GameObject spawnedWeapon = Instantiate(boucningLaserPrefab.gameObject, weapon.transform.parent);
            addedWeapons.Add(spawnedWeapon);
            return true;
        }
    }
}
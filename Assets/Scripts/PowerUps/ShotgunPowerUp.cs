using RedButton.Mech;
using RedButton.Mech.Examples;
using UnityEngine;

namespace RedButton.GamePlay
{

    public class ShotgunPowerUp : WeaponPowerUpBase
    {
        public BasicConeShotgunWeapon shotgunPrefab;

        public override void Copy(PowerUpCore source)
        {
            base.Copy(source);
            shotgunPrefab = (source as ShotgunPowerUp).shotgunPrefab;
        }

        protected override bool AddWeapon(WeaponCore weapon)
        {
            GameObject spawnedWeapon = Instantiate(shotgunPrefab.gameObject, weapon.transform.parent);
            addedWeapons.Add(spawnedWeapon);
            return true;
        }

        protected override bool AddWeapon(WeaponGroup weapon)
        {
            GameObject spawnedWeapon = Instantiate(shotgunPrefab.gameObject, weapon.transform.parent);
            addedWeapons.Add(spawnedWeapon);
            return true;
        }
    }
}
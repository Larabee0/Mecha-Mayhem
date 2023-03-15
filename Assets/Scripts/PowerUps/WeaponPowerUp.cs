using RedButton.Mech;
using RedButton.Mech.Examples;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedButton.GamePlay
{
    public class WeaponPowerUp : PowerUpCore
    {
        public float weaponTime;

        public WeaponPowerUpMap[] powerUpPrefabs;
        [Space(100)]

        private readonly Dictionary<Type,GameObject> weaponMap = new();

        [SerializeField] private List<GameObject> addedWeapons=new();
        [SerializeField] private WeaponGroup[] stockGroups;
        [SerializeField] private List<WeaponCore> stockWeapons;

        public override void Copy(PowerUpCore source)
        {
            WeaponPowerUp weaponPowerUp = source as WeaponPowerUp;

            weaponTime = weaponPowerUp.weaponTime;
            powerUpPrefabs = weaponPowerUp.powerUpPrefabs;

            for (int i = 0; i < powerUpPrefabs.Length; i++)
            {
                weaponMap.Add(powerUpPrefabs[i].stockWeapon.GetType(), powerUpPrefabs[i].prefab);
            }
        }

        public override void AddTo(CentralMechComponent target)
        {
            if(target.gameObject.GetComponent<WeaponCore>() == null)
            {
                WeaponPowerUp mechInstance = target.gameObject.AddComponent<WeaponPowerUp>();
                mechInstance.Copy(this);
                mechInstance.SetUp(target);
            }
        }

        public void SetUp(CentralMechComponent target)
        {
            stockGroups = target.GetComponentsInChildren<WeaponGroup>();
            stockWeapons =new( target.GetComponentsInChildren<WeaponCore>());
            stockWeapons.RemoveAt(0);
            if(stockGroups.Length > 0)
            {
                RemoveSelf();
                return;
            }
            target.ResetWeaponOriginIndex();
            for (int i = 0; i < stockWeapons.Count; i++)
            {
                stockWeapons[i].enabled = false;
                if (!AddWeapon(stockWeapons[i]))
                {
                    RemoveSelf();
                    return;
                }
            }

            StartCoroutine(PowerUpTimeOut());
        }


        private bool AddWeapon(WeaponCore baseWeapon)
        {
            Type baseType = baseWeapon.GetType();
            if (weaponMap.ContainsKey(baseType))
            {
                GameObject spawnedWeapon = Instantiate(weaponMap[baseType], baseWeapon.transform.parent);
                addedWeapons.Add(spawnedWeapon);
                if (spawnedWeapon.GetComponentInChildren<WeaponGroup>() == null)
                {
                    spawnedWeapon.GetComponentInChildren<WeaponCore>().muzzleOriginPoint = baseWeapon.muzzleOriginPoint;
                }
                return true;
            }
            return false;
        }

        public IEnumerator PowerUpTimeOut()
        {
            yield return new WaitForSeconds(weaponTime);
            RemoveSelf();
        }

        public void RemoveSelf()
        {
            for (int i = 0; i < stockWeapons.Count; i++)
            {
                stockWeapons[i].enabled = true;
            }
            for (int i = 0; i < addedWeapons.Count; i++)
            {
                Destroy(addedWeapons[i]);
            }
            Destroy(this);
        }

        private void OnDisable()
        {
            if (GetComponent<CentralMechComponent>() != null)
            {
                RemoveSelf();
            }
        }
    }

    [Serializable]
    public struct WeaponPowerUpMap
    {
        public WeaponCore stockWeapon;
        public GameObject prefab;
    }
}
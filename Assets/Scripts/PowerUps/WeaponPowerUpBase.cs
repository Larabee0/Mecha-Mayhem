using RedButton.Mech;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedButton.GamePlay
{
    public abstract class WeaponPowerUpBase : PowerUpCore
    {
        public float weaponTime;
        [SerializeField] protected string weaponName;
        [SerializeField] protected WeaponGroup[] stockGroups;
        [SerializeField] protected List<GameObject> addedWeapons = new();
        [SerializeField] protected List<WeaponCore> stockWeapons;

        public override void Copy(PowerUpCore source)
        {
            WeaponPowerUpBase weaponPowerUp = source as WeaponPowerUpBase;
            weaponTime = weaponPowerUp.weaponTime;
            weaponName = weaponPowerUp.weaponName;
            powerUpIcon = weaponPowerUp.powerUpIcon;
        }

        public override void AddTo(CentralMechComponent target)
        {
            if (target.gameObject.GetComponent<WeaponPowerUpBase>() == null)
            {
                base.AddTo(target);
                WeaponPowerUpBase mechInstance = target.gameObject.AddComponent(GetType()) as WeaponPowerUpBase;
                mechInstance.Copy(this);
                mechInstance.SetUp(target);
            }
        }

        public virtual void SetUp(CentralMechComponent target)
        {
            stockGroups = target.GetComponentsInChildren<WeaponGroup>();
            stockWeapons = new(target.GetComponentsInChildren<WeaponCore>());
            stockWeapons.RemoveAt(0);
            target.ResetWeaponOriginIndex();
            if (stockGroups.Length == 2)
            {
                for (int i = 0; i < stockGroups.Length; i++)
                {
                    stockGroups[i].enabled = false;
                    if (!AddWeapon(stockGroups[i]))
                    {
                        RemoveSelf();
                        return;
                    }
                }
            }
            else
            {
                for (int i = 0; i < stockWeapons.Count; i++)
                {
                    stockWeapons[i].enabled = false;
                    if (!AddWeapon(stockWeapons[i]))
                    {
                        RemoveSelf();
                        return;
                    }
                }
            }
            StartCoroutine(PowerUpTimeOut());

        }

        protected abstract bool AddWeapon(WeaponCore weapon);
        protected abstract bool AddWeapon(WeaponGroup weapon);

        public virtual IEnumerator PowerUpTimeOut()
        {
            CentralMechComponent cmc = GetComponent<CentralMechComponent>();
            cmc.OnWeaponChanged?.Invoke(weaponName);
            cmc.OnWeaponChangedImage?.Invoke(powerUpIcon);
            yield return new WaitForSeconds(weaponTime);
            RemoveSelf();
        }

        public virtual void RemoveSelf()
        {
            for (int i = 0; i < stockWeapons.Count; i++)
            {
                stockWeapons[i].enabled = true;
            }
            for (int i = 0; i < stockGroups.Length; i++)
            {
                stockGroups[i].enabled = true;
            }
            for (int i = 0; i < addedWeapons.Count; i++)
            {
                Destroy(addedWeapons[i]);
            }
            Destroy(this);
            CentralMechComponent cmc = GetComponent<CentralMechComponent>();
            cmc.OnWeaponChanged?.Invoke(cmc.defaultWeapon);
            cmc.OnWeaponChangedImage?.Invoke(cmc.defaultWeaponIcon);
        }

        protected virtual void OnDisable()
        {
            if (GetComponent<CentralMechComponent>() != null)
            {
                RemoveSelf();
            }
        }
    }
}
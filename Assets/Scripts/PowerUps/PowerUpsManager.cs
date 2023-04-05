using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedButton.GamePlay
{
    public class PowerUpsManager : MonoBehaviour
    {
        [SerializeField] private Vector3[] spawnPoints = new Vector3[4];
        [SerializeField] private PowerUpCore[] powerUps;
        [SerializeField] private MapPowerUp powerUpPrefab;
        [SerializeField] private List<MapPowerUp> powerUpInstances;
        [SerializeField] private Dictionary<Type, int> onMapPowerUps = new();
        [SerializeField] private int globalCountOffset;
        [SerializeField] private int maxPowerUps;
        [SerializeField] private int powerUpActivateWaveSize;
        [SerializeField] private float powerUpStartOfRoundDelay = 5f;
        public readonly HashSet<MapPowerUp> activePowerUps = new();
        public readonly HashSet<MapPowerUp> inactivePowerUps = new();
        
        public float minPowerUpTime;
        public float maxPowerUpTime;
        private void Awake()
        {
            if(maxPowerUps <= 0)
            {
                maxPowerUps = spawnPoints.Length;
            }
        }

        public void SetUpPowerUps()
        {
            if (powerUpInstances.Count!= 0)
            {
                StopAllCoroutines();
                powerUpInstances.ForEach(powerUpInstance => Destroy(powerUpInstance.gameObject));
                powerUpInstances.Clear();
                onMapPowerUps.Clear();
                inactivePowerUps.Clear();
                activePowerUps.Clear();
            }

            for (int i = 0; i < spawnPoints.Length; i++)
            {
                powerUpInstances.Add(Instantiate(powerUpPrefab, spawnPoints[i], Quaternion.identity, transform));
                powerUpInstances[^1].manager = this;
            }

            for (int i = 0; i < powerUps.Length; i++)
            {
                onMapPowerUps.Add(powerUps[i].GetType(), 0);
            }
            inactivePowerUps.UnionWith(powerUpInstances);

            InitilisePowerUps();
        }

        public void InitilisePowerUps()
        {
            StartCoroutine(Initializer());
        }

        private IEnumerator Initializer()
        {
            yield return new WaitForSeconds(powerUpStartOfRoundDelay);
            List<MapPowerUp> inactive = new(inactivePowerUps);
            while (activePowerUps.Count < maxPowerUps) {
                for (int i = 0; i < powerUpActivateWaveSize; i++)
                {
                    if (inactive.Count == 0)
                    {
                        break;
                    }
                    MapPowerUp powerUp = inactive[UnityEngine.Random.Range(0, inactive.Count)];
                    while (activePowerUps.Contains(powerUp))
                    {
                        powerUp = inactive[UnityEngine.Random.Range(0, inactive.Count)];
                    }
                    inactive.Remove(powerUp);
                    powerUp.ForceRespawn();
                }
                yield return null;
            }
        }

        public float GetRespawnTime()
        {
            return UnityEngine.Random.Range(minPowerUpTime, maxPowerUpTime);
        }

        public PowerUpCore GetPowerUp()
        {
            List<Type> types = new(onMapPowerUps.Keys);
            PowerUpCore lowestType = null;
            int count = int.MaxValue;
            Type type = null;
            for (int i = 0; i < types.Count; i++)
            {
                if (onMapPowerUps[types[i]] <= count && onMapPowerUps[types[i]] < powerUps[i].limit + globalCountOffset)
                {
                    type = types[i];
                    lowestType = powerUps[i];
                    count = onMapPowerUps[types[i]];
                }
            }
            if(lowestType != null)
            {
                onMapPowerUps[type]++;
            }
            return lowestType;
        }

        public void ReleasePowerUp(Type powerUpType)
        {
            onMapPowerUps[powerUpType] = onMapPowerUps[powerUpType] <= 0 ? 0 : onMapPowerUps[powerUpType]-=1;
        }
    }
}
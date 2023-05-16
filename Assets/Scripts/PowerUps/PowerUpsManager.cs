using RedButton.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

namespace RedButton.GamePlay
{
    public class PowerUpsManager : MonoBehaviour
    {
        [SerializeField] private GameArbiter gameArbiter;
        
        [SerializeField] private PowerUpContainer[] powerUps;
        [Space(100)]
        [SerializeField] private List<MapPowerUp> powerUpInstances;
        [SerializeField] private Dictionary<Type, int> onMapPowerUps = new();
        [SerializeField] private int globalCountOffset;
        [SerializeField] private int maxPowerUps;
        [SerializeField] private bool autoCalculateMaxPowerUps;
        [SerializeField] private int autoCalMultiplier = 3;
        [SerializeField] private int powerUpActivateWaveSize;
        [SerializeField] private float powerUpStartOfRoundDelay = 5f;
        [SerializeField] private float powerUpUpdateRate = 5f;
        [SerializeField] private float favourDamagedMechWeight = 0.6f;
        [SerializeField] private float favourMostAvaliablePowerUpWeight = 0.6f;
        public float powerUpTimeOut = 30f;

        public readonly HashSet<MapPowerUp> activePowerUps = new();
        public readonly HashSet<MapPowerUp> inactivePowerUps = new();

        public float minPowerUpTime;
        public float maxPowerUpTime;

        private void Awake()
        {
            if (gameArbiter == null)
            {
                gameArbiter = GetComponent<GameArbiter>();
            }

            powerUpInstances = new(GetComponentsInChildren<MapPowerUp>());
            gameArbiter.OnRoundStarted += OnRoundStart;
        }

        private void OnRoundStart()
        {
            if (maxPowerUps == 0 || autoCalculateMaxPowerUps)
            {
                maxPowerUps = gameArbiter.PlayerCount * autoCalMultiplier;
            }
        }

        public void StopAndClear()
        {
            if (powerUpInstances.Count != 0)
            {
                StopAllCoroutines();
                powerUpInstances.ForEach(powerUpInstance => powerUpInstance.ConsumePowerUp());
                onMapPowerUps.Clear();
                inactivePowerUps.Clear();
                activePowerUps.Clear();
            }
        }

        public void SetUpPowerUps()
        {
            StopAndClear();
            
            powerUpInstances.ForEach(powerUp => powerUp.manager = this);

            for (int i = 0; i < powerUps.Length; i++)
            {
                onMapPowerUps.Add(powerUps[i].powerUp.GetType(), 0);
            }
            inactivePowerUps.UnionWith(powerUpInstances);

            StartCoroutine(PowerUpUpdator());
        }

        private IEnumerator PowerUpUpdator()
        {
            yield return new WaitForSeconds(powerUpStartOfRoundDelay);
            while (true)
            {
                if (activePowerUps.Count < maxPowerUps && inactivePowerUps.Count != 0)
                {
                    MapPowerUp[] inactivePowerUps = this.inactivePowerUps.ToArray();
                    MapPowerUp closestPoint;
                    if (UnityEngine.Random.value < favourDamagedMechWeight && gameArbiter.GetLowestHealthMechPosition(out Vector3 targetPos))
                    {
                        closestPoint = inactivePowerUps[0];
                        float dst = math.distancesq(targetPos, closestPoint.transform.position);
                        for (int i = 1; i < inactivePowerUps.Length; i++)
                        {
                            float dst2 = math.distancesq(targetPos, inactivePowerUps[i].transform.position);
                            if (dst2 < dst)
                            {
                                dst = dst2;
                                closestPoint = inactivePowerUps[i];
                            }
                        }
                    }
                    else
                    {
                        closestPoint = inactivePowerUps[UnityEngine.Random.Range(0, inactivePowerUps.Length)];
                    }

                    if (closestPoint != null && !closestPoint.mechInTrigger)
                    {
                        closestPoint.SetPowerUp(GetPowerUp());
                    }
                }
                yield return new WaitForSeconds(powerUpUpdateRate);
            }
        }

        public PowerUpCore GetPowerUp()
        {
            List<Type> types = new(onMapPowerUps.Keys);
            PowerUpCore lowestType = null;
            int count = int.MaxValue;
            Type type = null;

            if (UnityEngine.Random.value < favourMostAvaliablePowerUpWeight)
            {
                for (int i = 0; i < types.Count; i++)
                {
                    if (onMapPowerUps[types[i]] <= count && onMapPowerUps[types[i]] < powerUps[i].limit + globalCountOffset)
                    {
                        type = types[i];
                        lowestType = powerUps[i].powerUp;
                        count = onMapPowerUps[types[i]];
                    }
                }
            }
            else
            {
                int safety = 0;
                while (lowestType == null && safety < powerUps.Length * 10)
                {
                    int index = UnityEngine.Random.Range(0, types.Count);
                    PowerUpCore powerUp = powerUps[index].powerUp;
                    if (onMapPowerUps[types[index]] < powerUp.limit + globalCountOffset)
                    {
                        type = types[index];
                        lowestType = powerUp;
                    }
                    safety++;
                }
            }
            if (lowestType != null)
            {
                onMapPowerUps[type]++;
            }
            return lowestType;
        }

        public void ReleasePowerUp(Type powerUpType)
        {
            onMapPowerUps[powerUpType] = onMapPowerUps[powerUpType] <= 0 ? 0 : onMapPowerUps[powerUpType] -= 1;
        }
    }
}
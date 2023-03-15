using RedButton.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Burst.Intrinsics.X86.Avx;

namespace RedButton.Mech
{
    [Serializable]
    public struct Colourable
    {
        public MeshRenderer colourableTarget;
        public int materialIndex;
        public bool all;
    }

    /// <summary>
    /// Often listed as CMC this is the main control script for a mech and handles health.
    /// Acts as an interface for movement and weapons
    /// </summary>
    public class CentralMechComponent : MonoBehaviour
    {
        public delegate void MechPassThroughDelegeate(CentralMechComponent cmc);

        [Header("Items that should recieve the player colour")]
        [SerializeField] private Colourable[] colourables;
        [SerializeField] protected Transform animationCentre;
        [SerializeField] private Transform[] weaponOriginPoints;
        private int weaponOriginIndex = 0;

        [Header("Health")]
        public ShieldScript shield;
        [SerializeField] private Texture2D healthBackgroundDeath;
        public Texture2D HealthBackgroundDeath => healthBackgroundDeath;
        [SerializeField] private int minHealth = 0;
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private int health = 100;
        [SerializeField, Tooltip("Use T to increase health, Y to decrease")] private bool debugging = false;
        public float MinHealth => minHealth;
        public float MaxHealth => maxHealth;
        public float Health => health;

        public FloatPassThrough OnHealthChange;
        public MechPassThroughDelegeate OnMechDied;


        [Header("For debug info only, DO NOT set these properries",order = 0)]
        [SerializeField] private PlayerInput inputController;
        [SerializeField] private MovementCore movementCore;
        [SerializeField] private WeaponCore[] weapons;
        [SerializeField] private Collider[] mechColliders;
        public PlayerInput MechInputController => inputController;
        public MovementCore MechMovementCore => movementCore;
        public Collider[] MechColliders => mechColliders;
        public Color MechAccentColour => inputController.playerColour;
        public bool ShieldActive => shield != null && shield.ShieldActive;

        private void Awake()
        {
            if (MechInputController== null)
            {
                Debug.LogErrorFormat("Critical Error Mech {0}, CMC trying to initilise with no Player Input Script assigned!",gameObject.name);
                Destroy(this);
                return;
            }
            movementCore = GetComponentInChildren<MovementCore>();

            mechColliders = GetComponentsInChildren<Collider>();
            weapons = GetComponentsInChildren<WeaponCore>();

            for (int i = 0; i < weapons.Length; i++)
            {
                if (weapons[i] is ShieldScript shield)
                {
                    this.shield = shield;
                }
            }

            if(movementCore == null)
            {
                Debug.LogErrorFormat("Mech {0}, belonging to player {1} is missing movement!", gameObject.name, inputController.Player);
            }

            if(weapons == null || weapons.Length == 0 )
            {
                Debug.LogErrorFormat("Mech {0}, belonging to player {1} is missing weapons!", gameObject.name, inputController.Player);
            }
        }

        private void Start()
        {
            OnHealthChange += OnHealthChanged;
            for (int i = 0; i < colourables.Length; i++)
            {
                if (colourables[i].all)
                {
                    for (int m = 0; i < colourables[i].colourableTarget.materials.Length; m++)
                    {
                        colourables[i].colourableTarget.materials[m].SetColor("_BaseColor", MechAccentColour);
                    }
                }
                else
                {
                    colourables[i].colourableTarget.materials[colourables[i].materialIndex].SetColor("_BaseColor", MechAccentColour);
                }
                
            }
        }

        private void Update()
        {
            Vector3 lookTarget = MechMovementCore.TargetPoint.position;
            lookTarget.y = animationCentre.position.y;
            animationCentre.LookAt(lookTarget);
#if UNITY_EDITOR
            if (debugging)
            {
                if (Input.GetKeyDown(KeyCode.T))
                {
                    UpdateHealth(10);
                }
                if (Input.GetKeyDown(KeyCode.Y))
                {
                    UpdateHealth(-10);
                }
            }
#endif
        }

        public Transform GetNextWeaponOrigin()
        {
            Transform origin = weaponOriginPoints[weaponOriginIndex];
            weaponOriginIndex = (weaponOriginIndex + 1) % weaponOriginPoints.Length;
            return origin;
        }

        public void ResetWeaponOriginIndex()
        {
            weaponOriginIndex = 0;
        }

        public void AssignInputController(PlayerInput inputController)
        {
            this.inputController = inputController;
        }

        public void UpdateHealth(int damage)
        {
            if (damage != 0)
            {
                health -= damage;
                health = Mathf.Clamp(health, minHealth, maxHealth);
                OnHealthChange?.Invoke(health);
            }
        }

        private void OnHealthChanged(float newValue)
        {
            if (newValue <= 0)
            {
                // we died
                Die();
            }
        }

        private void Die()
        {
            OnMechDied?.Invoke(this);
            MechInputController.Disable();
            transform.root.gameObject.SetActive(false);
        }
    }
}
using RedButton.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace RedButton.Mech
{
    [Serializable]
    public struct Colourable
    {
        public Renderer colourableTarget;
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
        [SerializeField] private Renderer[] fixedColourables;
        [SerializeField] private DecalProjector[] decals;

        [Header("AimPoint")]
        [SerializeField] private SpriteRenderer aimPointSR;
        [SerializeField] protected Sprite[] aimPointSprites;
        [Header("Transform points")]
        [SerializeField] private Transform animationCentre;
        [SerializeField] private Transform[] weaponOriginPoints;
        private int weaponOriginIndex = 0;

        [Header("Health")]
        public GameObject deathExplodePrefab;
        public ShieldScript shield;
        [SerializeField] private Texture2D healthBackgroundDeath;
        public Texture2D HealthBackgroundDeath => healthBackgroundDeath;
        [SerializeField] private int minHealth = 0;
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private int health = 100;
        [SerializeField, Tooltip("Use T to increase health, Y to decrease")] private bool debugging = false;
        [SerializeField] private Color flashColour = Color.yellow;
        [SerializeField] private float flashTime = 1.0f;
        public float MinHealth => minHealth;
        public float MaxHealth => maxHealth;
        public float Health => health;

        [Header("Player Stats")]
        public MechResults stats=new();

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
            aimPointSR.sprite =  aimPointSprites[(int)MechInputController.Player];
            stats.player = string.Format("Player {0}", ((int)MechInputController.Player) + 1);
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

        private void OnEnable()
        {
            SetMechColour(MechAccentColour);
            SetFixedMechColours(MechAccentColour);
        }

        private void Start()
        {
            OnHealthChange += OnHealthChanged;
        }

        private void SetFixedMechColours(Color colour)
        {
            for (int i = 0; i < fixedColourables.Length; i++)
            {
                if (fixedColourables[i] is SpriteRenderer spriteRenderer)
                {
                    spriteRenderer.color = colour;
                    spriteRenderer.material.SetTexture("_UnlitColorMap", spriteRenderer.sprite.texture);
                }
                fixedColourables[i].material.SetColor("_BaseColor", colour);
                fixedColourables[i].material.SetColor("_UnlitColor", colour);

            }

            for (int i = 0; i < decals.Length; i++)
            {
                decals[i].material = new Material(decals[i].material);
                decals[i].material.SetColor("_BaseColor", colour);
                // decals[i].material.SetColor("_EmissiveColorHDR", colour * 1.5f);
            }
        }

        private void SetMechColour(Color colour)
        {
            for (int i = 0; i < colourables.Length; i++)
            {
                if (colourables[i].all)
                {
                    for (int m = 0; i < colourables[i].colourableTarget.materials.Length; m++)
                    {
                        colourables[i].colourableTarget.materials[m].SetColor("_BaseColor", colour);
                    }
                }
                else
                {
                    colourables[i].colourableTarget.materials[colourables[i].materialIndex].SetColor("_BaseColor", colour);
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

        private void OnDisable()
        {
            if (MechInputController)
            {
                MechInputController.StopRumbleMotor(RumbleMotor.Both);
            }
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

        private IEnumerator Flashmech()
        {
            SetMechColour(flashColour);
            yield return new WaitForSeconds(flashTime);
            SetMechColour(MechAccentColour);
        }

        public void UpdateHealth(int damage)
        {
            if (damage != 0)
            {
                health -= damage;
                health = Mathf.Clamp(health, minHealth, maxHealth);
                OnHealthChange?.Invoke(health);
            }
            if(damage > 0)
            {
                stats.damageRecieved += damage;
                stats.hitsTaken++;
            }
        }

        private void OnHealthChanged(float newValue)
        {
            MechInputController.RumbleMotor(flashTime, 0.25f, RumbleMotor.Both);
            StartCoroutine(Flashmech());
            if (newValue <= 0)
            {
                // we died
                Die();
            }
        }

        public void Revive()
        {
            UpdateHealth(-maxHealth);
        }

        private void Die()
        {
            Destroy(Instantiate(deathExplodePrefab, transform.position, Quaternion.identity), 5f);
            Debug.Log("Died", gameObject);
            Debug.LogFormat("Death Time {0}", Time.realtimeSinceStartup);
            MechInputController.Disable();
            transform.root.gameObject.SetActive(false);
            OnMechDied?.Invoke(this);
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RedButton.Core;
using RedButton.Mech;
using RedButton.Core.UI;

namespace RedButton.GamePlay
{
    public class GameArbiter : MonoBehaviour
    {
        public static CentralMechComponent[] mechsToSpawn;

        [SerializeField] private ControlArbiter controlArbiterPrefab;
        [SerializeField] private int playerCount;
        [SerializeField] private CentralMechComponent[] editorMechs;
        [SerializeField] private Vector3[] spawnPoints = new Vector3[4];
        private readonly HashSet<int> usedSpawnPoints = new();
        [SerializeField] private List<CentralMechComponent> activeMechs = new();
        [SerializeField] private Stack<CentralMechComponent> deathOrder = new();
        
        [SerializeField] private bool roundStarted = false;
        public bool RoundStarted => roundStarted;
        public Pluse OnActiveSceneChanged;

        [SerializeField] private PowerUpsManager powerUpsManager;

        private void Awake()
        {
            powerUpsManager = FindObjectOfType<PowerUpsManager>();
            roundStarted = false;
            if (ControlArbiter.Instance == null)
            {
                Debug.Log("Missing Control Arbiter, creating one for hot start...");
                Instantiate(controlArbiterPrefab);
            }
            ControlArbiter.Instance.OnPauseMenuQuit += EndRound;
        }

        private void Start()
        {
            Debug.Log("Starting Game Arbiter...");
            playerCount = ((int)ControlArbiter.playerMode) + 1;
            if(mechsToSpawn == null ||mechsToSpawn.Length == 0)
            {
                List<CentralMechComponent> internalSpawns = new();
                for (int i = 0; i < playerCount; i++)
                {
                    internalSpawns.Add(editorMechs[i]);
                }
                mechsToSpawn = internalSpawns.ToArray();
            }
            SpawnMechs();
            ControlArbiter.Instance.MainUIController.SetPlayers(activeMechs);
            StartRound();
        }

        private void SpawnMechs()
        {
            if(spawnPoints.Length == 0)
            {
                //Debug.LogException(new System.InvalidOperationException("Spawn points array is empty, cannot spawn any mechs"), gameObject);
                throw new System.InvalidOperationException("Spawn points array is empty, cannot spawn any mechs");
            }
            if(playerCount > spawnPoints.Length)
            {
                Debug.LogWarning("Number of spawn points is lower than the number of players!",gameObject);
            }
            usedSpawnPoints.Clear();
            for (int i = 0; i < mechsToSpawn.Length; i++)
            {
                // Setting the current player's input controller to the target mech prefab's input property is a 
                // wprk around so the has an assigned input controller when it is instantiated.
                // This is to allow awake methods in the newly spawned mech work as expected.
                mechsToSpawn[i].AssignInputController(ControlArbiter.Instance[i]);
                int spawnPointIndex = Random.Range(0,spawnPoints.Length);
                int safety = 0;
                while (usedSpawnPoints.Contains(spawnPointIndex) && safety < 1000)
                {
                    spawnPointIndex = Random.Range(0, spawnPoints.Length);
                    safety++;
                }
                usedSpawnPoints.Add(spawnPointIndex);
                activeMechs.Add(Instantiate(mechsToSpawn[i], spawnPoints[spawnPointIndex], Quaternion.identity));
                activeMechs[^1].OnMechDied += OnMechDeath;

                // To ensure minimal unintended cosnquences, I set the input controller property back to null
                // after the mech gets Instantiated.
                mechsToSpawn[i].AssignInputController(null);
            }
        }

        private void StartRound()
        {
            if (powerUpsManager != null)
            {
                powerUpsManager.SetUpPowerUps();
            }
            deathOrder.Clear();
            ControlArbiter.Instance.ValidateControllersAndPlayers();
            roundStarted= true;
        }

        private void EndRound()
        {
            ControlArbiter.Instance.LockOutAllPlayers();
            roundStarted = false;
            if (activeMechs.Count > 0)
            {
                deathOrder.Push(activeMechs[0]);
                activeMechs.Clear();
            }

            /*
             * Populate a score board in the order defined by the deathOrder stack?
             */

            // display some other ui after some time delay
            // for now lets say this goes back to the level select screen

            ControlArbiter.Instance.MainUIController.EndScreenController.ShowEndScreen();

        }

        private void OnMechDeath(CentralMechComponent cmc)
        {
            if (activeMechs.Contains(cmc))
            {
                activeMechs.Remove(cmc);
                deathOrder.Push(cmc);
                cmc.OnMechDied -= OnMechDeath;
            }

            if (roundStarted && activeMechs.Count <= 1)
            {
                // end round
                EndRound();
            }
        }

        private void OnDestroy()
        {
            ControlArbiter.Instance.OnPauseMenuQuit -= EndRound;
        }
    }
}
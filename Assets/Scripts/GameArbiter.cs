using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RedButton.Core;
using RedButton.Mech;

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
        [SerializeField] private List<CentralMechComponent> spawnedMechs = new();
        [SerializeField] private bool roundStarted = false;
        public bool RoundStarted => roundStarted;
        public Pluse OnActiveSceneChanged;

        [SerializeField] private PowerUpsManager powerUpsManager;
        [SerializeField] private int roundCount = 3;
        [SerializeField] private int currentRound = 1;
        private readonly Dictionary<int, int> playerVictories = new();
        private string lastRoundWinner;

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
            PrepGame();

            List<int> targetPlayers = new();

            activeMechs.ForEach(mech => targetPlayers.Add((int)mech.MechInputController.Player));
            activeMechs.Clear();
            StartRoundWithOptions("", targetPlayers);
            ControlArbiter.Instance.UITranslator.EndScreenUI.OverrideText(string.Format("Round {0} of {1}", currentRound, roundCount), "");
        }

        private void SpawnMechs()
        {
            if(spawnPoints.Length == 0)
            {
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
                activeMechs.Add(Instantiate(mechsToSpawn[i], Vector3.zero, Quaternion.identity));
                // activeMechs[^1].OnMechDied += OnMechDeath;
                spawnedMechs.Add(activeMechs[^1]);

                // To ensure minimal unintended cosnquences, I set the input controller property back to null
                // after the mech gets Instantiated.
                mechsToSpawn[i].AssignInputController(null);
            }
        }

        private void PrepGame()
        {
            roundCount = PersistantOptions.instance.userSettings.roundCount;
            for (int i = 0; i < playerCount; i++)
            {
                playerVictories.Add(i, 0);
            }

            deathOrder.Clear();
            ControlArbiter.Instance.ValidateControllersAndPlayers();

            activeMechs.ForEach(mech => { mech.MechInputController.Disable();
                mech.transform.root.gameObject.SetActive(false);
            });
        }

        private void EndRound()
        {
            ControlArbiter.Instance.LockOutAllPlayers();
            roundStarted = false;
            while (activeMechs.Count > 0)
            {
                deathOrder.Push(activeMechs[0]);
                activeMechs.RemoveAt(0);
            }
            activeMechs.Clear();

            playerVictories[(int)deathOrder.Peek().MechInputController.Player] += 1;
            lastRoundWinner = string.Format("Player {0} ", ((int)deathOrder.Peek().MechInputController.Player)+1);
            currentRound += 1;
            bool anyoneCanWin = PossibleForAnyPlayerVictory();
            // end game if rounds finished
            if (currentRound > roundCount)
            {
                bool tie = NeedTie();
                if (tie)
                {
                    TieBreaker();
                    return;
                }

                int winner = 0;
                int wins = 0;

                for (int i = 0; i < playerCount; i++)
                {
                    if(playerVictories[i] > wins)
                    {
                        wins = playerVictories[i];
                        winner = i;
                    }
                }

                lastRoundWinner = string.Format("Player {0} ", (winner + 1).ToString());
                ControlArbiter.Instance.UITranslator.EndScreenUI.OpenEndofGame(lastRoundWinner);
                return;
            }
            List<int> targetPlayers = new();

            Debug.LogFormat("Prep Restart Round Time {0}", Time.realtimeSinceStartup);
            while (deathOrder.Count > 0)
            {
                CentralMechComponent mech = deathOrder.Pop();
                targetPlayers.Add((int)mech.MechInputController.Player);
            }

            StartRoundWithOptions(string.Format("Round {0} of {1}", currentRound, roundCount), targetPlayers);
            /*
             * Populate a score board in the order defined by the deathOrder stack?
             */

            // display some other ui after some time delay
            // for now lets say this goes back to the level select screen



        }

        private IEnumerator RoundIntro(string name)
        {
            ControlArbiter.Instance.UITranslator.EndScreenUI.OpenNextRound(name, lastRoundWinner, currentRound - 1);

            Debug.LogFormat("Prep round UI interrupt Time {0}", Time.realtimeSinceStartup);
            while (!ControlArbiter.Instance.UITranslator.EndScreenUI.startNextRound)
            {
                yield return null;
            }
            Debug.LogFormat("Starting round Time {0}", Time.realtimeSinceStartup);
            roundStarted = true;
            if (powerUpsManager != null)
            {
                powerUpsManager.SetUpPowerUps();
            }
            // hide round number
            for (int i = 0; i < activeMechs.Count; i++)
            {
                activeMechs[i].MechInputController.Enable();
                activeMechs[i].gameObject.SetActive(true);

                activeMechs[i].OnMechDied += OnMechDeath;
            }
        }

        private void StartRoundWithOptions(string roundName, List<int>targetPlayers)
        {
            
            usedSpawnPoints.Clear();
            deathOrder.Clear();
            int spawnPointIndex = Random.Range(0, spawnPoints.Length);
            int safety = 0;
            for (int i = 0; i < targetPlayers.Count; i++)
            {
                CentralMechComponent target = spawnedMechs[targetPlayers[i]];
                while (usedSpawnPoints.Contains(spawnPointIndex) && safety < 1000)
                {
                    spawnPointIndex = Random.Range(0, spawnPoints.Length);
                    safety++;
                }
                usedSpawnPoints.Add(spawnPointIndex);
                target.transform.position = spawnPoints[spawnPointIndex];
                target.Revive();
                activeMechs.Add(target);
                target.gameObject.SetActive(false);
            }

            StartCoroutine(RoundIntro(roundName));
        }

        private bool NeedTie()
        {
            return false;
        }

        private void TieBreaker()
        {

        }

        private bool PossibleForAnyPlayerVictory()
        {
            return true;
        }

        private void OnMechDeath(CentralMechComponent cmc)
        {
            if (roundStarted)
            {

                if (activeMechs.Contains(cmc))
                {
                    activeMechs.Remove(cmc);
                    deathOrder.Push(cmc);
                    cmc.OnMechDied -= OnMechDeath;
                }
                if (activeMechs.Count <= 1)
                {
                    // end round
                    Debug.LogFormat("End Time {0}", Time.realtimeSinceStartup);
                    EndRound();
                }
            }
            else
            {
                Debug.LogFormat(cmc.gameObject,"Unexpected mech death at {0} ", Time.realtimeSinceStartup);
            }
        }

        private void OnDestroy()
        {
            ControlArbiter.Instance.OnPauseMenuQuit -= EndRound;
        }
    }
}
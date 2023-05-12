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
        public int PlayerCount => playerCount;
        [SerializeField] private CentralMechComponent[] editorMechs;
        [SerializeField] private Vector3[] spawnPoints = new Vector3[4];
        private readonly HashSet<int> usedSpawnPoints = new();
        [SerializeField] private List<CentralMechComponent> activeMechs = new();
        [SerializeField] private Stack<CentralMechComponent> deathOrder = new();
        [SerializeField] private List<CentralMechComponent> spawnedMechs = new();
        [SerializeField] private bool roundStarted = false;
        public bool RoundStarted => roundStarted;
        public Pluse OnActiveSceneChanged;
        public Pluse OnRoundStarted;

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
            OnRoundStarted += TestRound;
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
            if (powerUpsManager != null)
            {
                powerUpsManager.StopAndClear();
            }
            ControlArbiter.Instance.LockOutAllPlayers();
            roundStarted = false;
            while (activeMechs.Count > 0)
            {
                deathOrder.Push(activeMechs[0]);
                activeMechs.RemoveAt(0);
            }
            activeMechs.Clear();

            playerVictories[(int)deathOrder.Peek().MechInputController.Player] += 1;
            deathOrder.Peek().stats.roundsWon += 1;
            lastRoundWinner = string.Format("Player {0} ", ((int)deathOrder.Peek().MechInputController.Player) + 1);
            currentRound += 1;


            // end game if rounds finished
            if (currentRound > roundCount)
            {
                if (TieBreakerRound())
                {
                    PrepareNextRound(string.Format("Tie Breaker Round!"));
                    return;
                }

                List<MechResults> results = new(playerCount);

                for (int i = 0; i < playerCount; i++)
                {
                    results.Add(spawnedMechs[i].stats);
                }
                results.Sort();
                results.Reverse();
                // lastRoundWinner = string.Format("Player {0} ", (winner + 1).ToString());
                ControlArbiter.Instance.UITranslator.EndScreenUI.OpenEndofGame(results);
                return;
            }

            PrepareNextRound(string.Format("Round {0} of {1}", currentRound, roundCount));
        }

        private void PrepareNextRound(string roundName)
        {
            List<int> targetPlayers = new();

            Debug.LogFormat("Prep Restart Round Time {0}", Time.realtimeSinceStartup);
            while (deathOrder.Count > 0)
            {
                CentralMechComponent mech = deathOrder.Pop();
                targetPlayers.Add((int)mech.MechInputController.Player);
            }

            StartRoundWithOptions(roundName, targetPlayers);
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
            activeMechs.ForEach(mech => {
                mech.MechInputController.Disable();
            });
            // hide round number
            for (int i = 0; i < activeMechs.Count; i++)
            {
                activeMechs[i].MechInputController.SetPausingAllowed(true);
                activeMechs[i].MechInputController.Enable();
                activeMechs[i].gameObject.SetActive(true);

                activeMechs[i].OnMechDied += OnMechDeath;
            }
            OnRoundStarted?.Invoke();
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

        private bool TieBreakerRound()
        {
            int mostWins = 0;
            int matches = 0;

            for (int i = 0; i < playerCount; i++)
            {
                if (playerVictories[i] > mostWins)
                {
                    mostWins = playerVictories[i];
                }
            }

            List<int> players = new();

            for (int i = 0; i < playerCount; i++)
            {
                if (playerVictories[i] == mostWins)
                {
                    players.Add(i);
                    matches++;
                }
            }

            bool tie = matches > 1;
            if (tie)
            {
                List<CentralMechComponent> playersMeches = new();
                while (deathOrder.Count > 0)
                {
                    playersMeches.Add(deathOrder.Pop());
                }

                for (int i = 0; i < playersMeches.Count; i++)
                {
                    if (players.Contains((int)playersMeches[i].MechInputController.Player))
                    {
                        deathOrder.Push(playersMeches[i]);
                    }

                }
            }
            return tie;
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


        public void TestRound()
        {
            Debug.Log("Round Started");
        }

        private void OnDestroy()
        {
            ControlArbiter.Instance.OnPauseMenuQuit -= EndRound;
        }

        public bool GetLowestHealthMechPosition(out Vector3 pos)
        {
            pos = Vector3.zero;
            CentralMechComponent lowestedHealthMech = null;
            for (int i = 0; i < activeMechs.Count; i++)
            {
                if (lowestedHealthMech == null || activeMechs[i].Health< lowestedHealthMech.Health)
                {
                    lowestedHealthMech = activeMechs[i];
                }
            }

            if(lowestedHealthMech != null && lowestedHealthMech.Health < lowestedHealthMech.MaxHealth/2)
            {
                pos = lowestedHealthMech.transform.position;
                return true;
            }
            return false;
        }
    }
}
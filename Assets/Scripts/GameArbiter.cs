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
        [SerializeField] private List<CentralMechComponent> activeMechs = new();
        [SerializeField] private Stack<CentralMechComponent> deathOrder = new();
        
        [SerializeField] private bool roundStarted = false;
        public bool RoundStarted => roundStarted;
        public Pluse OnActiveSceneChanged;

        private void Awake()
        {
            roundStarted = false;
            if (ControlArbiter.Instance == null)
            {
                Debug.Log("Missing Control Arbiter, creating one for hot start...");
                Instantiate(controlArbiterPrefab);
            }
        }

        private void Start()
        {
            Debug.Log("Starting Game Arbiter...");
            playerCount = ((int)ControlArbiter.playerMode) + 1;
            mechsToSpawn ??= editorMechs;
            SpawnMechs();
            ControlArbiter.Instance.MainUIController.SetPlayers(activeMechs);
            StartRound();
        }

        private void SpawnMechs()
        {
            for (int i = 0; i < mechsToSpawn.Length; i++)
            {
                // Setting the current player's input controller to the target mech prefab's input property is a 
                // wprk around so the has an assigned input controller when it is instantiated.
                // This is to allow awake methods in the newly spawned mech work as expected.
                mechsToSpawn[i].AssignInputController(ControlArbiter.Instance[i]);

                activeMechs.Add(Instantiate(mechsToSpawn[i], new Vector3(i+0.5f, 1, 0), Quaternion.identity));
                activeMechs[^1].OnMechDied += OnMechDeath;

                // To ensure minimal unintended cosnquences, I set the input controller property back to null
                // after the mech gets Instantiated.
                mechsToSpawn[i].AssignInputController(null);
            }
        }

        private void StartRound()
        {
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
            GameSceneManager.Instance.LoadScene(0);
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
    }
}
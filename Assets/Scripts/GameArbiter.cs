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
        [SerializeField] private ControlArbiter controlArbiterPrefab;
        [SerializeField] private int playerCount;
        [SerializeField] private CentralMechComponent[] editorMechs;
        [SerializeField] private List<CentralMechComponent> activeMechs;
        public static CentralMechComponent[] mechsToSpawn;

        private void Awake()
        {
            if(ControlArbiter.Instance == null)
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
                
                // To ensure minimal unintended cosnquences, I set the input controller property back to null
                // after the mech gets Instantiated.
                mechsToSpawn[i].AssignInputController(null);
            }
        }
    }
}
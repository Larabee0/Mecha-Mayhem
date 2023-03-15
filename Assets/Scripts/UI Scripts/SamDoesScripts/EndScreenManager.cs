using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RedButton.Core.UI
{
    public class EndScreenManager : MonoBehaviour
    {
        [SerializeField] private Text TitleBar;
        [SerializeField] private Text winnerText;
        [SerializeField] private Button nextRoundButton;
        [SerializeField] private Button quitButton;
        [HideInInspector]public bool startNextRound = false;

        public void OpenNextRound(string newRoundName, string lastRoundWinner, int lastRound)
        {
            ControlArbiter.Instance.GiveInputAuthority(0);
            nextRoundButton.gameObject.SetActive(true);
            startNextRound = false;
            winnerText.text = string.Format("Next: {0}",newRoundName);
            TitleBar.text = string.Format("{0} Wins round {1}", lastRoundWinner, lastRound);
            gameObject.SetActive(true);
            EventSystem.current.SetSelectedGameObject(nextRoundButton.gameObject);
        }

        public void OpenEndofGame(string gameWinner)
        {
            ControlArbiter.Instance.GiveInputAuthority(0);
            TitleBar.text = "Victory!";
            winnerText.text = string.Format("{0} Wins Overall!", gameWinner);

            gameObject.SetActive(true);
            nextRoundButton.gameObject.SetActive(false);
            EventSystem.current.SetSelectedGameObject(quitButton.gameObject);
        }

        public void NextRound()
        {
            startNextRound = true;
            gameObject.SetActive(false);
        }

        public void ReturnToLevelSelect()
        {
            ControlArbiter.Instance.MainUIController.StartScreenController.ReturnToLevelSelectScreenFromLevel();
            gameObject.SetActive(false);
            ControlArbiter.Instance.GameArbiterToControlArbiterHandback();
        }
    }
}
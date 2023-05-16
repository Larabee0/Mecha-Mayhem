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

        [SerializeField] private GameObject VictoryStats;
        [SerializeField] private Text[] scoreBoard;

        [HideInInspector] public bool startNextRound = false;

        public void OpenNextRound(string newRoundName, string lastRoundWinner, int lastRound)
        {
            ControlArbiter.Instance.GiveInputAuthority(0);
            nextRoundButton.gameObject.SetActive(true);
            VictoryStats.SetActive(false);
            startNextRound = false;
            winnerText.text = string.Format("Next: {0}",newRoundName);
            this.winnerText.gameObject.SetActive(true);
            TitleBar.text = string.Format("{0} Wins round {1}", lastRoundWinner, lastRound);
            gameObject.SetActive(true);
            nextRoundButton.GetComponentInChildren<Text>().text = "Next Round";
            EventSystem.current.SetSelectedGameObject(nextRoundButton.gameObject);
        }

        public void OverrideText(string winnerText, string titleBarText)
        {
            VictoryStats.SetActive(false);
            this.winnerText.text = winnerText;
            this.winnerText.gameObject.SetActive(true);
            TitleBar.text = titleBarText;
            nextRoundButton.GetComponentInChildren<Text>().text = "Start";
        }

        public void OpenEndofGame(List<MechResults> results)
        {
            ControlArbiter.Instance.GiveInputAuthority(0);
            TitleBar.text = "Victory!";
            winnerText.gameObject.SetActive(false);
            PopulateScoreBoard(results);
            VictoryStats.SetActive(true);
            gameObject.SetActive(true);
            nextRoundButton.gameObject.SetActive(false);
            EventSystem.current.SetSelectedGameObject(quitButton.gameObject);
        }

        private void PopulateScoreBoard(List<MechResults> results)
        {
            for (int i = 0; i < scoreBoard.Length; i++)
            {
                scoreBoard[i].gameObject.SetActive(false);
            }

            for (int row = 0; row < results.Count; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    Text field = GetStatField(col, row);
                    field.gameObject.SetActive(true);
                    field.text = results[row][col];
                }
            }
        }

        private Text GetStatField(int x, int y)
        {
            return scoreBoard[y * 8 + x];
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
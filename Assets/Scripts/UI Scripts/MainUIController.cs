using RedButton.Core;
using RedButton.Mech;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

namespace RedButton.Core.UI
{
    public class MainUIController : MonoBehaviour
    {
        [SerializeField] private UIDocument mainDocument;
        [SerializeField] private VisualTreeAsset startScreenUI;

        private StartScreenUI startScreenController;
        private EndScreenUI endScreenController;
        public EndScreenUI EndScreenController => endScreenController;
        public StartScreenUI StartScreenController => startScreenController;

        private VisualElement RootVisualElenement => mainDocument.rootVisualElement;

        public ControllerInterruptUI controllerInterruptUI;
        public HealthBarsUI[] healthBarsUI = new HealthBarsUI[4];
        private bool StartScene => startScreenUI != null;

        private void Awake()
        {
            mainDocument = GetComponent<UIDocument>();
        }

        private void Start()
        {
            controllerInterruptUI = new ControllerInterruptUI(RootVisualElenement.Q("ControllerInterrupt"));
            endScreenController = new EndScreenUI(RootVisualElenement.Q("EndScreenContainer"), this);
            SetUpHealthBars();
            if (StartScene)
            {
                startScreenController = new StartScreenUI(startScreenUI.Instantiate());
                RootVisualElenement.Add(startScreenController.RootVisualElement);
            }
        }

        #region HealthBars
        private void SetUpHealthBars()
        {
            healthBarsUI[0] = new(RootVisualElenement.Q<ProgressBar>("HBP1"));
            healthBarsUI[1] = new(RootVisualElenement.Q<ProgressBar>("HBP2"));
            healthBarsUI[2] = new(RootVisualElenement.Q<ProgressBar>("HBP3"));
            healthBarsUI[3] = new(RootVisualElenement.Q<ProgressBar>("HBP4"));
            
            HideHealthBars();
        }

        public void HideHealthBars()
        {
            for (int i = 0; i < healthBarsUI.Length; i++)
            {
                healthBarsUI[i].Hide();
            }
        }

        public void SetPlayers(List<CentralMechComponent> players)
        {
            int i = 0;
            for (; i < players.Count; i++)
            {
                healthBarsUI[i].SetPlayer(players[i]);
            }
            for (; i < healthBarsUI.Length; i++)
            {
                healthBarsUI[i].Hide();
            }

            if(startScreenController != null)
            {
                startScreenController.RootVisualElement.style.display = DisplayStyle.None;
            }
        }

        #endregion

        #region Controller Connect/Disconnect
        public void HandleControllerDisconnect(int player, Color colour)
        {
            controllerInterruptUI.Show(player, colour);
        }

        public void HandleControllerReconnect()
        {
            controllerInterruptUI.Hide();
        }

        public void FlashControllerConnected(string controller)
        {
            StartCoroutine(FlashControllerConnectedCoroutine(controller));
        }

        private IEnumerator FlashControllerConnectedCoroutine(string controller)
        {
            controllerInterruptUI.CtrlConnText = controller;
            for (float i = 0; i < 1f; i += Time.deltaTime * 0.5f)
            {
                controllerInterruptUI.CtrlConnFade = i;
                yield return null;
            }
            controllerInterruptUI.CtrlConnFade = 1f;
            yield return new WaitForSeconds(5.5f);

            for (float i = 1f; i > 0f; i -= Time.deltaTime * 2.5f)
            {
                controllerInterruptUI.CtrlConnFade = i;
                yield return null;
            }
            controllerInterruptUI.CtrlConnFade = 0f;
        }
        #endregion
    }
}
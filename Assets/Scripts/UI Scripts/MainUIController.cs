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

        [SerializeField] private HealthBar[] healthBars;

        private StartScreenUI startScreenController;
        public StartScreenUI StartScreenController => startScreenController;

        private VisualElement RootVisualElenement => mainDocument.rootVisualElement;

        public ControllerInterruptUI controllerInterruptUI;
        public bool StartScene = false;

        public bool UIShown
        {
            set
            {
                if(value)
                {
                    RootVisualElenement.style.display = DisplayStyle.Flex;
                }
                else
                {
                    RootVisualElenement.style.display = DisplayStyle.None;
                }
            }
        }

        private void Awake()
        {
            mainDocument = GetComponent<UIDocument>();
        }

        private void Start()
        {
            controllerInterruptUI = new ControllerInterruptUI(RootVisualElenement.Q("ControllerInterrupt"));
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
            for (int i = 0; i < healthBars.Length; i++)
            {
                healthBars[i].Setup();
            }

            HideHealthBars();
        }

        public void HideHealthBars()
        {
            for (int i = 0; i < healthBars.Length; i++)
            {
                healthBars[i].Hide();
            }
        }

        public void SetPlayers(List<CentralMechComponent> players)
        {
            int i = 0;
            for (; i < players.Count; i++)
            {
                SetPlayer((int)players[i].MechInputController.Player, players[i]);
            }
            for (; i < healthBars.Length; i++)
            {
                healthBars[i].Hide();
            }

            if(startScreenController != null)
            {
                startScreenController.RootVisualElement.style.display = DisplayStyle.None;
            }
        }

        public void SetPlayer(int playerIndex,CentralMechComponent player)
        {
            healthBars[playerIndex].SetPlayer(player);
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
using RedButton.Core;
using RedButton.Core.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RedButton.Core.UI
{
    public class OptionsManager : MonoBehaviour
    {
        [SerializeField] private StartMenuManagerScript startMenuManager;

        [SerializeField] private Image backgroundImage;

        [SerializeField] private GameObject mainOptions;
        [SerializeField] private GameObject sensivitiyOptions;
        [SerializeField] private Button sensitivityButton;
        [SerializeField] private Button returnButton;

        [SerializeField] private Text gimmickText;
        [SerializeField] private Slider gimmickDelaySlider;

        [SerializeField] private Text p1Text;
        [SerializeField] private Slider p1SenstivitySlider;
        [SerializeField] private Text p2Text;
        [SerializeField] private Slider p2SenstivitySlider;
        [SerializeField] private Text p3Text;
        [SerializeField] private Slider p3SenstivitySlider;
        [SerializeField] private Text p4Text;
        [SerializeField] private Slider p4SenstivitySlider;
        private bool inGame = false;
        private bool SensitivtyOpen => sensivitiyOptions.activeSelf;

        private int GimmickDisplay { set => gimmickText.text = string.Format("Gimmick Delay: {0} Seconds.", value); }

        private int P1Display { set => p1Text.text = string.Format("Player 1 Sensitivty: {0}%", value); }
        private int P2Display { set => p2Text.text = string.Format("Player 2 Sensitivty: {0}%", value); }
        private int P3Display { set => p3Text.text = string.Format("Player 3 Sensitivty: {0}%", value); }
        private int P4Display { set => p4Text.text = string.Format("Player 4 Sensitivty: {0}%", value); }

        public void LoadFromSettings()
        {
            UserSettingsSaveData settings = PersistantOptions.instance.userSettings;

            p1SenstivitySlider.value = settings.player1Sens * 100;
            p2SenstivitySlider.value = settings.player2Sens * 100;
            p3SenstivitySlider.value = settings.player3Sens * 100;
            p4SenstivitySlider.value = settings.player4Sens * 100;

            gimmickDelaySlider.value = settings.gimmickDelay;
        }


        public void UpdateSettings()
        {
            UserSettingsSaveData settings = PersistantOptions.instance.userSettings;

            settings.player1Sens = p1SenstivitySlider.value / 100;
            settings.player2Sens = p2SenstivitySlider.value / 100;
            settings.player3Sens = p3SenstivitySlider.value / 100;
            settings.player4Sens = p4SenstivitySlider.value / 100;

            settings.gimmickDelay = (int)gimmickDelaySlider.value;
            PersistantOptions.instance.OnUserSettingsChangedData?.Invoke();
        }

        public void Open()
        {
            backgroundImage.enabled = true;
            gameObject.SetActive(true);
            mainOptions.SetActive(true);
            sensivitiyOptions.SetActive(false);
            GimmickDisplay = (int)gimmickDelaySlider.value;
            EventSystem.current.SetSelectedGameObject(sensitivityButton.gameObject);
        }

        private void OpenSensitivty()
        {
            ControlArbiter.Instance.startScreenState = StartScreenState.SenstivityScreen;
            gameObject.SetActive(true);
            mainOptions.SetActive(false);
            sensivitiyOptions.SetActive(true);
            P1Display = (int)p1SenstivitySlider.value;
            P2Display = (int)p2SenstivitySlider.value;
            P3Display = (int)p3SenstivitySlider.value;
            P4Display = (int)p4SenstivitySlider.value;
            EventSystem.current.SetSelectedGameObject(p1SenstivitySlider.gameObject);
            ControlArbiter.Instance.GoForwardToSenstitivty();
        }

        public void OnSliderChange()
        {
            GimmickDisplay = (int)gimmickDelaySlider.value;
            P1Display = (int)p1SenstivitySlider.value;
            P2Display = (int)p2SenstivitySlider.value;
            P3Display = (int)p3SenstivitySlider.value;
            P4Display = (int)p4SenstivitySlider.value;
        }

        public void CloseSensitivty()
        {
            ControlArbiter.Instance.startScreenState = StartScreenState.OptionsMenu;
            Open();
        }

        public void Close()
        {
            gameObject.SetActive(false);
            mainOptions.SetActive(false);
            sensivitiyOptions.SetActive(false);
            UpdateSettings();
            startMenuManager.CloseOptions();
        }

        public void ReturnButtonCallback()
        {
            if (inGame)
            {
                CloseSensitivtiyInGame();
                return;
            }
            if (SensitivtyOpen)
            {
                ControlArbiter.Instance.GoBackToOptionsMain(new());
            }
            else
            {
                Close();
            }
        }

        public void SensitivtyButtonCallback()
        {
            OpenSensitivty();
        }

        public void OpenSensitivityInGame()
        {
            p1SenstivitySlider.gameObject.SetActive(false);
            p2SenstivitySlider.gameObject.SetActive(false);
            p3SenstivitySlider.gameObject.SetActive(false);
            p4SenstivitySlider.gameObject.SetActive(false);
            Slider targetSlider = ControlArbiter.CurrentAuthority.Player switch
            {
                Controller.One => p1SenstivitySlider,
                Controller.Two => p2SenstivitySlider,
                Controller.Three => p3SenstivitySlider,
                Controller.Four => p4SenstivitySlider,
                _ => null,
            };
            targetSlider.gameObject.SetActive(true);
            ControlArbiter.Instance.startScreenState = StartScreenState.SenstivityScreen;
            backgroundImage.enabled = inGame = true;
            gameObject.SetActive(true);
            mainOptions.SetActive(false);
            sensivitiyOptions.SetActive(true);
            P1Display = (int)p1SenstivitySlider.value;
            P2Display = (int)p2SenstivitySlider.value;
            P3Display = (int)p3SenstivitySlider.value;
            P4Display = (int)p4SenstivitySlider.value;
            EventSystem.current.SetSelectedGameObject(targetSlider.gameObject);
        }

        public void CloseSensitivtiyInGame()
        {
            p1SenstivitySlider.gameObject.SetActive(true);
            p2SenstivitySlider.gameObject.SetActive(true);
            p3SenstivitySlider.gameObject.SetActive(true);
            p4SenstivitySlider.gameObject.SetActive(true);
            gameObject.SetActive(false);
            mainOptions.SetActive(false);
            sensivitiyOptions.SetActive(false);
            UpdateSettings();
            ControlArbiter.Instance.UITranslator.PauseMenuUI.Open();
        }
    }
}
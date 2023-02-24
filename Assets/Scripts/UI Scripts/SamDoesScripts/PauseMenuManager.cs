using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RedButton.Core.UI
{
    public class PauseMenuManager : MonoBehaviour
    {
        [SerializeField] GameObject continueButton;
        public void Open()
        {
            gameObject.SetActive(true);
            EventSystem.current.SetSelectedGameObject(continueButton);
        }

        public void ExitButton()
        {
            ControlArbiter.Instance.UnPauseGame();
            ControlArbiter.Instance.OnPauseMenuQuit?.Invoke();
        }

        public void CloseButton()
        {
            ControlArbiter.Instance.UnPauseGame();
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedButton.Core.UI
{
    public class PauseMenuManager : MonoBehaviour
    {
        public void Open()
        {
            gameObject.SetActive(true);
        }

        public void ExitButton()
        {

        }

        public void CloseButton()
        {
            gameObject.SetActive(false);
        }
    }
}
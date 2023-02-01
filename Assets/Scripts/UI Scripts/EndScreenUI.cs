using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace RedButton.Core.UI {
    public class EndScreenUI
    {
        private MainUIController mainUI;
        private VisualElement rootVisualElement;

        public EndScreenUI(TemplateContainer rootVisualElement, MainUIController mainUI)
        {
            this.mainUI = mainUI;
            this.rootVisualElement = rootVisualElement[0];
        }

        public VisualElement RootVisualElement => rootVisualElement;


        public void ReturnToLevelsScreenCallback()
        {
            if(mainUI.StartScreenController == null)
            {
                Debug.LogException(new NullReferenceException("EndScreen 'ReturnToLevelsScreenCallback' cannot handle case where StartScreenController is null"));
                return;
            }
            mainUI.HideHealthBars();
            mainUI.StartScreenController.ReturnToLevelSelectScreenFromLevel();
        }
    }
}
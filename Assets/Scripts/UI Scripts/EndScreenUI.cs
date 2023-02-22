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
        private Button continueButton;
        public VisualElement RootVisualElement => rootVisualElement;

        public EndScreenUI(VisualElement rootVisualElement, MainUIController mainUI)
        {
            this.mainUI = mainUI;
            this.rootVisualElement = rootVisualElement;
            rootVisualElement.style.display = DisplayStyle.None;
            rootVisualElement.Q<Button>("ExitButton").RegisterCallback<ClickEvent>(ev=> Exit());
            continueButton = rootVisualElement.Q<Button>("ContinueButton");
            continueButton.RegisterCallback<ClickEvent>(ev => ReturnToLevelsScreenCallback());
            rootVisualElement.Q<Button>("ExitButton").RegisterCallback<NavigationSubmitEvent>(ev => Exit());
            rootVisualElement.Q<Button>("ContinueButton").RegisterCallback<NavigationSubmitEvent>(ev => ReturnToLevelsScreenCallback());
        }

        public void ShowEndScreen()
        {
            mainUI.HideHealthBars();
            rootVisualElement.style.display = DisplayStyle.Flex;
            continueButton.Focus();
        }

        private void Exit()
        {
            Application.Quit();
        }

        public void ReturnToLevelsScreenCallback()
        {
            if(mainUI.StartScreenController == null)
            {
                Debug.LogException(new NullReferenceException("EndScreen 'ReturnToLevelsScreenCallback' cannot handle case where StartScreenController is null"));
                return;
            }
            mainUI.StartScreenController.ReturnToLevelSelectScreenFromLevel();
            rootVisualElement.style.display = DisplayStyle.None;
            ControlArbiter.Instance.GameArbiterToControlArbiterHandback();
        }
    }
}
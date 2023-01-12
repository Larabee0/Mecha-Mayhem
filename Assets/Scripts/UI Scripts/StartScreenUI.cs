using RedButton.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class StartScreenUI
{
    private VisualElement rootVisualElement;
    public VisualElement RootVisualElement => rootVisualElement;

    public VisualElement startScreen;
    public VisualElement mainMenu;
    public VisualElement ControllerAssignment;

    private ControllerAssignHelper PlayerOneAssign;
    private ControllerAssignHelper PlayerTwoAssign;
    private ControllerAssignHelper PlayerThreeAssign;
    private ControllerAssignHelper PlayerFourAssign;

    public StartScreenUI(TemplateContainer rootVisualElement)
    {
        this.rootVisualElement = rootVisualElement[0];
        startScreen = rootVisualElement.Q("StartScreen");
        mainMenu = rootVisualElement.Q("PlayerModePick");
        ControllerAssignment = rootVisualElement.Q("PlayerAssignment");
        mainMenu.style.display = DisplayStyle.None;
        ControllerAssignment.style.display = DisplayStyle.None;
        startScreen.style.display = DisplayStyle.Flex;

        PlayerOneAssign = new(ControllerAssignment.Q<Label>("OnePlayer"), 1);
        PlayerTwoAssign = new(ControllerAssignment.Q<Label>("TwoPlayer"), 2);
        PlayerThreeAssign = new(ControllerAssignment.Q<Label>("ThreePlayer"), 3);
        PlayerFourAssign = new(ControllerAssignment.Q<Label>("FourPlayer"), 4);

        mainMenu.Q<Button>("OnePlayer").RegisterCallback<ClickEvent>(ev => PlayerSelectCallback(1));
        mainMenu.Q<Button>("OnePlayer").RegisterCallback<NavigationSubmitEvent>(ev => PlayerSelectCallback(1));

        mainMenu.Q<Button>("TwoPlayer").RegisterCallback<ClickEvent>(ev => PlayerSelectCallback(2));
        mainMenu.Q<Button>("TwoPlayer").RegisterCallback<NavigationSubmitEvent>(ev => PlayerSelectCallback(2));

        mainMenu.Q<Button>("ThreePlayer").RegisterCallback<ClickEvent>(ev => PlayerSelectCallback(3));
        mainMenu.Q<Button>("ThreePlayer").RegisterCallback<NavigationSubmitEvent>(ev => PlayerSelectCallback(3));

        mainMenu.Q<Button>("FourPlayer").RegisterCallback<ClickEvent>(ev => PlayerSelectCallback(4));
        mainMenu.Q<Button>("FourPlayer").RegisterCallback<NavigationSubmitEvent>(ev => PlayerSelectCallback(4));
    }

    public void ProgressToPlayerCountPikcer()
    {
        startScreen.style.display = DisplayStyle.None;
        mainMenu.style.display = DisplayStyle.Flex;
    }

    private void PlayerSelectCallback(int playerCount)
    {
        mainMenu.style.display = DisplayStyle.None;
        PlayerOneAssign.Set(ControlArbiter.PlayerOne);
        PlayerOneAssign.SetHidden(DisplayStyle.Flex);

        Queue<ControllerAssignHelper> players = new();
        switch (playerCount)
        {
            case 2:
                players.Enqueue(PlayerTwoAssign);
                PlayerTwoAssign.SetHidden(DisplayStyle.Flex);
                PlayerThreeAssign.SetHidden(DisplayStyle.None);
                PlayerFourAssign.SetHidden(DisplayStyle.None);
                break;
            case 3:
                players.Enqueue(PlayerTwoAssign);
                players.Enqueue(PlayerThreeAssign);
                PlayerTwoAssign.SetHidden(DisplayStyle.Flex);
                PlayerThreeAssign.SetHidden(DisplayStyle.Flex);
                PlayerFourAssign.SetHidden(DisplayStyle.None);
                break;
            case 4:
                players.Enqueue(PlayerTwoAssign);
                players.Enqueue(PlayerThreeAssign);
                players.Enqueue(PlayerFourAssign);
                PlayerTwoAssign.SetHidden(DisplayStyle.Flex);
                PlayerThreeAssign.SetHidden(DisplayStyle.Flex);
                PlayerFourAssign.SetHidden(DisplayStyle.Flex);
                break;
            default:
                PlayerTwoAssign.SetHidden(DisplayStyle.None);
                PlayerThreeAssign.SetHidden(DisplayStyle.None);
                PlayerFourAssign.SetHidden(DisplayStyle.None);
                break;
        }
        ControllerAssignment.style.display = DisplayStyle.Flex;

        ControlArbiter.Instance.StartControllerAssignment(players);
    }

    public class ControllerAssignHelper
    {
        public int playerNum;
        public Label visualElement;

        public ControllerAssignHelper(Label visualElement, int playerNum)
        {
            this.playerNum = playerNum;
            this.visualElement = visualElement;
            visualElement.text = FirstLine(playerNum);
            visualElement.style.color= visualElement.style.color = visualElement.style.borderBottomColor = visualElement.style.borderLeftColor =
                visualElement.style.borderRightColor = visualElement.style.borderTopColor =Color.gray;
            SetHidden(DisplayStyle.None);

        }

        public void Highlight()
        {
            visualElement.style.color = visualElement.style.color = visualElement.style.borderBottomColor = visualElement.style.borderLeftColor =
                visualElement.style.borderRightColor = visualElement.style.borderTopColor = Color.white;

            visualElement.text = string.Format(FirstLine(playerNum), "Press Any Button");
        }

        public void Set(PlayerInput player)
        {
            visualElement.style.color = visualElement.style.borderBottomColor = visualElement.style.borderLeftColor =
                visualElement.style.borderRightColor = visualElement.style.borderTopColor = player.playerColour;
            visualElement.text = string.Format(FirstLine(playerNum), "{0}", player.DeviceName);
        }

        public void SetHidden(DisplayStyle displayStyle)
        {
            visualElement.style.display = displayStyle;
        }


        public static string FirstLine(int playerNum)
        {
            return playerNum switch
            {
                1 => "Player One\n",
                2 => "Player Two\n",
                3 => "Player Three\n",
                4 => "Player Four\n",
                _ => "Invalid Player\n"
            };
        }
    }
}

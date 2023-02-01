using RedButton.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace RedButton.Core.UI
{
    public class StartScreenUI
    {
        private VisualElement rootVisualElement;
        public VisualElement RootVisualElement => rootVisualElement;

        public VisualElement startScreen;
        public VisualElement mainMenu;

        public StartScreenUI(TemplateContainer rootVisualElement)
        {
            this.rootVisualElement = rootVisualElement[0];
            startScreen = rootVisualElement.Q("StartScreen");
            mainMenu = rootVisualElement.Q("PlayerModePick");
            ControllerAssignment = rootVisualElement.Q("PlayerAssignment");
            mechSelectScreen = rootVisualElement.Q("MechSelector");
            
            PlayerCountUIQueries();
            ControllerAssignementUIQueries();
            MechSelectorUIQueries();
            LevelSelectorQueries();

            ShowMainMenu();
        }

        /// <summary>
        /// shows the main start screen
        /// </summary>
        public void ShowMainMenu()
        {
            startScreen.style.display = DisplayStyle.Flex;
            mainMenu.style.display = DisplayStyle.None;
            ControllerAssignment.style.display = DisplayStyle.None;
            mechSelectScreen.style.display = DisplayStyle.None;
            LevelSelectScreen.style.display = DisplayStyle.None;
        }

        #region Player Count Selection
        private void PlayerCountUIQueries()
        {
            PlayerOneAssign = new(ControllerAssignment.Q<Label>("OnePlayer"), Controller.One);
            PlayerTwoAssign = new(ControllerAssignment.Q<Label>("TwoPlayer"), Controller.Two);
            PlayerThreeAssign = new(ControllerAssignment.Q<Label>("ThreePlayer"), Controller.Three);
            PlayerFourAssign = new(ControllerAssignment.Q<Label>("FourPlayer"), Controller.Four);

            mainMenu.Q<Button>("OnePlayer").RegisterCallback<ClickEvent>(ev => PlayerSelectCallback(Controller.One));
            mainMenu.Q<Button>("OnePlayer").RegisterCallback<NavigationSubmitEvent>(ev => PlayerSelectCallback(Controller.One));

            mainMenu.Q<Button>("TwoPlayer").RegisterCallback<ClickEvent>(ev => PlayerSelectCallback(Controller.Two));
            mainMenu.Q<Button>("TwoPlayer").RegisterCallback<NavigationSubmitEvent>(ev => PlayerSelectCallback(Controller.Two));

            mainMenu.Q<Button>("ThreePlayer").RegisterCallback<ClickEvent>(ev => PlayerSelectCallback(Controller.Three));
            mainMenu.Q<Button>("ThreePlayer").RegisterCallback<NavigationSubmitEvent>(ev => PlayerSelectCallback(Controller.Three));

            mainMenu.Q<Button>("FourPlayer").RegisterCallback<ClickEvent>(ev => PlayerSelectCallback(Controller.Four));
            mainMenu.Q<Button>("FourPlayer").RegisterCallback<NavigationSubmitEvent>(ev => PlayerSelectCallback(Controller.Four));
        }

        /// <summary>
        /// shows the player count select screen for choosing between
        /// 1-4 player local co-op
        /// </summary>
        public void ShowPlayerCountPicker()
        {
            startScreen.style.display = DisplayStyle.None;
            mainMenu.style.display = DisplayStyle.Flex;
            ControllerAssignment.style.display = DisplayStyle.None;
            mechSelectScreen.style.display = DisplayStyle.None;
            PlayerOneAssign.SetHidden(DisplayStyle.None);
            PlayerTwoAssign.SetHidden(DisplayStyle.None);
            PlayerThreeAssign.SetHidden(DisplayStyle.None);
            PlayerFourAssign.SetHidden(DisplayStyle.None);
            mainMenu.Q<Button>("TwoPlayer").Focus();
        }

        /// <summary>
        /// Gathers all ui elements to assign players to and triggers the control arbiter to start assignment.
        /// Called by player count select UI and Control Arbiter assignment reset.
        /// </summary>
        /// <param name="playerCount">number of players playing</param>
        /// <param name="existingCheck">check for existing player assignments, default true</param>
        public void PlayerSelectCallback(Controller playerCount, bool existingCheck = true)
        {
            mainMenu.style.display = DisplayStyle.None;
            ControlArbiter.playerMode = playerCount;
            if (existingCheck && TryLoadAssignmentScreenWithCurrent())
            {
                return;
            }
            PlayerOneAssign.SetHidden(DisplayStyle.Flex);
            PlayerTwoAssign.SetHidden(DisplayStyle.None);
            PlayerThreeAssign.SetHidden(DisplayStyle.None);
            PlayerFourAssign.SetHidden(DisplayStyle.None);
            UIAssignPlayerOne();

            Queue<ControllerAssignHelper> players = new();

            if (ControlArbiter.PlayerOne == null)
            {
                players.Enqueue(PlayerOneAssign);
            }
            
            switch (ControlArbiter.playerMode)
            {
                case Controller.Two:
                    players.Enqueue(PlayerTwoAssign);
                    PlayerTwoAssign.SetHidden(DisplayStyle.Flex);
                    break;
                case Controller.Three:
                    players.Enqueue(PlayerTwoAssign);
                    players.Enqueue(PlayerThreeAssign);
                    PlayerTwoAssign.SetHidden(DisplayStyle.Flex);
                    PlayerThreeAssign.SetHidden(DisplayStyle.Flex);
                    break;
                case Controller.Four:
                    players.Enqueue(PlayerTwoAssign);
                    players.Enqueue(PlayerThreeAssign);
                    players.Enqueue(PlayerFourAssign);
                    PlayerTwoAssign.SetHidden(DisplayStyle.Flex);
                    PlayerThreeAssign.SetHidden(DisplayStyle.Flex);
                    PlayerFourAssign.SetHidden(DisplayStyle.Flex);
                    break;
            }
            ControllerAssignment.style.display = DisplayStyle.Flex;
            controllerAssignmentButtonPanel.style.display = DisplayStyle.None;

            ControlArbiter.Instance.StartControllerAssignment(players);
        }
        #endregion

        #region Controller Assignement
        public VisualElement ControllerAssignment;
        public VisualElement controllerAssignmentButtonPanel;

        private ControllerAssignHelper PlayerOneAssign;
        private ControllerAssignHelper PlayerTwoAssign;
        private ControllerAssignHelper PlayerThreeAssign;
        private ControllerAssignHelper PlayerFourAssign;

        private void ControllerAssignementUIQueries()
        {
            controllerAssignmentButtonPanel = ControllerAssignment.Q("ButtonContainer");

            ControllerAssignment.Q<Button>("OkButton").RegisterCallback<ClickEvent>(ev => AcceptControllerAssignmentCallback());
            ControllerAssignment.Q<Button>("OkButton").RegisterCallback<NavigationSubmitEvent>(ev => AcceptControllerAssignmentCallback());

            ControllerAssignment.Q<Button>("ChangeButton").RegisterCallback<ClickEvent>(ev => ChangeControllerAssignmentCallback());
            ControllerAssignment.Q<Button>("ChangeButton").RegisterCallback<NavigationSubmitEvent>(ev => ChangeControllerAssignmentCallback());
        }

        /// <summary>
        /// shows the ok/change controller assignment button panel
        /// </summary>
        public void ShowAssignmentButtonPanel()
        {
            controllerAssignmentButtonPanel.style.display = DisplayStyle.Flex;
            ControllerAssignment.Q<Button>("OkButton").Focus();
        }

        /// <summary>
        /// Change Controller assignment button callback
        /// Starts the process of reassign all players controllers.
        /// </summary>
        private void ChangeControllerAssignmentCallback()
        {
            ControlArbiter.Instance.ResetControllerAssignment();
        }

        /// <summary>
        /// Accpet controller assignment button callback
        /// moves onto next screen (mech selector)
        /// </summary>
        private void AcceptControllerAssignmentCallback()
        {
            //ShowMechSelector();
            ShowLevelSelectScreen();
        }

        /// <summary>
        /// Tries to load exisitng controllers into assignment UI
        /// Called by PlayerSelectcallback when existingcheck = true
        /// Also called when the main player goes back from the mech selector screen
        /// </summary>
        /// <returns>whether existing controllers could be loaded</returns>
        public bool TryLoadAssignmentScreenWithCurrent()
        {
            mechSelectScreen.style.display = DisplayStyle.None;
            PlayerOneAssign.SetHidden(DisplayStyle.None);
            PlayerTwoAssign.SetHidden(DisplayStyle.None);
            PlayerThreeAssign.SetHidden(DisplayStyle.None);
            PlayerFourAssign.SetHidden(DisplayStyle.None);

            bool allExpectPresent = ControlArbiter.playerMode switch
            {
                Controller.Two => UIAssignPlayerOne() && UIAssignPlayerTwo(),
                Controller.Three => UIAssignPlayerOne() && UIAssignPlayerTwo() && UIAssignPlayerThree(),
                Controller.Four => UIAssignPlayerOne() && UIAssignPlayerTwo() && UIAssignPlayerThree() && UIAssignPlayerFour(),
                _ => UIAssignPlayerOne(),
            };
            ControllerAssignment.style.display = DisplayStyle.Flex;
            if (allExpectPresent)
            {
                ShowAssignmentButtonPanel();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Button callback for going back to assignment screen
        /// Tries to load existing player assignments, if unsucessful will trigger a full controller assignment change.
        /// </summary>
        private void BackToAssignmentCallback()
        {
            if (!TryLoadAssignmentScreenWithCurrent())
            {
                // some or all expected controllers missing, rest assignment.
                ChangeControllerAssignmentCallback();
            }
        }

        /// <summary>
        /// Colours UI for player one if player one exists
        /// </summary>
        /// <returns>Wether player one exists</returns>
        private bool UIAssignPlayerOne()
        {
            if (ControlArbiter.PlayerOne != null)
            {
                UIAssignPlayer(ControlArbiter.PlayerOne, PlayerOneAssign);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Colours UI for player two if player two exists
        /// </summary>
        /// <returns>Wether player two exists</returns>
        public bool UIAssignPlayerTwo()
        {
            if (ControlArbiter.PlayerTwo != null)
            {
                UIAssignPlayer(ControlArbiter.PlayerTwo, PlayerTwoAssign);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Colours UI for player three if player three exists
        /// </summary>
        /// <returns>Wether player three exists</returns>
        public bool UIAssignPlayerThree()
        {
            if (ControlArbiter.PlayerThree != null)
            {
                UIAssignPlayer(ControlArbiter.PlayerThree, PlayerThreeAssign);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Colours UI for player four if player four exists
        /// </summary>
        /// <returns>Wether player four exists</returns>
        public bool UIAssignPlayerFour()
        {
            if (ControlArbiter.PlayerFour != null)
            {
                UIAssignPlayer(ControlArbiter.PlayerFour, PlayerFourAssign);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Colours UI for a given player input script
        /// </summary>
        /// <param name="playerInput">Player Input Target</param>
        /// <param name="controllerAssignHelper">UI target</param>
        public void UIAssignPlayer(PlayerInput playerInput, ControllerAssignHelper controllerAssignHelper)
        {
            controllerAssignHelper.SetHidden(DisplayStyle.Flex);
            controllerAssignHelper.Set(playerInput);
        }

        /// <summary>
        /// Helper class to manipulate assignment UI.
        /// </summary>
        public class ControllerAssignHelper
        {
            public Controller playerNum;
            public Label visualElement;

            public ControllerAssignHelper(Label visualElement, Controller playerNum)
            {
                this.playerNum = playerNum;
                this.visualElement = visualElement;
                visualElement.text = FirstLine(playerNum);
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
                // Debug.Log(player.DeviceName);
                visualElement.style.color = visualElement.style.borderBottomColor = visualElement.style.borderLeftColor =
                    visualElement.style.borderRightColor = visualElement.style.borderTopColor = player.playerColour;
                visualElement.text = string.Format("{0}{1}",FirstLine(playerNum), player.DeviceName);
            }

            public void SetHidden(DisplayStyle displayStyle)
            {
                visualElement.style.display = displayStyle;
                visualElement.style.color = visualElement.style.color = visualElement.style.borderBottomColor = visualElement.style.borderLeftColor =
                    visualElement.style.borderRightColor = visualElement.style.borderTopColor = Color.gray;
                visualElement.text = FirstLine(playerNum);
            }


            public static string FirstLine(Controller playerNum)
            {
                return playerNum switch
                {
                    Controller.One => "Player One\n",
                    Controller.Two => "Player Two\n",
                    Controller.Three => "Player Three\n",
                    Controller.Four => "Player Four\n",
                    _ => "Invalid Player\n"
                };
            }
        }
        #endregion

        #region Mech Selector
        public VisualElement mechSelectScreen;

        private void MechSelectorUIQueries()
        {
            mechSelectScreen.Q<Button>("BackToAssignment").RegisterCallback<ClickEvent>(ev => BackToAssignmentCallback());
            mechSelectScreen.Q<Button>("BackToAssignment").RegisterCallback<NavigationSubmitEvent>(ev => BackToAssignmentCallback());
        }

        /// <summary>
        /// Shows mech selector UI
        /// </summary>
        public void ShowMechSelector()
        {
            startScreen.style.display = DisplayStyle.None;
            mainMenu.style.display = DisplayStyle.None;
            ControllerAssignment.style.display = DisplayStyle.None;
            mechSelectScreen.style.display = DisplayStyle.Flex;
        }

        #endregion

        #region Level Selector
        public VisualElement LevelSelectScreen;
        public VisualElement BigPeview;

        public LevelHelper level1;
        public LevelHelper level2;
        public LevelHelper level3;
        public LevelHelper level4;

        private void LevelSelectorQueries()
        {
            LevelSelectScreen = rootVisualElement.Q("LevelSelectScreen");
            BigPeview = LevelSelectScreen.Q("BigPreviewImage");
            level1 = new(LevelSelectScreen.Q("Level1"));
            level2 = new(LevelSelectScreen.Q("Level2"));
            level3 = new(LevelSelectScreen.Q("Level3"));
            level4 = new(LevelSelectScreen.Q("Level4"));

            for (int i = 0; i < 4; i++)
            {
                LevelHelper helper = GetLevelDisplay(i);
                helper.root.RegisterCallback<FocusInEvent>(ev => OnLevelFocusCallback(helper));
                helper.root.RegisterCallback<ClickEvent>(ev=> OnLevelClickCallback(helper));
                helper.root.RegisterCallback<NavigationSubmitEvent>(ev => OnLevelClickCallback(helper));
                helper.Hide();
            }
        }

        public void ReturnToLevelSelectScreenFromLevel()
        {
            GameSceneManager.Instance.OnActiveSceneChanged += ReturnToLevelSelectScreen;
            GameSceneManager.Instance.LoadScene(0);
        }

        private void ReturnToLevelSelectScreen()
        {
            GameSceneManager.Instance.OnActiveSceneChanged -= ReturnToLevelSelectScreen;
            ShowLevelSelectScreen();
            // enable player one UI control
        }

        public void ShowLevelSelectScreen()
        {
            LevelSelectScreen.style.display = DisplayStyle.Flex;
            mainMenu.style.display = DisplayStyle.None;
            ControllerAssignment.style.display = DisplayStyle.None;
            mechSelectScreen.style.display = DisplayStyle.None;
            startScreen.style.display = DisplayStyle.None;
            PopulateLevels();
            level1.root.Focus();
        }

        private void PopulateLevels()
        {
            SceneInfo[] scenes = GameSceneManager.Instance.Scenes;
            int runTo = Mathf.Min(scenes.Length, 4);
            for (int i = 0; i < runTo; i++)
            {
                GetLevelDisplay(i).AssignScene(scenes[i]);
            }
        }

        private void OnLevelFocusCallback(LevelHelper helper)
        {
            if(helper.current.preview != null)
            {
                BigPeview.style.backgroundImage = helper.current.preview;
            }
        }

        private void OnLevelClickCallback(LevelHelper helper)
        {
            GameSceneManager.Instance.LoadScene(helper.current.buildIndex);
        }

        public LevelHelper GetLevelDisplay(int index) => index switch
        {
            0 => level1,
            1 => level2,
            2 => level3,
            3 => level4,
            _ => null,
        };

        public class LevelHelper
        {
            public VisualElement root;
            public VisualElement levelPreviewImage;
            public Label levelName;
            
            public SceneInfo current;

            public LevelHelper(VisualElement root)
            {
                this.root = root;
                levelPreviewImage = root.Q("LevelPreviewImage");
                levelName = root.Q<Label>("LevelName");
            }

            public void Hide()
            {
                root.style.display = DisplayStyle.None;
            }

            public void Show()
            {
                root.style.display= DisplayStyle.Flex;
            }

            public void AssignScene(SceneInfo scene)
            {
                current = scene;
                levelName.text = scene.name;
                levelPreviewImage.style.backgroundImage = scene.preview;
                if (scene.hideInLevelPicker)
                {
                    Hide();
                    return;
                }
                Show();
            }
        }
        #endregion
    }
}
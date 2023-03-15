using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RedButton.Mech;
using System;
using UnityEngine.EventSystems;
using RedButton.GamePlay;

namespace RedButton.Core.UI
{
    [Serializable]
    public struct MechDiscriptor
    {
        public string name;
        public string description;
        public Sprite previewImage;
        public CentralMechComponent prefab;
    }

    [Serializable]
    public struct IntBool
    {
        public int playerIndex;
        public bool right;

        public IntBool(int playerIndex, bool right)
        {
            this.playerIndex = playerIndex;
            this.right = right;
        }
    }

    public class MechSelectorManager : MonoBehaviour
    {
        [SerializeField] private StartMenuManagerScript startMenuManager;
        [Header("UI Objects")]
        [SerializeField] private Button returnButton;
        [SerializeField] private SelectionButtonContainer player1;
        [SerializeField] private SelectionButtonContainer player2;
        [SerializeField] private SelectionButtonContainer player3;
        [SerializeField] private SelectionButtonContainer player4;

        [SerializeField] private Color selectorBackgroundColourInactive;
        [SerializeField] private Color selectorBackgroundColourActive;

        public SelectionButtonContainer this[int index] => index switch
        {
            0 => player1,
            1 => player2,
            2 => player3,
            3 => player4,
            _=> player1
        };

        [Header("Selection Options")]
        [SerializeField] private MechDiscriptor[] mechs;
        [Space(200)]
        [SerializeField] private bool thisDoesNothingPaddingForMechDiscriptors;
        private int mechPrefabIndex;
        private int currentPlayerIndex = 0;

        private List<CentralMechComponent> mechsToSpawn = new();

        private void Start()
        {
            startMenuManager = GetComponentInParent<StartMenuManagerScript>();
        }

        public void LeftCallback(int playerIndex)
        {
            SelectionButtonContainer container = this[playerIndex];
            mechPrefabIndex = (mechPrefabIndex - 1) % mechs.Length;
            mechPrefabIndex = mechPrefabIndex < 0 ? mechs.Length - 1 : mechPrefabIndex;
            container.previewImage.sprite = mechs[mechPrefabIndex].previewImage;

        }

        public void RightCallback(int playerIndex)
        {
            SelectionButtonContainer container = this[playerIndex];
            mechPrefabIndex = (mechPrefabIndex + 1) % mechs.Length;
            container.previewImage.sprite = mechs[mechPrefabIndex].previewImage;
        }

        public void SelectCallback(int playerIndex)
        {
            SelectionButtonContainer container = this[playerIndex];
            container.SetInteractable(false, selectorBackgroundColourInactive);
            mechsToSpawn.Add(mechs[mechPrefabIndex].prefab);

            mechPrefabIndex = 0;

            currentPlayerIndex = (currentPlayerIndex + 1) % (((int)ControlArbiter.playerMode) + 1);
            if (currentPlayerIndex == 0)
            {
                startMenuManager.OpenLvlSelect();
                GameArbiter.mechsToSpawn = mechsToSpawn.ToArray();
                ControlArbiter.Instance.GiveInputAuthority(0);
                ControlArbiter.Instance.GoForwardFromMechSelector();
                return;
            }


            container = this[currentPlayerIndex];
            container.SetInteractable(true, ControlArbiter.Instance[currentPlayerIndex].playerColour);

            ControlArbiter.Instance.GiveInputAuthority(currentPlayerIndex,true);

            EventSystem.current.SetSelectedGameObject(container.selectRight.gameObject);
        }

        public void OpenSelector()
        {
            currentPlayerIndex = 0;
            ControlArbiter.Instance.GiveInputAuthority(0);
            gameObject.SetActive(true);
            player1.SetInteractable(true, ControlArbiter.PlayerOneColour);
            player2.SetInteractable(false, selectorBackgroundColourInactive);
            player3.SetInteractable(false, selectorBackgroundColourInactive);
            player4.SetInteractable(false, selectorBackgroundColourInactive);
            player1.previewImage.sprite = mechs[0].previewImage;
            player2.previewImage.sprite = mechs[0].previewImage;
            player3.previewImage.sprite = mechs[0].previewImage;
            player4.previewImage.sprite = mechs[0].previewImage;
            player1.Container.SetActive(false);
            player2.Container.SetActive(false);
            player3.Container.SetActive(false);
            player4.Container.SetActive(false);

            switch (ControlArbiter.playerMode)
            {
                case Controller.One:
                    player1.Container.SetActive(true);
                    break;
                case Controller.Two:
                    player1.Container.SetActive(true);
                    player2.Container.SetActive(true);
                    break;
                case Controller.Three:
                    player1.Container.SetActive(true);
                    player2.Container.SetActive(true);
                    player3.Container.SetActive(true);
                    break;
                case Controller.Four:
                    player1.Container.SetActive(true);
                    player2.Container.SetActive(true);
                    player3.Container.SetActive(true);
                    player4.Container.SetActive(true);
                    break;
            }


            mechsToSpawn.Clear();
            GameArbiter.mechsToSpawn = null;

            EventSystem.current.SetSelectedGameObject(player1.selectRight.gameObject);
        }

        public void ReturnCallback()
        {
            gameObject.SetActive(false);
            ControlArbiter.Instance.GiveInputAuthority(0);
            ControlArbiter.Instance.GoBackToControllerAssignment(new());
        }

        [Serializable]
        public struct SelectionButtonContainer
        {
            public int playerIndex;
            public Button selectLeft;
            public Button selectRight;
            public Button confirmSelection;
            public Image previewImage;
            public Image backgroundImage;
            public GameObject Container => previewImage.transform.parent.gameObject;
            public string ConfirmText { set => confirmSelection.GetComponentInChildren<Text>().text = value; }

            public void SetInteractable(bool value, Color backgroundColour)
            {
                confirmSelection.interactable = selectRight.interactable = selectLeft.interactable = value;
                backgroundImage.color = backgroundColour;
            }
        }
    }
}
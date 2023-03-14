using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RedButton.Mech;
using System;

namespace RedButton.Core.UI
{
    [Serializable]
    public struct MechDiscriptor
    {
        public string name;
        public string description;
        public Texture2D previewImage;
        public CentralMechComponent prefab;
    }

    public class MechSelectorManager : MonoBehaviour
    {
        [Header("UI Objects")]
        [SerializeField] private Button returnButton;
        [SerializeField] private SelectionButtonContainer player1;
        [SerializeField] private SelectionButtonContainer player2;
        [SerializeField] private SelectionButtonContainer player3;
        [SerializeField] private SelectionButtonContainer player4;
        [Header("Selection Options")]
        [SerializeField] private MechDiscriptor[] mechs;
        [Space(200)]
        [SerializeField] private bool thisDoesNothingPaddingForMechDiscriptors;

        [Serializable]
        public struct SelectionButtonContainer
        {
            public Button selectLeft;
            public Button selectRight;
            public Button confirmSelection;
            public Image previewImage;
            public GameObject Container => previewImage.transform.parent.gameObject;
            public string ConfirmText { set => confirmSelection.GetComponentInChildren<Text>().text = value; }
            public bool Interactable { set { confirmSelection.interactable = selectRight.interactable = selectLeft.interactable = value; } }
        }
    }

}
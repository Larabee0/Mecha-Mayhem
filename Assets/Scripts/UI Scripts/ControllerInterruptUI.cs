using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ControllerInterruptUI
{
    private readonly VisualElement controllerInterruptOverlay;
    private readonly Label playerLabel;
    private readonly Label bigText;
    private readonly Label instructionlabel;
    private readonly Label controllerConnectedText;

    public bool Open => controllerInterruptOverlay.style.display == DisplayStyle.Flex;

    public float CtrlConnFade
    {
        set
        {
            Color currentColour = new(0.8235294f, 0.8235294f, 0.3176471f, Mathf.Clamp01(value));
            if(value < float.Epsilon)
            {
                controllerConnectedText.style.visibility = Visibility.Hidden;
            }
            else
            {
                controllerConnectedText.style.visibility = Visibility.Visible;
            }
            controllerConnectedText.style.color = currentColour;
        }
    }

    public string CtrlConnText
    {
        set
        {
            controllerConnectedText.text = string.Format("Controller Connected: {0}", value);
        }
    }

    private Color LabelColours
    {
        get { return playerLabel.style.color.value; }
        set { playerLabel.style.color = bigText.style.color = instructionlabel.style.color = value; }
    }

    public ControllerInterruptUI(VisualElement controllerInterruptOverlay)
    {
        this.controllerInterruptOverlay = controllerInterruptOverlay;
        playerLabel = controllerInterruptOverlay.Q<Label>("ControllerInfo");
        bigText = controllerInterruptOverlay.Q<Label>("BigLabel");
        instructionlabel = controllerInterruptOverlay.Q<Label>("InfoLabel");
        controllerConnectedText = controllerInterruptOverlay.Q<Label>("ControllerConnected");
        controllerInterruptOverlay.style.display = DisplayStyle.None;
        CtrlConnFade = 0f;
    }

    public void Show(int playerDisconnected,Color? colour)
    {
        controllerInterruptOverlay.style.display = DisplayStyle.Flex;
        if (!colour.HasValue)
        {
            colour= Color.white;
        }

        LabelColours = colour.Value;

        playerLabel.text = string.Format("Player {0} Controller Disconnected", playerDisconnected);
    }

    public void Hide()
    {
        if (Open)
        {
            controllerInterruptOverlay.style.display = DisplayStyle.None;
        }
    }
}

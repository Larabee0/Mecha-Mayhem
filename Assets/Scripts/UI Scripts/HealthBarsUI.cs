using RedButton.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class HealthBarsUI
{
    private readonly ProgressBar healthBar;
    // private MechHealthScript healthScript;
    private readonly VisualElement progressBarBackground;
    private readonly VisualElement progressBarProgress;
    private readonly Label progressBarLabel;

    public Texture2D HealthBarBackground { set => progressBarBackground.style.backgroundImage = value; }
    public Color HealthBarBackgroundTint { set => progressBarBackground.style.unityBackgroundImageTintColor = value; }
    public Color HealthBarProgressColour { set => progressBarProgress.style.backgroundColor = value; }
    public float HealthMin { get => healthBar.lowValue; set => healthBar.lowValue = value; }
    public float HealthMax { get => healthBar.highValue; set => healthBar.highValue = value; }
    public float HealthValue { get => healthBar.value; set => healthBar.value = value; }
    public string HealthBarLabel { get => progressBarLabel.text; set => progressBarLabel.text = value; }
    private string healthBarLabelText;

    public HealthBarsUI(ProgressBar healthBar)
    {
        this.healthBar = healthBar;
        progressBarProgress = healthBar.Q(null, "unity-progress-bar__progress");
        progressBarBackground = healthBar.Q(null, "unity-progress-bar__background");
        progressBarLabel = healthBar.Q<Label>();
        healthBarLabelText = HealthBarLabel;
        Hide();
    }

    public void SetPlayer(PlayerInput player)
    {
        // if(healthScript != null)
        // {
        //     healthScript.OnHealthChange -= OnHealthChange;
        // }
        HealthBarProgressColour = player.playerColour;
        HealthBarBackgroundTint = player.playerColour;
        HealthBarBackground = null;
        HealthBarLabel = healthBarLabelText;
        // if (player.TryGetComponent(out healthScript))
        // {
        //     HealthMin = healthScript.MinHealth;
        //     HealthMax = healthScript.MaxHealth;
        //     HealthValue = healthScript.Health;
        //     healthScript.OnHealthChange += OnHealthChange;
        //     Show();
        // }
        // else
        // {
        //     healthScript = null;
        //     Hide();
        //     Debug.LogWarningFormat("Health Bar UI, bound to player: '{0}', but failed to find MechHealthScript!", player.gameObject.name);
        // }
    }

    public void Show()
    {
        healthBar.style.visibility = Visibility.Visible;
    }

    public void Hide()
    {
        healthBar.style.visibility = Visibility.Hidden;
    }

    private void OnHealthChange(float value)
    {
        HealthValue = value;
        if(value <= 0)
        {
            HealthBarLabel = healthBarLabelText + ": Dead";
            // HealthBarBackground = healthScript.HealthBackgroundDeath;
        }
    }

}

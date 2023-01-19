using RedButton.Core;
using RedButton.Mech;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class HealthBarsUI
{
    private readonly ProgressBar healthBar;
    private CentralMechComponent targetMech;
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

    public void SetPlayer(CentralMechComponent mech)
    {
        targetMech  = mech;
        if(targetMech != null)
        {
            targetMech.OnHealthChange -= OnHealthChange;
        }
        HealthBarProgressColour = targetMech.MechAccentColour;
        HealthBarBackgroundTint = targetMech.MechAccentColour;
        HealthBarBackground = null;
        HealthBarLabel = healthBarLabelText;
        HealthMin = targetMech.MinHealth;
        HealthMax = targetMech.MaxHealth;
        HealthValue = targetMech.Health;
        targetMech.OnHealthChange += OnHealthChange;
        Show();
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
            HealthBarBackground = targetMech.HealthBackgroundDeath;
        }
    }

}

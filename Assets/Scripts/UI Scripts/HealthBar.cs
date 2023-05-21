using RedButton.Mech;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RedButton.Core.UI {
    public class HealthBar : MonoBehaviour
    {
        private CentralMechComponent targetMech;
        [SerializeField] private Text healthLabel;
        [SerializeField] private TextMeshProUGUI weaponLabel;
        [SerializeField] private Slider slider;
        [SerializeField] private Image background;
        [SerializeField] private Image weaponIcon;
        private string weaponInitialText = "";
        public Color BarColour
        {
            get => background.color;
            set => background.color = value;
        }

        public float Value
        {
            set => slider.value = value;
        }

        public string HealthBarLabel
        {
            get { return healthLabel.text; }
            set { healthLabel.text = value; }
        }

        private string healthBarLabelText;
        
        public void Setup()
        {
            healthBarLabelText = HealthBarLabel;
            weaponInitialText = weaponLabel.text;
        }

        public void SetPlayer(CentralMechComponent mech)
        {
            targetMech = mech;
            if (targetMech != null)
            {
                targetMech.OnHealthChange -= OnHealthChange;
                targetMech.OnWeaponChanged -= SetWeapon;
                targetMech.OnWeaponChangedImage -= SetWeapon;
            }
            BarColour = targetMech.MechAccentColour;
            HealthBarLabel = healthBarLabelText;
            slider.minValue = targetMech.MinHealth;
            slider.maxValue = targetMech.MaxHealth;
            Value = targetMech.Health;
            targetMech.OnHealthChange += OnHealthChange;
            targetMech.OnWeaponChanged += SetWeapon;
            targetMech.OnWeaponChangedImage += SetWeapon;
            Show();
        }

        public void SetWeapon(string weapon)
        {
            weaponLabel.text = string.Format("{0} {1}", weaponInitialText, weapon);
        }

        public void SetWeapon(Sprite weapon)
        {
            weaponIcon.sprite = weapon;
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void OnHealthChange(float value)
        {
            Value = value;
        }
    }
}
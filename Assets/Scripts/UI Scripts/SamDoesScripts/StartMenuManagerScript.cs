using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartMenuManagerScript : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] GameObject bgPanel;
    [SerializeField] GameObject btnPanel;
    [SerializeField] GameObject creditsPanel;

    [SerializeField] Sprite bgSprite;
    [SerializeField] Sprite willSprite;

    public void OpenCredits()
    {
        bgPanel.GetComponent<Image>().sprite = willSprite;
        btnPanel.SetActive(false);
        creditsPanel.SetActive(true);
    }
    public void CloseCredits()
    {
        bgPanel.GetComponent<Image>().sprite = bgSprite;
        btnPanel.SetActive(true);
        creditsPanel.SetActive(false);
    }

    public void Quit()
    {
        Debug.Log("Quitting");
        Application.Quit();
    }
}

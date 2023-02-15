using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class StartMenuManagerScript : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] GameObject bgPanel;
    [SerializeField] GameObject bindPanel;
    [SerializeField] GameObject btnPanel;
    [SerializeField] GameObject optionsPanel;
    [SerializeField] GameObject creditsPanel;
    [SerializeField] GameObject willCreditsPanel;
    [SerializeField] GameObject playerNumPanel;
    [SerializeField] GameObject lvlSelectPanel;

    [Header("Buttons")]
    [SerializeField] GameObject btnPanelBtn;
    [SerializeField] GameObject optionsPanelBtn;
    [SerializeField] GameObject creditsPanelBtn;
    [SerializeField] GameObject playerNumPanelBtn;
    [SerializeField] GameObject lvlSelectPanelBtn;

    [SerializeField] EventSystem eventSystem;

    [SerializeField] Sprite bgSprite;
    [SerializeField] Sprite willSprite;


    private void Update()
    {
        if (bindPanel.activeSelf)
        {
            if (Input.anyKeyDown)
            {
                bindPanel.SetActive(false);
                BindController();
            }
        }
    }


    public void BindController()
    {
        btnPanel.SetActive(true);
        eventSystem.SetSelectedGameObject(btnPanelBtn);
        //controller binding goes here
    }



    public void OpenPlayerSelect()
    {
        btnPanel.SetActive(false);
        playerNumPanel.SetActive(true);
        eventSystem.SetSelectedGameObject(playerNumPanelBtn);
    }

    public void ClosePlayerSelect()
    {
        btnPanel.SetActive(true);
        playerNumPanel.SetActive(false);
        eventSystem.SetSelectedGameObject(btnPanelBtn);
    }



    public void OpenLvlSelect(int playerNum)
    {
        playerNumPanel.SetActive(false);
        lvlSelectPanel.SetActive(true);
        Debug.Log(playerNum);
        eventSystem.SetSelectedGameObject(lvlSelectPanelBtn);
    }

    public void lvlBtnClick(int lvl)
    {
        Debug.Log("Selected level is "+lvl);
    }

    public void CloseLvlSelect()
    {
        playerNumPanel.SetActive(true);
        lvlSelectPanel.SetActive(false);
        bgPanel.GetComponent<Image>().sprite = bgSprite;
        eventSystem.SetSelectedGameObject(playerNumPanelBtn);
    }



    public void OpenOptions()
    {
        btnPanel.SetActive(false);
        optionsPanel.SetActive(true);
        eventSystem.SetSelectedGameObject(optionsPanelBtn);
    }
    public void CloseOptions()
    {
        btnPanel.SetActive(true);
        optionsPanel.SetActive(false);
        eventSystem.SetSelectedGameObject(btnPanelBtn);
    }



    public void OpenCredits()
    {
        btnPanel.SetActive(false);
        creditsPanel.SetActive(true);
        eventSystem.SetSelectedGameObject(creditsPanelBtn);
    }
    public void CloseCredits()
    {
        btnPanel.SetActive(true);
        creditsPanel.SetActive(false);
        eventSystem.SetSelectedGameObject(btnPanelBtn);
    }

    //Will Credits
    public void OpenWillCredits()
    {
        bgPanel.GetComponent<Image>().sprite = willSprite;
        btnPanel.SetActive(false);
        willCreditsPanel.SetActive(true);
    }
    public void CloseWillCredits()
    {
        bgPanel.GetComponent<Image>().sprite = bgSprite;
        btnPanel.SetActive(true);
        willCreditsPanel.SetActive(false);
    }



    public void Quit()
    {
        Debug.Log("Quitting");
        Application.Quit();
    }
}

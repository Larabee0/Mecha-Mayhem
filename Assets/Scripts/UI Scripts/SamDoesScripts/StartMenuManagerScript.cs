using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartMenuManagerScript : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] GameObject bgPanel;
    [SerializeField] GameObject bindPanel;
    [SerializeField] GameObject btnPanel;
    [SerializeField] GameObject creditsPanel;
    [SerializeField] GameObject willCreditsPanel;
    [SerializeField] GameObject playerNumPanel;

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
        //controller binding goes here
    }



    public void OpenPlayerSelect()
    {
        btnPanel.SetActive(false);
        playerNumPanel.SetActive(true);
    }

    public void ClosePlayerSelect()
    {
        btnPanel.SetActive(true);
        playerNumPanel.SetActive(false);
    }



    public void OpenOptions()
    {
        
    }
    public void CloseOptions()
    {

    }



    public void OpenCredits()
    {
        btnPanel.SetActive(false);
        creditsPanel.SetActive(true);
    }
    public void CloseCredits()
    {
        btnPanel.SetActive(true);
        creditsPanel.SetActive(false);
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

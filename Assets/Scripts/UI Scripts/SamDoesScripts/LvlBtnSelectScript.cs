using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LvlBtnSelectScript : MonoBehaviour, ISelectHandler
{
    [SerializeField] Image bgPanel;
    [SerializeField] Sprite lvlBgSprite;

    public void OnSelect(BaseEventData eventData)
    {
        bgPanel.sprite = lvlBgSprite;
    }
}

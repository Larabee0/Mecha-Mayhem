using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ColourOnSelect : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    public Selectable source;
    public List<Graphic> targets = new();
    public bool selected = false;

    public void OnSelect(BaseEventData eventData)
    {
        selected = true;
        targets.ForEach(graphic => graphic.color = source.colors.selectedColor);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        selected = false;
        targets.ForEach(graphic => graphic.color = source.colors.normalColor);
    }

    private void FixedUpdate()
    {
        
        if (selected && source.gameObject != EventSystem.current.currentSelectedGameObject && !EventSystem.current.alreadySelecting)
        {
            targets.ForEach(graphic => graphic.color = source.colors.normalColor);

        }
        else if(selected && targets[0].color != source.colors.selectedColor)
        {
            targets.ForEach(graphic => graphic.color = source.colors.selectedColor);
        }
    }
}

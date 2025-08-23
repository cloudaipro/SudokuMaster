using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class RubberButton : Selectable, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        GameEvents.OnClearNumberMethod();
    }   
}

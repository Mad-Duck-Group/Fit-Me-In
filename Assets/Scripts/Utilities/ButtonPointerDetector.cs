using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonPointerDetector : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerDownHandler, IPointerExitHandler, IPointerUpHandler, IPointerMoveHandler
{

    public void OnPointerEnter(PointerEventData eventData)
    {
        SoundManager.Instance.PlaySoundFX(SoundFXTypes.ButtonHover, out _);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        SoundManager.Instance.PlaySoundFX(SoundFXTypes.ButtonClick, out _);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        
    }
}

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Slider))]
public class NonInteractableSlider : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        // 아무것도 안 함 → 클릭 무시
    }

    public void OnDrag(PointerEventData eventData)
    {
        // 아무것도 안 함 → 드래그 무시
    }
}

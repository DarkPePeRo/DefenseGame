using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Slider))]
public class NonInteractableSlider : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        // �ƹ��͵� �� �� �� Ŭ�� ����
    }

    public void OnDrag(PointerEventData eventData)
    {
        // �ƹ��͵� �� �� �� �巡�� ����
    }
}

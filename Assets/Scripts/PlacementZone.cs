using UnityEngine;

public class PlacementZone : MonoBehaviour
{
    public int slotIndex; // ���� ��ȣ (0~3) �����Ϳ��� �Ҵ�
    public GameObject canvas;
    public GameObject placementzoneCanvas;
    public GameObject restartCheckUI;
    private void OnMouseDown()
    {
        Character selected = CharacterSelection.Instance.selectedCharacter;
        if (selected == null) return;

        GameManager.Instance.PlaceCharacterAtSlot(slotIndex, selected.gameObject);
        // ���� �������� ����
        CharacterSelection.Instance.selectedCharacter = null;
        Time.timeScale = 0;
        canvas.SetActive(true);
        placementzoneCanvas.SetActive(false);
        restartCheckUI.SetActive(true);
    }
}

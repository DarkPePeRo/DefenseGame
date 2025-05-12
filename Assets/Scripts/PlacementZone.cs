using UnityEngine;

public class PlacementZone : MonoBehaviour
{
    public int slotIndex; // 슬롯 번호 (0~3) 에디터에서 할당
    public GameObject canvas;
    public GameObject placementzoneCanvas;
    public GameObject restartCheckUI;
    private void OnMouseDown()
    {
        Character selected = CharacterSelection.Instance.selectedCharacter;
        if (selected == null) return;

        GameManager.Instance.PlaceCharacterAtSlot(slotIndex, selected.gameObject);
        // 선택 해제하지 않음
        CharacterSelection.Instance.selectedCharacter = null;
        Time.timeScale = 0;
        canvas.SetActive(true);
        placementzoneCanvas.SetActive(false);
        restartCheckUI.SetActive(true);
    }
}

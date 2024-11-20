using UnityEngine;

public class CharacterUI : MonoBehaviour
{
    public GameObject canvas;
    public GameObject placementzoneCanvas;
    public GameObject characterUI;
    public GameObject characterDetailUI;

    public void OnCharacterButtonClick(int characterIndex)
    {
        canvas.SetActive(false);
        CharacterSelection.Instance.SelectCharacter(characterIndex);
        placementzoneCanvas.SetActive(true);
        characterUI.SetActive(false);
        characterDetailUI.SetActive(false);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public class CharacterDetailUISet
{
    public string characterName; // ex. "archer"
    public GameObject rootObject;
    public CanvasGroup canvasGroup;
}

public class CharacterUIController : MonoBehaviour
{
    [Header("전체 UI")]
    public GameObject mainCanvas;
    public GameObject placementCanvas;
    public GameObject characterSelectionUI;

    [Header("캐릭터 상세 UI")]
    public List<CharacterDetailUISet> detailUISets;

    private Dictionary<string, CharacterDetailUISet> detailUIMap;

    private void Start()
    {
        detailUIMap = new();
        foreach (var set in detailUISets)
        {
            if (set.rootObject == null || set.canvasGroup == null)
            {
                Debug.LogWarning($"[CharacterUIController] {set.characterName} UI 정보 누락");
                continue;
            }
            detailUIMap[set.characterName.ToLower()] = set;
        }
    }

    public void OnCharacterButtonClick(int characterIndex)
    {
        CharacterSelection.Instance.SelectCharacter(characterIndex);

        var selectedCharacter = CharacterSelection.Instance.GetSelectedCharacter();
        if (selectedCharacter == null) return;

        mainCanvas.SetActive(false);
        placementCanvas.SetActive(true);

        if (detailUIMap.TryGetValue(selectedCharacter.name.ToLower(), out var ui))
        {
            ui.rootObject.SetActive(false);
        }
    }

    public void ShowCharacterDetail(string characterName)
    {
        if (TryGetUI(characterName, out var ui))
        {
            StartCoroutine(FadeCanvas(ui.canvasGroup, true));

            Character character = CharacterManager.Instance.GetCharacterByName(characterName);
            if (character != null)
            {
                CharacterManager.Instance.UpdateUI(character);
            }
        }
    }

    public void HideCharacterDetail(string characterName)
    {
        if (TryGetUI(characterName, out var ui))
        {
            StartCoroutine(FadeCanvas(ui.canvasGroup, false));
        }
    }

    private bool TryGetUI(string characterName, out CharacterDetailUISet ui)
    {
        characterName = characterName.ToLower();
        if (!detailUIMap.TryGetValue(characterName, out ui))
        {
            Debug.LogError($"[CharacterUIController] {characterName} UI를 찾을 수 없습니다.");
            return false;
        }
        return true;
    }

    private IEnumerator FadeCanvas(CanvasGroup group, bool fadeIn)
    {
        float duration = 0.2f;
        float elapsed = 0f;

        if (fadeIn) group.gameObject.SetActive(true);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = fadeIn ? (elapsed / duration) : (1 - elapsed / duration);
            group.alpha = Mathf.Clamp01(alpha);
            yield return null;
        }
        if (!fadeIn) group.gameObject.SetActive(false);
    }
}
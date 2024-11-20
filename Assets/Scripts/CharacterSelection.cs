using System.Collections.Generic;
using UnityEngine;

public class CharacterSelection : MonoBehaviour
{
    public static CharacterSelection Instance;
    public GameObject selectedCharacterPrefab; // 선택된 캐릭터 프리팹
    public GameObject selectedInstance = null; // 배치된 인스턴스
    public List<GameObject> availableCharacters; // 배치 가능한 캐릭터 목록

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // 캐릭터 선택 메서드
    public void SelectCharacter(int characterIndex)
    {
        if (characterIndex >= 0 && characterIndex < availableCharacters.Count)
        {
            selectedCharacterPrefab = availableCharacters[characterIndex];
            Debug.Log($"캐릭터 {characterIndex} 선택됨: {selectedCharacterPrefab.name}");
        }
    }

    // 배치된 캐릭터의 인스턴스를 저장
    public void SetSelectedInstance(GameObject instance)
    {
        selectedInstance = instance;
        Debug.Log($"Selected instance: {instance.name}");
    }
}

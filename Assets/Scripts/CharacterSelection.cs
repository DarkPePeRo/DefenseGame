using System.Collections.Generic;
using UnityEngine;

public class CharacterSelection : MonoBehaviour
{
    public static CharacterSelection Instance;
    public GameObject selectedCharacterPrefab; // ���õ� ĳ���� ������
    public GameObject selectedInstance = null; // ��ġ�� �ν��Ͻ�
    public List<GameObject> availableCharacters; // ��ġ ������ ĳ���� ���

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // ĳ���� ���� �޼���
    public void SelectCharacter(int characterIndex)
    {
        if (characterIndex >= 0 && characterIndex < availableCharacters.Count)
        {
            selectedCharacterPrefab = availableCharacters[characterIndex];
            Debug.Log($"ĳ���� {characterIndex} ���õ�: {selectedCharacterPrefab.name}");
        }
    }

    // ��ġ�� ĳ������ �ν��Ͻ��� ����
    public void SetSelectedInstance(GameObject instance)
    {
        selectedInstance = instance;
        Debug.Log($"Selected instance: {instance.name}");
    }
}

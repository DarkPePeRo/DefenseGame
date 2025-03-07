using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private Dictionary<Vector2, GameObject> placedCharacters = new Dictionary<Vector2, GameObject>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
            Destroy(gameObject);
    }
    public void Start()
    {
        PlayFabLogin.Instance.LoadPlacedCharactersFromPlayFab();
    }
    public void PlaceCharacter(Vector2 position, GameObject character)
    {
        if (character == null)
        {
            // ��ġ���� ĳ���� ����
            if (placedCharacters.ContainsKey(position))
            {
                Debug.Log($"ĳ���� ���ŵ�: {position}");
                placedCharacters.Remove(position);
            }
        }
        else
        {
            // ���� ��ġ���� ĳ���� ����
            Vector2? oldPosition = GetCharacterPosition(character);
            if (oldPosition.HasValue)
            {
                Debug.Log($"ĳ���� �̵���: {oldPosition.Value}");
                placedCharacters.Remove(oldPosition.Value);
            }

            // ���ο� ��ġ�� ĳ���� �߰�
            placedCharacters[position] = character;
            Debug.Log($"ĳ���� ��ġ��: {position}");
        }
    }

    public Vector2? GetCharacterPosition(GameObject character)
    {
        foreach (var kvp in placedCharacters)
        {
            if (kvp.Value == character) // ������ ��
            {
                Debug.Log($"ĳ���� ��ġ Ȯ�ε�: {kvp.Key}");
                return kvp.Key;
            }
        }

        Debug.LogWarning($"ĳ���Ͱ� placedCharacters�� �����ϴ�: {character?.name}");
        return null;
    }

    public List<Vector2> GetCharacterPositions(GameObject character)
    {
        List<Vector2> matchingPositions = new List<Vector2>();

        foreach (var kvp in placedCharacters)
        {
            if (kvp.Value.name == character.name) // �̸����� ��
            {
                matchingPositions.Add(kvp.Key);
                Debug.Log($"��Ī�� ĳ���� ��ġ: {kvp.Key}");
            }
        }

        return matchingPositions;
    }

    // Ư�� ��ġ�� ��ġ�� ĳ���͸� ��ȯ
    public GameObject GetPlacedCharacter(Vector2 position)
    {
        if (placedCharacters.ContainsKey(position))
        {
            return placedCharacters[position];
        }
        return null;
    }

    public void DebugPlacedCharacters()
    {
        if (placedCharacters.Count == 0)
        {
            Debug.Log("���� placedCharacters�� ��� �ֽ��ϴ�.");
            return;
        }

        Debug.Log("���� placedCharacters ����:");
        foreach (var kvp in placedCharacters)
        {
            Debug.Log($"��ġ: {kvp.Key}, ĳ����: {kvp.Value?.name}");
        }
    }
    public Dictionary<Vector2, GameObject> GetPlacedCharacters()
    {
        return placedCharacters;
    }
    public void ClearPlacedCharacters()
    {
        if (placedCharacters.Count == 0)
        {
            Debug.Log("placedCharacters�� �̹� ��� �ֽ��ϴ�.");
            return;
        }

        // ��ųʸ��� �����Ͽ� �����ϰ� ��ȸ
        var charactersToClear = new List<KeyValuePair<Vector2, GameObject>>(placedCharacters);

        foreach (var kvp in charactersToClear)
        {
            if (kvp.Value != null)
            {
                Debug.Log($"Destroying GameObject at position: {kvp.Key}, Name: {kvp.Value.name}");
                Destroy(kvp.Value);
            }
            else
            {
                Debug.LogWarning($"Null GameObject found at position: {kvp.Key}");
            }
        }

        placedCharacters.Clear();
        Debug.Log("placedCharacters �ʱ�ȭ �Ϸ�");

        // �ʿ� �� �޸� ���� ȣ��
        Resources.UnloadUnusedAssets();
    }


}

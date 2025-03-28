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
        PlayFabCharacterPlacementService.Load(() => { });
    }
    public void PlaceCharacter(Vector2 position, GameObject character)
    {
        if (character == null)
        {
            // ��ġ���� ĳ���� ����
            if (placedCharacters.ContainsKey(position))
            {
                placedCharacters.Remove(position);
            }
        }
        else
        {
            // ���� �̸� ���� ĳ���Ͱ� �ٸ� ��ġ�� �ִٸ� ����
            var positionsToRemove = new List<Vector2>();
            foreach (var kvp in placedCharacters)
            {
                if (kvp.Value.name.Replace("(Clone)", "").Trim() == character.name.Replace("(Clone)", "").Trim())
                {
                    positionsToRemove.Add(kvp.Key);
                }
            }

            foreach (var pos in positionsToRemove)
            {
                Destroy(placedCharacters[pos]);
                placedCharacters.Remove(pos);
            }

            // �� ��ġ�� ĳ���� ��ġ
            placedCharacters[position] = character;
        }
    }

    public Vector2? GetCharacterPosition(GameObject character)
    {
        foreach (var kvp in placedCharacters)
        {
            // �̸����� �� (Clone ���� �� �� ����)
            if (kvp.Value != null && kvp.Value.name.Replace("(Clone)", "").Trim() == character.name.Replace("(Clone)", "").Trim())
            {
                return kvp.Key;
            }
        }

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

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
            // 위치에서 캐릭터 제거
            if (placedCharacters.ContainsKey(position))
            {
                placedCharacters.Remove(position);
            }
        }
        else
        {
            // 같은 이름 가진 캐릭터가 다른 위치에 있다면 제거
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

            // 새 위치에 캐릭터 배치
            placedCharacters[position] = character;
        }
    }

    public Vector2? GetCharacterPosition(GameObject character)
    {
        foreach (var kvp in placedCharacters)
        {
            // 이름으로 비교 (Clone 제거 후 비교 가능)
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
            if (kvp.Value.name == character.name) // 이름으로 비교
            {
                matchingPositions.Add(kvp.Key);
                Debug.Log($"매칭된 캐릭터 위치: {kvp.Key}");
            }
        }

        return matchingPositions;
    }

    // 특정 위치에 배치된 캐릭터를 반환
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
            Debug.Log("현재 placedCharacters는 비어 있습니다.");
            return;
        }

        Debug.Log("현재 placedCharacters 상태:");
        foreach (var kvp in placedCharacters)
        {
            Debug.Log($"위치: {kvp.Key}, 캐릭터: {kvp.Value?.name}");
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
            Debug.Log("placedCharacters가 이미 비어 있습니다.");
            return;
        }

        // 딕셔너리를 복사하여 안전하게 순회
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
        Debug.Log("placedCharacters 초기화 완료");

        // 필요 시 메모리 정리 호출
        Resources.UnloadUnusedAssets();
    }


}

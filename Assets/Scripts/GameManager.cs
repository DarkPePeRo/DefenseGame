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
            // 위치에서 캐릭터 제거
            if (placedCharacters.ContainsKey(position))
            {
                Debug.Log($"캐릭터 제거됨: {position}");
                placedCharacters.Remove(position);
            }
        }
        else
        {
            // 기존 위치에서 캐릭터 제거
            Vector2? oldPosition = GetCharacterPosition(character);
            if (oldPosition.HasValue)
            {
                Debug.Log($"캐릭터 이동됨: {oldPosition.Value}");
                placedCharacters.Remove(oldPosition.Value);
            }

            // 새로운 위치에 캐릭터 추가
            placedCharacters[position] = character;
            Debug.Log($"캐릭터 배치됨: {position}");
        }
    }

    public Vector2? GetCharacterPosition(GameObject character)
    {
        foreach (var kvp in placedCharacters)
        {
            if (kvp.Value == character) // 참조로 비교
            {
                Debug.Log($"캐릭터 위치 확인됨: {kvp.Key}");
                return kvp.Key;
            }
        }

        Debug.LogWarning($"캐릭터가 placedCharacters에 없습니다: {character?.name}");
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

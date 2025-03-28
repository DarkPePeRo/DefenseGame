using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlacementData
{
    public string characterName;
    public Vector2 position;
}

[Serializable]
public class PlacementSaveData
{
    public List<PlacementData> placements = new();
}

public static class PlayFabCharacterPlacementService
{
    private const string PlacementKey = "PlacedCharacters";

    public static void Save(Dictionary<Vector2, GameObject> placedCharacters)
    {
        PlacementSaveData data = new();
        foreach (var kvp in placedCharacters)
        {
            string name = kvp.Value.name.Replace("(Clone)", "").Trim();
            data.placements.Add(new PlacementData { characterName = name, position = kvp.Key });
        }

        var json = JsonUtility.ToJson(data);
        var request = new UpdateUserDataRequest { Data = new Dictionary<string, string> { { PlacementKey, json } } };
        PlayFabClientAPI.UpdateUserData(request,
            result => Debug.Log("캐릭터 배치 저장 성공"),
            error => Debug.LogError("캐릭터 배치 저장 실패: " + error.GenerateErrorReport()));
    }

    public static void Load(Action onComplete)
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), result => {
            if (result.Data == null || !result.Data.ContainsKey(PlacementKey)) { onComplete?.Invoke(); return; }
            string json = result.Data[PlacementKey].Value;
            PlacementSaveData data = JsonUtility.FromJson<PlacementSaveData>(json);

            GameManager.Instance.ClearPlacedCharacters();

            foreach (var placement in data.placements)
            {
                var prefab = Resources.Load<GameObject>($"Characters/{placement.characterName}");
                if (prefab == null)
                {
                    Debug.LogWarning($"프리팹 {placement.characterName}를 찾을 수 없습니다.");
                    continue;
                }

                GameObject instance = GameObject.Instantiate(prefab, placement.position, Quaternion.identity);
                instance.name = prefab.name;
                CharacterManager.Instance.characters.Add(instance.GetComponent<Character>());
                GameManager.Instance.PlaceCharacter(placement.position, instance);
            }

            Debug.Log("배치 캐릭터 로드 완료");
            onComplete?.Invoke();
        },
        error => {
            Debug.LogError("배치 캐릭터 로드 실패: " + error.GenerateErrorReport());
            onComplete?.Invoke();
        });
    }
}

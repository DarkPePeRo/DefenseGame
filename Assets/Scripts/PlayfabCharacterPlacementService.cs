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
            result => Debug.Log("ĳ���� ��ġ ���� ����"),
            error => Debug.LogError("ĳ���� ��ġ ���� ����: " + error.GenerateErrorReport()));
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
                    Debug.LogWarning($"������ {placement.characterName}�� ã�� �� �����ϴ�.");
                    continue;
                }

                GameObject instance = GameObject.Instantiate(prefab, placement.position, Quaternion.identity);
                instance.name = prefab.name;
                CharacterManager.Instance.characters.Add(instance.GetComponent<Character>());
                GameManager.Instance.PlaceCharacter(placement.position, instance);
            }

            Debug.Log("��ġ ĳ���� �ε� �Ϸ�");
            onComplete?.Invoke();
        },
        error => {
            Debug.LogError("��ġ ĳ���� �ε� ����: " + error.GenerateErrorReport());
            onComplete?.Invoke();
        });
    }
}

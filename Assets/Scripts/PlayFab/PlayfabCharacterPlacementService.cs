using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
[Serializable]
public class PlacementData
{
    public string characterName;
    public int slotIndex;
}

[Serializable]
public class PlacementSaveData
{
    public PlacementData[] placements;
}
public static class PlayFabCharacterPlacementService
{
    private const string PlacementKey = "PlacedCharacters";

    public static void Save(GameObject[] placedCharacters)
    {
        List<PlacementData> placements = new();
        List<CharacterSaveData> progress = new();

        for (int i = 0; i < placedCharacters.Length; i++)
        {
            var obj = placedCharacters[i];
            if (obj == null) continue;

            string cleanName = obj.name.Replace("(Clone)", "").Trim();
            placements.Add(new PlacementData { characterName = cleanName, slotIndex = i });

            var c = obj.GetComponent<Character>();
            if (c != null)
            {
                progress.Add(new CharacterSaveData
                {
                    characterId = cleanName,
                    level = Mathf.Max(c.CurrentLevel, 1)
                });
            }
        }

        var data = new PlacementSaveData { placements = placements.ToArray() };
        string json = JsonUtility.ToJson(data);

        PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string> { { PlacementKey, json } }
        },
        result => Debug.Log("배치 저장 완료"),
        error => Debug.LogError("배치 저장 실패: " + error.GenerateErrorReport()));

        PlayFabCharacterProgressService.Save(progress);
    }

    public static void Load(Action onComplete)
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), result =>
        {
            if (result.Data == null || !result.Data.ContainsKey(PlacementKey))
            {
                onComplete?.Invoke();
                return;
            }

            var json = result.Data[PlacementKey].Value;
            var data = JsonUtility.FromJson<PlacementSaveData>(json);

            GameManager.Instance.ClearPlacedCharacters();

            foreach (var p in data.placements)
            {
                var character = CharacterManager.Instance.GetCharacterByName(p.characterName);
                if (character == null)
                {
                    Debug.LogWarning($"[Placement] 캐릭터 '{p.characterName}' 못 찾음");
                    continue;
                }

                GameManager.Instance.PlaceCharacterAtSlot(p.slotIndex, character.gameObject);

            }

            CharacterManager.Instance.RefreshAllCharacterUI();
            onComplete?.Invoke();
        },
        error =>
        {
            Debug.LogError("배치 로드 실패: " + error.GenerateErrorReport());
            onComplete?.Invoke();
        });
    }
}


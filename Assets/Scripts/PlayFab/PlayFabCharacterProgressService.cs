using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections.Generic;
using UnityEngine;

public static class PlayFabCharacterProgressService
{
    const string SaveKey = "CharacterProgress";
    public static void Save(List<CharacterSaveData> characters)
    {
        string json = JsonUtility.ToJson(new CharacterSaveWrapper { characters = characters });
        PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string> { { SaveKey, json } }
        },
        result => Debug.Log("캐릭터 저장 완료"),
        error => Debug.LogError("캐릭터 저장 실패: " + error.GenerateErrorReport()));
    }
    public static void Load(Action<List<CharacterSaveData>> onLoaded)
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), result =>
        {
            if (result.Data != null && result.Data.ContainsKey(SaveKey))
            {
                string json = result.Data[SaveKey].Value;
                var wrapper = JsonUtility.FromJson<CharacterSaveWrapper>(json);
                onLoaded?.Invoke(wrapper.characters);
            }
            else
            {
                    var defaultData = new List<CharacterSaveData>
                {
                    new CharacterSaveData { characterId = "archer", level = 1 },
                    new CharacterSaveData { characterId = "astro", level = 1 }
                    // 필요한 초기 캐릭터들 여기에 추가
                };
                Save(defaultData);
                onLoaded?.Invoke(defaultData);
            }
        },
        error => { Debug.LogError("로드 실패: " + error.GenerateErrorReport()); onLoaded?.Invoke(new List<CharacterSaveData>()); });
    }
    public static void UpdateLevel(string characterId, int level)
    {
        Load(currentData =>
        {
            var existing = currentData.Find(c => c.characterId == characterId);
            if (existing != null) existing.level = level;
            else currentData.Add(new CharacterSaveData { characterId = characterId, level = level });
            Save(currentData);
        });
    }
}
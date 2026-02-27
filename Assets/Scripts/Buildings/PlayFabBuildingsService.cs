// =======================================
// File: PlayFabBuildingsService.cs (수정)
// Folder: Assets/Scripts/Buildings/
// =======================================
using System;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;

namespace SLG.Buildings
{
    public class PlayFabBuildingsService : MonoBehaviour
    {
        public const string USERDATA_KEY = "Buildings";

        // 핵심: 키가 없었는지 isNew로 알려준다
        public void Load(Action<BuildingsSave, bool> onSuccess, Action<PlayFabError> onError)
        {
            var req = new GetUserDataRequest
            {
                Keys = new List<string> { USERDATA_KEY }
            };

            PlayFabClientAPI.GetUserData(req,
                res =>
                {
                    if (res.Data != null && res.Data.TryGetValue(USERDATA_KEY, out var v))
                    {
                        var save = BuildingsJson.FromJson(v.Value);
                        onSuccess?.Invoke(save, false);
                        return;
                    }

                    // 키 없음(최초 접속)
                    onSuccess?.Invoke(null, true);
                },
                err => onError?.Invoke(err));
        }

        public void Save(BuildingsSave save, Action onSuccess, Action<PlayFabError> onError)
        {
            var json = BuildingsJson.ToJson(save);

            var req = new UpdateUserDataRequest
            {
                Data = new Dictionary<string, string>
                {
                    { USERDATA_KEY, json }
                }
            };

            PlayFabClientAPI.UpdateUserData(req,
                _ => onSuccess?.Invoke(),
                err => onError?.Invoke(err));
        }
    }
}

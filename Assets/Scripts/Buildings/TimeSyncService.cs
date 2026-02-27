// ==================================
// File: TimeSyncService.cs
// Folder: Assets/Scripts/Services/
// ==================================
using System;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;

namespace SLG.Services
{
    public class TimeSyncService : MonoBehaviour
    {
        public bool IsReady => _isReady;
        public long ServerNowUnixSec => _isReady ? (long)(DateTimeOffset.UtcNow.ToUnixTimeSeconds() + _serverDeltaSec) : DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        private bool _isReady;
        private double _serverDeltaSec; // serverNow - localUtcNow

        public event Action<long> OnSynced;

        // 로그인 직후 1회 호출 권장
        public void SyncServerTime(Action onSuccess = null, Action<PlayFabError> onError = null)
        {
            var req = new GetTimeRequest();
            PlayFabClientAPI.GetTime(req,
                res =>
                {
                    // res.Time : DateTime (UTC)
                    long server = new DateTimeOffset(res.Time).ToUnixTimeSeconds();
                    long local = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    _serverDeltaSec = server - local;
                    _isReady = true;

                    OnSynced?.Invoke(ServerNowUnixSec);
                    onSuccess?.Invoke();
                },
                err =>
                {
                    _isReady = false;
                    onError?.Invoke(err);
                });
        }
    }
}

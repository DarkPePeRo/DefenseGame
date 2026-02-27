using PlayFab;
using PlayFab.GroupsModels;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuildManager : MonoBehaviour
{
    public void CreateGuild(string guildName, string iconId)
    {
        var request = new CreateGroupRequest
        {
            GroupName = null // 捞抚篮 流立 包府
        };

        PlayFabGroupsAPI.CreateGroup(request,
            result =>
            {
                string groupId = result.Group.Id;
                Debug.Log($"[Guild] Created GroupId: {groupId}");

            },
            error =>
            {
                Debug.LogError(error.GenerateErrorReport());
            });
    }

}

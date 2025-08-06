using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDK3.Data;
using VRC.Udon;
using VRCLinking.Modules;


public class VrcLinkingGraphModule : VrcLinkingModuleBase
{
    public override string ModuleName => "VrcLinkingGraphModule";
    [NonSerialized] public DataDictionary downloadedData;
    [NonSerialized] public DataDictionary guildUsers;

    [Header("Event sending")]
    public UdonBehaviour[] eventReceivers;
    public string eventName;
        
    void SendEventToEventReceivers()
    {
        foreach (var ub in eventReceivers)
        {
            ub.SendCustomEvent(eventName);
        }
    }

    public override void OnDataLoaded()
    {
        downloadedData = downloader.parsedData;
        
        if (!downloadedData.ContainsKey("GuildUsers") && !downloadedData.ContainsKey("GuildUsers"))
        {
            LogError("Missing required key in JSON: GuildUsers");
            return;
        }
        
        guildUsers = downloadedData["GuildUsers"].DataDictionary;
        
        SendEventToEventReceivers();
    }
    
}

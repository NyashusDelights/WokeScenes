using Dalamud.Configuration;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;
using Dalamud.Plugin.Services;

namespace WokeScenes;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public Dictionary<ulong, CharConfiguration> CharacterConfigs { get; set; } = new();
    
    [NonSerialized]
    private DalamudPluginInterface? PluginInterface;
    [NonSerialized]
    private IClientState? ClientState;

    public void Initialize(DalamudPluginInterface pluginInterface, IClientState clientState)
    {
        PluginInterface = pluginInterface;
        ClientState = clientState;
    }

    public void Save()
    {
        PluginInterface!.SavePluginConfig(this);
    }

    public CharConfiguration? GetConfigForCurrentChar()
    {
        if (!ClientState!.IsLoggedIn)
        {
            // Can't load config if not logged in
            return null;
        }

        CharConfiguration charConf;
        ulong charId = ClientState!.LocalContentId;
        
        try
        {
            charConf = CharacterConfigs[charId];
        }
        catch (KeyNotFoundException)
        {
            charConf = new CharConfiguration();
            CharacterConfigs[charId] = charConf;
        }

        return charConf;
    }
}

[Serializable]
public class CharConfiguration
{
    public bool SetRace { get; set; } = false;
    public int RaceOverride { get; set; } = 1;

    public bool SetGender { get; set; } = false;
    public int GenderOverride { get; set; } = 1;
    
    public bool SetCharName { get; set; } = false;
    public string CharForename { get; set; } = "";
    public string CharSurname { get; set; } = "";
}

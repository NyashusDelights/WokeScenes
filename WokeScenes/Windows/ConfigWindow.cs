using System;
using System.Linq;
using System.Numerics;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using ImGuiNET;

namespace WokeScenes.Windows;

public class ConfigWindow : Window, IDisposable
{
    private Configuration Configuration;
    private IClientState ClientState;

    private PlayerParameters PlayerParameters;
    // private ExcelSheet<Race> Races;

    // These are defined in the same order as the game sheets
    private static readonly string[] Races = { "Hyur", "Elezen", "Lalafell", "Miqo'te", "Roegadyn", "Au Ra", "Hrothgar", "Viera" };
    private static readonly string[] Genders = { "Male", "Female" };

    private bool Dirty;

    // We give this window a constant ID using ###
    // This allows for labels being dynamic, like "{FPS Counter}fps###XYZ counter window",
    // and the window ID will always be "###XYZ counter window" for ImGui
    public ConfigWindow(Plugin plugin) : base("WokeScenes Configuration###WokeScenes Conf Window")
    {
        Size = new Vector2(500, 350);
        SizeCondition = ImGuiCond.Always;

        Configuration = plugin.Configuration;
        ClientState = plugin.ClientState;
        PlayerParameters = plugin.PlayerParameters;

        Dirty = false;
        // Races = plugin.DataManager.GetExcelSheet<Race>()!;
    }

    public void Dispose() { }

    public override void PreDraw() { }

    public override void Draw()
    {
        var charConf = Configuration.GetConfigForCurrentChar();
        if (charConf != null)
        {
            var isValid = true;
            
            var playerName = ClientState.LocalPlayer?.Name.TextValue ?? "Unknown Character (this is bad)";
            ImGui.Text($"Setting overrides for {playerName} (ID {ClientState.LocalContentId})");
            
            // Race
            ImGui.Separator();

            var overrideRace = charConf.SetRace;
            if (ImGui.Checkbox("Override character race", ref overrideRace))
            {
                charConf.SetRace = overrideRace;
                Dirty = true;
            }
            if (overrideRace)
            {
                var raceIndex = charConf.RaceOverride - 1;
                if (ImGui.Combo("New race", ref raceIndex, Races, Races.Length))
                {
                    charConf.RaceOverride = raceIndex + 1;
                    Dirty = true;
                }
            }
            
            // Gender
            ImGui.Separator();
            
            var overrideGender = charConf.SetGender;
            if (ImGui.Checkbox("Override character gender", ref overrideGender))
            {
                charConf.SetGender = overrideGender;
                Dirty = true;
            }
            if (overrideGender)
            {
                var genderValue = charConf.GenderOverride;
                if (ImGui.Combo("New gender", ref genderValue, Genders, Genders.Length))
                {
                    charConf.GenderOverride = genderValue;
                    Dirty = true;
                }
            }
            
            // Char name
            ImGui.Separator();

            var overrideName = charConf.SetCharName;
            if (ImGui.Checkbox("Override character name", ref overrideName))
            {
                charConf.SetCharName = overrideName;
                Dirty = true;
            }
            if (overrideName)
            {
                var forenameValue = charConf.CharForename;
                if (ImGui.InputText("Forename", ref forenameValue, 15))
                {
                    if (ValidateCharName(forenameValue))
                    {
                        charConf.CharForename = forenameValue;
                        Dirty = true;
                    }
                    else
                    {
                        isValid = false;
                        ImGui.Text("Invalid forename, same restrictions as game apply.");
                    }
                }
            
                var surnameValue = charConf.CharSurname;
                if (ImGui.InputText("Surname", ref surnameValue, 15))
                {
                    if (ValidateCharName(surnameValue))
                    {
                        charConf.CharSurname = surnameValue;
                        Dirty = true;
                    }
                    else
                    {
                        isValid = false;
                        ImGui.Text("Invalid surname, same restrictions as game apply.");
                    }
                }
            
                // Check both names together are valid
                if (!ValidateCharFullName(forenameValue, surnameValue))
                {
                    ImGui.Text("Character name failed validation, check length and invalid characters");
                    isValid = false;
                }
            }
            
            // Save section
            ImGui.Separator();

            if (isValid)
            {
                if (ImGui.Button("Save"))
                {
                    Configuration.Save();
                    Dirty = false;
                    PlayerParameters.ApplyOverrides();
                }
                if (Dirty)
                    ImGui.Text("There are unsaved changes.");
            }
            else
            {
                ImGui.Text("Unable to save until invalid fields are corrected.");
            }
        }
        else
        {
            // Not logged in
            ImGui.Text("Please log in first.");
        }
    }

    private bool ValidateCharName(string name)
    {
        if (name.Length > 15 || name.Length < 2)
            return false;
        if (!name.All(x => char.IsAsciiLetter(x) || x == '-' || x == '\''))
            return false;
        if (!char.IsAsciiLetter(name[0]))
            return false;
        if (name.Contains("--") || name.Contains("-'") || name.Contains("'-"))
            return false;
        return true;
    }

    private bool ValidateCharFullName(string forename, string surname)
    {
        return ValidateCharName(forename) && ValidateCharName(surname) && forename.Length + surname.Length <= 20;
    }
}

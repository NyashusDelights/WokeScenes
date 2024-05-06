using System;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVClientStructs.FFXIV.Component.Text;

namespace WokeScenes;

public class PlayerParameters: IDisposable
{
    private Configuration Configuration { get; init; }
    
    public PlayerParameters(Configuration configuration)
    {
        Configuration = configuration;
    }
    
    public void Dispose() {}
    
    public unsafe void ApplyOverrides()
    {
        var charConfig = Configuration.GetConfigForCurrentChar();
        if (charConfig != null)
        {
            if (charConfig.SetRace)
            {
                int charOverride = charConfig.RaceOverride;
                GetGlobalTextParameter(70)->SetInteger(&charOverride);
            }

            if (charConfig.SetGender)
            {
                int genderOverride = charConfig.GenderOverride;
                GetGlobalTextParameter(3)->SetInteger(&genderOverride);
            }

            if (charConfig.SetCharName)
            {
                var charName = new Utf8String($"{charConfig.CharForename} {charConfig.CharSurname}");
                ReferencedUtf8String* refStr = null;
                ReferencedUtf8String.Create(&refStr, &charName);
                GetGlobalTextParameter(0)->SetReferencedUtf8String(&refStr);
            }
        }
    }

    private unsafe TextParameter* GetGlobalTextParameter(ulong idx)
    {
        // Gets a reference so we can update in-place, StdDeque.Get gets a copy instead
        var textModule = RaptureTextModule.Instance();
        var globalParams = &textModule->TextModule.MacroDecoder.GlobalParameters;
        
        if (idx >= globalParams->MySize)
            throw new IndexOutOfRangeException($"Global TextParameter index out of Range: {idx}");
        
        // For TextParameter StdDeque block size is 1
        var actualIdx = globalParams->MyOff + idx;
        var block = actualIdx & (globalParams->MapSize - 1);
        
        var ptr = globalParams->Map[block];
        if (ptr == null)
            throw new NullReferenceException($"Global parameter pointer in StdDeque was null for idx: {idx}");
        
        return ptr;
    }
}

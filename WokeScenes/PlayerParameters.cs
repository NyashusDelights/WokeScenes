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
                var param = GetGlobalTextParameter(70);
                SetTextParameterInteger(param, charOverride);
            }

            if (charConfig.SetGender)
            {
                int genderOverride = charConfig.GenderOverride;
                var param = GetGlobalTextParameter(3);
                SetTextParameterInteger(param, genderOverride);
            }

            if (charConfig.SetCharName)
            {
                var charName = $"{charConfig.CharForename} {charConfig.CharSurname}";
                var param = GetGlobalTextParameter(0);
                SetTextParameterUtf8String(param, charName);
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

    private static unsafe void SetTextParameterInteger(TextParameter* textParameter, int value)
    {
        if (textParameter->Type != TextParameterType.Integer)
            throw new ArgumentException("TextParameter is not of type int");
        textParameter->IntValue = value;
        textParameter->ValuePtr = &textParameter->IntValue;
    }
    
    private static unsafe void SetTextParameterUtf8String(TextParameter* textParameter, string value)
    {
        if (textParameter->Type != TextParameterType.ReferencedUtf8String)
            throw new ArgumentException("TextParameter is not of type utf8string");
        if (textParameter->ReferencedUtf8StringValue == null)
            throw new NullReferenceException("TextParameter Utf8String object is unset");
        textParameter->ReferencedUtf8StringValue->Utf8String.SetString(value);
        textParameter->ValuePtr = &textParameter->ReferencedUtf8StringValue;
    }
}

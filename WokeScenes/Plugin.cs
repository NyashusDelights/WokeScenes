using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using WokeScenes.Windows;

namespace WokeScenes;

public sealed class Plugin : IDalamudPlugin
{
    private const string CommandName = "/wokescenes";

    private DalamudPluginInterface PluginInterface { get; init; }
    private ICommandManager CommandManager { get; init; }
    public PlayerParameters PlayerParameters;
    public IClientState ClientState { get; init; }
    public IDataManager DataManager { get; init; }
    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("WokeScenes");
    private ConfigWindow ConfigWindow { get; init; }

    public Plugin(
        [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
        [RequiredVersion("1.0")] ICommandManager commandManager,
        [RequiredVersion("1.0")] IClientState clientState,
        [RequiredVersion("1.0")] IDataManager dataManager)
    {
        PluginInterface = pluginInterface;
        CommandManager = commandManager;
        ClientState = clientState;
        DataManager = dataManager;

        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        Configuration.Initialize(PluginInterface, ClientState);

        PlayerParameters = new PlayerParameters(Configuration);

        ConfigWindow = new ConfigWindow(this);
        WindowSystem.AddWindow(ConfigWindow);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnConfShowCommand)
        {
            HelpMessage = "Open the configuration window for WokeScenes"
        });

        PluginInterface.UiBuilder.Draw += DrawUI;
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;

        ClientState.Login += PlayerParameters.ApplyOverrides;
    }

    public void Dispose()
    {
        ClientState.Login -= PlayerParameters.ApplyOverrides;
        
        WindowSystem.RemoveAllWindows();
        ConfigWindow.Dispose();
        PlayerParameters.Dispose();
        CommandManager.RemoveHandler(CommandName);
    }

    private void OnConfShowCommand(string command, string args)
    {
        ToggleConfigUI();
    }

    private void DrawUI() => WindowSystem.Draw();

    public void ToggleConfigUI() => ConfigWindow.Toggle();
}

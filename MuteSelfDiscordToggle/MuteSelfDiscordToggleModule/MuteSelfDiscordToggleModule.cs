using PInvoke;
using VRCOSC.Game.Modules;

namespace VRCOSC.Modules.MuteSelfDiscordToggle;

[ModuleTitle("MuteSelfDiscordToggle")]
[ModuleDescription("Toggles the Discord mic mute state when the MuteSelf parameter changes.")]
[ModuleAuthor("Zex", "https://github.com/Zexxx/VRCOSC-Radio", "https://avatars.githubusercontent.com/u/21136842?v=4&size=64")]
[ModuleGroup(ModuleType.Integrations)]
public sealed class MuteSelfDiscordToggleModule : IntegrationModule
{
    private bool _lastMuteSelfState;

    protected override string TargetProcess => "discord";

    protected override void CreateAttributes()
    {
        CreateParameter<bool>(MuteSelfDiscordToggleParameter.MuteSelf, ParameterMode.Read, "MuteSelf", "Mute Self", "Returns true if the user has muted themselves, false if unmuted");

        RegisterKeyCombination(MuteSelfDiscordToggleParameter.MuteSelf, User32.VirtualKey.VK_RSHIFT, User32.VirtualKey.VK_F20);
    }

    protected override void OnRegisteredParameterReceived(AvatarParameter parameter)
    {
        if (parameter.Name == "MuteSelf")
        {
            bool currentState = parameter.ValueAs<bool>();
            if (currentState != _lastMuteSelfState)
            {
                ExecuteKeyCombination(MuteSelfDiscordToggleParameter.MuteSelf);
                _lastMuteSelfState = currentState;
            }
        }
    }

    private enum MuteSelfDiscordToggleParameter
    {
        MuteSelf
    }
}

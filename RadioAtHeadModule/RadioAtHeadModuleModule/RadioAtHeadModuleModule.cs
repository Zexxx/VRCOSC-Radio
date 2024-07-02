using System;
using System.Threading;
using VRCOSC.Game.Modules;
using VRCOSC.Game.Modules.Avatar;
using VRCOSC.Game.OSC.VRChat;

namespace RadioAtHeadModule
{
    [ModuleTitle("Radio At Head AutoMuter")]
    [ModuleDescription("Automatically mutes voice input when 'RadioAtHead' is true.")]
    [ModuleAuthor("Zex", "https://github.com/Zexxx/VRCOSC-Radio")]
    [ModuleGroup(ModuleType.General)]
    public sealed class RadioAtHeadModule : AvatarModule
    {
        private enum RadioAtHeadParameter
        {
            RadioAtHead,
            MuteSelf
        }

        private Timer? _syncTimer;
        private Timer? _checkTimer;
        private bool _isRadioAtHead;
        private bool _localMuteState;
        private bool _isMuted;

        protected override void CreateAttributes()
        {
            CreateParameter<bool>(RadioAtHeadParameter.RadioAtHead, ParameterMode.Read, "RadioAtHead", "RadioAtHead", "Whether the radio is at head level");
            CreateParameter<bool>(RadioAtHeadParameter.MuteSelf, ParameterMode.Read, "MuteSelf", "MuteSelf", "Whether the user is muted");
        }

        protected override void OnModuleStart()
        {
            base.OnModuleStart();
            // Sync initial mute state
            _localMuteState = _isMuted;
        }

        protected override void OnRegisteredParameterReceived(AvatarParameter parameter)
        {
            if (parameter.Name == "RadioAtHead")
            {
                bool radioAtHead = parameter.ValueAs<bool>();
                HandleRadioAtHeadChange(radioAtHead);
            }
            else if (parameter.Name == "MuteSelf")
            {
                bool muteSelf = parameter.ValueAs<bool>();
                _isMuted = muteSelf;
                Log($"MuteSelf updated: {_isMuted}");
            }
        }

        private void HandleRadioAtHeadChange(bool radioAtHead)
        {
            _isRadioAtHead = radioAtHead;

            if (_isRadioAtHead)
            {
                Log("RadioAtHead is true. Muting voice input...");
                ToggleVoiceInput(true);
                _localMuteState = true;
            }
            else
            {
                Log("RadioAtHead is false. Scheduling to unmute voice input after 150ms...");
                _syncTimer?.Dispose(); // Dispose of any existing timer
                _syncTimer = new Timer(UnmuteIfRadioAtHeadFalse, null, 150, Timeout.Infinite);
            }
        }

        private void ToggleVoiceInput(bool mute)
        {
            if (Player != null)
            {
                if (mute)
                {
                    Player.Mute();
                }
                else
                {
                    Player.UnMute();
                }
                Log($"ToggleVoiceInput: Setting local mute state to {mute}");
                _localMuteState = mute;

                // Check and ensure state sync after 150ms
                _checkTimer?.Dispose();
                _checkTimer = new Timer(CheckAndSyncMuteState, null, 150, Timeout.Infinite);
            }
            else
            {
                Log("Player not found, unable to change mute state.");
            }
        }

        private void UnmuteIfRadioAtHeadFalse(object? state)
        {
            if (!_isRadioAtHead)
            {
                Log("Unmuting voice input...");
                ToggleVoiceInput(false);

                // Schedule to check sync after 150ms
                _checkTimer?.Dispose();
                _checkTimer = new Timer(CheckAndSyncMuteState, null, 150, Timeout.Infinite);
            }
        }

        private void CheckAndSyncMuteState(object? state)
        {
            if (Player != null)
            {
                Log($"CheckAndSyncMuteState: Local mute state: {_localMuteState}, MuteSelf: {_isMuted}");

                if (_localMuteState != _isMuted)
                {
                    Log("Mismatch detected, running mute loop again to sync states...");
                    ToggleVoiceInput(!_localMuteState);
                }
            }
            else
            {
                Log("Player not found, unable to sync mute state.");
            }
        }

        protected override void OnModuleStop()
        {
            base.OnModuleStop();
            Log("OnModuleStop: Cleaning up...");
            // Dispose of the timers if they are still active
            _syncTimer?.Dispose();
            _checkTimer?.Dispose();
        }
    }
}
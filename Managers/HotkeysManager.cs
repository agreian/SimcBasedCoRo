using System.Windows.Forms;
using SimcBasedCoRo.Utilities;
using Styx.Common;
using Styx.WoWInternals;

namespace SimcBasedCoRo.Managers
{
    public static class HotkeysManager
    {
        private const string AOE_ON_KEY = "AoeOn";
        private const string AUTO_KICK_ON_KEY = "AutoKickOn";
        private const string COOLDOWN_ON_KEY = "CooldownOn";
        private const string DEFENSIVE_COOLDOWN_ON_KEY = "DefensiveCooldownOn";

        #region Fields

        private static bool _keysRegistered;

        #endregion

        #region Public Methods

        public static void RegisterHotKeys()
        {
            if (_keysRegistered)
                return;

            Styx.Common.HotkeysManager.Register(AOE_ON_KEY, Keys.A, ModifierKeys.Alt, ret =>
            {
                Spell.UseAoe = !Spell.UseAoe;
                Lua.DoString(Spell.UseAoe ? "print('AoE Mode: Enabled!')" : @"print('AoE Mode: Disabled!')");
            });

            Styx.Common.HotkeysManager.Register(AUTO_KICK_ON_KEY, Keys.R, ModifierKeys.Alt, ret =>
            {
                Spell.UseAutoKick = !Spell.UseAutoKick;
                Lua.DoString(Spell.UseAutoKick ? "print('Auto Kick: Enabled!')" : @"print('Auto Kick: Disabled!')");
            });

            Styx.Common.HotkeysManager.Register(COOLDOWN_ON_KEY, Keys.C, ModifierKeys.Alt, ret =>
            {
                Spell.UseCooldown = !Spell.UseCooldown;
                Lua.DoString(Spell.UseCooldown ? "print('Automatic Cooldown usage: Enabled!')" : @"print('Automatic Cooldown usage: Disabled!')");
            });

            Styx.Common.HotkeysManager.Register(DEFENSIVE_COOLDOWN_ON_KEY, Keys.D, ModifierKeys.Alt, ret =>
            {
                Spell.UseDefensiveCooldown = !Spell.UseDefensiveCooldown;
                Lua.DoString(Spell.UseDefensiveCooldown ? "print('Automatic Defensive Cooldown usage: Enabled!')" : @"print('Automatic Defensive Cooldown usage: Disabled!')");
            });

            _keysRegistered = true;
        }

        public static void RemoveHotkeys()
        {
            if (!_keysRegistered)
                return;

            Styx.Common.HotkeysManager.Unregister(AOE_ON_KEY);
            Styx.Common.HotkeysManager.Unregister(AUTO_KICK_ON_KEY);
            Styx.Common.HotkeysManager.Unregister(COOLDOWN_ON_KEY);
            Styx.Common.HotkeysManager.Unregister(DEFENSIVE_COOLDOWN_ON_KEY);

            _keysRegistered = false;
        }

        #endregion
    }
}
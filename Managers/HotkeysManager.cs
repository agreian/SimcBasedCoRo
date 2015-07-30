using System.Windows.Forms;
using SimcBasedCoRo.Utilities;
using Styx.Common;
using Styx.WoWInternals;

namespace SimcBasedCoRo.Managers
{
    public static class HotkeysManager
    {
        private const string AOE_ON_KEY = "AoeOn";
        private const string COOLDOWN_ON_KEY = "AoeOn";

        #region Fields

        private static bool _keysRegistered;

        #endregion

        #region Public Methods

        public static void RegisterHotKeys()
        {
            if (_keysRegistered)
                return;

            Styx.Common.HotkeysManager.Register(AOE_ON_KEY, Keys.A, ModifierKeys.Alt , ret =>
            {
                Spell.UseAoe = !Spell.UseAoe;
                Lua.DoString(Spell.UseAoe ? "print('AoE Mode: Enabled!')" : @"print('AoE Mode: Disabled!')");
            });

            Styx.Common.HotkeysManager.Register(COOLDOWN_ON_KEY, Keys.C, ModifierKeys.Alt, ret =>
            {
                Spell.UseCooldown = !Spell.UseCooldown;
                Lua.DoString(Spell.UseCooldown ? "print('Automatic Cooldown usage: Enabled!')" : @"print('Automatic Cooldown usage: Disabled!')");
            });

            _keysRegistered = true;
        }

        public static void RemoveHotkeys()
        {
            if (!_keysRegistered)
                return;

            Styx.Common.HotkeysManager.Unregister(AOE_ON_KEY);
            Styx.Common.HotkeysManager.Unregister(COOLDOWN_ON_KEY);

            _keysRegistered = false;
        }

        #endregion
    }
}
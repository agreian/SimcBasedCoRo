using System.Windows.Forms;
using SimcBasedCoRo.Utilities;
using Styx.Common;
using Styx.WoWInternals;

namespace SimcBasedCoRo.Managers
{
    public static class HotkeysManager
    {
        private const string AOE_ON_KEY = "AoeOn";

        #region Fields

        private static bool _keysRegistered;

        #endregion

        #region Public Methods

        public static void RegisterHotKeys()
        {
            if (_keysRegistered)
                return;

            Styx.Common.HotkeysManager.Register(AOE_ON_KEY, Keys.A, ModifierKeys.Control, ret =>
            {
                Spell.UseAoe = !Spell.UseAoe;
                Lua.DoString(Spell.UseAoe ? "print('AoE Mode: Enabled!')" : @"print('AoE Mode: Disabled!')");
            });

            _keysRegistered = true;
        }

        public static void RemoveHotkeys()
        {
            if (!_keysRegistered)
                return;

            Styx.Common.HotkeysManager.Unregister(AOE_ON_KEY);

            _keysRegistered = false;
        }

        #endregion
    }
}
using System.Linq;
using Styx;

namespace SimcBasedCoRo.ClassSpecific.DeathKnight
{
    public static class talent
    {
        #region Properties

        public static bool blood_tap_enabled
        {
            get { return HasTalent(DeathKnight.blood_tap); }
        }

        private static bool HasTalent(string name)
        {
            return StyxWoW.Me.GetLearnedTalents().Any(x => x.Name == name);
        }

        public static bool breath_of_sindragosa_enabled
        {
            get { return HasTalent(DeathKnight.breath_of_sindragosa); }
        }

        public static bool defile_enabled
        {
            get { return HasTalent(DeathKnight.defile); }
        }

        public static bool necrotic_plague_enabled
        {
            get { return HasTalent(DeathKnight.necrotic_plague); }
        }

        public static bool runic_empowerment_enabled
        {
            get { return HasTalent(DeathKnight.runic_empowerment); }
        }

        public static bool unholy_blight_enabled
        {
            get { return HasTalent(DeathKnight.unholy_blight); }
        }

        #endregion
    }
}
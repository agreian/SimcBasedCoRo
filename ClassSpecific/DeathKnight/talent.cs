using System.Linq;
using Styx;
using Styx.Common;

namespace SimcBasedCoRo.ClassSpecific.DeathKnight
{
    public static class talent
    {
        #region Properties

        public static bool blood_tap_enabled
        {
            get { return HasTalent(DeathKnightTalents.BloodTap); }
        }

        public static bool breath_of_sindragosa_enabled
        {
            get { return HasTalent(DeathKnightTalents.BreathOfSindragosa); }
        }

        public static bool defile_enabled
        {
            get { return HasTalent(DeathKnightTalents.Defile); }
        }

        public static bool necrotic_plague_enabled
        {
            get { return HasTalent(DeathKnightTalents.NecroticPlague); }
        }

        public static bool runic_empowerment_enabled
        {
            get { return HasTalent(DeathKnightTalents.RunicEmpowerment); }
        }

        public static bool unholy_blight_enabled
        {
            get { return HasTalent(DeathKnightTalents.UnholyBlight); }
        }

        #endregion

        #region Private Methods

        private static bool HasTalent(DeathKnightTalents tal)
        {
            return TalentManager.IsSelected((int) tal);
        }

        #endregion
    }
}
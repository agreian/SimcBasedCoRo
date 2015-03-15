using Styx;
using Styx.WoWInternals.WoWObjects;

namespace SimcBasedCoRo.ClassSpecific.DeathKnight
{
    public static class dot
    {
        #region Properties

        public static double blood_plague_remains
        {
            get { return StyxWoW.Me.CurrentTarget.GetAuraTimeLeft(DeathKnight.blood_plague).TotalSeconds; }
        }

        public static bool blood_plague_ticking
        {
            get { return blood_plague_remains > 0; }
        }

        public static double breath_of_sindragosa_remains
        {
            get { return StyxWoW.Me.CurrentTarget.GetAuraTimeLeft(DeathKnight.breath_of_sindragosa).TotalSeconds; }
        }

        public static bool breath_of_sindragosa_ticking
        {
            get { return breath_of_sindragosa_remains > 0; }
        }

        public static double frost_fever_remains
        {
            get { return StyxWoW.Me.CurrentTarget.GetAuraTimeLeft(DeathKnight.frost_fever).TotalSeconds; }
        }

        public static bool frost_fever_ticking
        {
            get { return frost_fever_remains > 0; }
        }

        public static double necrotic_plague_remains
        {
            get { return StyxWoW.Me.CurrentTarget.GetAuraTimeLeft(DeathKnight.necrotic_plague).TotalSeconds; }
        }

        public static bool necrotic_plague_ticking
        {
            get { return necrotic_plague_remains > 0; }
        }

        #endregion

        #region Public Methods

        public static double necrotic_plague_remains_on(WoWUnit unit)
        {
            return unit.GetAuraTimeLeft(DeathKnight.necrotic_plague).TotalSeconds;
        }

        public static bool necrotic_plague_ticking_on(WoWUnit unit)
        {
            return necrotic_plague_remains_on(unit) > 0;
        }

        #endregion
    }
}
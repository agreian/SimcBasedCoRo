using Styx;
using Styx.WoWInternals.WoWObjects;

namespace SimcBasedCoRo.ClassSpecific.DeathKnight
{
    public static class disease
    {
        #region Fields

        private static readonly string[] listBase = {DeathKnight.blood_plague, DeathKnight.frost_fever};
        private static readonly string[] listWithNecroticPlague = {DeathKnight.necrotic_plague};

        #endregion

        #region Properties

        private static string[] diseaseArray
        {
            get { return talent.necrotic_plague_enabled ? listWithNecroticPlague : listBase; }
        }

        public static double max_remains
        {
            get { return max_remains_on(StyxWoW.Me.CurrentTarget); }
        }

        public static bool max_ticking
        {
            get { return max_ticking_on(StyxWoW.Me.CurrentTarget); }
        }

        public static double min_remains
        {
            get { return min_remains_on(StyxWoW.Me.CurrentTarget); }
        }

        public static bool min_ticking
        {
            get { return ticking; }
        }

        public static bool ticking
        {
            get { return ticking_on(StyxWoW.Me.CurrentTarget); }
        }

        #endregion

        #region Public Methods

        public static double max_remains_on(WoWUnit unit)
        {
            var max = double.MinValue;
            foreach (var s in diseaseArray)
            {
                var rmn = unit.GetAuraTimeLeft(s).TotalSeconds;
                if (rmn > max)
                    max = rmn;
            }

            if (max == double.MinValue)
                max = 0;

            return max;
        }

        public static double min_remains_on(WoWUnit unit)
        {
            var min = double.MaxValue;
            foreach (var s in diseaseArray)
            {
                var rmn = unit.GetAuraTimeLeft(s).TotalSeconds;
                if (rmn < min)
                    min = rmn;
            }

            if (min == double.MaxValue)
                min = 0;

            return min;
        }

        public static bool ticking_on(WoWUnit unit)
        {
            return unit.HasAllMyAuras(diseaseArray);
        }

        #endregion

        #region Private Methods

        private static bool max_ticking_on(WoWUnit unit)
        {
            return unit.HasAnyOfMyAuras(diseaseArray);
        }

        #endregion
    }
}
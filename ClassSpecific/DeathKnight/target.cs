using Styx;

namespace SimcBasedCoRo.ClassSpecific.DeathKnight
{
    public static class target
    {
        #region Properties

        public static double health_pct
        {
            get { return StyxWoW.Me.CurrentTarget.HealthPercent; }
        }

        public static long time_to_die
        {
            get { return StyxWoW.Me.CurrentTarget.TimeToDeath(); }
        }

        #endregion
    }
}
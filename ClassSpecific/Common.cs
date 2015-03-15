using System.Collections.Generic;
using System.Linq;
using Styx;
using Styx.WoWInternals.WoWObjects;

namespace SimcBasedCoRo.ClassSpecific
{
    // ReSharper disable InconsistentNaming

    public abstract class Common
    {
        #region Properties

        protected static LocalPlayer Me
        {
            get { return StyxWoW.Me; }
        }

        protected static int active_enemies
        {
            get { return active_enemies_list.Count(); }
        }

        protected static IEnumerable<WoWUnit> active_enemies_list
        {
            get
            {
                var distance = 40;

                switch (StyxWoW.Me.Specialization)
                {
                    case WoWSpec.DeathKnightUnholy:
                        distance = TalentManager.HasGlyph(DeathKnight.DeathKnight.blood_boil) ? 15 : 10;
                        break;
                    case WoWSpec.DeathKnightBlood:
                        distance = 20;
                        break;
                }

                return
                    StyxWoW.Me.UnfriendlyUnits()
                        .Where(x => x.Distance < distance);
            }
        }

        protected static double health_pct
        {
            get { return Me.HealthPercent; }
        }

        #endregion
    }

    // ReSharper restore InconsistentNaming
}
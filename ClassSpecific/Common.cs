using System.Collections.Generic;
using System.Linq;
using Styx;
using Styx.WoWInternals.WoWObjects;

namespace SimcBasedCoRo.ClassSpecific
{
    public abstract class Common
    {
        #region Properties

        protected static LocalPlayer Me
        {
            get { return StyxWoW.Me; }
        }

        protected static int active_enemies
        {
            get { return active_enemies_list.Count; }
        }

        protected static List<WoWUnit> active_enemies_list
        {
            get
            {
                return
                    Common.Scenario.Mobs.Where(x => x.Distance < (TalentManager.HasGlyph("Blood Boil") ? 15 : 10))
                        .ToList();
            }
        }

        protected static double health_pct
        {
            get { return Me.HealthPercent; }
        }

        #endregion
    }
}
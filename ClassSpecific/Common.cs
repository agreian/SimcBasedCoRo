using System;
using System.Collections.Generic;
using System.Linq;
using SimcBasedCoRo.Extensions;
using SimcBasedCoRo.Managers;
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
                        distance = TalentManager.HasGlyph(DeathKnight.blood_boil) ? 15 : 10;
                        break;
                    case WoWSpec.DeathKnightBlood:
                        distance = 20;
                        break;
                }

                return SimCraftCombatRoutine.ActiveEnemies.Where(u => u.Distance < distance);
            }
        }

        protected static double health_pct
        {
            get { return Me.HealthPercent; }
        }

        protected static double mana_pct
        {
            get { return Me.ManaPercent; }
        }

        protected static double spell_haste
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        #region Types

        protected static class target
        {
            #region Properties

            public static double health_pct
            {
                get
                {
                    if (StyxWoW.Me.CurrentTarget == null) return 100;

                    return StyxWoW.Me.CurrentTarget.HealthPercent;
                }
            }

            public static long time_to_die
            {
                get
                {
                    if (StyxWoW.Me.CurrentTarget == null) return 0;
                    
                    return StyxWoW.Me.CurrentTarget.TimeToDeath();
                }
            }

            #endregion
        }

        #endregion
    }

    // ReSharper restore InconsistentNaming
}
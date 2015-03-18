using System.Collections.Generic;
using System.Linq;
using SimcBasedCoRo.Extensions;
using SimcBasedCoRo.Managers;
using SimcBasedCoRo.Utilities;
using Styx;
using Styx.CommonBot;
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

        //protected static double health_pct
        //{
        //    get { return Me.HealthPercent; }
        //}

        protected static double mana_pct
        {
            get { return Me.ManaPercent; }
        }

        protected static double spell_haste
        {
            get { return StyxWoW.Me.SpellHasteModifier; }
        }

        protected static string prev_gcd
        {
            get { return Spell.PreviousGcdSpell; }
        }

        protected static double gcd
        {
            get { return SpellManager.GlobalCooldownLeft.TotalSeconds; }
        }

        #endregion

        #region Types

        protected static class target
        {
            #region Properties

            // ReSharper disable once MemberHidesStaticFromOuterClass
            public static double health_pct
            {
                get { return StyxWoW.Me.CurrentTarget.HealthPercent; }
            }

            public static long time_to_die
            {
                get { return StyxWoW.Me.CurrentTarget.TimeToDeath(int.MaxValue); }
            }

            #endregion
        }

        #endregion
    }

    // ReSharper restore InconsistentNaming
}
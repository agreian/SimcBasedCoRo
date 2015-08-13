using SimcBasedCoRo.Extensions;
using Styx.CommonBot;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;

namespace SimcBasedCoRo.Utilities
{
    internal class CastContext
    {
        #region Fields

        internal bool IsSpellBeingQueued;
        internal object Context;
        internal double Distance;
        internal double Health;
        internal string Name;
        internal SpellFindResults Sfr;
        internal WoWSpell Spell;
        internal WoWUnit Unit;

        #endregion

        #region Constructors

        internal CastContext(object ctx)
        {
            Context = ctx;
        }

        // always create passing the existing context so it is preserved for delegate usage
        internal CastContext(object ctx, SpellFindDelegate ssd, UnitSelectionDelegate onUnit)
        {
            if (ssd == null || onUnit == null)
                return;

            if (ssd(ctx, out Sfr))
            {
                Spell = Sfr.Override ?? Sfr.Original;
                Name = Spell.Name;
                Context = ctx;
                Unit = onUnit(ctx);

                // health/dist change quickly, so grab these now where
                // .. we check requirements so the log message we output
                // .. later reflects what they were when we were testing
                // .. as opposed to what they may have changed to
                // .. (since spell lookup, move while casting check, and cancast take time)
                if (Unit != null && Unit.IsValid)
                {
                    Health = Unit.HealthPercent;
                    Distance = Unit.SpellDistance();
                }
            }
        }

        #endregion
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;

namespace SimcBasedCoRo.Extensions
{
    public static class WoWSpellExtension
    {
        #region Fields

        private const int SPELL_CHARGES_REFRESH_INTERVAL = 200;
        private const int TICKS_PER_MILLISECOND = 10000;

        private static readonly Dictionary<int, TicksCharges> _spellsCharges = new Dictionary<int, TicksCharges>();

        #endregion

        #region Public Methods

        public static float ActualMaxRange(this WoWSpell spell, WoWUnit unit)
        {
            if (spell.MaxRange == 0)
                return 0;
            // 0.1 margin for error
            return unit != null ? spell.MaxRange + unit.CombatReach : spell.MaxRange;
        }

        public static float ActualMinRange(this WoWSpell spell, WoWUnit unit)
        {
            if (spell.MinRange == 0)
                return 0;

            // some code was using 1.66666675f instead of Me.CombatReach ?
            return unit != null ? spell.MinRange + unit.CombatReach : spell.MinRange;
        }

        public static int GetCharges(this WoWSpell spell)
        {
            if (_spellsCharges.ContainsKey(spell.Id) == false)
                _spellsCharges.Add(spell.Id, new TicksCharges(DateTime.UtcNow.Ticks, Lua.GetReturnVal<int>("return GetSpellCharges(" + spell.Id + ")", 0)));
            else if (DateTime.UtcNow.Ticks - _spellsCharges[spell.Id].Ticks > SPELL_CHARGES_REFRESH_INTERVAL * TICKS_PER_MILLISECOND)
                _spellsCharges[spell.Id] = new TicksCharges(DateTime.UtcNow.Ticks, Lua.GetReturnVal<int>("return GetSpellCharges(" + spell.Id + ")", 0));

            return _spellsCharges[spell.Id].Charges;
        }

        public static bool IsHeal(this WoWSpell spell)
        {
            return
                spell.SpellEffects.Any(
                    s =>
                        s.EffectType == WoWSpellEffectType.Heal || s.EffectType == WoWSpellEffectType.HealMaxHealth || s.EffectType == WoWSpellEffectType.HealPct ||
                        (s.EffectType == WoWSpellEffectType.ApplyAura && (s.AuraType == WoWApplyAuraType.PeriodicHeal || s.AuraType == WoWApplyAuraType.SchoolAbsorb)));
        }

        public static bool IsInstantCast(this WoWSpell spell)
        {
            return spell.CastTime == 0;
        }

        #endregion

        #region Types

        private class TicksCharges
        {
            #region Constructors

            public TicksCharges(long ticks, int charges)
            {
                Ticks = ticks;
                Charges = charges;
            }

            #endregion

            #region Properties

            public int Charges { get; private set; }
            public long Ticks { get; private set; }

            #endregion
        }

        #endregion
    }
}
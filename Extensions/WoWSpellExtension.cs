using System;
using System.Collections.Generic;
using Styx.WoWInternals;

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

        public static int GetCharges(this WoWSpell spell)
        {
            if (_spellsCharges.ContainsKey(spell.Id) == false)
                _spellsCharges.Add(spell.Id, new TicksCharges(DateTime.UtcNow.Ticks, Lua.GetReturnVal<int>("return GetSpellCharges(" + spell.Id + ")", 0)));
            else if (DateTime.UtcNow.Ticks - _spellsCharges[spell.Id].Ticks > SPELL_CHARGES_REFRESH_INTERVAL * TICKS_PER_MILLISECOND)
                _spellsCharges[spell.Id] = new TicksCharges(DateTime.UtcNow.Ticks, Lua.GetReturnVal<int>("return GetSpellCharges(" + spell.Id + ")", 0));

            return _spellsCharges[spell.Id].Charges;
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
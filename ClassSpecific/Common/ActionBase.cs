using System;
using SimcBasedCoRo.Utilities;
using Styx.CommonBot;

namespace SimcBasedCoRo.ClassSpecific.Common
{
    internal class ActionBase : Base
    {
        #region Constructors

        public ActionBase(string spellName)
            : base(spellName)
        {
        }

        #endregion

        // ReSharper disable InconsistentNaming

        #region Properties

        public int charges
        {
            get { return Spell.GetCharges(SpellName); }
        }

        public double execute_time
        {
            get
            {
                var cooldown = Spell.GetSpellCastTime(SpellName);
                return cooldown == TimeSpan.Zero ? ClassSpecificBase.gcd_max : cooldown.TotalSeconds;
            }
        }

        public double recharge_time
        {
            get
            {
                SpellFindResults sfr;
                if (SpellManager.FindSpell(SpellName, out sfr) == false || sfr == null || (sfr.Original == null && sfr.Override == null)) return 0;

                var spell = sfr.Override ?? sfr.Original;

                if (spell.Cooldown) return spell.CooldownTimeLeft.TotalSeconds;
                return spell.BaseCooldown / 1000.0;
            }
        }

        #endregion

        // ReSharper restore InconsistentNaming
    }
}
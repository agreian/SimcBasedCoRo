using System;
using Buddy.Coroutines;
using SimcBasedCoRo.ClassSpecific;
using SimcBasedCoRo.Extensions;
using Styx;
using Styx.CommonBot;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;

namespace SimcBasedCoRo.Utilities
{
    public class Spell : ISpellRun
    {
        #region Fields

        private readonly Func<bool> _requirements;
        private readonly string _spellName;
        private readonly SpellTypeEnum _spellTypeEnum;
        private readonly Func<WoWUnit> _target;

        #endregion

        #region Constructors

        public Spell(string spellName, Func<bool> requirements = null, Func<WoWUnit> target = null)
            : this(SpellTypeEnum.Cast, spellName, requirements, target)
        {
            switch (StyxWoW.Me.Class)
            {
                case WoWClass.DeathKnight:
                    _spellTypeEnum = DeathKnight.Spells[spellName];
                    break;
                case WoWClass.Mage:
                    _spellTypeEnum = Mage.Spells[spellName];
                    break;
                default:
                    throw new ArgumentException();
            }
        }

        public Spell(string spellName, Func<WoWUnit> target)
            : this(spellName, null, target)
        {
        }

        //public Spell(SpellTypeEnum spellTypeEnum, string spellName, Func<WoWUnit> target)
        //    : this(spellTypeEnum, spellName, null, target)
        //{
        //}

        private Spell(SpellTypeEnum spellTypeEnum, string spellName, Func<bool> requirements = null,
            Func<WoWUnit> target = null)
        {
            if (string.IsNullOrWhiteSpace(spellName))
                throw new ArgumentException("spellName");

            _spellTypeEnum = spellTypeEnum;
            _spellName = spellName;
            _requirements = requirements;
            _target = target;
        }

        #endregion

        #region Properties

        public static string PreviousGcdSpell { get; private set; }

        #endregion

        #region ISpellRun Members

        public SpellResultEnum Run()
        {
            if (_requirements != null && !_requirements())
                return SpellResultEnum.Failure;

            SpellFindResults result;
            if (!SpellManager.FindSpell(_spellName, out result))
                return SpellResultEnum.Failure;

            var spell = result.Override ?? result.Original;

            if (!StyxWoW.Me.KnowsSpell(spell.Id))
                return SpellResultEnum.Failure;

            var target = _target != null ? _target() : null;

            switch (_spellTypeEnum)
            {
                case SpellTypeEnum.Buff:
                    return Buff(target, spell);

                case SpellTypeEnum.Cast:
                    return Cast(target, spell);

                case SpellTypeEnum.CastAoe:
                    return !SimCraftCombatRoutine.UseAoe ? SpellResultEnum.Failure : Cast(target, spell);

                case SpellTypeEnum.CastOnGround:
                    return CastOnGround(target, spell);

                case SpellTypeEnum.CastOnGroundAoe:
                    return !SimCraftCombatRoutine.UseAoe ? SpellResultEnum.Failure : CastOnGround(target, spell);
            }

            return SpellResultEnum.Failure;
        }

        #endregion

        #region Public Methods

        public static SpellResultEnum CanStartCasting(WoWSpell spell = null, WoWUnit target = null)
        {
            if (StyxWoW.Me.IsDead)
                return SpellResultEnum.Failure;

            if (spell != null && target != null)
            {
                if (spell.IsMeleeSpell && !target.IsWithinMeleeRange)
                    return SpellResultEnum.Failure;
            }

            if (spell != null && target != null && spell.HasRange)
            {
                if (target.Distance < spell.MinRange)
                    return SpellResultEnum.Failure;

                if (target.Distance >= spell.MaxRange)
                    return SpellResultEnum.Failure;
            }

            if (StyxWoW.Me.CurrentCastTimeLeft.TotalMilliseconds >= SimCraftCombatRoutine.Latency)
                return SpellResultEnum.Failure;

            if (StyxWoW.Me.CurrentChannelTimeLeft.TotalMilliseconds >= SimCraftCombatRoutine.Latency)
                return SpellResultEnum.Failure;

            if (SpellManager.GlobalCooldownLeft.TotalMilliseconds >= SimCraftCombatRoutine.Latency)
                return SpellResultEnum.Failure;

            return SpellResultEnum.Success;
        }

        public static TimeSpan GetSpellCastTime(string s)
        {
            SpellFindResults sfr;
            if (SpellManager.FindSpell(s, out sfr))
            {
                var spell = sfr.Override ?? sfr.Original;
                return GetSpellCastTime(spell);
            }

            return TimeSpan.Zero;
        }

        public static int GetSpellCharges(string s)
        {
            SpellFindResults sfr;
            if (SpellManager.FindSpell(s, out sfr))
            {
                var spell = sfr.Override ?? sfr.Original;
                return spell.GetCharges();
            }

            return 0;
        }

        public static TimeSpan GetSpellCooldown(string spell)
        {
            SpellFindResults result;
            if (SpellManager.FindSpell(spell, out result))
                return (result.Override ?? result.Original).CooldownTimeLeft;

            return TimeSpan.MaxValue;
        }

        #endregion

        #region Private Methods

        private static SpellResultEnum Buff(WoWUnit target, WoWSpell spell)
        {
            if (target == null)
                target = StyxWoW.Me;

            if (CanStartCasting(spell, target) == SpellResultEnum.Failure)
                return SpellResultEnum.Failure;

            if (!SpellManager.CanBuff(spell, target))
                return SpellResultEnum.Failure;

            if (!SpellManager.Buff(spell, target)) return SpellResultEnum.Failure;

            Coroutine.Wait(Convert.ToInt32(SimCraftCombatRoutine.Latency) * 2, () => SpellManager.GlobalCooldown).Wait();
            if (SpellManager.GlobalCooldown) PreviousGcdSpell = spell.Name;

            return SpellResultEnum.Success;
        }

        private static SpellResultEnum Cast(WoWUnit target, WoWSpell spell)
        {
            if (target == null)
                target = StyxWoW.Me.CurrentTarget;

            if (CanStartCasting(spell, target) == SpellResultEnum.Failure)
                return SpellResultEnum.Failure;

            if (!SpellManager.CanCast(spell, target))
                return SpellResultEnum.Failure;

            if (!SpellManager.Cast(spell, target)) return SpellResultEnum.Failure;

            Coroutine.Wait(Convert.ToInt32(SimCraftCombatRoutine.Latency) * 2, () => SpellManager.GlobalCooldown).Wait();
            if (SpellManager.GlobalCooldown) PreviousGcdSpell = spell.Name;

            return SpellResultEnum.Success;
        }

        private static SpellResultEnum CastOnGround(WoWUnit target, WoWSpell spell)
        {
            if (target == null)
                target = StyxWoW.Me.CurrentTarget;

            if (CanStartCasting(spell, target) == SpellResultEnum.Failure)
                return SpellResultEnum.Failure;

            if (!SpellManager.CanCast(spell, target))
                return SpellResultEnum.Failure;

            SpellManager.Cast(spell, target);

            Coroutine.Wait(Convert.ToInt32(SimCraftCombatRoutine.Latency) * 5, () => StyxWoW.Me.CurrentPendingCursorSpell != null).Wait();
            if (StyxWoW.Me.CurrentPendingCursorSpell == null) return SpellResultEnum.Failure;

            if (!SpellManager.ClickRemoteLocation(target.Location))
                return SpellResultEnum.Failure;

            Coroutine.Wait(Convert.ToInt32(SimCraftCombatRoutine.Latency) * 2, () => SpellManager.GlobalCooldown).Wait();
            if (SpellManager.GlobalCooldown) PreviousGcdSpell = spell.Name;

            return SpellResultEnum.Success;
        }

        private static TimeSpan GetSpellCastTime(WoWSpell spell)
        {
            if (spell == null) return TimeSpan.Zero;

            var time = (int)spell.CastTime;
            if (time == 0)
                time = spell.BaseDuration;
            return TimeSpan.FromMilliseconds(time);
        }

        #endregion
    }
}
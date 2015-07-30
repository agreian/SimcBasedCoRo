using System;
using System.Collections.Generic;
using Buddy.Coroutines;
using CommonBehaviors.Actions;
using SimcBasedCoRo.Extensions;
using Styx;
using Styx.Common;
using Styx.CommonBot;
using Styx.TreeSharp;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;

namespace SimcBasedCoRo.Utilities
{
    public class Spell
    {
        #region Fields

        internal static readonly Dictionary<string, long> UndefinedSpells = new Dictionary<string, long>();

        private static bool _useAoe = true;

        #endregion

        #region Properties

        public static string PreviousGcdSpell { get; private set; }

        public static bool UseAoe
        {
            get { return _useAoe; }
            set { _useAoe = value; }
        }

        public static bool UseCooldown { get; set; }

        #endregion

        #region Public Methods

        public static Composite Buff(string spellName, Func<object, bool> requirements)
        {
            return Buff(spellName, on => StyxWoW.Me, requirements);
        }

        public static Composite Buff(string spellName, Func<object, WoWUnit> target, Func<object, bool> requirements)
        {
            WoWUnit currentTarget;

            if (target == null || target(null) == null) currentTarget = StyxWoW.Me;
            else currentTarget = target(null);

            SpellFindResults result;
            if (UndefinedSpells.ContainsKey(spellName) || !SpellManager.FindSpell(spellName, out result))
            {
                AddUndefinedSpell(spellName);

                return new ActionAlwaysFail();
            }

            var spell = result.Override ?? result.Original;
            if (spell == null)
            {
                AddUndefinedSpell(spellName);

                return new ActionAlwaysFail();
            }

            if (CanStartCasting(spell, currentTarget) == false) return new ActionAlwaysFail();

            if (!SpellManager.CanBuff(spell, currentTarget)) return new ActionAlwaysFail();

            if (!SpellManager.Buff(spell, currentTarget)) return new ActionAlwaysFail();

            Coroutine.Wait(SimCraftCombatRoutine.Latency * 2, () => SpellManager.GlobalCooldown).Wait();
            if (SpellManager.GlobalCooldown) PreviousGcdSpell = spell.Name;

            return new ActionAlwaysSucceed();
        }

        public static Composite BuffSelf(string spellName, Func<object, bool> requirements)
        {
            return Buff(spellName, on => StyxWoW.Me, requirements);
        }

        public static Composite BuffSelfAndWait(string spellName, Func<object, bool> requirements)
        {
            return Buff(spellName, on => StyxWoW.Me, requirements);
        }

        public static bool CanStartCasting(WoWSpell spell = null, WoWUnit target = null)
        {
            if (StyxWoW.Me.IsDead)
                return false;

            if (spell != null && target != null)
            {
                if (spell.IsMeleeSpell && !target.IsWithinMeleeRange)
                    return false;
            }

            if (spell != null && target != null && spell.HasRange)
            {
                if (target.Distance < spell.MinRange)
                    return false;

                if (target.Distance >= spell.MaxRange)
                    return false;
            }

            if (StyxWoW.Me.CurrentCastTimeLeft.TotalMilliseconds > 0) return false;
            if (StyxWoW.Me.CurrentChannelTimeLeft.TotalMilliseconds > 0) return false;
            if (SpellManager.GlobalCooldownLeft.TotalMilliseconds > 0) return false;

            return true;
        }

        public static Composite Cast(string spellName, Func<object, bool> requirements)
        {
            return Cast(spellName, on => StyxWoW.Me.CurrentTarget, requirements);
        }

        public static Composite Cast(string spellName, Func<object, WoWUnit> target, Func<object, bool> requirements)
        {
            WoWUnit currentTarget;

            if (target == null || target(null) == null) currentTarget = StyxWoW.Me.CurrentTarget;
            else currentTarget = target(null);

            SpellFindResults result;
            if (UndefinedSpells.ContainsKey(spellName) || !SpellManager.FindSpell(spellName, out result))
            {
                AddUndefinedSpell(spellName);

                return new ActionAlwaysFail();
            }

            var spell = result.Override ?? result.Original;
            if (spell == null)
            {
                AddUndefinedSpell(spellName);

                return new ActionAlwaysFail();
            }

            if (CanStartCasting(spell, currentTarget) == false) return new ActionAlwaysFail();

            if (!SpellManager.CanCast(spell, currentTarget)) return new ActionAlwaysFail();

            if (!SpellManager.Cast(spell, currentTarget)) return new ActionAlwaysFail();

            Coroutine.Wait(SimCraftCombatRoutine.Latency * 2, () => SpellManager.GlobalCooldown).Wait();
            if (SpellManager.GlobalCooldown) PreviousGcdSpell = spell.Name;

            return new ActionAlwaysSucceed();
        }

        public static Composite CastOnGround(string spellName, Func<object, WoWUnit> target, Func<object, bool> requirements)
        {
            WoWUnit currentTarget;

            if (target == null || target(null) == null) currentTarget = StyxWoW.Me.CurrentTarget;
            else currentTarget = target(null);

            SpellFindResults result;
            if (UndefinedSpells.ContainsKey(spellName) || !SpellManager.FindSpell(spellName, out result))
            {
                AddUndefinedSpell(spellName);

                return new ActionAlwaysFail();
            }

            var spell = result.Override ?? result.Original;
            if (spell == null)
            {
                AddUndefinedSpell(spellName);

                return new ActionAlwaysFail();
            }

            if (CanStartCasting(spell, currentTarget) == false) return new ActionAlwaysFail();

            if (!SpellManager.CanCast(spell, currentTarget)) return new ActionAlwaysFail();

            if (!SpellManager.Cast(spell, currentTarget)) return new ActionAlwaysFail();

            Coroutine.Wait(SimCraftCombatRoutine.Latency * 2, () => StyxWoW.Me.CurrentPendingCursorSpell != null).Wait();
            if (StyxWoW.Me.CurrentPendingCursorSpell == null) return new ActionAlwaysFail();

            if (!SpellManager.ClickRemoteLocation(currentTarget.Location)) return new ActionAlwaysFail();

            Coroutine.Wait(SimCraftCombatRoutine.Latency * 2, () => SpellManager.GlobalCooldown).Wait();
            if (SpellManager.GlobalCooldown) PreviousGcdSpell = spell.Name;

            return new ActionAlwaysSucceed();
        }

        public static TimeSpan GetSpellCastTime(string spellName)
        {
            SpellFindResults sfr;
            if (SpellManager.FindSpell(spellName, out sfr))
            {
                var spell = sfr.Override ?? sfr.Original;
                return GetSpellCastTime(spell);
            }

            return TimeSpan.Zero;
        }

        public static int GetSpellCharges(string spellName)
        {
            SpellFindResults sfr;
            if (SpellManager.FindSpell(spellName, out sfr))
            {
                var spell = sfr.Override ?? sfr.Original;
                return spell.GetCharges();
            }

            return 0;
        }

        public static TimeSpan GetSpellCooldown(string spellName)
        {
            SpellFindResults sfr;
            if (SpellManager.FindSpell(spellName, out sfr)) return (sfr.Override ?? sfr.Original).CooldownTimeLeft;

            return TimeSpan.MaxValue;
        }

        public static bool IsGlobalCooldown()
        {
            return SpellManager.GlobalCooldown;
        }

        #endregion

        #region Private Methods

        private static void AddUndefinedSpell(string spellName)
        {
            if (UndefinedSpells.ContainsKey(spellName))
                UndefinedSpells[spellName] = UndefinedSpells[spellName] + 1;
            else
                UndefinedSpells.Add(spellName, 1);
        }

        private static TimeSpan GetSpellCastTime(WoWSpell spell)
        {
            if (spell == null) return TimeSpan.Zero;

            var time = (int) spell.CastTime;
            if (time == 0) time = spell.BaseDuration;
            return TimeSpan.FromMilliseconds(time);
        }

        #endregion
    }
}
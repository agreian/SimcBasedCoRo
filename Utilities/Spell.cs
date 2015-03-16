using System;
using System.Threading;
using Bots.DungeonBuddy.Helpers;
using Styx;
using Styx.CommonBot;
using Styx.CommonBot.Coroutines;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;

namespace SimcBasedCoRo.Utilities
{
    public class Spell : ISpellRun
    {
        #region Delegates

        public delegate bool SimpleBooleanDelegate(object lol);

        public delegate WoWUnit UnitSelectionDelegate(object lol);

        #endregion

        #region Fields

        private readonly SimpleBooleanDelegate _requirements;
        private readonly string _spellName;
        private readonly SpellType _spellType;
        private readonly UnitSelectionDelegate _target;

        #endregion

        #region Constructors

        public Spell(SpellType spellType, string spellName, UnitSelectionDelegate target)
            : this(spellType, spellName, null, target)
        {
        }

        public Spell(SpellType spellType, string spellName, SimpleBooleanDelegate requirements = null,
            UnitSelectionDelegate target = null)
        {
            if (string.IsNullOrWhiteSpace(spellName))
                throw new ArgumentException("spellName");

            _spellType = spellType;
            _spellName = spellName;
            _requirements = requirements;
            _target = target;
        }

        #endregion

        #region Properties

        public static bool UseAoe
        {
            get { return true; }
        }

        #endregion

        #region ISpellRun Members

        public SpellResult Run()
        {
            if (_requirements != null && !_requirements(null))
                return SpellResult.Failure;

            // Cast Spell
            SpellFindResults result;
            if (!SpellManager.FindSpell(_spellName, out result))
                return SpellResult.Failure;

            var spell = result.Override ?? result.Original;

            var target = _target != null ? _target(null) : null;

            switch (_spellType)
            {
                case SpellType.Buff:
                    if (target == null)
                        target = StyxWoW.Me;

                    if (CanStartCasting(spell, target) == SpellResult.Failure)
                        return SpellResult.Failure;

                    if (!SpellManager.CanBuff(spell, target))
                        return SpellResult.Failure;

                    if (!SpellManager.Buff(spell, target)) return SpellResult.Failure;

                    CommonCoroutines.SleepForLagDuration().Wait();

                    return SpellResult.Success;

                case SpellType.Cast:
                    if (target == null)
                        target = StyxWoW.Me.CurrentTarget;

                    if (CanStartCasting(spell, target) == SpellResult.Failure)
                        return SpellResult.Failure;

                    if (!SpellManager.CanCast(spell, target))
                        return SpellResult.Failure;

                    if (!SpellManager.Cast(spell, target)) return SpellResult.Failure;

                    CommonCoroutines.SleepForLagDuration().Wait();

                    return SpellResult.Success;

                case SpellType.CastOnGround:
                    if (target == null)
                        target = StyxWoW.Me.CurrentTarget;

                    if (CanStartCasting(spell, target) == SpellResult.Failure)
                        return SpellResult.Failure;

                    if (!SpellManager.CanCast(spell, target))
                        return SpellResult.Failure;

                    SpellManager.Cast(spell, target);

                    //Coroutine.Wait(Convert.ToInt32(SimCraftCombatRoutine.Latency * 5), () => StyxWoW.Me.CurrentPendingCursorSpell != null).Wait();
                    while (StyxWoW.Me.CurrentPendingCursorSpell == null)
                        Thread.Sleep(10);

                    if (!SpellManager.ClickRemoteLocation(target.Location))
                        return SpellResult.Failure;

                    CommonCoroutines.SleepForLagDuration().Wait();

                    return SpellResult.Success;
            }

            return SpellResult.Failure;
        }

        #endregion

        #region Public Methods

        public static SpellResult CanStartCasting(WoWSpell spell = null, WoWUnit target = null)
        {
            if (StyxWoW.Me.IsDead)
                return SpellResult.Failure;

            if (StyxWoW.Me.IsMelee())
            {
                if (!StyxWoW.Me.CurrentTarget.IsWithinMeleeRange)
                    return SpellResult.Failure;
            }

            if (spell != null && target != null && spell.HasRange)
            {
                if (target.Distance < spell.MinRange)
                    return SpellResult.Failure;

                if (target.Distance >= spell.MaxRange)
                    return SpellResult.Failure;
            }

            if (StyxWoW.Me.CurrentCastTimeLeft.TotalMilliseconds > SimCraftCombatRoutine.Latency)
                return SpellResult.Failure;

            if (StyxWoW.Me.CurrentChannelTimeLeft.TotalMilliseconds > SimCraftCombatRoutine.Latency)
                return SpellResult.Failure;

            if (SpellManager.GlobalCooldownLeft.TotalMilliseconds > SimCraftCombatRoutine.Latency)
                return SpellResult.Failure;

            return SpellResult.Success;
        }

        public static TimeSpan GetSpellCooldown(string spell)
        {
            SpellFindResults result;
            if (SpellManager.FindSpell(spell, out result))
                return (result.Override ?? result.Original).CooldownTimeLeft;

            return TimeSpan.MaxValue;
        }

        #endregion
    }

    public enum SpellType
    {
        Cast,
        CastOnGround,
        Buff
    }
}
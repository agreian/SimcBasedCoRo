using System;
using Styx;
using Styx.Common;
using Styx.CommonBot;
using Styx.TreeSharp;
using Styx.WoWInternals.WoWObjects;

namespace SimcBasedCoRo
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
            Logging.Write("{0} : {1} Start Cast", DateTime.Now, _spellName);

            if (StyxWoW.Me.IsCasting || StyxWoW.Me.IsChanneling || StyxWoW.Me.IsMoving || StyxWoW.Me.IsDead)
                return SpellResult.Failure;

            if (_requirements != null && !_requirements.Invoke(null))
            {
                Logging.Write("{0} : {1} Requirements failure", DateTime.Now, _spellName);
                return SpellResult.Failure;
            }

            // Cast Spell
            SpellFindResults result;
            if(!SpellManager.FindSpell(_spellName, out result))
                return SpellResult.Failure;
            
            var spell = result.Override ?? result.Original;

            var target = _target != null ? _target.Invoke(null) : null;

            switch (_spellType)
            {
                case SpellType.Buff:
                    if (target == null)
                        target = StyxWoW.Me;

                    if(!SpellManager.CanBuff(spell, target))
                        return SpellResult.Failure;

                    return !SpellManager.Buff(spell, target) ? SpellResult.Failure : SpellResult.Success;

                case SpellType.Cast:
                    if (target == null)
                        target = StyxWoW.Me.CurrentTarget;

                    if (!SpellManager.CanCast(spell, target))
                        return SpellResult.Failure;

                    return !SpellManager.Cast(spell, target) ? SpellResult.Failure : SpellResult.Success;

                case SpellType.CastOnGround:
                    if (target == null)
                        target = StyxWoW.Me.CurrentTarget;

                    if (!SpellManager.CanCast(spell, target))
                        return SpellResult.Failure;

                    if (!SpellManager.Cast(spell, target)) return SpellResult.Failure;

                    return !SpellManager.ClickRemoteLocation(target.Location)
                        ? SpellResult.Failure
                        : SpellResult.Success;
            }

            return SpellResult.Failure;
        }

        #endregion

        #region Public Methods

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
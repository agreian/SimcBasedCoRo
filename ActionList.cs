using System.Collections.Generic;
using Styx;

namespace SimcBasedCoRo
{
    public class ActionList : List<ISpellRun>, ISpellRun
    {
        #region Fields

        private readonly Spell.SimpleBooleanDelegate _requirements;

        #endregion

        #region Constructors

        public ActionList()
        {
        }

        public ActionList(Spell.SimpleBooleanDelegate requirements, ISpellRun spellRun)
            : this()
        {
            _requirements = requirements;
            Add(spellRun);
        }

        #endregion

        #region ISpellRun Members

        public SpellResult Run()
        {
            if (StyxWoW.Me.IsCasting || StyxWoW.Me.IsChanneling || StyxWoW.Me.IsMoving || StyxWoW.Me.IsDead)
                return SpellResult.Failure;

            if (_requirements != null && !_requirements.Invoke(null))
                return SpellResult.Failure;

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var spellRun in AsReadOnly())
            {
                if (spellRun.Run() == SpellResult.Success)
                    return SpellResult.Success;
            }

            return SpellResult.Failure;
        }

        #endregion
    }
}
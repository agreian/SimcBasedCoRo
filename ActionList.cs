using System.Collections.Generic;
using SimcBasedCoRo.ClassSpecific.DeathKnight;
using Styx.Common;

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
            if(Spell.CanStartCasting() == SpellResult.Failure)
                return SpellResult.Failure;

            if (_requirements != null && !_requirements(null))
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
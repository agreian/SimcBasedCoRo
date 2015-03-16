using System;
using System.Collections.Generic;

namespace SimcBasedCoRo.Utilities
{
    public class ActionList : List<ISpellRun>, ISpellRun
    {
        #region Fields

        private readonly Func<bool> _requirements;

        #endregion

        #region Constructors

        public ActionList()
        {
        }

        private ActionList(IEnumerable<ISpellRun> spells)
            : base(spells)
        {
        }

        public ActionList(ISpellRun spellRun, Func<bool> requirements = null)
            : this()
        {
            _requirements = requirements;
            Add(spellRun);
        }

        #endregion

        #region ISpellRun Members

        public SpellResultEnum Run()
        {
            if (Spell.CanStartCasting() == SpellResultEnum.Failure)
                return SpellResultEnum.Failure;

            if (_requirements != null && !_requirements())
                return SpellResultEnum.Failure;

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var spellRun in AsReadOnly())
            {
                if (spellRun.Run() == SpellResultEnum.Success)
                    return SpellResultEnum.Success;
            }

            return SpellResultEnum.Failure;
        }

        #endregion

        #region Public Methods

        public static ActionList operator +(ActionList a, ISpellRun spellRun)
        {
            var actionList = new ActionList(a) { spellRun };

            return actionList;
        }

        #endregion
    }
}
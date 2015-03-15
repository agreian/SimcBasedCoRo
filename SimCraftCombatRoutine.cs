using SimcBasedCoRo.ClassSpecific.DeathKnight;
using Styx;
using Styx.Common;
using Styx.CommonBot.Routines;

namespace SimcBasedCoRo
{
    // ReSharper disable once UnusedMember.Global
    public class SimCraftCombatRoutine : CombatRoutine
    {
        #region Fields

        private ActionList _currentActionList;
        private WoWSpec _specialization;

        #endregion

        #region Properties

        public override WoWClass Class
        {
            get { return StyxWoW.Me.Class; }
        }

        public override string Name
        {
            get { return "SimcBasedCoRo"; }
        }

        private WoWSpec Specialization
        {
            get { return _specialization; }
            set
            {
                if (value == Specialization) return;

                _specialization = value;

                switch (_specialization)
                {
                    case WoWSpec.DeathKnightUnholy:
                        _currentActionList = DeathKnight.Unholy;
                        break;
                }
            }
        }

        #endregion

        #region Public Methods

        public override void Combat()
        {
            Specialization = StyxWoW.Me.Specialization;


            Logging.Write("{0}", _currentActionList.Run());
        }

        #endregion
    }
}
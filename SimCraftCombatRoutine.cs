using System;
using SimcBasedCoRo.ClassSpecific.DeathKnight;
using Styx;
using Styx.Common;
using Styx.Common.Helpers;
using Styx.CommonBot.Routines;

namespace SimcBasedCoRo
{
    // ReSharper disable once UnusedMember.Global
    // ReSharper disable once ClassNeverInstantiated.Global

    public class SimCraftCombatRoutine : CombatRoutine
    {
        #region Fields

        private static readonly WaitTimer _waitForLatencyCheck = new WaitTimer(TimeSpan.FromSeconds(5));
        private ActionList _currentActionList;
        private WoWSpec _specialization;

        #endregion

        #region Properties

        public static uint Latency { get; private set; }

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

            if (_currentActionList != null)
                _currentActionList.Run();
        }

        public override void Pulse()
        {
            if (_waitForLatencyCheck.IsFinished)
            {
                Latency = StyxWoW.WoWClient.Latency;
                _waitForLatencyCheck.Reset();
            }
        }

        #endregion
    }
}
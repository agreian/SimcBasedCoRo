using System;
using System.Linq;
using SimcBasedCoRo.ClassSpecific;
using SimcBasedCoRo.Extensions;
using SimcBasedCoRo.Managers;
using SimcBasedCoRo.Utilities;
using Styx;
using Styx.Common.Helpers;
using Styx.CommonBot.Routines;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;

namespace SimcBasedCoRo
{
    // ReSharper disable ClassNeverInstantiated.Global

    public class SimCraftCombatRoutine : CombatRoutine
    {
        #region Fields

        private static readonly WaitTimer _waitForLatencyCheck = new WaitTimer(TimeSpan.FromSeconds(5));
        private static readonly WaitTimer _waitForEnemiesCheck = new WaitTimer(TimeSpan.FromMilliseconds(500));
        private static bool _useAoe = true;

        private ActionList _currentActionList;
        private WoWSpec _specialization;

        #endregion

        #region Properties

        public static uint Latency { get; private set; }
        public static WoWUnit[] ActiveEnemies { get; private set; }

        #region Properties

        public static bool UseAoe
        {
            get { return _useAoe; }
            set { _useAoe = value; }
        }

        #endregion

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
                        _currentActionList = DeathKnight.UnholyActionList;
                        break;
                    case WoWSpec.MageArcane:
                        _currentActionList = Mage.ArcaneActionList;
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

            if (_waitForEnemiesCheck.IsFinished)
            {
                ActiveEnemies = ObjectManager.ObjectList.OfType<WoWUnit>().Where(u => u != null && u.IsAggressive()).ToArray();
                _waitForEnemiesCheck.Reset();
            }
        }

        public override void Initialize()
        {
            HotkeysManager.RegisterHotKeys();
        }

        public override void ShutDown()
        {
            HotkeysManager.RemoveHotkeys();
        }

        #endregion
    }

    // ReSharper restore ClassNeverInstantiated.Global
}
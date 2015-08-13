using System;
using System.Globalization;
using System.Linq;
using SimcBasedCoRo.ClassSpecific;
using SimcBasedCoRo.Extensions;
using SimcBasedCoRo.Utilities;
using Styx;
using Styx.Common;
using Styx.Common.Helpers;
using Styx.CommonBot.Routines;
using Styx.TreeSharp;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;
using HotkeysManager = SimcBasedCoRo.Managers.HotkeysManager;

namespace SimcBasedCoRo
{
    // ReSharper disable ClassNeverInstantiated.Global

    public class SimCraftCombatRoutine : CombatRoutine
    {
        #region Fields

        private static readonly WaitTimer _waitForEnemiesCheck = new WaitTimer(TimeSpan.FromMilliseconds(500));
        private static readonly WaitTimer _waitForLatencyCheck = new WaitTimer(TimeSpan.FromSeconds(5));

        private Composite _combatBehavior;
        private WoWSpec _specialization;

        #endregion

        #region Properties

        public static WoWUnit[] ActiveEnemies { get; private set; }
        public static int Latency { get; private set; }

        public override WoWClass Class
        {
            get { return StyxWoW.Me.Class; }
        }

        public override void OnButtonPress()
        {
        }

        public override Composite CombatBehavior
        {
            get
            {
                Specialization = StyxWoW.Me.Specialization;

                if (_combatBehavior != null) return _combatBehavior;

                return null;
            }
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
                    case WoWSpec.DeathKnightBlood:
                        _combatBehavior = DeathKnight.BloodActionList();
                        break;
                    case WoWSpec.DeathKnightFrost:
                        _combatBehavior = DeathKnight.FrostActionList();
                        break;
                    case WoWSpec.DeathKnightUnholy:
                        _combatBehavior = DeathKnight.UnholyActionList();
                        break;
                    case WoWSpec.MageArcane:
                        _combatBehavior = Mage.ArcaneActionList();
                        break;
                    case WoWSpec.ShamanEnhancement:
                        _combatBehavior = Shaman.EnhancementActionList();
                        break;
                    case WoWSpec.WarriorArms:
                        _combatBehavior = Warrior.ArmsActionList();
                        break;
                }
            }
        }

        #endregion

        #region Public Methods

        public override void Initialize()
        {
            HotkeysManager.RegisterHotKeys();
        }

        public override void Pulse()
        {
            if (_waitForLatencyCheck.IsFinished)
            {
                Latency = Convert.ToInt32(StyxWoW.WoWClient.Latency);
                _waitForLatencyCheck.Reset();
            }

            if (_waitForEnemiesCheck.IsFinished)
            {
                ActiveEnemies = ObjectManager.ObjectList.OfType<WoWUnit>().Where(u => u != null && u.IsAggressive()).ToArray();
                _waitForEnemiesCheck.Reset();
            }

            if (StyxWoW.Me.CurrentTarget != null && StyxWoW.Me.CurrentTarget.Attackable) StyxWoW.Me.CurrentTarget.TimeToDeath();
        }

        public override void ShutDown()
        {
            HotkeysManager.RemoveHotkeys();

            Logging.Write("-- Listing {0} Undefined Spells Referenced --", Spell.UndefinedSpells.Count);
            foreach (var v in Spell.UndefinedSpells)
            {
                Logging.Write("   {0}  {1}", v.Key.PadLeft(25), v.Value.ToString(CultureInfo.InvariantCulture).PadLeft(7));
            }
        }

        #endregion
    }

    // ReSharper restore ClassNeverInstantiated.Global
}
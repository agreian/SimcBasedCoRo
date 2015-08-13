using System;
using System.Collections.Generic;
using System.Linq;
using CommonBehaviors.Actions;
using SimcBasedCoRo.Extensions;
using SimcBasedCoRo.Managers;
using SimcBasedCoRo.Settings;
using SimcBasedCoRo.Utilities;
using Styx;
using Styx.TreeSharp;
using Styx.WoWInternals.WoWObjects;
using Action = Styx.TreeSharp.Action;

namespace SimcBasedCoRo.ClassSpecific.Common
{
    // ReSharper disable InconsistentNaming
    public abstract class ClassSpecificBase
    {
        #region Fields

        public const string ancient_hysteria = "Ancient Hysteria";
        public const string bloodlust = "Bloodlust";
        public const string time_warp = "Time Warp";

        public static readonly string[] BloodlustEquivalents = {ancient_hysteria, bloodlust, time_warp};

        protected static readonly Func<Func<bool>, Composite> arcane_torrent = cond => Spell.BuffSelfAndWait("Arcane Torrent", req => Spell.UseCooldown && cond(), gcd: HasGcd.No);
        protected static readonly Func<Func<bool>, Composite> berserking = cond => Spell.BuffSelfAndWait("Berserking", req => Spell.UseCooldown && cond(), gcd: HasGcd.No);
        protected static readonly Func<Func<bool>, Composite> blood_fury = cond => Spell.BuffSelfAndWait("Blood Fury", req => Spell.UseCooldown && cond(), gcd: HasGcd.No);

        protected static readonly Func<Composite> use_trinket = () =>
        {
            if (SimcBasedCoRoSettings.Instance.Trinket1Usage == TrinketUsage.Never && SimcBasedCoRoSettings.Instance.Trinket2Usage == TrinketUsage.Never)
            {
                return new Action(ret => RunStatus.Failure);
            }

            if (SimcBasedCoRoSettings.IsTrinketUsageWanted(TrinketUsage.OnCooldownInCombat))
            {
                return new Decorator(
                    ret => StyxWoW.Me.Combat && StyxWoW.Me.GotTarget() && ((StyxWoW.Me.IsMelee() && StyxWoW.Me.CurrentTarget.IsWithinMeleeRange) || StyxWoW.Me.CurrentTarget.SpellDistance() < 40) && Spell.UseCooldown,
                    Item.UseEquippedTrinket(TrinketUsage.OnCooldownInCombat));
            }

            return new Action(ret => RunStatus.Failure);
        };

        private static readonly Dictionary<WoWClass, uint> T18ClassTrinketIds = new Dictionary<WoWClass, uint>
        {
            {WoWClass.DeathKnight, 124513}, // Reaper's Harvest
            {WoWClass.Druid, 124514}, // Seed of Creation
            {WoWClass.Hunter, 124515}, // Talisman of the Master Tracker
            {WoWClass.Mage, 124516}, // Tome of Shifting Words
            {WoWClass.Monk, 124517}, // Sacred Draenic Incense
            {WoWClass.Paladin, 124518}, // Libram of Vindication
            {WoWClass.Priest, 124519}, // Repudiation of War
            {WoWClass.Rogue, 124520}, // Bleeding Hollow Toxin Vessel
            {WoWClass.Shaman, 124521}, // Core of the Primal Elements
            {WoWClass.Warlock, 124522}, // Fragment of the Dark Star
            {WoWClass.Warrior, 124523}, // Worldbreaker's Resolve
        };

        private static readonly WoWItemWeaponClass[] _oneHandWeaponClasses = {WoWItemWeaponClass.Axe, WoWItemWeaponClass.Mace, WoWItemWeaponClass.Sword, WoWItemWeaponClass.Dagger, WoWItemWeaponClass.Fist};
        private static double? _baseGcd;

        private static WoWUnit _unitInterrupt;

        #endregion

        #region Properties

        public static double gcd_max
        {
            get
            {
                if (_baseGcd == null)
                {
                    switch (Me.Class)
                    {
                        case WoWClass.DeathKnight:
                        case WoWClass.Hunter:
                        case WoWClass.Monk:
                        case WoWClass.Rogue:
                            _baseGcd = 1;
                            break;
                        case WoWClass.Druid:
                            _baseGcd = Me.Shapeshift == ShapeshiftForm.Cat ? 1 : 1.5;
                            break;
                        default:
                            _baseGcd = 1.5;
                            break;
                    }
                }

                var gcdMax = _baseGcd.Value * Me.SpellHasteModifier;

                return gcdMax < 1 ? 1.0 : gcdMax;
            }
        }

        protected static LocalPlayer Me
        {
            get { return StyxWoW.Me; }
        }

        protected static int active_enemies
        {
            get { return Spell.UseAoe ? SimCraftCombatRoutine.ActiveEnemies.Count() : 1; }
        }

        protected static double gcd
        {
            get { return gcd_max; }
        }

        protected static string prev_gcd
        {
            get { return Spell.LastSpellCast; }
        }


        protected static double spell_haste
        {
            get { return StyxWoW.Me.SpellHasteModifier; }
        }

        protected static bool t18_class_trinket
        {
            get
            {
                if (!T18ClassTrinketIds.ContainsKey(Me.Class)) return false;
                var classTrinketId = T18ClassTrinketIds[Me.Class];

                var trinket1 = StyxWoW.Me.Inventory.GetItemBySlot((uint) WoWInventorySlot.Trinket1);
                var trinket2 = StyxWoW.Me.Inventory.GetItemBySlot((uint) WoWInventorySlot.Trinket2);

                if (trinket1 != null && trinket2 != null) return trinket1.ItemInfo.Id == classTrinketId || trinket2.ItemInfo.Id == classTrinketId;
                if (trinket1 != null) return trinket1.ItemInfo.Id == classTrinketId;
                if (trinket2 != null) return trinket2.ItemInfo.Id == classTrinketId;

                return false;
            }
        }

        #endregion

        #region Public Methods

        public static Composite auto_kick()
        {
            if (SimcBasedCoRoSettings.Instance.InterruptTarget == CheckTargets.None)
                return new ActionAlwaysFail();

            Composite actionSelectTarget;
            if (SimcBasedCoRoSettings.Instance.InterruptTarget == CheckTargets.Current)
                actionSelectTarget = new Action(
                    ret =>
                    {
                        _unitInterrupt = null;
                        //if (Me.Class == WoWClass.Shaman && Shaman.Totems.Exist(WoWTotem.Grounding))
                        //    return RunStatus.Failure;

                        WoWUnit u = Me.CurrentTarget;
                        _unitInterrupt = IsInterruptTarget(u) ? u : null;

                        return _unitInterrupt == null ? RunStatus.Failure : RunStatus.Success;
                    }
                    );
            else // if ( SingularSettings.Instance.InterruptTarget == InterruptType.All )
            {
                actionSelectTarget = new Action(
                    ret =>
                    {
                        _unitInterrupt = null;
                        //if (Me.Class == WoWClass.Shaman && Shaman.Totems.Exist(WoWTotem.Grounding))
                        //    return RunStatus.Failure;

                        _unitInterrupt = SimCraftCombatRoutine.ActiveEnemies.Where(IsInterruptTarget).OrderBy(u => u.Distance).FirstOrDefault();

                        return _unitInterrupt == null ? RunStatus.Failure : RunStatus.Success;
                    }
                    );
            }

            var prioSpell = new PrioritySelector();

            #region Pet Spells First!

            if (Me.Class == WoWClass.Warlock)
            {
                // this will be either a Optical Blast or Spell Lock
                prioSpell.AddChild(Spell.Cast("Command Demon", on => _unitInterrupt,
                    ret => _unitInterrupt != null && _unitInterrupt.Distance < 40 && (Warlock.GetCurrentPet() == Warlock.WarlockPet.Felhunter || Warlock.GetCurrentPet() == Warlock.WarlockPet.Doomguard)));
            }

            #endregion

            #region Melee Range

            if (Me.Class == WoWClass.Paladin)
                prioSpell.AddChild(Spell.Cast("Rebuke", ctx => _unitInterrupt));

            if (Me.Class == WoWClass.Rogue)
            {
                prioSpell.AddChild(Spell.Cast("Kick", ctx => _unitInterrupt));
                prioSpell.AddChild(TalentManager.HasGlyph("Gouge")
                    ? Spell.Cast("Gouge", ctx => _unitInterrupt, ret => !_unitInterrupt.IsBoss() && Me.IsSafelyFacing(_unitInterrupt, 150f))
                    : Spell.Cast("Gouge", ctx => _unitInterrupt, ret => !_unitInterrupt.IsBoss() && Me.IsSafelyFacing(_unitInterrupt, 150f) && _unitInterrupt.IsSafelyFacing(Me, 150f)));
            }

            if (Me.Class == WoWClass.Warrior)
                prioSpell.AddChild(Spell.Cast("Pummel", ctx => _unitInterrupt));

            if (Me.Class == WoWClass.Monk)
                prioSpell.AddChild(Spell.Cast("Spear Hand Strike", ctx => _unitInterrupt));

            if (Me.Class == WoWClass.Druid)
            {
                // Spell.Cast("Skull Bash (Cat)", ctx => _unitInterrupt, ret => StyxWoW.Me.Shapeshift == ShapeshiftForm.Cat));
                // Spell.Cast("Skull Bash (Bear)", ctx => _unitInterrupt, ret => StyxWoW.Me.Shapeshift == ShapeshiftForm.Bear));
                prioSpell.AddChild(Spell.Cast("Skull Bash", ctx => _unitInterrupt, ret => StyxWoW.Me.Shapeshift == ShapeshiftForm.Bear || StyxWoW.Me.Shapeshift == ShapeshiftForm.Cat));
                prioSpell.AddChild(Spell.Cast("Mighty Bash", ctx => _unitInterrupt, ret => !_unitInterrupt.IsBoss() && _unitInterrupt.IsWithinMeleeRange));
            }

            if (Me.Class == WoWClass.DeathKnight)
                prioSpell.AddChild(Spell.Cast("Mind Freeze", ctx => _unitInterrupt));

            if (Me.Race == WoWRace.Pandaren)
                prioSpell.AddChild(Spell.Cast("Quaking Palm", ctx => _unitInterrupt));

            #endregion

            #region 8 Yard Range

            if (Me.Race == WoWRace.BloodElf)
                prioSpell.AddChild(Spell.Cast("Arcane Torrent", ctx => _unitInterrupt, req => _unitInterrupt.Distance < 8 && !SimCraftCombatRoutine.ActiveEnemies.Any(u => u.IsSensitiveDamage(8))));

            if (Me.Race == WoWRace.Tauren)
                prioSpell.AddChild(Spell.Cast("War Stomp", ctx => _unitInterrupt, ret => _unitInterrupt.Distance < 8 && !_unitInterrupt.IsBoss() && !SimCraftCombatRoutine.ActiveEnemies.Any(u => u.IsSensitiveDamage(8))));

            #endregion

            #region 10 Yards

            if (Me.Class == WoWClass.Paladin)
                prioSpell.AddChild(Spell.Cast("Hammer of Justice", ctx => _unitInterrupt));

            if (Me.Specialization == WoWSpec.DruidBalance)
                prioSpell.AddChild(Spell.Cast("Hammer of Justice", ctx => _unitInterrupt));

            if (Me.Class == WoWClass.Warrior)
                prioSpell.AddChild(Spell.Cast("Disrupting Shout", ctx => _unitInterrupt));

            #endregion

            #region 25 yards

            if (Me.Class == WoWClass.Shaman)
                prioSpell.AddChild(Spell.Cast("Wind Shear", ctx => _unitInterrupt, req => Me.IsSafelyFacing(_unitInterrupt)));

            #endregion

            #region 30 yards

            // Druid
            if (TalentManager.HasGlyph("Fae Silence"))
                prioSpell.AddChild(Spell.Cast("Faerie Fire", ctx => _unitInterrupt, req => Me.Shapeshift == ShapeshiftForm.Bear));

            if (Me.Specialization == WoWSpec.PaladinProtection)
                prioSpell.AddChild(Spell.Cast("Avenger's Shield", ctx => _unitInterrupt));

            if (Me.Class == WoWClass.Warrior && TalentManager.HasGlyph("Gag Order"))
                // Gag Order only works on non-bosses due to it being a silence, not an interrupt!
                prioSpell.AddChild(Spell.Cast("Heroic Throw", ctx => _unitInterrupt, ret => !_unitInterrupt.IsBoss()));

            if (Me.Class == WoWClass.Priest)
                prioSpell.AddChild(Spell.Cast("Silence", ctx => _unitInterrupt));

            if (Me.Class == WoWClass.DeathKnight && Me.CurrentTarget != null && Me.CurrentTarget.IsPlayer && Me.CurrentTarget.IsAggressive())
                prioSpell.AddChild(Spell.Cast("Strangulate", ctx => _unitInterrupt));

            if (Me.Class == WoWClass.Mage)
                prioSpell.AddChild(Spell.Cast("Frostjaw", ctx => _unitInterrupt));

            #endregion

            #region 40 yards

            if (Me.Class == WoWClass.Mage)
                prioSpell.AddChild(Spell.Cast("Counterspell", ctx => _unitInterrupt));

            if (Me.Class == WoWClass.Hunter)
                prioSpell.AddChild(Spell.Cast("Counter Shot", ctx => _unitInterrupt));

            if (Me.Specialization == WoWSpec.HunterMarksmanship)
                prioSpell.AddChild(Spell.Cast("Silencing Shot", ctx => _unitInterrupt));

            if (Me.Class == WoWClass.Druid)
                prioSpell.AddChild(Spell.Cast("Solar Beam", ctx => _unitInterrupt, ret => StyxWoW.Me.Shapeshift == ShapeshiftForm.Moonkin));

            if (Me.Specialization == WoWSpec.ShamanElemental || Me.Specialization == WoWSpec.ShamanEnhancement)
                prioSpell.AddChild(Spell.Cast("Solar Beam", ctx => _unitInterrupt, ret => true));

            #endregion

            return new ThrottlePasses(2, TimeSpan.FromMilliseconds(500),
                new Decorator(
                    req => Spell.UseAutoKick,
                    new Sequence(
                        actionSelectTarget,
                        // majority of these are off GCD, so throttle all to avoid most fail messages
                        prioSpell
                        )
                    ));
        }

        #endregion

        #region Private Methods

        protected static IOrderedEnumerable<WoWUnit> Enemies(byte distance)
        {
            return SimCraftCombatRoutine.ActiveEnemies.Where(x => x.Distance <= distance).OrderBy(x => x.Distance);
        }

        protected static int EnemiesCountNearTarget(WoWUnit target, byte distance)
        {
            return SimCraftCombatRoutine.ActiveEnemies.Where(x => target != x).Count(x => target.Location.Distance(x.Location) <= distance);
        }

        protected static int time_to_die(WoWUnit target, int indeterminateValue)
        {
            return target.TimeToDeath(indeterminateValue);
        }

        private static bool IsInterruptTarget(WoWUnit u)
        {
            if (u == null || !u.IsCasting)
                return false;

            if (!u.CanInterruptCurrentSpellCast)
            {
                //if (!SingularSettings.Debug)
                //    Logger.WriteDebug("IsInterruptTarget: {0} casting {1} but CanInterruptCurrentSpellCast == false", u.SafeName(), (u.CastingSpell == null ? "(null)" : u.CastingSpell.Name));
                return false;
            }

            if (!u.InLineOfSpellSight)
            {
                //if (!SingularSettings.Debug)
                //    Logger.WriteDebug("IsInterruptTarget: {0} casting {1} but LoSS == false", u.SafeName(), (u.CastingSpell == null ? "(null)" : u.CastingSpell.Name));
                return false;
            }

            if (!StyxWoW.Me.IsSafelyFacing(u))
            {
                //if (!SingularSettings.Debug)
                //    Logger.WriteDebug("IsInterruptTarget: {0} casting {1} but Facing == false", u.SafeName(), (u.CastingSpell == null ? "(null)" : u.CastingSpell.Name));
                return false;
            }

            if (u.CurrentCastTimeLeft.TotalMilliseconds < 250)
            {
                // not worth interrupting at this point
                return false;
            }

            return true;
        }

        #endregion

        #region Types

        protected static class health
        {
            #region Properties

            public static double pct
            {
                get { return Me.HealthPercent; }
            }

            #endregion
        }

        protected static class main_hand
        {
            #region Properties

            public static bool _1h
            {
                get { return Me.Inventory.Equipped.MainHand != null && _oneHandWeaponClasses.Contains(Me.Inventory.Equipped.MainHand.ItemInfo.WeaponClass); }
            }

            public static bool _2h
            {
                get { return _1h == false; }
            }

            #endregion
        }

        protected static class mana
        {
            #region Properties

            public static double pct
            {
                get { return Me.ManaPercent; }
            }

            #endregion
        }

        protected static class set_bonus
        {
            #region fields

            private static readonly WoWInventorySlot[] _setPartsSlots =
            {
                WoWInventorySlot.Chest,
                WoWInventorySlot.Hands,
                WoWInventorySlot.Head,
                WoWInventorySlot.Legs,
                WoWInventorySlot.Shoulder
            };

            private static readonly Dictionary<WoWClass, uint[]> _t17Sets = new Dictionary<WoWClass, uint[]>
            {
                {WoWClass.DeathKnight, new uint[] {115535, 115536, 115537, 115538, 115539}},
                {WoWClass.Druid, new uint[] {115540, 115541, 115542, 115543, 115544}},
                {WoWClass.Hunter, new uint[] {115545, 115546, 115547, 115548, 115549}},
                {WoWClass.Mage, new uint[] {115550, 115551, 115552, 115553, 115554}},
                {WoWClass.Monk, new uint[] {115555, 115556, 115557, 115558, 115559}},
                {WoWClass.Paladin, new uint[] {115565, 115566, 115567, 115568, 115569}},
                {WoWClass.Priest, new uint[] {115560, 115561, 115562, 115563, 115564}},
                {WoWClass.Rogue, new uint[] {115570, 115571, 115572, 115573, 115574}},
                {WoWClass.Shaman, new uint[] {115575, 115576, 115577, 115578, 115579}},
                {WoWClass.Warlock, new uint[] {115585, 115586, 115587, 115588, 115589}},
                {WoWClass.Warrior, new uint[] {115580, 115581, 115582, 115583, 115584}}
            };

            private static readonly Dictionary<WoWClass, uint[]> _t18Sets = new Dictionary<WoWClass, uint[]>
            {
                {WoWClass.DeathKnight, new uint[] {124317, 124327, 124332, 124338, 124344}},
                {WoWClass.Druid, new uint[] {124246, 124255, 124261, 124267, 124272}},
                {WoWClass.Hunter, new uint[] {124284, 124292, 124296, 124301, 124307}},
                {WoWClass.Mage, new uint[] {124154, 124160, 124165, 124171, 124177}},
                {WoWClass.Monk, new uint[] {124247, 124256, 124262, 124268, 124273}},
                {WoWClass.Paladin, new uint[] {124318, 124328, 124333, 124339, 124345}},
                {WoWClass.Priest, new uint[] {124155, 124161, 124166, 124172, 124178}},
                {WoWClass.Rogue, new uint[] {124248, 124257, 124263, 124269, 124274}},
                {WoWClass.Shaman, new uint[] {124293, 124297, 124302, 124303, 124308}},
                {WoWClass.Warlock, new uint[] {124156, 124162, 124167, 124173, 124179}},
                {WoWClass.Warrior, new uint[] {124319, 124329, 124334, 124340, 124346}}
            };

            #endregion

            #region Properties

            public static bool tier17_2pc
            {
                get { return SetPartsCount(_t17Sets) >= 2; }
            }

            public static bool tier17_4pc
            {
                get { return SetPartsCount(_t17Sets) >= 4; }
            }

            public static bool tier18_2pc
            {
                get { return SetPartsCount(_t18Sets) >= 2; }
            }

            public static bool tier18_4pc
            {
                get { return SetPartsCount(_t18Sets) >= 4; }
            }

            #endregion

            #region Private Methods

            private static int SetPartsCount(IReadOnlyDictionary<WoWClass, uint[]> set)
            {
                if (!set.ContainsKey(Me.Class)) return 0;
                var ids = set[Me.Class];

                return _setPartsSlots.Select(woWInventorySlot => StyxWoW.Me.Inventory.GetItemBySlot((uint) woWInventorySlot)).Count(item => item != null && ids.Contains(item.ItemInfo.Id));
            }

            #endregion
        }

        protected static class target
        {
            // ReSharper disable MemberHidesStaticFromOuterClass

            #region Properties

            public static WoWUnit current
            {
                get { return Me.CurrentTarget; }
            }

            public static double distance
            {
                get { return StyxWoW.Me.CurrentTarget.Distance; }
            }

            public static int time_to_die
            {
                get { return StyxWoW.Me.CurrentTarget.TimeToDeath(); }
            }

            #endregion

            #region Types

            public static class health
            {
                #region Properties

                public static double pct
                {
                    get { return StyxWoW.Me.CurrentTarget.HealthPercent; }
                }

                #endregion
            }

            #endregion
        }

        #endregion
    }
}
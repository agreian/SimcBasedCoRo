using System;
using CommonBehaviors.Actions;
using SimcBasedCoRo.ClassSpecific.Common;
using SimcBasedCoRo.Utilities;
using Styx.TreeSharp;

namespace SimcBasedCoRo.ClassSpecific
{
    public class Shaman : ClassSpecificBase
    {
        #region Fields

        private static readonly Func<Func<bool>, Composite> lightning_shield = cond => Spell.BuffSelf(ShamanSpells.lightning_shield, req => cond());

        #endregion

        #region Enums

        public enum ShamanTalents
        {
            NaturesGuardian = 1,
            StoneBulwarkTotem,
            AstralShift,

            FrozenPower,
            EarthgrabTotem,
            WindwalkTotem,

            CallOfTheElements,
            TotemicPersistence,
            TotemicProjection,

            ElementalMastery,
            AncestralSwiftness,
            EchoOfTheElements,

            RushingStreams,
            AncestralGuidance,
            Conductivity,

            UnleashedFury,
            ElementalBlast,
            PrimalElementalist,

            ElementalFusion,
            CloudburstTotem = ElementalFusion,
            StormElementalTotem,
            LiquidMagma,
            HighTide = LiquidMagma
        }

        #endregion

        #region Public Methods

        public static Composite Buffs()
        {
            return new PrioritySelector(
                lightning_shield(() => true),
                new ActionAlwaysFail()
                );
        }

        public static Composite EnhancementActionList()
        {
            return new Decorator(ret => !Spell.IsGlobalCooldown(), new PrioritySelector(
                auto_kick(),
                use_trinket(),
                new ActionAlwaysFail()
                ));
        }

        public static Composite EnhancementInstancePull()
        {
            return EnhancementActionList();
        }

        #endregion

        #region Types
        
        private static class ShamanSpells
        {
            #region Fields

            public const string lightning_shield = "Lightning Shield";

            #endregion
        }

        #endregion
    }
}
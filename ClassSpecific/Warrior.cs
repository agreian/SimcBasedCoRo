﻿using System;
using System.Linq;
using CommonBehaviors.Actions;
using SimcBasedCoRo.ClassSpecific.Common;
using SimcBasedCoRo.Extensions;
using SimcBasedCoRo.Utilities;
using Styx.TreeSharp;
using Styx.WoWInternals.WoWObjects;

namespace SimcBasedCoRo.ClassSpecific
{
    // ReSharper disable ClassNeverInstantiated.Global
    // ReSharper disable InconsistentNaming
    // ReSharper disable CompareOfFloatsByEqualityOperator
    public class Warrior : ClassSpecificBase
    {
        #region Fields

        private const byte EXECUTE_DISTANCE = 5;
        private const byte REND_DISTANCE = 5;
        private const byte WHIRLWIND_DISTANCE = 8;
        private const byte WHIRLWIND_GLYPH_DISTANCE = 12;

        private static readonly Func<Func<bool>, Composite> avatar = cond => Spell.BuffSelfAndWait(WarriorSpells.avatar, req => Spell.UseCooldown && cond());
        private static readonly Func<Composite> battle_shout = () => Spell.Cast(WarriorSpells.battle_shout, ret => !Me.HasAura(WarriorSpells.battle_shout) && !Me.HasMyAura(WarriorSpells.commanding_shout) && !Me.HasPartyBuff(PartyBuffType.AttackPower));
        private static readonly Func<Func<bool>, Composite> bladestorm = cond => Spell.Cast(WarriorSpells.bladestorm, req => Spell.UseCooldown && Spell.UseAoe && cond());
        private static readonly Func<Func<bool>, Composite> bloodbath = cond => Spell.BuffSelfAndWait(WarriorSpells.bloodbath, req => Spell.UseCooldown && cond(), gcd: HasGcd.No);
        private static readonly Func<Func<bool>, Composite> colossus_smash = cond => Spell.Cast(WarriorSpells.colossus_smash, req => cond());
        private static readonly Func<Composite> commanding_shout = () => Spell.Cast(WarriorSpells.commanding_shout, ret => !Me.HasAura(WarriorSpells.battle_shout) && !Me.HasMyAura(WarriorSpells.commanding_shout) && !Me.HasPartyBuff(PartyBuffType.Stamina));
        private static readonly Func<Composite> die_by_the_sword = () => Spell.BuffSelf(WarriorSpells.die_by_the_sword, req => Spell.UseCooldown && health.pct < 50, HasGcd.No);
        private static readonly Func<Func<bool>, Composite> dragon_roar = cond => Spell.Cast(WarriorSpells.dragon_roar, req => Spell.UseAoe && cond());
        private static readonly Func<Func<WoWUnit>, Func<bool>, Composite> execute = (target, cond) => Spell.Cast(WarriorSpells.execute, on => target(), req => target() != null && cond());
        private static readonly Func<Func<bool>, Composite> heroic_throw = cond => Spell.Cast(WarriorSpells.heroic_throw, req => cond());
        private static readonly Func<Func<bool>, Composite> impending_victory = cond => Spell.Cast(WarriorSpells.impending_victory, req => cond());
        private static readonly Func<Func<bool>, Composite> mortal_strike = cond => Spell.Cast(WarriorSpells.mortal_strike, req => cond());
        private static readonly Func<Composite> rallying_cry = () => Spell.BuffSelf(WarriorSpells.rallying_cry, req => Spell.UseCooldown && health.pct < 30, HasGcd.No);
        private static readonly Func<Func<bool>, Composite> ravager = cond => Spell.CastOnGround(WarriorSpells.ravager, on => Me.CurrentTarget, req => Spell.UseCooldown && Spell.UseAoe && cond());
        private static readonly Func<Func<bool>, Composite> recklessness = cond => Spell.BuffSelfAndWait(WarriorSpells.recklessness, req => Spell.UseCooldown && cond(), gcd: HasGcd.No);
        private static readonly Func<Func<WoWUnit>, Func<bool>, Composite> rend = (target, cond) => Spell.Cast(WarriorSpells.rend, on => target(), req => target() != null && cond());
        private static readonly Func<Func<bool>, Composite> shockwave = cond => Spell.Cast(WarriorSpells.shockwave, req => cond());
        private static readonly Func<Func<bool>, Composite> siegebreaker = cond => Spell.Cast(WarriorSpells.siegebreaker, req => cond());
        private static readonly Func<Func<bool>, Composite> slam = cond => Spell.Cast(WarriorSpells.slam, req => cond());
        private static readonly Func<Composite> spell_reflect = () => Spell.Cast(WarriorSpells.spell_reflect, on => SpellReflectTarget, req => SpellReflectTarget != null);
        private static readonly Func<Func<bool>, Composite> storm_bolt = cond => Spell.Cast(WarriorSpells.storm_bolt, req => cond());
        private static readonly Func<Func<bool>, Composite> sweeping_strikes = cond => Spell.BuffSelfAndWait(WarriorSpells.sweeping_strikes, req => Spell.UseAoe && cond(), gcd: HasGcd.No);
        private static readonly Func<Func<bool>, Composite> thunder_clap = cond => Spell.Cast(WarriorSpells.thunder_clap, req => Spell.UseAoe && cond());
        private static readonly Func<Composite> victory_rush = () => Spell.Cast(WarriorSpells.victory_rush, ret => !talent.impending_victory.enabled && health.pct < 80 && buff.victory_rush.up);
        private static readonly Func<Func<bool>, Composite> whirlwind = cond => Spell.Cast(WarriorSpells.whirlwind, req => cond());

        #endregion

        #region Enums

        public enum WarriorTalents
        {
            // ReSharper disable UnusedMember.Local
            Juggernaut = 1,
            DoubleTime,
            Warbringer,

            EnragedRegeneration,
            SecondWind,
            ImpendingVictory,

            TasteForBlood,
            FuriousStrikes = TasteForBlood,
            HeavyRepercussions = TasteForBlood,
            SuddenDeath,
            Slam,
            UnquenchableThirst = Slam,
            UnyieldingStrikes = Slam,

            StormBolt,
            Shockwave,
            DragonRoar,

            MassSpellReflection,
            Safeguard,
            Vigilance,

            Avatar,
            Bloodbath,
            Bladestorm,

            AngerManagement,
            Ravager,
            Siegebreaker,
            GladiatorsResolve = Siegebreaker
            // ReSharper restore UnusedMember.Local
        }

        #endregion

        #region Properties

        public static WoWUnit SpellReflectTarget
        {
            get { return SimCraftCombatRoutine.ActiveEnemies.FirstOrDefault(u => u.IsCasting && u.CurrentTarget == Me && (!u.CanInterruptCurrentSpellCast || Spell.IsSpellOnCooldown(WarriorSpells.pummel) || !Spell.CanCastHack(WarriorSpells.pummel, u))); }
        }

        public static uint rage
        {
            get { return Me.CurrentRage; }
        }

        public static uint rage_deficit
        {
            get { return rage_max - rage; }
        }

        public static uint rage_max
        {
            get { return Me.MaxRage; }
        }

        #endregion

        #region Public Methods

        public static Composite ArmsActionList()
        {
            return new Decorator(ret => !Spell.IsGlobalCooldown(), new PrioritySelector(
                auto_kick(),
                use_trinket(),
                victory_rush(),
                spell_reflect(),
                die_by_the_sword(),
                rallying_cry(),
                //actions=charge,if=debuff.charge.down
                //actions+=/auto_attack
                //# This is mostly to prevent cooldowns from being accidentally used during movement.
                //actions+=/run_action_list,name=movement,if=movement.distance>5
                new Decorator(ArmsMovement(), req => Me.IsMoving && !Me.CurrentTarget.IsWithinMeleeRange),
                //actions+=/use_item,name=thorasus_the_stone_heart_of_draenor,if=(buff.bloodbath.up|(!talent.bloodbath.enabled&debuff.colossus_smash.up))
                //actions+=/potion,name=draenic_strength,if=(target.health.pct<20&buff.recklessness.up)|target.time_to_die<25
                //# This incredibly long line (Due to differing talent choices) says 'Use recklessness on cooldown with colossus smash, unless the boss will die before the ability is usable again, and then use it with execute.'
                //actions+=/recklessness,if=(((target.time_to_die>190|target.health.pct<20)&(buff.bloodbath.up|!talent.bloodbath.enabled))|target.time_to_die<=12|talent.anger_management.enabled)&((desired_targets=1&!raid_event.adds.exists)|!talent.bladestorm.enabled)
                recklessness(() => (((target.time_to_die > 190 || target.health.pct < 20) && (buff.bloodbath.up || !talent.bloodbath.enabled)) || target.time_to_die <= 12 || talent.anger_management.enabled) && (!talent.bladestorm.enabled)),
                //actions+=/bloodbath,if=(dot.rend.ticking&cooldown.colossus_smash.remains<5&((talent.ravager.enabled&prev_gcd.ravager)|!talent.ravager.enabled))|target.time_to_die<20
                bloodbath(() => (dot.rend.ticking && cooldown.colossus_smash.remains < 5 && ((talent.ravager.enabled && prev_gcd == WarriorSpells.ravager) || !talent.ravager.enabled)) || target.time_to_die < 20),
                //actions+=/avatar,if=buff.recklessness.up|target.time_to_die<25
                avatar(() => buff.recklessness.up || target.time_to_die < 25),
                //actions+=/blood_fury,if=buff.bloodbath.up|(!talent.bloodbath.enabled&debuff.colossus_smash.up)|buff.recklessness.up
                blood_fury(() => buff.bloodbath.up || (!talent.bloodbath.enabled && debuff.colossus_smash.up) || buff.recklessness.up),
                //actions+=/berserking,if=buff.bloodbath.up|(!talent.bloodbath.enabled&debuff.colossus_smash.up)|buff.recklessness.up
                berserking(() => buff.bloodbath.up || (!talent.bloodbath.enabled && debuff.colossus_smash.up) || buff.recklessness.up),
                //actions+=/arcane_torrent,if=rage<rage.max-40
                arcane_torrent(() => rage < rage_max - 40),
                //actions+=/heroic_leap,if=(raid_event.movement.distance>25&raid_event.movement.in>45)|!raid_event.movement.exists
                //heroic_leap(),
                //actions+=/call_action_list,name=aoe,if=spell_targets.whirlwind>1
                new Decorator(ArmsAoe(), req => spell_targets.whirlwind > 1),
                //actions+=/call_action_list,name=single
                new Decorator(ArmsSingle()),
                new ActionAlwaysFail()
                ));
        }

        public static Composite ArmsInstancePull()
        {
            return ArmsActionList();
        }

        public static Composite Buffs()
        {
            return new PrioritySelector(
                battle_shout(),
                commanding_shout(),
                new ActionAlwaysFail()
                );
        }

        #endregion

        #region Private Methods

        private static Composite ArmsAoe()
        {
            return new PrioritySelector(
                //actions.aoe=sweeping_strikes
                sweeping_strikes(() => true),
                //actions.aoe+=/rend,if=dot.rend.remains<5.4&target.time_to_die>4
                rend(() => Me.CurrentTarget, () => dot.rend.remains < 5.4 && target.time_to_die > 4),
                //actions.aoe+=/rend,cycle_targets=1,max_cycle_targets=2,if=dot.rend.remains<5.4&target.time_to_die>8&!buff.colossus_smash_up.up&talent.taste_for_blood.enabled
                rend(() => Enemies(REND_DISTANCE).Take(2).FirstOrDefault(x => dot.rend.Remains(x) < 5.4 && time_to_die(x, 8) > 8), () => !buff.colossus_smash_up.up && talent.taste_for_blood.enabled),
                //actions.aoe+=/rend,cycle_targets=1,if=dot.rend.remains<5.4&target.time_to_die-remains>18&!buff.colossus_smash_up.up&spell_targets.whirlwind<=8
                rend(() => Enemies(REND_DISTANCE).FirstOrDefault(x => dot.rend.Remains(x) < 5.4 && time_to_die(x, 18) - dot.rend.Remains(x) > 18), () => !buff.colossus_smash_up.up && spell_targets.whirlwind <= 8),
                //actions.aoe+=/ravager,if=buff.bloodbath.up|cooldown.colossus_smash.remains<4
                ravager(() => buff.bloodbath.up || cooldown.colossus_smash.remains < 4),
                //actions.aoe+=/bladestorm,if=((debuff.colossus_smash.up|cooldown.colossus_smash.remains>3)&target.health.pct>20)|(target.health.pct<20&rage<30&cooldown.colossus_smash.remains>4)
                bladestorm(() => ((debuff.colossus_smash.up || cooldown.colossus_smash.remains > 3) && target.health.pct > 20) || (target.health.pct < 20 && rage < 30 && cooldown.colossus_smash.remains > 4)),
                //actions.aoe+=/colossus_smash,if=dot.rend.ticking
                colossus_smash(() => dot.rend.ticking),
                //actions.aoe+=/execute,cycle_targets=1,if=!buff.sudden_death.react&spell_targets.whirlwind<=8&((rage>72&cooldown.colossus_smash.remains>gcd)|rage>80|target.time_to_die<5|debuff.colossus_smash.up)
                execute(
                    () =>
                        Enemies(EXECUTE_DISTANCE).FirstOrDefault(x => !buff.sudden_death.react && spell_targets.whirlwind <= 8 && ((rage > 72 && cooldown.colossus_smash.remains > gcd) || rage > 80 || time_to_die(x, 5) < 5 || debuff.colossus_smash.Up(x))),
                    () => true),
                //actions.aoe+=/heroic_charge,cycle_targets=1,if=target.health.pct<20&rage<70&swing.mh.remains>2&debuff.charge.down
                //# Heroic Charge is an event that makes the warrior heroic leap out of melee range for an instant
                //#If heroic leap is not available, the warrior will simply run out of melee to charge range, and then charge back in.
                //#This can delay autoattacks, but typically the rage gained from charging (Especially with bull rush glyphed) is more than
                //#The amount lost from delayed autoattacks. Charge only grants rage from charging a different target than the last time.
                //#Which means this is only worth doing on AoE, and only when you cycle your charge target.
                //actions.aoe+=/mortal_strike,if=target.health.pct>20&(rage>60|debuff.colossus_smash.up)&spell_targets.whirlwind<=5
                mortal_strike(() => target.health.pct > 20 && (rage > 60 || debuff.colossus_smash.up) && spell_targets.whirlwind <= 5),
                //actions.aoe+=/dragon_roar,if=!debuff.colossus_smash.up
                dragon_roar(() => !debuff.colossus_smash.up),
                //actions.aoe+=/thunder_clap,if=(target.health.pct>20|spell_targets.whirlwind>=9)&glyph.resonating_power.enabled
                thunder_clap(() => (target.health.pct > 20 || spell_targets.whirlwind >= 9) && glyph.resonating_power.enabled),
                //actions.aoe+=/rend,cycle_targets=1,if=dot.rend.remains<5.4&target.time_to_die>8&!buff.colossus_smash_up.up&spell_targets.whirlwind>=9&rage<50&!talent.taste_for_blood.enabled
                rend(() => Enemies(REND_DISTANCE).FirstOrDefault(x => dot.rend.Remains(x) < 5.4 && time_to_die(x, 8) > 8), () => !buff.colossus_smash_up.up && spell_targets.whirlwind >= 9 && rage < 50 && !talent.taste_for_blood.enabled),
                //actions.aoe+=/whirlwind,if=target.health.pct>20|spell_targets.whirlwind>=9
                whirlwind(() => target.health.pct > 20 || spell_targets.whirlwind >= 9),
                //actions.aoe+=/siegebreaker
                siegebreaker(() => true),
                //actions.aoe+=/storm_bolt,if=cooldown.colossus_smash.remains>4|debuff.colossus_smash.up
                storm_bolt(() => cooldown.colossus_smash.remains > 4 || debuff.colossus_smash.up),
                //actions.aoe+=/shockwave
                shockwave(() => true),
                //actions.aoe+=/execute,if=buff.sudden_death.react
                execute(() => Me.CurrentTarget, () => buff.sudden_death.react),
                new ActionAlwaysFail()
                );
        }

        private static Composite ArmsMovement()
        {
            return new PrioritySelector(
                // actions.movement=heroic_leap
                //heroic_leap(),
                // # May as well throw storm bolt if we can.
                // actions.movement+=/storm_bolt
                storm_bolt(() => true),
                // actions.movement+=/heroic_throw
                heroic_throw(() => true),
                new ActionAlwaysSucceed()
                );
        }

        private static Composite ArmsSingle()
        {
            return new PrioritySelector(
                //actions.single=rend,if=target.time_to_die>4&dot.rend.remains<5.4
                rend(() => Me.CurrentTarget, () => target.time_to_die > 4 && dot.rend.remains < 5.4),
                //actions.single+=/ravager,if=cooldown.colossus_smash.remains<4&(!raid_event.adds.exists|raid_event.adds.in>55)
                //actions.single+=/colossus_smash,if=debuff.colossus_smash.down
                colossus_smash(() => debuff.colossus_smash.down),
                //actions.single+=/mortal_strike,if=target.health.pct>20&(debuff.colossus_smash.up|rage>60)
                mortal_strike(() => target.health.pct > 20 && (debuff.colossus_smash.up || rage > 60)),
                //actions.single+=/colossus_smash
                colossus_smash(() => true),
                //actions.single+=/bladestorm,if=(((debuff.colossus_smash.up|cooldown.colossus_smash.remains>3)&target.health.pct>20)|(target.health.pct<20&rage<30&cooldown.colossus_smash.remains>4))&(!raid_event.adds.exists|raid_event.adds.in>55|(talent.anger_management.enabled&raid_event.adds.in>40))
                //actions.single+=/storm_bolt,if=debuff.colossus_smash.down
                storm_bolt(() => debuff.colossus_smash.down),
                //actions.single+=/siegebreaker
                siegebreaker(() => true),
                //actions.single+=/dragon_roar,if=!debuff.colossus_smash.up&(!raid_event.adds.exists|raid_event.adds.in>55|(talent.anger_management.enabled&raid_event.adds.in>40))
                //actions.single+=/execute,if=buff.sudden_death.react
                execute(() => Me.CurrentTarget, () => buff.sudden_death.react),
                //actions.single+=/execute,if=!buff.sudden_death.react&(rage>72&cooldown.colossus_smash.remains>gcd)|debuff.colossus_smash.up|target.time_to_die<5
                execute(() => Me.CurrentTarget, () => !buff.sudden_death.react && (rage > 72 && cooldown.colossus_smash.remains > gcd) || debuff.colossus_smash.up || target.time_to_die < 5),
                //actions.single+=/impending_victory,if=!set_bonus.tier18_4pc&(rage<40&target.health.pct>20&cooldown.colossus_smash.remains>1)
                impending_victory(() => !set_bonus.tier18_4pc && (rage < 40 && target.health.pct > 20 && cooldown.colossus_smash.remains > 1)),
                //actions.single+=/slam,if=(rage>20|cooldown.colossus_smash.remains>gcd)&target.health.pct>20&cooldown.colossus_smash.remains>1&!set_bonus.tier18_4pc
                slam(() => (rage > 20 || cooldown.colossus_smash.remains > gcd) && target.health.pct > 20 && cooldown.colossus_smash.remains > 1 && !set_bonus.tier18_4pc),
                //actions.single+=/thunder_clap,if=(!set_bonus.tier18_4pc|rage.deficit<30)&!talent.slam.enabled&target.health.pct>20&(rage>=40|debuff.colossus_smash.up)&glyph.resonating_power.enabled&cooldown.colossus_smash.remains>gcd
                thunder_clap(() => (!set_bonus.tier18_4pc || rage_deficit < 30) && !talent.slam.enabled && target.health.pct > 20 && (rage >= 40 || debuff.colossus_smash.up) && glyph.resonating_power.enabled && cooldown.colossus_smash.remains > gcd),
                //actions.single+=/whirlwind,if=(!set_bonus.tier18_4pc|rage.deficit<30)&!talent.slam.enabled&target.health.pct>20&(rage>=40|debuff.colossus_smash.up)&cooldown.colossus_smash.remains>gcd
                whirlwind(() => (!set_bonus.tier18_4pc || rage_deficit < 30) && !talent.slam.enabled && target.health.pct > 20 && (rage >= 40 || debuff.colossus_smash.up) && cooldown.colossus_smash.remains > gcd),
                //actions.single+=/shockwave
                shockwave(() => true),
                new ActionAlwaysFail()
                );
        }

        #endregion

        // ReSharper disable MemberHidesStaticFromOuterClass

        #region Types

        public static class WarriorSpells
        {
            #region Fields

            public const string avatar = "Avatar";
            public const string battle_shout = "Battle Shout";
            public const string bladestorm = "Bladestorm";
            public const string bloodbath = "Bloodbath";
            public const string charge = "Charge";
            public const string charge_stun = "Charge Stun";
            public const string colossus_smash = "Colossus Smash";
            public const string commanding_shout = "Commanding Shout";
            public const string die_by_the_sword = "Die by the Sword";
            public const string dragon_roar = "Dragon Roar";
            public const string execute = "Execute";
            public const string heroic_leap = "Heroic Leap";
            public const string heroic_throw = "Heroic Throw";
            public const string impending_victory = "Impending Victory";
            public const string mortal_strike = "Mortal Strike";
            public const string pummel = "Pummel";
            public const string rallying_cry = "Rallying Cry";
            public const string ravager = "Ravager";
            public const string recklessness = "Recklessness";
            public const string rend = "Rend";
            public const string shockwave = "Shockwave";
            public const string siegebreaker = "Siegebreaker";
            public const string slam = "Slam";
            public const string spell_reflect = "Spell Reflect";
            public const string storm_bolt = "Storm Bolt";
            public const int sudden_death = 52437;
            public const string sweeping_strikes = "Sweeping Strikes";
            public const string thunder_clap = "Thunder Clap";
            public const string victorious = "Victorious";
            public const string victory_rush = "Victory Rush";
            public const string warbringer = "Warbringer";
            public const string whirlwind = "Whirlwind";

            #endregion
        }

        public class spell_targets
        {
            #region Properties

            public static int whirlwind
            {
                get { return EnemiesCountNearTarget(Me, glyph.whirlwind.enabled ? WHIRLWIND_GLYPH_DISTANCE : WHIRLWIND_DISTANCE); }
            }

            #endregion
        }

        internal class debuff : DebuffBase
        {
            #region Fields

            public static readonly debuff colossus_smash = new debuff(WarriorSpells.colossus_smash);

            #endregion

            #region Constructors

            private debuff(string spell)
                : base(spell)
            {
            }

            #endregion
        }


        private class buff : BuffBase
        {
            #region Fields

            public static readonly buff bloodbath = new buff(WarriorSpells.bloodbath);
            public static readonly buff colossus_smash_up = new buff(WarriorSpells.colossus_smash);
            public static readonly buff recklessness = new buff(WarriorSpells.recklessness);
            public static readonly buff sudden_death = new buff(WarriorSpells.sudden_death);
            public static readonly buff victory_rush = new buff(WarriorSpells.victorious);

            #endregion

            #region Constructors

            private buff(string spell)
                : base(spell)
            {
            }

            private buff(int spell)
                : base(spell)
            {
            }

            #endregion
        }

        private class cooldown : CooldownBase
        {
            #region Fields

            public static readonly cooldown colossus_smash = new cooldown(WarriorSpells.colossus_smash);

            #endregion

            #region Constructors

            private cooldown(string spell)
                : base(spell)
            {
            }

            #endregion
        }

        private class dot : DotBase
        {
            #region Fields

            public static readonly dot rend = new dot(WarriorSpells.rend);

            #endregion

            #region Constructors

            private dot(string spell)
                : base(spell)
            {
            }

            #endregion
        }

        private class glyph : GlyphBase
        {
            #region Fields

            public static readonly glyph resonating_power = new glyph("Resonating Power");
            public static readonly glyph whirlwind = new glyph("Wind and Thunder");

            #endregion

            #region Constructors

            private glyph(string spellName)
                : base(spellName)
            {
            }

            #endregion
        }

        private class talent : TalentBase
        {
            #region Fields

            public static readonly talent anger_management = new talent((int) WarriorTalents.AngerManagement);
            public static readonly talent bladestorm = new talent((int) WarriorTalents.Bladestorm);
            public static readonly talent bloodbath = new talent((int) WarriorTalents.Bloodbath);
            public static readonly talent impending_victory = new talent((int) WarriorTalents.ImpendingVictory);
            public static readonly talent ravager = new talent((int) WarriorTalents.Ravager);
            public static readonly talent slam = new talent((int) WarriorTalents.Slam);
            public static readonly talent taste_for_blood = new talent((int) WarriorTalents.TasteForBlood);

            #endregion

            #region Constructors

            private talent(int talent)
                : base(talent)
            {
            }

            #endregion
        }

        #endregion
    }
}
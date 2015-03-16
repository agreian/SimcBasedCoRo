using System;
using System.Collections.Generic;
using SimcBasedCoRo.Extensions;
using SimcBasedCoRo.Managers;
using SimcBasedCoRo.Utilities;
using Styx;

namespace SimcBasedCoRo.ClassSpecific
{
    // ReSharper disable InconsistentNaming
    // ReSharper disable once ClassNeverInstantiated.Global
    public class Mage : Common
    {
        #region Fields

        public static readonly Dictionary<string, SpellTypeEnum> Spells = new Dictionary<string, SpellTypeEnum>
        {
            {rune_of_power, SpellTypeEnum.CastOnGround},
            {mirror_image, SpellTypeEnum.Buff},
            {arcane_power, SpellTypeEnum.Buff}
        };

        private const string arcane_power = "Arcane Power";
        private const string evocation = "Evocation";
        private const string mirror_image = "Mirror Image";
        private const string prismatic_crystal = "Prismatic Crystal";
        private const string rune_of_power = "Rune of Power";

        #endregion

        #region Enums

        private enum MageTalentsEnum
        {
            // ReSharper disable UnusedMember.Local
            Evanesce = 1,
            BlazingSpeed,
            IceFloes,

            AlterTime,
            Flameglow,
            IceBarrier,

            RingOfFrost,
            IceWard,
            Frostjaw,

            GreaterInvisibility,
            Cauterize,
            ColdSnap,

            NetherTempest,
            LivingBomb = NetherTempest,
            FrostBomb = NetherTempest,
            UnstableMagic,
            Supernova,
            BlastWave = Supernova,
            IceNova = Supernova,

            MirrorImage,
            RuneOfPower,
            IncantersFlow,

            Overpowered,
            Kindling = Overpowered,
            ThermalVoid = Overpowered,
            PrismaticCrystal,
            ArcaneOrb,
            Meteor = ArcaneOrb,
            CometStorm = ArcaneOrb
            // ReSharper restore UnusedMember.Local
        }

        #endregion

        #region Properties

        public static ActionList ArcaneActionList
        {
            get
            {
                return new ActionList
                {
                    //# Executed every time the actor is available.
                    //actions=counterspell,if=target.debuff.casting.react
                    //actions+=/blink,if=movement.distance>10
                    //actions+=/blazing_speed,if=movement.remains>0
                    //actions+=/cold_snap,if=health.pct<30
                    //actions+=/time_warp,if=target.health.pct<25|time>5
                    //actions+=/ice_floes,if=buff.ice_floes.down&(raid_event.movement.distance>0|raid_event.movement.in<action.arcane_missiles.cast_time)
                    //actions+=/rune_of_power,if=buff.rune_of_power.remains<cast_time
                    new Spell(rune_of_power, () => buff.rune_of_power_remain < cast_time(rune_of_power), () => Me),
                    //actions+=/mirror_image
                    new Spell(mirror_image),
                    //actions+=/cold_snap,if=buff.presence_of_mind.down&cooldown.presence_of_mind.remains>75
                    //actions+=/call_action_list,name=aoe,if=active_enemies>=5
                    new ActionList(arcane_aoe, () => active_enemies >= 5),
                    //actions+=/call_action_list,name=init_crystal,if=talent.prismatic_crystal.enabled&cooldown.prismatic_crystal.up
                    new ActionList(arcane_init_crystal, () => talent.prismatic_crystal_enabled && cooldown.prismatic_crystal_up),
                    //actions+=/call_action_list,name=crystal_sequence,if=talent.prismatic_crystal.enabled&pet.prismatic_crystal.active
                    new ActionList(arcane_crystal_sequence, () => talent.prismatic_crystal_enabled && pet.prismatic_crystal_active),
                    //actions+=/call_action_list,name=burn,if=time_to_die<mana.pct*0.35*spell_haste|cooldown.evocation.remains<=(mana.pct-30)*0.3*spell_haste|(buff.arcane_power.up&cooldown.evocation.remains<=(mana.pct-30)*0.4*spell_haste)
                    new ActionList(arcane_burn,
                        () => target.time_to_die < mana_pct*0.35*spell_haste || cooldown.evocation_remains <= (mana_pct - 30)*0.3*spell_haste || (buff.arcane_power_up && cooldown.evocation_remains <= (mana_pct - 30)*0.4*spell_haste)),
                    //actions+=/call_action_list,name=conserve
                    new ActionList(arcane_conserve)
                };
            }
        }

        private static ActionList arcane_aoe
        {
            get
            {
                return new ActionList
                {
                    //actions.aoe=call_action_list,name=cooldowns
                    new ActionList(arcane_cooldowns)
                    //actions.aoe+=/nether_tempest,cycle_targets=1,if=buff.arcane_charge.stack=4&(active_dot.nether_tempest=0|(ticking&remains<3.6))
                    //actions.aoe+=/supernova
                    //actions.aoe+=/arcane_orb,if=buff.arcane_charge.stack<4
                    //actions.aoe+=/arcane_explosion,if=prev_gcd.evocation
                    //actions.aoe+=/evocation,interrupt_if=mana.pct>96,if=mana.pct<85-2.5*buff.arcane_charge.stack
                    //actions.aoe+=/arcane_missiles,if=set_bonus.tier17_4pc&active_enemies<10&buff.arcane_charge.stack=4&buff.arcane_instability.react
                    //actions.aoe+=/nether_tempest,cycle_targets=1,if=talent.arcane_orb.enabled&buff.arcane_charge.stack=4&ticking&remains<cooldown.arcane_orb.remains
                    //actions.aoe+=/arcane_barrage,if=buff.arcane_charge.stack=4
                    //actions.aoe+=/cone_of_cold,if=glyph.cone_of_cold.enabled
                    //actions.aoe+=/arcane_explosion
                };
            }
        }

        private static ActionList arcane_burn
        {
            get
            {
                return new ActionList
                {
                    //actions.burn=call_action_list,name=cooldowns
                    //actions.burn+=/arcane_missiles,if=buff.arcane_missiles.react=3
                    //actions.burn+=/arcane_missiles,if=set_bonus.tier17_4pc&buff.arcane_instability.react&buff.arcane_instability.remains<action.arcane_blast.execute_time
                    //actions.burn+=/supernova,if=time_to_die<8|charges=2
                    //actions.burn+=/nether_tempest,cycle_targets=1,if=target!=prismatic_crystal&buff.arcane_charge.stack=4&(active_dot.nether_tempest=0|(ticking&remains<3.6))
                    //actions.burn+=/arcane_orb,if=buff.arcane_charge.stack<4
                    //actions.burn+=/arcane_barrage,if=talent.arcane_orb.enabled&active_enemies>=3&buff.arcane_charge.stack=4&(cooldown.arcane_orb.remains<gcd|prev_gcd.arcane_orb)
                    //actions.burn+=/presence_of_mind,if=mana.pct>96&(!talent.prismatic_crystal.enabled|!cooldown.prismatic_crystal.up)
                    //actions.burn+=/arcane_blast,if=buff.arcane_charge.stack=4&mana.pct>93
                    //actions.burn+=/arcane_missiles,if=buff.arcane_charge.stack=4&(mana.pct>70|!cooldown.evocation.up)
                    //actions.burn+=/supernova,if=mana.pct>70&mana.pct<96
                    //actions.burn+=/call_action_list,name=conserve,if=prev_gcd.evocation
                    //actions.burn+=/evocation,interrupt_if=mana.pct>92,if=time_to_die>10&mana.pct<30+2.5*active_enemies*(9-active_enemies)
                    //actions.burn+=/presence_of_mind,if=!talent.prismatic_crystal.enabled|!cooldown.prismatic_crystal.up
                    //actions.burn+=/arcane_blast
                };
            }
        }

        private static ActionList arcane_conserve
        {
            get
            {
                return new ActionList
                {
                    //actions.conserve=call_action_list,name=cooldowns,if=time_to_die<30|(buff.arcane_charge.stack=4&(!talent.prismatic_crystal.enabled|cooldown.prismatic_crystal.remains>15))
                    //actions.conserve+=/arcane_missiles,if=buff.arcane_missiles.react=3|(talent.overpowered.enabled&buff.arcane_power.up&buff.arcane_power.remains<action.arcane_blast.execute_time)
                    //actions.conserve+=/arcane_missiles,if=set_bonus.tier17_4pc&buff.arcane_instability.react&buff.arcane_instability.remains<action.arcane_blast.execute_time
                    //actions.conserve+=/nether_tempest,cycle_targets=1,if=target!=prismatic_crystal&buff.arcane_charge.stack=4&(active_dot.nether_tempest=0|(ticking&remains<3.6))
                    //actions.conserve+=/supernova,if=time_to_die<8|(charges=2&(buff.arcane_power.up|!cooldown.arcane_power.up)&(!talent.prismatic_crystal.enabled|cooldown.prismatic_crystal.remains>8))
                    //actions.conserve+=/arcane_orb,if=buff.arcane_charge.stack<2
                    //actions.conserve+=/presence_of_mind,if=mana.pct>96&(!talent.prismatic_crystal.enabled|!cooldown.prismatic_crystal.up)
                    //actions.conserve+=/arcane_blast,if=buff.arcane_charge.stack=4&mana.pct>93
                    //actions.conserve+=/arcane_barrage,if=talent.arcane_orb.enabled&active_enemies>=3&buff.arcane_charge.stack=4&(cooldown.arcane_orb.remains<gcd|prev_gcd.arcane_orb)
                    //actions.conserve+=/arcane_missiles,if=buff.arcane_charge.stack=4&(!talent.overpowered.enabled|cooldown.arcane_power.remains>10*spell_haste)
                    //actions.conserve+=/supernova,if=mana.pct<96&(buff.arcane_missiles.stack<2|buff.arcane_charge.stack=4)&(buff.arcane_power.up|(charges=1&cooldown.arcane_power.remains>recharge_time))&(!talent.prismatic_crystal.enabled|current_target=prismatic_crystal|(charges=1&cooldown.prismatic_crystal.remains>recharge_time+8))
                    //actions.conserve+=/nether_tempest,cycle_targets=1,if=target!=prismatic_crystal&buff.arcane_charge.stack=4&(active_dot.nether_tempest=0|(ticking&remains<(10-3*talent.arcane_orb.enabled)*spell_haste))
                    //actions.conserve+=/arcane_barrage,if=buff.arcane_charge.stack=4
                    //actions.conserve+=/presence_of_mind,if=buff.arcane_charge.stack<2&(!talent.prismatic_crystal.enabled|!cooldown.prismatic_crystal.up)
                    //actions.conserve+=/arcane_blast
                    //actions.conserve+=/arcane_barrage,moving=1
                };
            }
        }

        private static ActionList arcane_cooldowns
        {
            get
            {
                return new ActionList
                {
                    //actions.cooldowns=arcane_power
                    new Spell(arcane_power)
                    //actions.cooldowns+=/blood_fury
                    //actions.cooldowns+=/berserking
                    //actions.cooldowns+=/arcane_torrent
                    //actions.cooldowns+=/potion,name=draenic_intellect,if=buff.arcane_power.up&(!talent.prismatic_crystal.enabled|pet.prismatic_crystal.active)
                };
            }
        }

        private static ActionList arcane_crystal_sequence
        {
            get
            {
                return new ActionList
                {
                    //actions.crystal_sequence=call_action_list,name=cooldowns
                    //actions.crystal_sequence+=/nether_tempest,if=buff.arcane_charge.stack=4&!ticking&pet.prismatic_crystal.remains>8
                    //actions.crystal_sequence+=/supernova,if=mana.pct<96
                    //actions.crystal_sequence+=/presence_of_mind,if=cooldown.cold_snap.up|pet.prismatic_crystal.remains<action.arcane_blast.cast_time
                    //actions.crystal_sequence+=/arcane_blast,if=buff.arcane_charge.stack=4&mana.pct>93&pet.prismatic_crystal.remains>cast_time+buff.arcane_missiles.stack*2*spell_haste+action.arcane_missiles.travel_time
                    //actions.crystal_sequence+=/arcane_missiles,if=pet.prismatic_crystal.remains>2*spell_haste+travel_time
                    //actions.crystal_sequence+=/supernova,if=pet.prismatic_crystal.remains<action.arcane_blast.cast_time
                    //actions.crystal_sequence+=/choose_target,if=pet.prismatic_crystal.remains<action.arcane_blast.cast_time&buff.presence_of_mind.down
                    //actions.crystal_sequence+=/arcane_blast
                };
            }
        }

        private static ActionList arcane_init_crystal
        {
            get
            {
                return new ActionList
                {
                    //actions.init_crystal=call_action_list,name=conserve,if=buff.arcane_charge.stack<4
                    //actions.init_crystal+=/prismatic_crystal,if=buff.arcane_charge.stack=4&cooldown.arcane_power.remains<0.5
                    //actions.init_crystal+=/prismatic_crystal,if=glyph.arcane_power.enabled&buff.arcane_charge.stack=4&cooldown.arcane_power.remains>75
                };
            }
        }

        #endregion

        #region Private Methods

        private static double cast_time(string spell)
        {
            return Spell.GetSpellCastTime(spell).TotalSeconds;
        }

        #endregion

        #region Types

        private static class buff
        {
            #region Properties

            public static bool arcane_power_up
            {
                get { return Up(arcane_power); }
            }

            public static double rune_of_power_remain
            {
                get { return Remain(rune_of_power); }
            }

            #endregion

            #region Private Methods

            private static double Remain(string aura)
            {
                return StyxWoW.Me.GetAuraTimeLeft(aura).TotalSeconds;
            }

            private static bool Up(string aura)
            {
                return Remain(aura) > 0;
            }

            #endregion
        }

        private static class cooldown
        {
            #region Properties

            public static double evocation_remains
            {
                get { return Remains(evocation); }
            }

            public static bool prismatic_crystal_up
            {
                get { return Up(prismatic_crystal); }
            }

            #endregion

            #region Private Methods

            private static double Remains(string spell)
            {
                return Spell.GetSpellCooldown(spell).TotalSeconds;
            }

            private static bool Up(string spell)
            {
                return Remains(spell) <= 0;
            }

            #endregion
        }

        private static class pet
        {
            #region Properties

            public static bool prismatic_crystal_active
            {
                get { throw new NotImplementedException(); }
            }

            #endregion
        }

        private static class talent
        {
            #region Properties

            public static bool prismatic_crystal_enabled
            {
                get { return HasTalent(MageTalentsEnum.PrismaticCrystal); }
            }

            #endregion

            #region Private Methods

            private static bool HasTalent(MageTalentsEnum tal)
            {
                return TalentManager.IsSelected((int)tal);
            }

            #endregion
        }

        #endregion
    }
}
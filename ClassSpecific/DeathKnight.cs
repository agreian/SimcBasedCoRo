using System.Collections.Generic;
using System.Linq;
using SimcBasedCoRo.Extensions;
using SimcBasedCoRo.Managers;
using SimcBasedCoRo.Utilities;
using Styx;
using Styx.WoWInternals.WoWObjects;

namespace SimcBasedCoRo.ClassSpecific
{
    // ReSharper disable InconsistentNaming
    // ReSharper disable once ClassNeverInstantiated.Global
    public class DeathKnight : Common
    {
        #region Constant

        //public const string army_of_the_dead = "Army of the Dead";
        public const string blood_boil = "Blood Boil";

        public static readonly Dictionary<string, SpellTypeEnum> Spells = new Dictionary<string, SpellTypeEnum>
        {
            {blood_boil, SpellTypeEnum.CastAoe},
            {blood_tap, SpellTypeEnum.Cast},
            {breath_of_sindragosa, SpellTypeEnum.Buff},
            {dark_transformation, SpellTypeEnum.Buff},
            {death_and_decay, SpellTypeEnum.CastOnGroundAoe},
            {death_coil, SpellTypeEnum.Cast},
            {defile, SpellTypeEnum.CastOnGroundAoe},
            {empower_rune_weapon, SpellTypeEnum.Cast},
            {festering_strike, SpellTypeEnum.Cast},
            {icy_touch, SpellTypeEnum.Cast},
            {outbreak, SpellTypeEnum.Cast},
            {plague_leech, SpellTypeEnum.Cast},
            {plague_strike, SpellTypeEnum.Cast},
            {raise_dead, SpellTypeEnum.Buff},
            {scourge_strike, SpellTypeEnum.Cast},
            {soul_reaper, SpellTypeEnum.Cast},
            {summon_gargoyle, SpellTypeEnum.Cast},
            {unholy_blight, SpellTypeEnum.Buff}
        };

        private const string antimagic_shell = "Anti-Magic Shell";
        private const string blood_charge = "Blood Charge";
        private const string blood_plague = "Blood Plague";
        private const string blood_tap = "Blood Tap";
        //public const string bone_shield = "Bone Shield";
        private const string breath_of_sindragosa = "Breath of Sindragosa";
        //public const string conversion = "Conversion";
        private const int crimson_scourge = 81141;
        //public const string dancing_rune_weapon = "Dancing Rune Weapon";
        private const string dark_transformation = "Dark Transformation";
        private const string death_and_decay = "Death and Decay";
        private const string death_coil = "Death Coil";
        private const string defile = "Defile";
        private const string empower_rune_weapon = "Empower Rune Weapon";
        private const string festering_strike = "Festering Strike";
        private const int freezing_fog = 59052;
        private const string frost_fever = "Frost Fever";
        //public const string icebound_fortitude = "Icebound Fortitude";
        private const string icy_touch = "Icy Touch";
        private const int killing_machine = 51124;
        //public const string lichborne = "Lichborne";
        private const string necrotic_plague = "Necrotic Plague";
        private const string outbreak = "Outbreak";
        private const string pillar_of_frost = "Pillar of Frost";
        private const string plague_leech = "Plague Leech";
        private const string plague_strike = "Plague Strike";
        private const string raise_dead = "Raise Dead";
        //public const string rune_tap = "Rune Tap";
        //public const string runic_empowerment = "Runic Empowerment";
        private const string scourge_strike = "Scourge Strike";
        private const string shadow_infusion = "Shadow Infusion";
        private const string soul_reaper = "Soul Reaper";
        private const int sudden_doom = 81340;
        private const string summon_gargoyle = "Summon Gargoyle";
        private const string unholy_blight = "Unholy Blight";
        //public const string vampiric_blood = "Vampiric Blood";

        #endregion

        #region Enums

        private enum DeathKnightTalentsEnum
        {
            // ReSharper disable UnusedMember.Local
            Plaguebearer = 1,
            PlagueLeech,
            UnholyBlight,

            Lichborne,
            AntiMagicZone,
            Purgatory,

            DeathsAdvance,
            Chilblains,
            Asphyxiate,

            BloodTap,
            RunicEmpowerment,
            RunicCorruption,

            DeathPact,
            DeathSiphon,
            Conversion,

            GorefiendsGrasp,
            RemorselessWinter,
            DesecratedGround,

            NecroticPlague,
            Defile,
            BreathOfSindragosa
            // ReSharper restore UnusedMember.Local
        }

        #endregion

        #region Properties

        public static ActionList UnholyActionList
        {
            get
            {
                return new ActionList
                {
                    // # Executed every time the actor is available.
                    new Spell(raise_dead, () => !StyxWoW.Me.GotAlivePet),
                    //actions+=/run_action_list,name=aoe,if=(!talent.necrotic_plague.enabled&active_enemies>=2)|active_enemies>=4
                    new ActionList(unholy_aoe, () => (!talent.necrotic_plague_enabled && active_enemies >= 2) || active_enemies >= 4),
                    //actions+=/run_action_list,name=single_target,if=(!talent.necrotic_plague.enabled&active_enemies<2)|active_enemies<4
                    new ActionList(unholy_single_target, () => (!talent.necrotic_plague_enabled && active_enemies < 2) || active_enemies < 4)
                };
            }
        }

        private static int blood
        {
            get { return Me.GetRuneCount(0) + Me.GetRuneCount(1); }
        }

        private static int death
        {
            get { return Me.GetRuneCount(RuneType.Death); }
        }

        private static int frost
        {
            get { return Me.GetRuneCount(2) + Me.GetRuneCount(3); }
        }

        private static uint runic_power
        {
            get { return StyxWoW.Me.CurrentRunicPower; }
        }

        private static int unholy
        {
            get { return Me.GetRuneCount(4) + Me.GetRuneCount(5); }
        }

        private static ActionList unholy_aoe
        {
            get
            {
                return new ActionList
                {
                    //actions.aoe=unholy_blight
                    new Spell(unholy_blight),
                    //actions.aoe+=/call_action_list,name=spread,if=!dot.blood_plague.ticking|!dot.frost_fever.ticking|(!dot.necrotic_plague.ticking&talent.necrotic_plague.enabled)
                    new ActionList(unholy_spread, () => (!dot.blood_plague_ticking || !dot.frost_fever_ticking || (!dot.necrotic_plague_ticking && talent.necrotic_plague_enabled))),
                    //actions.aoe+=/defile
                    new Spell(defile),
                    //actions.aoe+=/breath_of_sindragosa,if=runic_power>75
                    new Spell(breath_of_sindragosa, () => runic_power > 75 && !Me.HasAura(breath_of_sindragosa)),
                    //actions.aoe+=/run_action_list,name=bos_aoe,if=dot.breath_of_sindragosa.ticking
                    new ActionList(unholy_bos_aoe, () => dot.breath_of_sindragosa_ticking),
                    //actions.aoe+=/blood_boil,if=blood=2|(frost=2&death=2)
                    new Spell(blood_boil, () => (blood == 2 || (frost == 2 && death == 2))),
                    //actions.aoe+=/summon_gargoyle
                    new Spell(summon_gargoyle),
                    //actions.aoe+=/dark_transformation
                    new Spell(dark_transformation, () => Me.Pet),
                    //actions.aoe+=/blood_tap,if=level<=90&buff.shadow_infusion.stack=5
                    new Spell(blood_tap, () => Me.Level <= 90 && buff.shadow_infusion_stack == 5),
                    //actions.aoe+=/defile
                    new Spell(defile),
                    //actions.aoe+=/death_and_decay,if=unholy=1
                    new Spell(death_and_decay, () => unholy == 1),
                    //actions.aoe+=/soul_reaper,if=target.health.pct-3*(target.health.pct%target.time_to_die)<=45
                    new Spell(soul_reaper, () => target.health_pct <= 46),
                    //actions.aoe+=/scourge_strike,if=unholy=2
                    new Spell(scourge_strike, () => unholy == 2),
                    //actions.aoe+=/blood_tap,if=buff.blood_charge.stack>10
                    new Spell(blood_tap, () => buff.blood_charge_stack > 10),
                    //actions.aoe+=/death_coil,if=runic_power>90|buff.sudden_doom.react|(buff.dark_transformation.down&unholy<=1)
                    new Spell(death_coil, () => runic_power > 90 || buff.sudden_doom_react || (buff.dark_transformation_down && unholy <= 1)),
                    //actions.aoe+=/blood_boil
                    new Spell(blood_boil),
                    //actions.aoe+=/icy_touch
                    new Spell(icy_touch),
                    //actions.aoe+=/scourge_strike,if=unholy=1
                    new Spell(scourge_strike, () => unholy == 1),
                    //actions.aoe+=/death_coil
                    new Spell(death_coil),
                    //actions.aoe+=/blood_tap
                    new Spell(blood_tap),
                    //actions.aoe+=/plague_leech
                    new Spell(plague_leech, () => disease.min_ticking),
                    //actions.aoe+=/empower_rune_weapon
                    new Spell(empower_rune_weapon)
                };
            }
        }

        private static ActionList unholy_bos_aoe
        {
            get
            {
                return new ActionList
                {
                    //actions.bos_aoe=death_and_decay,if=runic_power<88
                    new Spell(death_and_decay, () => runic_power < 88),
                    //actions.bos_aoe+=/blood_boil,if=runic_power<88
                    new Spell(blood_boil, () => runic_power < 88),
                    //actions.bos_aoe+=/scourge_strike,if=runic_power<88&unholy=1
                    new Spell(scourge_strike, () => runic_power < 88 && unholy == 1),
                    //actions.bos_aoe+=/icy_touch,if=runic_power<88
                    new Spell(icy_touch, () => runic_power < 88),
                    //actions.bos_aoe+=/blood_tap,if=buff.blood_charge.stack>=5
                    new Spell(blood_tap, () => buff.blood_charge_stack >= 5),
                    //actions.bos_aoe+=/plague_leech
                    new Spell(plague_leech, () => disease.min_ticking),
                    //actions.bos_aoe+=/empower_rune_weapon
                    new Spell(empower_rune_weapon),
                    //actions.bos_aoe+=/death_coil,if=buff.sudden_doom.react
                    new Spell(death_coil, () => buff.sudden_doom_react),
                };
            }
        }

        private static ActionList unholy_bos_st
        {
            get
            {
                return new ActionList
                {
                    //actions.bos_st=death_and_decay,if=runic_power<88
                    new Spell(death_and_decay, () => runic_power < 88),
                    //actions.bos_st+=/festering_strike,if=runic_power<77
                    new Spell(festering_strike, () => runic_power < 77),
                    //actions.bos_st+=/scourge_strike,if=runic_power<88
                    new Spell(scourge_strike, () => runic_power < 88),
                    //actions.bos_st+=/blood_tap,if=buff.blood_charge.stack>=5
                    new Spell(blood_tap, () => buff.blood_charge_stack >= 5),
                    //actions.bos_st+=/plague_leech
                    new Spell(plague_leech, () => disease.min_ticking),
                    //actions.bos_st+=/empower_rune_weapon
                    new Spell(empower_rune_weapon),
                    //actions.bos_st+=/death_coil,if=buff.sudden_doom.react
                    new Spell(death_coil, () => buff.sudden_doom_react),
                };
            }
        }

        private static ActionList unholy_single_target
        {
            get
            {
                return new ActionList
                {
                    //actions.single_target=plague_leech,if=(cooldown.outbreak.remains<1)&((blood<1&frost<1)|(blood<1&unholy<1)|(frost<1&unholy<1))
                    new Spell(plague_leech, () => disease.min_ticking && (cooldown.outbreak_remains < 1) && ((blood < 1 && frost < 1) || (blood < 1 && unholy < 1) || (frost < 1 && unholy < 1))),
                    //actions.single_target+=/plague_leech,if=((blood<1&frost<1)|(blood<1&unholy<1)|(frost<1&unholy<1))&disease.min_remains<3
                    new Spell(plague_leech, () => disease.min_ticking && ((blood < 1 && frost < 1) || (blood < 1 && unholy < 1) || (frost < 1 && unholy < 1)) && disease.min_remains < 3),
                    //actions.single_target+=/plague_leech,if=disease.min_remains<1
                    new Spell(plague_leech, () => disease.min_ticking && disease.min_remains < 1),
                    //actions.single_target+=/outbreak,if=!disease.min_ticking
                    new Spell(outbreak, () => !disease.min_ticking),
                    //actions.single_target+=/unholy_blight,if=!talent.necrotic_plague.enabled&disease.min_remains<3
                    new Spell(unholy_blight, () => !talent.necrotic_plague_enabled && disease.min_remains < 3),
                    //actions.single_target+=/unholy_blight,if=talent.necrotic_plague.enabled&dot.necrotic_plague.remains<1
                    new Spell(unholy_blight, () => talent.necrotic_plague_enabled && dot.necrotic_plague_remains < 1),
                    //actions.single_target+=/death_coil,if=runic_power>90
                    new Spell(death_coil, () => runic_power > 90),
                    //actions.single_target+=/soul_reaper,if=(target.health.pct-3*(target.health.pct%target.time_to_die))<=45
                    new Spell(soul_reaper, () => target.health_pct <= 46),
                    //actions.single_target+=/breath_of_sindragosa,if=runic_power>75
                    new Spell(breath_of_sindragosa, () => runic_power > 75 && !Me.HasAura(breath_of_sindragosa)),
                    //actions.single_target+=/run_action_list,name=bos_st,if=dot.breath_of_sindragosa.ticking
                    new ActionList(unholy_bos_st, () => dot.breath_of_sindragosa_ticking),
                    //actions.single_target+=/death_and_decay,if=cooldown.breath_of_sindragosa.remains<7&runic_power<88&talent.breath_of_sindragosa.enabled
                    new Spell(death_and_decay, () => cooldown.breath_of_sindragosa_remains < 7 && runic_power < 88 && talent.breath_of_sindragosa_enabled),
                    //actions.single_target+=/scourge_strike,if=cooldown.breath_of_sindragosa.remains<7&runic_power<88&talent.breath_of_sindragosa.enabled
                    new Spell(scourge_strike, () => cooldown.breath_of_sindragosa_remains < 7 && runic_power < 88 && talent.breath_of_sindragosa_enabled),
                    //actions.single_target+=/festering_strike,if=cooldown.breath_of_sindragosa.remains<7&runic_power<76&talent.breath_of_sindragosa.enabled
                    new Spell(festering_strike, () => cooldown.breath_of_sindragosa_remains < 7 && runic_power < 76 && talent.breath_of_sindragosa_enabled),
                    //actions.single_target+=/blood_tap,if=((target.health.pct-3*(target.health.pct%target.time_to_die))<=45)&cooldown.soul_reaper.remains=0
                    new Spell(blood_tap, () => (target.health_pct <= 46) && cooldown.soul_reaper_remains <= 0),
                    //actions.single_target+=/death_and_decay,if=unholy=2
                    new Spell(death_and_decay, () => unholy == 2),
                    //actions.single_target+=/defile,if=unholy=2
                    new Spell(defile, () => unholy == 2),
                    //actions.single_target+=/plague_strike,if=!disease.min_ticking&unholy=2
                    new Spell(plague_strike, () => !disease.min_ticking && unholy == 2),
                    //actions.single_target+=/scourge_strike,if=unholy=2
                    new Spell(scourge_strike, () => unholy == 2),
                    //actions.single_target+=/death_coil,if=runic_power>80
                    new Spell(death_coil, () => runic_power > 80),
                    //actions.single_target+=/festering_strike,if=talent.necrotic_plague.enabled&talent.unholy_blight.enabled&dot.necrotic_plague.remains<cooldown.unholy_blight.remains%2
                    new Spell(festering_strike, () => talent.necrotic_plague_enabled && talent.unholy_blight_enabled && dot.necrotic_plague_remains < cooldown.unholy_blight_remains%2),
                    //actions.single_target+=/festering_strike,if=blood=2&frost=2&(((Frost-death)>0)|((Blood-death)>0))
                    new Spell(festering_strike, () => blood == 2 && frost == 2 && (((frost - death) > 0) || ((blood - death) > 0))),
                    //actions.single_target+=/festering_strike,if=(blood=2|frost=2)&(((Frost-death)>0)&((Blood-death)>0))
                    new Spell(festering_strike, () => (blood == 2 || frost == 2) && (((frost - death) > 0) && ((blood - death) > 0))),
                    //actions.single_target+=/defile,if=blood=2|frost=2
                    new Spell(defile, () => blood == 2 || frost == 2),
                    //actions.single_target+=/plague_strike,if=!disease.min_ticking&(blood=2|frost=2)
                    new Spell(plague_strike, () => !disease.min_ticking && (blood == 2 || frost == 2)),
                    //actions.single_target+=/scourge_strike,if=blood=2|frost=2
                    new Spell(scourge_strike, () => blood == 2 || frost == 2),
                    //actions.single_target+=/festering_strike,if=((Blood-death)>1)
                    new Spell(festering_strike, () => ((blood - death) > 1)),
                    //actions.single_target+=/blood_boil,if=((Blood-death)>1)
                    new Spell(blood_boil, () => ((blood - death) > 1)),
                    //actions.single_target+=/festering_strike,if=((Frost-death)>1)
                    new Spell(festering_strike, () => ((frost - death) > 1)),
                    //actions.single_target+=/blood_tap,if=((target.health.pct-3*(target.health.pct%target.time_to_die))<=45)&cooldown.soul_reaper.remains=0
                    new Spell(blood_tap, () => target.health_pct <= 46 && cooldown.soul_reaper_remains <= 0),
                    //actions.single_target+=/summon_gargoyle
                    new Spell(summon_gargoyle),
                    //actions.single_target+=/death_and_decay
                    new Spell(death_and_decay),
                    //actions.single_target+=/defile
                    new Spell(defile),
                    //actions.single_target+=/blood_tap,if=cooldown.defile.remains=0
                    new Spell(blood_tap, () => cooldown.defile_remains <= 0),
                    //actions.single_target+=/plague_strike,if=!disease.min_ticking
                    new Spell(plague_strike, () => !disease.min_ticking),
                    //actions.single_target+=/dark_transformation
                    new Spell(dark_transformation, () => Me.Pet),
                    //actions.single_target+=/blood_tap,if=buff.blood_charge.stack>10&(buff.sudden_doom.react|(buff.dark_transformation.down&unholy<=1))
                    new Spell(blood_tap, () => buff.blood_charge_stack > 10 && (buff.sudden_doom_react || (buff.dark_transformation_down && unholy <= 1))),
                    //actions.single_target+=/death_coil,if=buff.sudden_doom.react|(buff.dark_transformation.down&unholy<=1)
                    new Spell(death_coil, () => buff.sudden_doom_react || (buff.dark_transformation_down && unholy <= 1)),
                    //actions.single_target+=/scourge_strike,if=!((target.health.pct-3*(target.health.pct%target.time_to_die))<=45)|(Unholy>=2)
                    new Spell(scourge_strike, () => !(target.health_pct <= 46) || (unholy >= 2)),
                    //actions.single_target+=/blood_tap
                    new Spell(blood_tap),
                    //actions.single_target+=/festering_strike,if=!((target.health.pct-3*(target.health.pct%target.time_to_die))<=45)|(((Frost-death)>0)&((Blood-death)>0))
                    new Spell(festering_strike, () => !(target.health_pct <= 46) || (((frost - death) > 0) && ((blood - death) > 0))),
                    //actions.single_target+=/death_coil
                    new Spell(death_coil),
                    //actions.single_target+=/plague_leech
                    new Spell(plague_leech, () => disease.min_ticking),
                    //actions.single_target+=/scourge_strike,if=cooldown.empower_rune_weapon.remains=0
                    new Spell(scourge_strike, () => cooldown.empower_rune_weapon_remains <= 0),
                    //actions.single_target+=/festering_strike,if=cooldown.empower_rune_weapon.remains=0
                    new Spell(festering_strike, () => cooldown.empower_rune_weapon_remains <= 0),
                    //actions.single_target+=/blood_boil,if=cooldown.empower_rune_weapon.remains=0
                    new Spell(blood_boil, () => cooldown.empower_rune_weapon_remains <= 0),
                    //actions.single_target+=/icy_touch,if=cooldown.empower_rune_weapon.remains=0
                    new Spell(icy_touch, () => cooldown.empower_rune_weapon_remains <= 0),
                    //actions.single_target+=/empower_rune_weapon,if=blood<1&unholy<1&frost<1
                    new Spell(empower_rune_weapon, () => blood < 1 && unholy < 1 && frost < 1)
                };
            }
        }

        private static ActionList unholy_spread
        {
            get
            {
                return new ActionList
                {
                    //actions.spread=blood_boil,cycle_targets=1,if=!disease.min_ticking
                    new Spell(blood_boil, () => active_enemies_list.Count(u => !disease.ticking_on(u)) > 0 && active_enemies_list.Any(disease.ticking_on)),
                    //actions.spread+=/outbreak,if=!disease.min_ticking
                    new Spell(outbreak, () => !disease.min_ticking),
                    //actions.spread+=/plague_strike,if=!disease.min_ticking
                    new Spell(plague_strike, () => !disease.min_ticking)
                };
            }
        }

        #endregion

        #region Types

        private static class buff
        {
            #region Properties

            public static uint blood_charge_stack
            {
                get { return Stack(blood_charge); }
            }

            public static bool crimson_scourge_react
            {
                get { return React(crimson_scourge); }
            }

            public static bool dark_transformation_down
            {
                get { return PetDown(dark_transformation); }
            }

            public static bool killing_machine_react
            {
                get { return React(killing_machine); }
            }

            public static bool rime_react
            {
                get { return React(freezing_fog); }
            }

            public static uint shadow_infusion_stack
            {
                get { return Stack(shadow_infusion); }
            }

            public static bool sudden_doom_react
            {
                get { return React(sudden_doom); }
            }

            #endregion

            #region Private Methods

            private static bool PetDown(string aura)
            {
                return !PetUp(aura);
            }

            private static bool PetUp(string aura)
            {
                return StyxWoW.Me.GotAlivePet && StyxWoW.Me.Pet.ActiveAuras.ContainsKey(aura);
            }

            private static bool React(int aura)
            {
                return StyxWoW.Me.HasAura(aura);
            }

            private static uint Stack(string aura)
            {
                return StyxWoW.Me.GetAuraStacks(aura);
            }

            #endregion
        }

        private static class cooldown
        {
            #region Properties

            public static double antimagic_shell_remains
            {
                get { return Remains(antimagic_shell); }
            }

            public static double breath_of_sindragosa_remains
            {
                get { return Remains(breath_of_sindragosa); }
            }

            public static double defile_remains
            {
                get { return Remains(defile); }
            }

            public static double empower_rune_weapon_remains
            {
                get { return Remains(empower_rune_weapon); }
            }

            public static double outbreak_remains
            {
                get { return Remains(outbreak); }
            }

            public static double pillar_of_frost_remains
            {
                get { return Remains(pillar_of_frost); }
            }

            public static double soul_reaper_remains
            {
                get { return Remains(soul_reaper); }
            }

            public static double unholy_blight_remains
            {
                get { return Remains(unholy_blight); }
            }

            #endregion

            #region Private Methods

            private static double Remains(string spell)
            {
                return Spell.GetSpellCooldown(spell).TotalSeconds;
            }

            #endregion
        }

        private static class disease
        {
            #region Fields

            private static readonly string[] listBase = {blood_plague, frost_fever};
            private static readonly string[] listWithNecroticPlague = {necrotic_plague};

            #endregion

            #region Properties

            public static double max_remains
            {
                get { return max_remains_on(StyxWoW.Me.CurrentTarget); }
            }

            public static bool max_ticking
            {
                get { return max_ticking_on(StyxWoW.Me.CurrentTarget); }
            }

            public static double min_remains
            {
                get { return min_remains_on(StyxWoW.Me.CurrentTarget); }
            }

            public static bool min_ticking
            {
                get { return ticking; }
            }

            public static bool ticking
            {
                get { return ticking_on(StyxWoW.Me.CurrentTarget); }
            }

            private static string[] diseaseArray
            {
                get { return talent.necrotic_plague_enabled ? listWithNecroticPlague : listBase; }
            }

            #endregion

            #region Public Methods

            public static bool ticking_on(WoWUnit unit)
            {
                if (unit == null) return false;

                return unit.HasAllMyAuras(diseaseArray);
            }

            #endregion

            #region Private Methods

            private static double max_remains_on(WoWUnit unit)
            {
                if (unit == null) return 0;

                var max = double.MinValue;

                // ReSharper disable once LoopCanBeConvertedToQuery
                foreach (var s in diseaseArray)
                {
                    var rmn = unit.GetAuraTimeLeft(s).TotalSeconds;
                    if (rmn > max)
                        max = rmn;
                }

                if (max <= double.MinValue)
                    max = 0;

                return max;
            }

            private static bool max_ticking_on(WoWUnit unit)
            {
                if(unit == null) return false;

                return unit.HasAnyOfMyAuras(diseaseArray);
            }

            private static double min_remains_on(WoWUnit unit)
            {
                if (unit == null) return 0;

                var min = double.MaxValue;

                // ReSharper disable once LoopCanBeConvertedToQuery
                foreach (var s in diseaseArray)
                {
                    var rmn = unit.GetAuraTimeLeft(s).TotalSeconds;
                    if (rmn < min)
                        min = rmn;
                }

                if (min >= double.MaxValue)
                    min = 0;

                return min;
            }

            #endregion
        }

        private static class dot
        {
            #region Properties

            public static bool blood_plague_ticking
            {
                get { return blood_plague_remains > 0; }
            }

            public static bool breath_of_sindragosa_ticking
            {
                get { return breath_of_sindragosa_remains > 0; }
            }

            public static bool frost_fever_ticking
            {
                get { return frost_fever_remains > 0; }
            }

            public static double necrotic_plague_remains
            {
                get { return Remains(necrotic_plague); }
            }

            public static bool necrotic_plague_ticking
            {
                get { return necrotic_plague_remains > 0; }
            }

            private static double blood_plague_remains
            {
                get { return Remains(blood_plague); }
            }

            private static double breath_of_sindragosa_remains
            {
                get { return Remains(breath_of_sindragosa); }
            }

            private static double frost_fever_remains
            {
                get { return Remains(frost_fever); }
            }

            #endregion

            #region Private Methods

            private static double Remains(string aura)
            {
                if (StyxWoW.Me.CurrentTarget == null) return 0;

                return StyxWoW.Me.CurrentTarget.GetAuraTimeLeft(aura).TotalSeconds;
            }

            #endregion
        }

        private static class obliterate
        {
            #region Properties

            public static double ready_in
            {
                get { return 0; }
            }

            #endregion
        }

        private static class talent
        {
            #region Properties

            public static bool blood_tap_enabled
            {
                get { return HasTalent(DeathKnightTalentsEnum.BloodTap); }
            }

            public static bool breath_of_sindragosa_enabled
            {
                get { return HasTalent(DeathKnightTalentsEnum.BreathOfSindragosa); }
            }

            public static bool defile_enabled
            {
                get { return HasTalent(DeathKnightTalentsEnum.Defile); }
            }

            public static bool necrotic_plague_enabled
            {
                get { return HasTalent(DeathKnightTalentsEnum.NecroticPlague); }
            }

            public static bool runic_empowerment_enabled
            {
                get { return HasTalent(DeathKnightTalentsEnum.RunicEmpowerment); }
            }

            public static bool unholy_blight_enabled
            {
                get { return HasTalent(DeathKnightTalentsEnum.UnholyBlight); }
            }

            #endregion

            #region Private Methods

            private static bool HasTalent(DeathKnightTalentsEnum tal)
            {
                return TalentManager.IsSelected((int) tal);
            }

            #endregion
        }

        #endregion
    }

    // ReSharper restore InconsistentNaming
}
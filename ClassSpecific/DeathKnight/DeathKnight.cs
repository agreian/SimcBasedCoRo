using System.Linq;
using Styx;
using Styx.Common;
using Styx.WoWInternals.WoWObjects;

namespace SimcBasedCoRo.ClassSpecific.DeathKnight
{
    // ReSharper disable InconsistentNaming    
    public abstract class DeathKnight : Common
    {
        // ReSharper disable MemberCanBePrivate.Global

        #region Constant

        public const string antimagic_shell = "Anti-Magic Shell";
        public const string army_of_the_dead = "Army of the Dead";
        public const string blood_boil = "Blood Boil";
        public const string blood_charge = "Blood Charge";
        public const string blood_plague = "Blood Plague";
        public const string blood_tap = "Blood Tap";
        public const string bone_shield = "Bone Shield";
        public const string breath_of_sindragosa = "Breath of Sindragosa";
        public const string conversion = "Conversion";
        public const int crimson_scourge = 81141;
        //public const string crimson_scourge = "Crimson Scourge";
        public const string dancing_rune_weapon = "Dancing Rune Weapon";
        public const string dark_transformation = "Dark Transformation";
        public const string death_and_decay = "Death and Decay";
        public const string death_coil = "Death Coil";
        public const string defile = "Defile";
        public const string empower_rune_weapon = "Empower Rune Weapon";
        public const string festering_strike = "Festering Strike";
        public const int freezing_fog = 59052;
        //public const string freezing_fog = "Freezing Fog";
        public const string frost_fever = "Frost Fever";
        public const string icebound_fortitude = "Icebound Fortitude";
        public const string icy_touch = "Icy Touch";
        public const int killing_machine = 51124;
        //public const string killing_machine = "Killing Machine";
        public const string lichborne = "Lichborne";
        public const string necrotic_plague = "Necrotic Plague";
        public const string outbreak = "Outbreak";
        public const string pillar_of_frost = "Pillar of Frost";
        public const string plague_leech = "Plague Leech";
        public const string plague_strike = "Plague Strike";
        public const string rune_tap = "Rune Tap";
        public const string runic_empowerment = "Runic Empowerment";
        public const string scourge_strike = "Scourge Strike";
        public const string shadow_infusion = "Shadow Infusion";
        public const string soul_reaper = "Soul Reaper";
        public const int sudden_doom = 81340;
        //public const string sudden_doom = "Sudden Doom";
        public const string summon_gargoyle = "Summon Gargoyle";
        public const string unholy_blight = "Unholy Blight";
        public const string vampiric_blood = "Vampiric Blood";

        #endregion

        // ReSharper restore MemberCanBePrivate.Global

        #region Properties

        public static ActionList Unholy
        {
            get
            {
                return new ActionList
                {
                    // # Executed every time the actor is available.
                    //actions+=/run_action_list,name=aoe,if=(!talent.necrotic_plague.enabled&active_enemies>=2)|active_enemies>=4
                    new ActionList(
                        req => (!talent.necrotic_plague_enabled && active_enemies >= 2) || active_enemies >= 4,
                        UnholyAoe),
                    //actions+=/run_action_list,name=single_target,if=(!talent.necrotic_plague.enabled&active_enemies<2)|active_enemies<4
                    new ActionList(req => (!talent.necrotic_plague_enabled && active_enemies < 2) || active_enemies < 4,
                        UnholySingleTarget)
                };
            }
        }

        private static ActionList UnholyAoe
        {
            get
            {
                return new ActionList
                {
                    //actions.aoe=unholy_blight
                    new Spell(SpellType.Buff, unholy_blight),
                    //actions.aoe+=/call_action_list,name=spread,if=!dot.blood_plague.ticking|!dot.frost_fever.ticking|(!dot.necrotic_plague.ticking&talent.necrotic_plague.enabled)
                    new ActionList(
                        req =>
                            (!dot.blood_plague_ticking || !dot.frost_fever_ticking ||
                             (!dot.necrotic_plague_ticking && talent.necrotic_plague_enabled)),
                        UnholySpread),
                    //actions.aoe+=/defile
                    new Spell(SpellType.CastOnGround, defile, req => Spell.UseAoe, on => Me.CurrentTarget),
                    //actions.aoe+=/breath_of_sindragosa,if=runic_power>75
                    new Spell(SpellType.Buff, breath_of_sindragosa,
                        req => runic_power > 75 && !Me.HasAura(breath_of_sindragosa)),
                    //actions.aoe+=/run_action_list,name=bos_aoe,if=dot.breath_of_sindragosa.ticking
                    //actions.aoe+=/blood_boil,if=blood=2|(frost=2&death=2)
                    new Spell(SpellType.Cast, blood_boil,
                        req => Spell.UseAoe && (blood == 2 || (frost == 2 && death == 2))),
                    //actions.aoe+=/summon_gargoyle
                    new Spell(SpellType.Cast, summon_gargoyle),
                    //actions.aoe+=/dark_transformation
                    new Spell(SpellType.Buff, dark_transformation, on => Me.Pet),
                    //actions.aoe+=/blood_tap,if=level<=90&buff.shadow_infusion.stack=5
                    new Spell(SpellType.Cast, blood_tap, req => Me.Level <= 90 && buff.shadow_infusion_stack == 5),
                    //actions.aoe+=/defile
                    new Spell(SpellType.CastOnGround, defile, req => Spell.UseAoe, on => Me.CurrentTarget),
                    //actions.aoe+=/death_and_decay,if=unholy=1
                    new Spell(SpellType.CastOnGround, death_and_decay, req => Spell.UseAoe && unholy == 1,
                        on => Me.CurrentTarget),
                    //actions.aoe+=/soul_reaper,if=target.health.pct-3*(target.health.pct%target.time_to_die)<=45
                    new Spell(SpellType.Cast, soul_reaper,
                        req => target.health_pct /*- 3 * (target.health_pct % target.time_to_die) <= 45*/<= 46),
                    //actions.aoe+=/scourge_strike,if=unholy=2
                    new Spell(SpellType.Cast, scourge_strike, req => unholy == 2),
                    //actions.aoe+=/blood_tap,if=buff.blood_charge.stack>10
                    new Spell(SpellType.Cast, blood_tap, req => buff.blood_charge_stack > 10),
                    //actions.aoe+=/death_coil,if=runic_power>90|buff.sudden_doom.react|(buff.dark_transformation.down&unholy<=1)
                    new Spell(SpellType.Cast, death_coil,
                        req =>
                            runic_power > 90 || buff.sudden_doom_react || (buff.dark_transformation_down && unholy <= 1)),
                    //actions.aoe+=/blood_boil
                    new Spell(SpellType.Cast, blood_boil, req => Spell.UseAoe),
                    //actions.aoe+=/icy_touch
                    new Spell(SpellType.Buff, icy_touch),
                    //actions.aoe+=/scourge_strike,if=unholy=1
                    new Spell(SpellType.Cast, scourge_strike, req => unholy == 1),
                    //actions.aoe+=/death_coil
                    new Spell(SpellType.Cast, death_coil),
                    //actions.aoe+=/blood_tap
                    new Spell(SpellType.Cast, blood_tap),
                    //actions.aoe+=/plague_leech
                    new Spell(SpellType.Cast, plague_leech, req => disease.min_ticking),
                    //actions.aoe+=/empower_rune_weapon
                    new Spell(SpellType.Cast, empower_rune_weapon)
                };
            }
        }

        private static ActionList UnholySingleTarget
        {
            get
            {
                return new ActionList
                {
                    //actions.single_target=plague_leech,if=(cooldown.outbreak.remains<1)&((blood<1&frost<1)|(blood<1&unholy<1)|(frost<1&unholy<1))
                    new Spell(SpellType.Cast, plague_leech,
                        req =>
                            (cooldown.outbreak_remains < 1) && disease.min_ticking &&
                            ((blood < 1 && frost < 1) || (blood < 1 && unholy < 1) || (frost < 1 && unholy < 1))),
                    //actions.single_target+=/plague_leech,if=((blood<1&frost<1)|(blood<1&unholy<1)|(frost<1&unholy<1))&disease.min_remains<3
                    new Spell(SpellType.Cast, plague_leech,
                        req =>
                            ((blood < 1 && frost < 1) || (blood < 1 && unholy < 1) || (frost < 1 && unholy < 1)) &&
                            disease.min_ticking && disease.min_remains < 3),
                    //actions.single_target+=/plague_leech,if=disease.min_remains<1
                    new Spell(SpellType.Cast, plague_leech, req => disease.min_ticking && disease.min_remains < 1),
                    //actions.single_target+=/outbreak,if=!disease.min_ticking
                    new Spell(SpellType.Cast, outbreak, req => !disease.min_ticking),
                    //actions.single_target+=/unholy_blight,if=!talent.necrotic_plague.enabled&disease.min_remains<3
                    new Spell(SpellType.Buff, unholy_blight,
                        req => !talent.necrotic_plague_enabled && disease.min_remains < 3),
                    //actions.single_target+=/unholy_blight,if=talent.necrotic_plague.enabled&dot.necrotic_plague.remains<1
                    new Spell(SpellType.Buff, unholy_blight,
                        req => talent.necrotic_plague_enabled && dot.necrotic_plague_remains < 1),
                    //actions.single_target+=/death_coil,if=runic_power>90
                    new Spell(SpellType.Cast, death_coil, req => runic_power > 90),
                    //actions.single_target+=/soul_reaper,if=(target.health.pct-3*(target.health.pct%target.time_to_die))<=45
                    new Spell(SpellType.Cast, soul_reaper, req => target.health_pct <= 46),
                    //actions.single_target+=/breath_of_sindragosa,if=runic_power>75
                    new Spell(SpellType.Buff, breath_of_sindragosa,
                        req => runic_power > 75 && !Me.HasAura(breath_of_sindragosa)),
                    //actions.single_target+=/run_action_list,name=bos_st,if=dot.breath_of_sindragosa.ticking
                    //actions.single_target+=/death_and_decay,if=cooldown.breath_of_sindragosa.remains<7&runic_power<88&talent.breath_of_sindragosa.enabled
                    new Spell(SpellType.CastOnGround, death_and_decay,
                        req =>
                            Spell.UseAoe && cooldown.breath_of_sindragosa_remains < 7 && runic_power < 88 &&
                            talent.breath_of_sindragosa_enabled, on => Me.CurrentTarget),
                    //actions.single_target+=/scourge_strike,if=cooldown.breath_of_sindragosa.remains<7&runic_power<88&talent.breath_of_sindragosa.enabled
                    new Spell(SpellType.Cast, scourge_strike,
                        req =>
                            cooldown.breath_of_sindragosa_remains < 7 && runic_power < 88 &&
                            talent.breath_of_sindragosa_enabled),
                    //actions.single_target+=/festering_strike,if=cooldown.breath_of_sindragosa.remains<7&runic_power<76&talent.breath_of_sindragosa.enabled
                    new Spell(SpellType.Cast, festering_strike,
                        req =>
                            cooldown.breath_of_sindragosa_remains < 7 && runic_power < 76 &&
                            talent.breath_of_sindragosa_enabled),
                    //actions.single_target+=/blood_tap,if=((target.health.pct-3*(target.health.pct%target.time_to_die))<=45)&cooldown.soul_reaper.remains=0
                    new Spell(SpellType.Cast, blood_tap,
                        req => (target.health_pct <= 46) && cooldown.soul_reaper_remains == 0),
                    //actions.single_target+=/death_and_decay,if=unholy=2
                    new Spell(SpellType.CastOnGround, death_and_decay, req => Spell.UseAoe && unholy == 2,
                        on => Me.CurrentTarget),
                    //actions.single_target+=/defile,if=unholy=2
                    new Spell(SpellType.CastOnGround, defile, req => Spell.UseAoe && unholy == 2, on => Me.CurrentTarget),
                    //actions.single_target+=/plague_strike,if=!disease.min_ticking&unholy=2
                    new Spell(SpellType.Buff, plague_strike, req => !disease.min_ticking && unholy == 2),
                    //actions.single_target+=/scourge_strike,if=unholy=2
                    new Spell(SpellType.Cast, scourge_strike, req => unholy == 2),
                    //actions.single_target+=/death_coil,if=runic_power>80
                    new Spell(SpellType.Cast, death_coil, req => runic_power > 80),
                    //actions.single_target+=/festering_strike,if=talent.necrotic_plague.enabled&talent.unholy_blight.enabled&dot.necrotic_plague.remains<cooldown.unholy_blight.remains%2
                    new Spell(SpellType.Cast, festering_strike,
                        req =>
                            talent.necrotic_plague_enabled && talent.unholy_blight_enabled &&
                            dot.necrotic_plague_remains < cooldown.unholy_blight_remains%2),
                    //actions.single_target+=/festering_strike,if=blood=2&frost=2&(((Frost-death)>0)|((Blood-death)>0))
                    new Spell(SpellType.Cast, festering_strike,
                        req => blood == 2 && frost == 2 && (((frost - death) > 0) || ((blood - death) > 0))),
                    //actions.single_target+=/festering_strike,if=(blood=2|frost=2)&(((Frost-death)>0)&((Blood-death)>0))
                    new Spell(SpellType.Cast, festering_strike,
                        req => (blood == 2 || frost == 2) && (((frost - death) > 0) && ((blood - death) > 0))),
                    //actions.single_target+=/defile,if=blood=2|frost=2
                    new Spell(SpellType.CastOnGround, defile, req => Spell.UseAoe && (blood == 2 || frost == 2),
                        on => Me.CurrentTarget),
                    //actions.single_target+=/plague_strike,if=!disease.min_ticking&(blood=2|frost=2)
                    new Spell(SpellType.Buff, plague_strike,
                        req => !disease.min_ticking && (blood == 2 || frost == 2)),
                    //actions.single_target+=/scourge_strike,if=blood=2|frost=2
                    new Spell(SpellType.Cast, scourge_strike, req => blood == 2 || frost == 2),
                    //actions.single_target+=/festering_strike,if=((Blood-death)>1)
                    new Spell(SpellType.Cast, festering_strike, req => ((blood - death) > 1)),
                    //actions.single_target+=/blood_boil,if=((Blood-death)>1)
                    new Spell(SpellType.Cast, blood_boil, req => Spell.UseAoe && ((blood - death) > 1)),
                    //actions.single_target+=/festering_strike,if=((Frost-death)>1)
                    new Spell(SpellType.Cast, festering_strike, req => ((frost - death) > 1)),
                    //actions.single_target+=/blood_tap,if=((target.health.pct-3*(target.health.pct%target.time_to_die))<=45)&cooldown.soul_reaper.remains=0
                    new Spell(SpellType.Cast, blood_tap,
                        req => (target.health_pct <= 46) && cooldown.soul_reaper_remains == 0),
                    //actions.single_target+=/summon_gargoyle
                    new Spell(SpellType.Cast, summon_gargoyle),
                    //actions.single_target+=/death_and_decay
                    new Spell(
                        SpellType.CastOnGround, death_and_decay, req => Spell.UseAoe, on => Me.CurrentTarget),
                    //actions.single_target+=/defile
                    new Spell(
                        SpellType.CastOnGround, defile, req => Spell.UseAoe, on => Me.CurrentTarget),
                    //actions.single_target+=/blood_tap,if=cooldown.defile.remains=0
                    new Spell(SpellType.Cast, blood_tap, req => cooldown.defile_remains == 0),
                    //actions.single_target+=/plague_strike,if=!disease.min_ticking
                    new Spell(SpellType.Buff, plague_strike, req => !disease.ticking),
                    //actions.single_target+=/dark_transformation
                    new Spell(SpellType.Buff, dark_transformation, on => Me.Pet),
                    //actions.single_target+=/blood_tap,if=buff.blood_charge.stack>10&(buff.sudden_doom.react|(buff.dark_transformation.down&unholy<=1))
                    new Spell(SpellType.Cast, blood_tap,
                        req =>
                            buff.blood_charge_stack > 10 &&
                            (buff.sudden_doom_react || (buff.dark_transformation_down && unholy <= 1))),
                    //actions.single_target+=/death_coil,if=buff.sudden_doom.react|(buff.dark_transformation.down&unholy<=1)
                    new Spell(SpellType.Cast, death_coil, req => buff.sudden_doom_react || (buff.dark_transformation_down && unholy <= 1)),
                    //actions.single_target+=/scourge_strike,if=!((target.health.pct-3*(target.health.pct%target.time_to_die))<=45)|(Unholy>=2)
                    new Spell(SpellType.Cast, scourge_strike,
                        req => !(target.health_pct <= 46) || (unholy >= 2)),
                    //actions.single_target+=/blood_tap
                    new Spell(SpellType.Cast, blood_tap),
                    //actions.single_target+=/festering_strike,if=!((target.health.pct-3*(target.health.pct%target.time_to_die))<=45)|(((Frost-death)>0)&((Blood-death)>0))
                    new Spell(SpellType.Cast, festering_strike,
                        req =>
                            !(target.health_pct <= 46) ||
                            (((frost - death) > 0) && ((blood - death) > 0))),
                    //actions.single_target+=/death_coil
                    new Spell(SpellType.Cast, death_coil),
                    //actions.single_target+=/plague_leech
                    new Spell(SpellType.Cast, plague_leech, req => disease.min_ticking),
                    //actions.single_target+=/scourge_strike,if=cooldown.empower_rune_weapon.remains=0
                    new Spell(SpellType.Cast, scourge_strike,
                        req => cooldown.empower_rune_weapon_remains == 0),
                    //actions.single_target+=/festering_strike,if=cooldown.empower_rune_weapon.remains=0
                    new Spell(SpellType.Cast, festering_strike,
                        req => cooldown.empower_rune_weapon_remains == 0),
                    //actions.single_target+=/blood_boil,if=cooldown.empower_rune_weapon.remains=0
                    new Spell(SpellType.Cast, blood_boil,
                        req => Spell.UseAoe && cooldown.empower_rune_weapon_remains == 0),
                    //actions.single_target+=/icy_touch,if=cooldown.empower_rune_weapon.remains=0
                    new Spell(SpellType.Buff, icy_touch,
                        req => cooldown.empower_rune_weapon_remains == 0),
                    //actions.single_target+=/empower_rune_weapon,if=blood<1&unholy<1&frost<1
                    new Spell(SpellType.Cast, empower_rune_weapon,
                        req => blood < 1 && unholy < 1 && frost < 1)
                };
            }
        }

        private static ActionList UnholySpread
        {
            get
            {
                return new ActionList
                {
                    //actions.spread=blood_boil,cycle_targets=1,if=!disease.min_ticking
                    new Spell(SpellType.Cast, blood_boil,
                        req =>
                            Spell.UseAoe && disease.min_ticking &&
                            active_enemies_list.Count(u => !disease.ticking_on(u)) > 0),
                    //actions.spread+=/outbreak,if=!disease.min_ticking
                    new Spell(SpellType.Buff, outbreak, req => !disease.min_ticking),
                    //actions.spread+=/plague_strike,if=!disease.min_ticking
                    new Spell(SpellType.Buff, plague_strike, req => !disease.min_ticking)
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

        #endregion
    }

    // ReSharper disable UnusedMember.Global

    public enum DeathKnightTalents
    {
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
    }

    // ReSharper restore UnusedMember.Global

    // ReSharper restore InconsistentNaming
}
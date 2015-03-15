using Styx;
using Styx.WoWInternals.WoWObjects;
using System.Linq;

namespace SimcBasedCoRo.ClassSpecific.DeathKnight
{
    public abstract class DeathKnight : Common
    {
        #region Fields

        public const string antimagic_shell = "Anti-Magic Shell";
        public const string army_of_the_dead = "Army of the Dead";
        public const string blood_charge = "Blood Charge";
        public const string bone_shield = "Bone Shield";
        public const string breath_of_sindragosa = "Breath of Sindragosa";
        public const string conversion = "Conversion";
        public const string crimson_sourge = "Crimson Scourge";
        public const string dancing_rune_weapon = "Dancing Rune Weapon";
        public const string dark_transformation = "Dark Transformation";
        public const string defile = "Defile";
        public const string empower_rune_weapon = "Empower Rune Weapon";
        public const string icebound_fortitude = "Icebound Fortitude";
        public const string outbreak = "Outbreak";
        public const string pillar_of_frost = "Pillar of Frost";
        public const string shadow_infusion = "Shadow Infusion";
        public const string soul_reaper = "Soul Reaper";
        public const string unholy_blight = "Unholy Blight";
        public const string vampiric_blood = "Vampiric Blood";
        public static string killing_machine = "Killing Machine";
        public static string freezing_fog = "Freezing Fog";
        public static string sudden_doom = "Sudden Doom";

        public const string blood_boil = "Blood Boil";
        public const string blood_tap = "Blood Tap";
        public const string death_and_decay = "Death and Decay";
        public const string death_coil = "Death Coil";
        public const string festering_strike = "Festering Strike";
        public const string icy_touch = "Icy Touch";
        public const string lichborne = "Lichborne";
        public const string plague_leech = "Plague Leech";
        public const string plague_strike = "Plague Strike";
        public const string rune_tap = "Rune Tap";
        public const string scourge_strike = "Scourge Strike";
        public const string summon_gargoyle = "Summon Gargoyle";

        public const string necrotic_plague = "Necrotic Plague";
        public const string blood_plague = "Blood Plague";
        public const string frost_fever = "Frost Fever";
        public const string runic_empowerment = "Runic Empowerment";

        #endregion

        #region Properties

        protected static int blood
        {
            get { return Me.GetRuneCount(0) + Me.GetRuneCount(1); ; }
        }

        internal static CombatScenario Scenario
        {
            get { return _scenario ?? (_scenario = new CombatScenario(40, 1.5f)); }
        }

        protected static int death
        {
            get { return Me.GetRuneCount(RuneType.Death); }
        }

        protected static int frost
        {
            get { return Me.GetRuneCount(2) + Me.GetRuneCount(3); }
        }

        protected static uint runic_power
        {
            get { return StyxWoW.Me.CurrentRunicPower; }
        }

        protected static int unholy
        {
            get { return Me.GetRuneCount(4) + Me.GetRuneCount(5); }
        }

        #endregion
    }

    public static class target
    {
        #region Properties

        public static double health_pct
        {
            get { return StyxWoW.Me.CurrentTarget.HealthPercent; }
        }

        public static long time_to_die
        {
            get { return StyxWoW.Me.CurrentTarget.TimeToDeath(); }
        }

        #endregion
    }

    internal static class buff
    {

        #region Properties

        public static bool crimson_scourge_react
        {
            get { return React(DeathKnight.crimson_sourge); }
        }

        public static bool killing_machine_react
        {
            get { return React(DeathKnight.killing_machine); }
        }

        public static bool rime_react
        {
            get { return React(DeathKnight.freezing_fog); }
        }

        public static bool sudden_doom_react
        {
            get { return React(DeathKnight.sudden_doom); }
        }

        private static bool React(string aura)
        {
            return StyxWoW.Me.HasAura(aura);
        }

        #endregion
    }

    public static class cooldown
    {
        #region Properties

        public static double antimagic_shell_remains
        {
            get { return Spell.GetSpellCooldown(DeathKnight.antimagic_shell).TotalSeconds; }
        }

        public static double breath_of_sindragosa_remains
        {
            get { return Spell.GetSpellCooldown(DeathKnight.breath_of_sindragosa).TotalSeconds; }
        }

        public static double defile_remains
        {
            get { return Spell.GetSpellCooldown(DeathKnight.defile).TotalSeconds; }
        }

        public static double empower_rune_weapon_remains
        {
            get { return Spell.GetSpellCooldown(DeathKnight.empower_rune_weapon).TotalSeconds; }
        }

        public static double outbreak_remains
        {
            get { return Spell.GetSpellCooldown(DeathKnight.outbreak).TotalSeconds; }
        }

        public static double pillar_of_frost_remains
        {
            get { return Spell.GetSpellCooldown(DeathKnight.pillar_of_frost).TotalSeconds; }
        }

        public static double soul_reaper_remains
        {
            get { return Spell.GetSpellCooldown(DeathKnight.soul_reaper).TotalSeconds; }
        }

        public static double unholy_blight_remains
        {
            get { return Spell.GetSpellCooldown(DeathKnight.unholy_blight).TotalSeconds; }
        }

        #endregion
    }

    public static class obliterate
    {
        #region Properties

        public static double ready_in
        {
            get { return 0; }
        }

        #endregion
    }

    public static class talent
    {
        #region Properties

        public static bool blood_tap_enabled
        {
            get { return HasTalent(DeathKnight.blood_tap); }
        }

        private static bool HasTalent(string name)
        {
            return StyxWoW.Me.GetLearnedTalents().Any(x => x.Name == name);
        }

        public static bool breath_of_sindragosa_enabled
        {
            get { return HasTalent(DeathKnight.breath_of_sindragosa); }
        }

        public static bool defile_enabled
        {
            get { return HasTalent(DeathKnight.defile); }
        }

        public static bool necrotic_plague_enabled
        {
            get { return HasTalent(DeathKnight.necrotic_plague); }
        }

        public static bool runic_empowerment_enabled
        {
            get { return HasTalent(DeathKnight.runic_empowerment); }
        }

        public static bool unholy_blight_enabled
        {
            get { return HasTalent(DeathKnight.unholy_blight); }
        }

        #endregion
    }

    public static class disease
    {
        #region Fields

        private static readonly string[] listBase = { DeathKnight.blood_plague, DeathKnight.frost_fever };
        private static readonly string[] listWithNecroticPlague = { DeathKnight.necrotic_plague };

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

        public static double max_remains_on(WoWUnit unit)
        {
            double max = double.MinValue;
            foreach (var s in diseaseArray)
            {
                double rmn = unit.GetAuraTimeLeft(s).TotalSeconds;
                if (rmn > max)
                    max = rmn;
            }

            if (max == double.MinValue)
                max = 0;

            return max;
        }

        public static double min_remains_on(WoWUnit unit)
        {
            double min = double.MaxValue;
            foreach (var s in diseaseArray)
            {
                double rmn = unit.GetAuraTimeLeft(s).TotalSeconds;
                if (rmn < min)
                    min = rmn;
            }

            if (min == double.MaxValue)
                min = 0;

            return min;
        }

        public static bool ticking_on(WoWUnit unit)
        {
            return unit.HasAllMyAuras(diseaseArray);
        }

        #endregion

        #region Private Methods

        private static bool max_ticking_on(WoWUnit unit)
        {
            return unit.HasAnyOfMyAuras(diseaseArray);
        }

        #endregion
    }

    public static class dot
    {
        #region Properties

        public static double blood_plague_remains
        {
            get { return StyxWoW.Me.CurrentTarget.GetAuraTimeLeft(DeathKnight.blood_plague).TotalSeconds; }
        }

        public static bool blood_plague_ticking
        {
            get { return blood_plague_remains > 0; }
        }

        public static double breath_of_sindragosa_remains
        {
            get { return StyxWoW.Me.CurrentTarget.GetAuraTimeLeft(DeathKnight.breath_of_sindragosa).TotalSeconds; }
        }

        public static bool breath_of_sindragosa_ticking
        {
            get { return breath_of_sindragosa_remains > 0; }
        }

        public static double frost_fever_remains
        {
            get { return StyxWoW.Me.CurrentTarget.GetAuraTimeLeft(DeathKnight.frost_fever).TotalSeconds; }
        }

        public static bool frost_fever_ticking
        {
            get { return frost_fever_remains > 0; }
        }

        public static double necrotic_plague_remains
        {
            get { return StyxWoW.Me.CurrentTarget.GetAuraTimeLeft(DeathKnight.necrotic_plague).TotalSeconds; }
        }

        public static bool necrotic_plague_ticking
        {
            get { return necrotic_plague_remains > 0; }
        }

        #endregion

        #region Public Methods

        public static double necrotic_plague_remains_on(WoWUnit unit)
        {
            return unit.GetAuraTimeLeft(DeathKnight.necrotic_plague).TotalSeconds;
        }

        public static bool necrotic_plague_ticking_on(WoWUnit unit)
        {
            return necrotic_plague_remains_on(unit) > 0;
        }

        #endregion
    }
}
namespace SimcBasedCoRo.ClassSpecific.DeathKnight
{
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
}
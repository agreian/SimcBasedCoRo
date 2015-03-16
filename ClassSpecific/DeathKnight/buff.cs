using Styx;
using Styx.Common;

namespace SimcBasedCoRo.ClassSpecific.DeathKnight
{
    internal static class buff
    {
        #region Properties

        public static uint blood_charge_stack
        {
            get { return Stack(DeathKnight.blood_charge); }
        }

        public static bool crimson_scourge_react
        {
            get { return React(DeathKnight.crimson_scourge); }
        }

        public static bool dark_transformation_down
        {
            get { return PetDown(DeathKnight.dark_transformation); }
        }

        public static bool killing_machine_react
        {
            get { return React(DeathKnight.killing_machine); }
        }

        public static bool rime_react
        {
            get { return React(DeathKnight.freezing_fog); }
        }

        public static uint shadow_infusion_stack
        {
            get { return Stack(DeathKnight.shadow_infusion); }
        }

        public static bool sudden_doom_react
        {
            get { return React(DeathKnight.sudden_doom); }
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
}
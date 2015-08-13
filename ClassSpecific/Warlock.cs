using SimcBasedCoRo.ClassSpecific.Common;

namespace SimcBasedCoRo.ClassSpecific
{
    public class Warlock : ClassSpecificBase
    {
        public enum WarlockPet
        {
            None = 0,
            Auto = 1,
            Imp = 23,       // Pet.CreatureFamily.Id
            Voidwalker = 16,
            Succubus = 17,
            Felhunter = 15,
            Felguard = 29,
            Doomguard = 19,
            Infernal = 108,
            Other = 99999     // a quest or other pet forced upon us for some reason
        }

        public enum WarlockGrimoireOfSupremecyPets
        {
            FelImp = 100,
            Wrathguard = 104,
            Voidlord = 101,
            Observer = 103,
            Shivarra = 102,
            Terrorguard = 147,
            Abyssal = 148
        }

        public static WarlockPet GetCurrentPet()
        {
            if (!Me.GotAlivePet)
                return WarlockPet.None;

            if (Me.Pet == null)
            {
                return WarlockPet.None;
            }

            try
            {
                // following will fail when we have a non-creature warlock pet
                // .. this happens in quests where we get a pet assigned as Me.Pet (like Eric "The Swift")
                var id = Me.Pet.CreatureFamilyInfo.Id;
            }
            catch
            {
                return WarlockPet.Other;
            }

            switch ((WarlockGrimoireOfSupremecyPets)Me.Pet.CreatureFamilyInfo.Id)
            {
                case (WarlockGrimoireOfSupremecyPets)WarlockPet.Imp:
                case (WarlockGrimoireOfSupremecyPets)WarlockPet.Felguard:
                case (WarlockGrimoireOfSupremecyPets)WarlockPet.Voidwalker:
                case (WarlockGrimoireOfSupremecyPets)WarlockPet.Felhunter:
                case (WarlockGrimoireOfSupremecyPets)WarlockPet.Succubus:
                case (WarlockGrimoireOfSupremecyPets)WarlockPet.Infernal:
                case (WarlockGrimoireOfSupremecyPets)WarlockPet.Doomguard:
                    return (WarlockPet)Me.Pet.CreatureFamilyInfo.Id;

                case WarlockGrimoireOfSupremecyPets.FelImp:
                    return WarlockPet.Imp;
                case WarlockGrimoireOfSupremecyPets.Wrathguard:
                    return WarlockPet.Felguard;
                case WarlockGrimoireOfSupremecyPets.Voidlord:
                    return WarlockPet.Voidwalker;
                case WarlockGrimoireOfSupremecyPets.Observer:
                    return WarlockPet.Felhunter;
                case WarlockGrimoireOfSupremecyPets.Shivarra:
                    return WarlockPet.Succubus;
                case WarlockGrimoireOfSupremecyPets.Abyssal:
                    return WarlockPet.Infernal;
                case WarlockGrimoireOfSupremecyPets.Terrorguard:
                    return WarlockPet.Doomguard;
            }

            return WarlockPet.Other;
        }

        public enum WarlockTalents
        {
            DarkRegeneration = 1,
            SoulLeech,
            HarvestLife,
            SearingFlames = HarvestLife,

            HowlOfTerror,
            MortalCoil,
            Shadowfury,

            SoulLink,
            SacrificialPact,
            DarkBargain,

            BloodHorror,
            BurningRush,
            UnboundWill,

            GrimoireOfSupremacy,
            GrimoireOfService,
            GrimoireOfSacrifice,
            GrimoireOfSynergy = GrimoireOfSacrifice,

            ArchimondesDarkness,
            KiljaedensCunning,
            MannorothsFury,

            SoulburnHaunt,
            Demonbolt = SoulburnHaunt,
            CharredRemains = SoulburnHaunt,
            Cataclysm,
            DemonicServitude
        }

        internal class talent : TalentBase
        {
            #region Fields

            public static readonly talent kiljaedens_cunning = new talent(WarlockTalents.KiljaedensCunning);

            #endregion

            #region Constructors

            private talent(WarlockTalents talent)
                : base((int)talent)
            {
            }

            #endregion
        }
    }
}

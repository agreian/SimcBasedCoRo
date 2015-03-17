using Styx.WoWInternals;

namespace SimcBasedCoRo.Extensions
{
    public static class WoWSpellExtension
    {
        public static int GetCharges(this WoWSpell spell)
        {
            return Lua.GetReturnVal<int>("return GetSpellCharges(" + spell.Id + ")", 0);
        }
    }
}

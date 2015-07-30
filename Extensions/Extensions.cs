namespace SimcBasedCoRo.Extensions
{
    public static class Extensions
    {
        public static bool ToBool(this int intValue)
        {
            return intValue != 0;
        }

        public static bool ToBool(this uint uintValue)
        {
            return uintValue != 0;
        }

        public static int ToInt(this bool boolValue)
        {
            return boolValue ? 1 : 0;
        }
    }
}

using System;
using System.Collections.Generic;
using Styx.WoWInternals;

namespace SimcBasedCoRo
{
    internal static class TalentManager
    {
        private readonly static List<string> _glyphs = new List<string>();

        #region Constructors

        static TalentManager()
        {
            Lua.Events.AttachEvent("GLYPH_UPDATED", Update);

            Update();
        }

        #endregion

        #region Private Methods

        private static void Update()
        {
            Glyphs.Clear();

            // 6 glyphs all the time. Plain and simple!
            for (var i = 1; i <= 6; i++)
            {
                var glyphInfo = Lua.GetReturnValues(String.Format("return GetGlyphSocketInfo({0})", i));

                // add check for 4 members before access because empty sockets weren't returning 'nil' as documented
                if (glyphInfo != null && glyphInfo.Count >= 4 && glyphInfo[3] != "nil" && !string.IsNullOrEmpty(glyphInfo[3]))
                {
                    Glyphs.Add(WoWSpell.FromId(int.Parse(glyphInfo[3])).Name.Replace("Glyph of ", "").Trim());
                }
            }
        }

        public static List<string> Glyphs
        {
            get { return _glyphs; }
        }

        public static bool HasGlyph(string glyphName)
        {
            return Glyphs.Contains(glyphName);
        }

        private static void Update(object sender, LuaEventArgs args)
        {
            Update();
        }

        #endregion
    }
}
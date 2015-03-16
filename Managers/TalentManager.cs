using System;
using System.Collections.Generic;
using Styx.WoWInternals;

namespace SimcBasedCoRo.Managers
{
    internal static class TalentManager
    {
        #region Fields

        private static readonly List<string> _glyphs = new List<string>();

        private static int[] _talentId;

        #endregion

        #region Constructors

        static TalentManager()
        {
            Lua.Events.AttachEvent("GLYPH_UPDATED", Update);
            Lua.Events.AttachEvent("ACTIVE_TALENT_GROUP_CHANGED", Update);
            Lua.Events.AttachEvent("PLAYER_SPECIALIZATION_CHANGED", Update);

            Update();
        }

        #endregion

        #region Public Methods

        public static bool HasGlyph(string glyphName)
        {
            return _glyphs.Contains(glyphName);
        }

        public static bool IsSelected(int index)
        {
            // return Talents.FirstOrDefault(t => t.Index == index).Selected;
            var tier = (index - 1)/3;
            if (tier >= 0 && tier <= 6)
                return _talentId[tier] == index;
            return false;
        }

        #endregion

        #region Private Methods

        private static void Update()
        {
            _talentId = new int[7];

            // Always 21 talents. 7 rows of 3 talents.
            for (var row = 0; row < 7; row++)
            {
                for (var col = 0; col < 3; col++)
                {
                    var selected = Lua.GetReturnVal<bool>(string.Format("local t = select(4, GetTalentInfo({0}, {1}, GetActiveSpecGroup())) if t then return 1 end return nil", row + 1, col + 1), 0);
                    var index = 1 + row*3 + col;

                    if (selected)
                        _talentId[row] = index;
                }
            }

            _glyphs.Clear();

            // 6 glyphs all the time. Plain and simple!
            for (var i = 1; i <= 6; i++)
            {
                var glyphInfo = Lua.GetReturnValues(String.Format("return GetGlyphSocketInfo({0})", i));

                // add check for 4 members before access because empty sockets weren't returning 'nil' as documented
                if (glyphInfo != null && glyphInfo.Count >= 4 && glyphInfo[3] != "nil" && !string.IsNullOrEmpty(glyphInfo[3]))
                {
                    _glyphs.Add(WoWSpell.FromId(int.Parse(glyphInfo[3])).Name.Replace("Glyph of ", "").Trim());
                }
            }
        }

        private static void Update(object sender, LuaEventArgs args)
        {
            Update();
        }

        #endregion
    }
}
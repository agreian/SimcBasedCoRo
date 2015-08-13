using Styx;
using Styx.CommonBot;
using Styx.WoWInternals;

namespace SimcBasedCoRo.Utilities
{
    internal class CogContext
    {
        #region Fields

        internal object Context;
        internal WoWPoint Loc;
        internal string Name;
        internal SpellFindResults Sfr;
        internal WoWSpell Spell;
        internal string TargetDesc;

        #endregion

        #region Constructors

        internal CogContext(object ctx, SpellFindDelegate ssd, SimpleLocationRetriever locrtrv, SimpleStringDelegate descrtrv)
        {
            if (ssd(ctx, out Sfr))
            {
                Spell = Sfr.Override ?? Sfr.Original;
                Name = Spell.Name;
                Context = ctx;

                Loc = WoWPoint.Empty;
                TargetDesc = "";
                if (locrtrv != null)
                {
                    Loc = locrtrv(ctx);
                    if (descrtrv != null)
                    {
                        TargetDesc = descrtrv(ctx) + " ";
                    }
                }
            }
        }

        internal CogContext(CastContext cc, SimpleStringDelegate descrtrv)
        {
            if (cc.Unit != null)
            {
                Loc = cc.Unit.Location;
                Spell = cc.Spell;
                Context = cc.Context;
                Name = cc.Name;
                Sfr = cc.Sfr;
                Spell = cc.Spell;
                if (descrtrv != null)
                {
                    TargetDesc = descrtrv(Context) + " ";
                }
            }
        }

        #endregion
    }
}
using System;
using Styx;
using Styx.CommonBot;
using Styx.WoWInternals.WoWObjects;

namespace SimcBasedCoRo.Utilities
{
    public delegate bool SimpleBooleanDelegate(object context);

    public delegate WoWUnit UnitSelectionDelegate(object context);

    public delegate string SimpleStringDelegate(object context);

    public delegate WoWPoint SimpleLocationRetriever(object context);

    public delegate TimeSpan SimpleTimeSpanDelegate(object context);

    public delegate bool CanCastDelegate(SpellFindResults sfr, WoWUnit unit, bool skipWowCheck = false);

    public delegate bool SpellFindDelegate(object contact, out SpellFindResults sfr);
}
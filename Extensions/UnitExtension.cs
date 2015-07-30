using System;
using System.Collections.Generic;
using System.Linq;
using Styx;
using Styx.CommonBot;
using Styx.WoWInternals.WoWObjects;

namespace SimcBasedCoRo.Extensions
{
    /// <summary>
    /// indicates buff category an aura belongs to.  values must be a unique bit to allow creating 
    /// masks to represent a single aura that provides buffs in multiple categories, such as 
    /// Arcane Brilliance being PartyBuff.Spellpower+PartyBuff.Crit
    /// </summary>
    [Flags]
    public enum PartyBuffType
    {
        // from http://www.wowhead.com/guide=1100
        None = 0,
        Stats = 1, // Mark of the Wild, Legacy of the Emperor, Blessing of Kings, Embrace of the Shale Spider
        Stamina = 1 << 1, // PW:Fortitude, Imp: Blood Pact, Commanding Shout, Qiraji Fortitude
        AttackPower = 1 << 2, // Horn of Winter, Trueshot Aura, Battle Shout
        SpellPower = 1 << 3, // Arcane Brilliance, Dalaran Brilliance, Dark Intent, Still Water
        Haste = 1 << 4, // Unholy Aura, Swiftblade's Cunning, Unleashed Rage, Crackling Howl, Serpent's Swiftness
        Crit = 1 << 5, // Leader of the Pack, Arcane Brilliance, Dalaran Brilliance, Legacy of the White Tiger, Bellowing Roar, Furious Howl, Terrifying Roar, Fearless Roar, Still Water
        Mastery = 1 << 6, // Blessing of Might, Grace of Air, Roar of Courage, Spirit Beast Blessing
        MultiStrike = 1 << 7,
        Versatility = 1 << 8,

        All = Stats | Stamina | AttackPower | SpellPower | Haste | Crit | Mastery | MultiStrike | Versatility
    }

    public static class UnitExtension
    {
        #region Constant

        private const int BANNER_OF_THE_ALLIANCE = 61573;
        private const int BANNER_OF_THE_HORDE = 61574;

        private static readonly Dictionary<string, PartyBuffType> _partyBuffs = new Dictionary<string, PartyBuffType>
        {
            {"Mark of the Wild", PartyBuffType.Stats},
            {"Legacy of the Emperor", PartyBuffType.Stats},
            {"Legacy of the White Tiger", PartyBuffType.Stats},
            {"Blessing of Kings", PartyBuffType.Stats},
            {"Blessing of Forgotten Kings", PartyBuffType.Stats},
            {"Embrace of the Shale Spider", PartyBuffType.Stats},
            {"Lone Wolf: Power of the Primates", PartyBuffType.Stats},
            {"Bark of the Wild", PartyBuffType.Stats},
            {"Blessing of Kongs", PartyBuffType.Stats},
            {"Strength of the Earth", PartyBuffType.Stats},
            {"Power Word: Fortitude", PartyBuffType.Stamina},
            {"Blood Pact", PartyBuffType.Stamina},
            {"Commanding Shout", PartyBuffType.Stamina},
            {"Lone Wolf: Fortitude of the Bear", PartyBuffType.Stamina},
            {"Invigorating Roar", PartyBuffType.Stamina},
            {"Sturdiness", PartyBuffType.Stamina},
            {"Savage Vigor", PartyBuffType.Stamina},
            {"Fortitude", PartyBuffType.Stamina},
            {"Qiraji Fortitude", PartyBuffType.Stamina | PartyBuffType.SpellPower},
            {"Horn of Winter", PartyBuffType.AttackPower},
            {"Trueshot Aura", PartyBuffType.AttackPower},
            {"Battle Shout", PartyBuffType.AttackPower},
            {"Arcane Brilliance", PartyBuffType.SpellPower},
            {"Dalaran Brilliance", PartyBuffType.SpellPower},
            {"Dark Intent", PartyBuffType.SpellPower},
            {"Lone Wolf: Wisdom of the Serpent", PartyBuffType.SpellPower | PartyBuffType.Crit},
            {"Still Water", PartyBuffType.SpellPower},
            {"Serpent's Cunning", PartyBuffType.SpellPower},
            {"Unholy Aura", PartyBuffType.Haste},
            {"Swiftblade's Cunning", PartyBuffType.Haste | PartyBuffType.MultiStrike},
            {"Mind Quickening", PartyBuffType.Haste},
            {"Grace of Air", PartyBuffType.Haste},
            {"Lone Wolf: Haste of the Hyena", PartyBuffType.Haste},
            {"Cackling Howl", PartyBuffType.Haste},
            {"Savage Vigor", PartyBuffType.Haste},
            {"Energizing Spores", PartyBuffType.Haste},
            {"Speed of the Swarm", PartyBuffType.Haste},
            {"Leader of the Pack", PartyBuffType.Crit},
            {"Bellowing Roar", PartyBuffType.Crit},
            {"Legacy of the White Tiger", PartyBuffType.Crit},
            {"Furious Howl", PartyBuffType.Crit},
            {"Terrifying Roar", PartyBuffType.Crit},
            {"Fearless Roar", PartyBuffType.Crit},
            {"Arcane Brilliance", PartyBuffType.Crit},
            {"Dalaran Brilliance", PartyBuffType.Crit},
            {"Lone Wolf: Ferocity of the Raptor", PartyBuffType.Crit},
            {"Terrifying Roar", PartyBuffType.Crit},
            {"Fearless Roar", PartyBuffType.Crit},
            {"Strength of the Pack", PartyBuffType.Crit},
            {"Embrace of the Shale Spider", PartyBuffType.Crit},
            {"Still Water", PartyBuffType.Crit},
            {"Furious Howl", PartyBuffType.Crit},
            {"Spirit Beast Blessing", PartyBuffType.Crit},
            {"Windflurry", PartyBuffType.MultiStrike},
            {"Mind Quickening", PartyBuffType.MultiStrike},
            {"Swiftblade's Cunning", PartyBuffType.MultiStrike},
            {"Dark Intent", PartyBuffType.MultiStrike},
            {"Lone Wolf: Quickness of the Dragonhawk", PartyBuffType.MultiStrike},
            {"Sonic Focus", PartyBuffType.MultiStrike},
            {"Wild Strength", PartyBuffType.MultiStrike},
            {"Double Bite", PartyBuffType.MultiStrike},
            {"Spry Attacks", PartyBuffType.MultiStrike},
            {"Breath of the Winds", PartyBuffType.MultiStrike},
            {"Unholy Aura", PartyBuffType.Versatility},
            {"Mark of the Wild", PartyBuffType.Versatility},
            {" Sanctity Aura", PartyBuffType.Versatility},
            {"Inspiring Presence", PartyBuffType.Versatility},
            {"Lone Wolf: Versatility of the Ravager", PartyBuffType.Versatility},
            {"Tenacity", PartyBuffType.Versatility},
            {"Indomitable", PartyBuffType.Versatility},
            {"Wild Strength", PartyBuffType.Versatility},
            {"Defensive Quills", PartyBuffType.Versatility},
            {"Chitinous Armor", PartyBuffType.Versatility},
            {"Grace", PartyBuffType.Versatility},
            {"Strength of the Earth", PartyBuffType.Versatility},
            {"Blessing of Might", PartyBuffType.Mastery},
            {"Grace of Air", PartyBuffType.Mastery},
            {"Roar of Courage", PartyBuffType.Mastery},
            {"Power of the Grave", PartyBuffType.Mastery},
            {"Moonkin Aura", PartyBuffType.Mastery},
            {"Blessing of Might", PartyBuffType.Mastery},
            {"Grace of Air", PartyBuffType.Mastery},
            {"Lone Wolf: Grace of the Cat", PartyBuffType.Mastery},
            {"Roar of Courage", PartyBuffType.Mastery},
            {"Keen Senses", PartyBuffType.Mastery},
            {"Spirit Beast Blessing", PartyBuffType.Mastery},
            {"Plainswalking", PartyBuffType.Mastery}
        };

        #endregion

        #region Public Methods

        public static uint GetAuraStacks(this WoWUnit onUnit, string auraName, bool fromMyAura = true)
        {
            if (onUnit == null) return 0;

            var wantedAura = onUnit.GetAllAuras().FirstOrDefault(a => a.Name == auraName && a.TimeLeft > TimeSpan.Zero && (!fromMyAura || a.CreatorGuid == StyxWoW.Me.Guid));

            if (wantedAura == null) return 0;

            return wantedAura.StackCount == 0 ? 1 : wantedAura.StackCount;
        }

        public static TimeSpan GetAuraTimeLeft(this WoWUnit onUnit, string auraName, bool fromMyAura = true)
        {
            if (onUnit == null) return TimeSpan.Zero;

            var wantedAura = onUnit.GetAllAuras().FirstOrDefault(a => a != null && a.Name == auraName && a.TimeLeft > TimeSpan.Zero && (!fromMyAura || a.CreatorGuid == StyxWoW.Me.Guid));

            return wantedAura != null ? wantedAura.TimeLeft : TimeSpan.Zero;
        }

        public static TimeSpan GetAuraTimeLeft(this WoWUnit onUnit, int auraId, bool fromMyAura = true)
        {
            if (onUnit == null) return TimeSpan.Zero;

            var wantedAura = onUnit.GetAllAuras().FirstOrDefault(a => a != null && a.SpellId == auraId && a.TimeLeft > TimeSpan.Zero && (!fromMyAura || a.CreatorGuid == StyxWoW.Me.Guid));

            return wantedAura != null ? wantedAura.TimeLeft : TimeSpan.Zero;
        }

        /// <summary>
        /// maps a Spell name to its associated PartyBuff vlaue
        /// </summary>
        /// <param name="name">spell name</param>
        /// <returns>PartyBuff enum mask if exists for spell, otherwise PartyBuff.None</returns>
        public static PartyBuffType GetPartyBuffForSpell(string name)
        {
            PartyBuffType bc;
            if (!_partyBuffs.TryGetValue(name, out bc))
                bc = PartyBuffType.None;

            return bc;
        }

        /// <summary>
        /// Checks if unit has a current target.  Differs from WoWUnit.GotTarget since it will only return true if targeting a WoWUnit
        /// </summary>
        /// <param name="unit">Unit to check for a CurrentTarget</param>
        /// <returns>false: if CurrentTarget == null, otherwise true</returns>
        public static bool GotTarget(this WoWUnit unit)
        {
            return unit.CurrentTarget != null;
        }

        public static bool HasAllMyAuras(this WoWUnit unit, params string[] auras)
        {
            return auras.All(unit.HasMyAura);
        }

        /// <summary>
        ///  Checks for my auras on a specified unit. Returns true if the unit has any aura in the auraNames list applied by player.
        /// </summary>
        /// <param name="unit"> The unit to check auras for. </param>
        /// <param name="auraNames"> Aura names to be checked. </param>
        /// <returns></returns>
        public static bool HasAnyOfMyAuras(this WoWUnit unit, params string[] auraNames)
        {
            var auras = unit.GetAllAuras();
            return auras.Any(a => a.CreatorGuid == StyxWoW.Me.Guid && auraNames.Contains(a.Name));
        }

        public static bool HasAnyShapeshift(this WoWUnit unit, params ShapeshiftForm[] forms)
        {
            ShapeshiftForm currentForm = StyxWoW.Me.Shapeshift;
            return forms.Any(f => f == currentForm);
        }

        /// <summary>
        /// aura considered expired if spell of same name as aura is known and aura not present or has less than specified time remaining
        /// </summary>
        /// <param name="u">unit</param>
        /// <param name="aura">name of aura with spell of same name that applies</param>
        /// <param name="tm"></param>
        /// <param name="myAura"></param>
        /// <returns>true if spell known and aura missing or less than 'secs' time left, otherwise false</returns>
        public static bool HasAuraExpired(this WoWUnit u, string aura, TimeSpan tm, bool myAura = true)
        {
            return u.HasAuraExpired(aura, aura, tm, myAura);
        }

        /// <summary>
        /// aura considered expired if spell is known and aura not present or has less than specified time remaining
        /// </summary>
        /// <param name="u">unit</param>
        /// <param name="spell">spell that applies aura</param>
        /// <param name="auraName">aura</param>
        /// <param name="tm"></param>
        /// <param name="myAura"></param>
        /// <returns>true if spell known and aura missing or less than 'secs' time left, otherwise false</returns>
        public static bool HasAuraExpired(this WoWUnit u, string spell, string auraName, TimeSpan tm, bool myAura = true)
        {
            // need to compare millisecs even though seconds are provided.  otherwise see it as expired 999 ms early because
            // .. of loss of precision
            if (!SpellManager.HasSpell(spell)) return false;

            var wantedAura = u.GetAllAuras().FirstOrDefault(a => a != null && a.Name.Equals(auraName, StringComparison.OrdinalIgnoreCase) && a.TimeLeft > TimeSpan.Zero && (!myAura || a.CreatorGuid == StyxWoW.Me.Guid));

            if (wantedAura == null) return true;

            // be aware: test previously was <= and vague recollection that was needed 
            // .. but no comment and need a way to consider passive ones found with timeleft of 0 as not expired if
            // .. if we pass 0 in as the timespan
            if (wantedAura.TimeLeft < tm) return true;

            return false;
        }

        /// <summary>
        /// aura considered expired if aura not present or less than specified time remaining.  
        /// differs from HasAuraExpired since it assumes you have learned the spell which applies it already
        /// </summary>
        /// <param name="u">unit</param>
        /// <param name="aura">aura</param>
        /// <param name="exp"></param>
        /// <param name="myAura"></param>
        /// <returns>true aura missing or less than 'secs' time left, otherwise false</returns>
        public static bool HasKnownAuraExpired(this WoWUnit u, string aura, TimeSpan exp, bool myAura = true)
        {
            return u.GetAuraTimeLeft(aura, myAura) < exp;
        }

        public static bool HasMyAura(this WoWUnit unit, string aura)
        {
            return HasMyAura(unit, aura, 0);
        }

        /// <summary>
        /// check a WoWUnit for a particular PartyBuff enum
        /// </summary>
        /// <param name="unit">unit to check for buff</param>
        /// <param name="currentPartyBuffType">buff to check for.  may be a mask of multiple buffs if any will do, such as PartyBuff.Stats + PartyBuff.Mastery</param>
        /// <returns>true if any buff matching the mask in 'cat' is found, otherwise false</returns>
        public static bool HasPartyBuff(this WoWUnit unit, PartyBuffType currentPartyBuffType)
        {
            return unit.GetAllAuras().Select(aura => GetPartyBuffForSpell(aura.Name)).Any(partyBuffType => (partyBuffType & currentPartyBuffType) != PartyBuffType.None);
        }

        /// <summary>
        /// checks if unit is targeting you, your minions, a group member, or group pets
        /// </summary>
        /// <param name="u">unit</param>
        /// <returns>true if targeting your guys, false if not</returns>
        public static bool IsAggressive(this WoWUnit u)
        {
            return u.Combat && (u.IsTargetingMeOrPet || u.IsTargetingAnyMinion || u.IsTargetingMyPartyMember || u.IsTargetingMyRaidMember);
        }

        /// <summary>
        /// determines if unit is a melee toon based upon .Class.  for Shaman and Druids 
        /// will return based upon presence of aura 
        /// </summary>
        /// <param name="unit">unit to test for melee-ness</param>
        /// <returns>true: melee toon, false: probably not</returns>
        public static bool IsMelee(this WoWUnit unit)
        {
            if (unit.Class == WoWClass.DeathKnight
                || unit.Class == WoWClass.Paladin
                || unit.Class == WoWClass.Rogue
                || unit.Class == WoWClass.Warrior) return true;

            if (!unit.IsMe)
            {
                if (unit.Class == WoWClass.Hunter
                    || unit.Class == WoWClass.Mage
                    || unit.Class == WoWClass.Priest
                    || unit.Class == WoWClass.Warlock) return false;

                if (unit.Class == WoWClass.Monk) // treat all enemy Monks as melee
                    return true;

                if (unit.Class == WoWClass.Druid && unit.HasAnyShapeshift(ShapeshiftForm.Cat, ShapeshiftForm.Bear)) return true;

                if (unit.Class == WoWClass.Shaman && unit.GetAllAuras().Any(a => a.Name == "Unleashed Rage" && a.CreatorGuid == unit.Guid)) return true;

                return false;
            }

            switch (StyxWoW.Me.Specialization)
            {
                case WoWSpec.DruidFeral:
                case WoWSpec.DruidGuardian:
                case WoWSpec.MonkBrewmaster:
                case WoWSpec.MonkWindwalker:
                case WoWSpec.ShamanEnhancement:
                    return true;
            }

            return false;
        }

        public static bool IsTrainingDummy(this WoWUnit unit)
        {
            var bannerId = StyxWoW.Me.IsHorde ? BANNER_OF_THE_ALLIANCE : BANNER_OF_THE_HORDE;

            return unit != null && unit.Level > 1 && ((unit.CurrentHealth == 1 && unit.MaxHealth < unit.Level) || unit.HasAura(bannerId) || unit.Name.Contains("Training Dummy"));
        }

        /// <summary>
        /// get the effective distance between two mobs accounting for their 
        /// combat reaches (hitboxes)
        /// </summary>
        /// <param name="unitOrigin">toon originating spell/ability.  If no destination specified then assume 'Me' originates and 'unit' is the target</param>
        /// <param name="unitTarget">target of spell.  if null, assume 'unit' is target of spell cast by 'Me'</param>
        /// <returns>normalized attack distance</returns>
        public static float SpellDistance(this WoWUnit unitOrigin, WoWUnit unitTarget = null)
        {
            // abort if mob null
            if (unitOrigin == null) return 0;

            // when called as SomeUnit.SpellDistance()
            // .. convert to SomeUnit.SpellDistance(Me)
            if (unitTarget == null) unitTarget = StyxWoW.Me;

            // when called as SomeUnit.SpellDistance(Me) then
            // .. convert to Me.SpellDistance(SomeUnit)
            if (unitTarget.IsMe)
            {
                unitTarget = unitOrigin;
                unitOrigin = StyxWoW.Me;
            }

            // only use CombatReach of destination target 
            float dist = unitTarget.Location.Distance(unitOrigin.Location) - unitTarget.CombatReach;
            return Math.Max(0, dist);
        }

        #endregion

        #region Private Methods

        private static bool HasAura(this WoWUnit unit, string aura, int stacks, WoWUnit creator)
        {
            if (unit == null) return false;

            return unit.GetAllAuras().Any(a => a.Name == aura && a.StackCount >= stacks && (creator == null || a.CreatorGuid == creator.Guid));
        }

        private static bool HasMyAura(this WoWUnit unit, string aura, int stacks)
        {
            return HasAura(unit, aura, stacks, StyxWoW.Me);
        }

        #endregion
    }
}
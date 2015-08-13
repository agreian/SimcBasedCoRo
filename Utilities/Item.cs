using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using CommonBehaviors.Actions;
using SimcBasedCoRo.Settings;
using Styx;
using Styx.Common;
using Styx.CommonBot;
using Styx.CommonBot.Frames;
using Styx.TreeSharp;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;
using Action = Styx.TreeSharp.Action;

namespace SimcBasedCoRo.Utilities
{
    public static class Item
    {
        #region Fields

        private static readonly Dictionary<uint, string> _itemsSpells = new Dictionary<uint, string>();

        #endregion

        #region Public Methods

        public static Composite CreateUsePotionAndHealthstone(double healthPercent, double manaPercent)
        {
            return new PrioritySelector(
                new Decorator(
                    ret => StyxWoW.Me.HealthPercent < healthPercent,
                    new PrioritySelector(
                        ctx => FindFirstUsableItemBySpell("Healthstone", "Healing Potion", "Life Spirit"),
                        new Decorator(
                            ret => ret != null,
                            new Sequence(
                                // new Action(ret => ((WoWItem)ret).UseContainerItem()),
                                new Action(ret => UseItem((WoWItem) ret, log => string.Format("/use {0} @ {1:F1}% Health", ((WoWItem) ret).Name, StyxWoW.Me.HealthPercent))),
                                CreateWaitForLagDuration()
                                )
                            ),
                        new Decorator(
                            req => StyxWoW.Me.Inventory.Equipped.Neck != null && IsUsableItemBySpell(StyxWoW.Me.Inventory.Equipped.Neck, new HashSet<string> {"Heal"}),
                            UseEquippedItem((uint) WoWInventorySlot.Neck)
                            )
                        )
                    ),
                new Decorator(
                    ret => StyxWoW.Me.PowerType == WoWPowerType.Mana && StyxWoW.Me.ManaPercent < manaPercent,
                    new PrioritySelector(
                        ctx => FindFirstUsableItemBySpell("Restore Mana", "Water Spirit"),
                        new Decorator(
                            ret => ret != null,
                            new Sequence(
                                new Action(ret => UseItem((WoWItem) ret, log => string.Format("/use {0} @ {1:F1}% Mana", ((WoWItem) ret).Name, StyxWoW.Me.ManaPercent))),
                                CreateWaitForLagDuration()
                                )
                            )
                        )
                    )
                );
        }

        public static Composite CreateWaitForLagDuration()
        {
            // return new WaitContinue(TimeSpan.FromMilliseconds((SingularRoutine.Latency * 2) + 150), ret => false, new ActionAlwaysSucceed());
            return CreateWaitForLagDuration(ret => false);
        }

        public static Composite CreateWaitForLagDuration(CanRunDecoratorDelegate orUntil)
        {
            return new DynaWaitContinue(ts => TimeSpan.FromMilliseconds((SimCraftCombatRoutine.Latency * 2) + 150), orUntil, new ActionAlwaysSucceed());
        }

        public static WoWItem FindFirstUsableItemBySpell(params string[] spellNames)
        {
            List<WoWItem> carried = StyxWoW.Me.CarriedItems;
            // Yes, this is a bit of a hack. But the cost of creating an object each call, is negated by the speed of the Contains from a hash set.
            // So take your optimization bitching elsewhere.
            var spellNameHashes = new HashSet<string>(spellNames);

            return (from i in carried
                let spells = i.Effects
                where IsUsableItemBySpell(i, spellNameHashes)
                orderby i.ItemInfo.Level descending
                select i)
                .FirstOrDefault();
        }

        public static bool IsUsableItemBySpell(WoWItem i, HashSet<string> spellNameHashes)
        {
            return i.Usable
                   && i.Cooldown == 0
                   && i.ItemInfo != null
                   && i.ItemInfo.RequiredLevel <= StyxWoW.Me.Level
                   && (i.ItemInfo.RequiredSkillId == 0 || i.ItemInfo.RequiredSkillLevel <= StyxWoW.Me.GetSkill(i.ItemInfo.RequiredSkillId).CurrentValue)
                   && (i.ItemInfo.RequiredSpellId == 0 || (SpellManager.HasSpell(i.ItemInfo.RequiredSpellId) || StyxWoW.Me.HasAura(i.ItemInfo.RequiredSpellId)))
                   && i.Effects.Any(s => s.Spell != null && spellNameHashes.Contains(s.Spell.Name) && s.Spell.CanCast)
                   && i.Entry != 32905 // ignore Bottled Nethergon Vapor
                ;
        }

        public static Composite UseEquippedItem(uint slot)
        {
            return new PrioritySelector(
                ctx => StyxWoW.Me.Inventory.GetItemBySlot(slot),
                new Decorator(
                    ctx => ctx != null && CanUseEquippedItem((WoWItem) ctx),
                    new Action(ctx => UseItem((WoWItem) ctx))
                    )
                );
        }

        public static Composite UseEquippedTrinket(TrinketUsage usage)
        {
            var ps = new PrioritySelector();

            if (SimcBasedCoRoSettings.Instance.Trinket1Usage == usage)
            {
                ps.AddChild(UseEquippedItem((uint) WoWInventorySlot.Trinket1));
            }

            if (SimcBasedCoRoSettings.Instance.Trinket2Usage == usage)
            {
                ps.AddChild(UseEquippedItem((uint) WoWInventorySlot.Trinket2));
            }

            if (!ps.Children.Any()) return new ActionAlwaysFail();

            return ps;
        }

        public static Composite UseItem(uint id)
        {
            return new PrioritySelector(
                ctx => ObjectManager.GetObjectsOfType<WoWItem>().FirstOrDefault(item => item.Entry == id),
                new Decorator(
                    ctx => ctx != null && CanUseItem((WoWItem) ctx),
                    new Action(ctx => UseItem((WoWItem) ctx))
                    )
                );
        }

        #endregion

        #region Private Methods

        private static bool CanUseEquippedItem(WoWItem item)
        {
            var entry = item.Entry;

            // Check for engineering tinkers!
            if (_itemsSpells.ContainsKey(entry) == false)
                _itemsSpells.Add(entry, Lua.GetReturnVal<string>("return GetItemSpell(" + item.Entry + ")", 0));

            if (string.IsNullOrEmpty(_itemsSpells[entry])) return false;

            return CanUseItem(item);
        }

        private static bool CanUseItem(WoWItem item)
        {
            return item.Usable && item.Cooldown <= 0 && !MerchantFrame.Instance.IsVisible;
        }

        private static void UseItem(WoWItem item, SimpleStringDelegate log)
        {
            if (!CanUseItem(item))
                return;

            if (log == null)
                log = s => string.Format("/use {0}", item.Name);

            Logging.Write(LogColor.Hilite, log(item));
            item.Use();
        }

        private static void UseItem(WoWItem item)
        {
            if (!CanUseItem(item)) return;

            Logging.Write(Colors.White, string.Format("/use {0}", item.Name));
            item.Use();
        }

        #endregion
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using CommonBehaviors.Actions;
using SimcBasedCoRo.Settings;
using Styx;
using Styx.Common;
using Styx.CommonBot.Frames;
using Styx.TreeSharp;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;
using Action = Styx.TreeSharp.Action;

namespace SimcBasedCoRo.Utilities
{
    public static class Item
    {
        private static readonly Dictionary<uint, string> _itemsSpells = new Dictionary<uint, string>();

        #region Public Methods

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

        private static void UseItem(WoWItem item)
        {
            if (!CanUseItem(item)) return;

            Logging.Write(Colors.White, string.Format("/use {0}", item.Name));
            item.Use();
        }

        #endregion
    }
}
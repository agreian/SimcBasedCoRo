using System.ComponentModel;
using System.IO;
using Styx;
using Styx.Helpers;
using DefaultValue = Styx.Helpers.DefaultValueAttribute;

namespace SimcBasedCoRo.Settings
{
    public class SimcBasedCoRoSettings : Styx.Helpers.Settings
    {
        #region Fields

        private static SimcBasedCoRoSettings _instance;

        #endregion

        #region Constructors

        public SimcBasedCoRoSettings()
            : base(Path.Combine(CharacterSettingsPath, "SimcBasedCoRo.xml"))
        {
        }

        #endregion

        #region Properties

        [Browsable(false)]
        public static string CharacterSettingsPath
        {
            get
            {
                var settingsDirectory = Path.Combine(Styx.Common.Utilities.AssemblyDirectory, "Settings");
                return Path.Combine(Path.Combine(settingsDirectory, StyxWoW.Me.RealmName), StyxWoW.Me.Name);
            }
        }

        [Browsable(false)]
        public static SimcBasedCoRoSettings Instance
        {
            get { return _instance ?? (_instance = new SimcBasedCoRoSettings()); }
            set { _instance = value; }
        }

        [Setting, ReadOnly(false)]
        [DefaultValue(false)]
        [Category("Pets")]
        [DisplayName("Disable Pet usage")]
        [Description("Enabling that will disable pet usage")]
        public bool DisablePetUsage { get; set; }

        [Setting, ReadOnly(false)]
        [DefaultValue(TrinketUsage.OnCooldownInCombat)]
        [Category("Items")]
        [DisplayName("Trinket 1 Usage")]
        public TrinketUsage Trinket1Usage { get; set; }

        [Setting, ReadOnly(false)]
        [DefaultValue(TrinketUsage.OnCooldownInCombat)]
        [Category("Items")]
        [DisplayName("Trinket 2 Usage")]
        public TrinketUsage Trinket2Usage { get; set; }

        [Setting, ReadOnly(false)]
        [DefaultValue(CheckTargets.All)]
        [Category("Enemy Control")]
        [DisplayName("Interrupt Targets")]
        [Description("None: disabled, Current: our target only, All: any enemy in range.")]
        public CheckTargets InterruptTarget { get; set; }

        #endregion

        #region Public Methods

        public static bool IsTrinketUsageWanted(TrinketUsage usage)
        {
            return usage == Instance.Trinket1Usage || usage == Instance.Trinket2Usage;
        }

        #endregion
    }

    public enum CheckTargets
    {
        None = 0,
        Current,
        All
    }

    public enum TrinketUsage
    {
        Never,
        OnCooldown,
        OnCooldownInCombat,
        LowPower,
        LowHealth,
        CrowdControlled,
        CrowdControlledSilenced
    }
}
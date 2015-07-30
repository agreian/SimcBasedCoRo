using SimcBasedCoRo.Managers;

namespace SimcBasedCoRo.ClassSpecific.Common
{
    internal class TalentBase
    {
        #region Fields

        private readonly int _talent;

        #endregion

        #region Constructors

        public TalentBase(int talent)
        {
            _talent = talent;
        }

        #endregion

        // ReSharper disable InconsistentNaming

        #region Properties

        public bool enabled
        {
            get { return TalentManager.IsSelected(_talent); }
        }

        #endregion

        // ReSharper restore InconsistentNaming
    }
}
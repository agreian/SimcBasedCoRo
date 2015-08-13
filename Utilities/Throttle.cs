using System;
using System.Collections.Generic;
using Styx.TreeSharp;

namespace SimcBasedCoRo.Utilities
{
    public class Throttle : Decorator
    {
        #region Fields

        private readonly RunStatus _limitStatus;

        private int _count;
        private DateTime _end;

        #endregion

        #region Constructors

        public Throttle(int limit, TimeSpan timeFrame, RunStatus limitStatus, params Composite[] children)
            : base(ChildComposite(children))
        {
            TimeFrame = timeFrame;
            Limit = limit;

            _end = DateTime.MinValue;
            _count = 0;
            _limitStatus = limitStatus;
        }

        public Throttle(TimeSpan timeFrame, params Composite[] children)
            : this(1, timeFrame, RunStatus.Failure, ChildComposite(children))
        {
        }

        public Throttle(int limit, TimeSpan timeFrame, params Composite[] children)
            : this(limit, timeFrame, RunStatus.Failure, ChildComposite(children))
        {
        }

        public Throttle(int limit, int timeSeconds, params Composite[] children)
            : this(limit, TimeSpan.FromSeconds(timeSeconds), RunStatus.Failure, ChildComposite(children))
        {
        }

        public Throttle(int timeSeconds, params Composite[] children)
            : this(1, TimeSpan.FromSeconds(timeSeconds), RunStatus.Failure, ChildComposite(children))
        {
        }

        public Throttle(params Composite[] children)
            : this(1, TimeSpan.FromMilliseconds(250), RunStatus.Failure, ChildComposite(children))
        {
        }

        #endregion

        #region Properties

        public int Limit { get; set; }

        public TimeSpan TimeFrame { get; set; }

        #endregion

        #region Private Methods

        protected override IEnumerable<RunStatus> Execute(object context)
        {
            if (DateTime.UtcNow < _end && _count >= Limit)
            {
                yield return _limitStatus;
                yield break;
            }

            // check not present in Decorator, but adding here
            if (DecoratedChild == null)
            {
                yield return RunStatus.Failure;
                yield break;
            }

            DecoratedChild.Start(context);
            
            while (DecoratedChild.Tick(context) == RunStatus.Running)
            {
                yield return RunStatus.Running;
            }

            DecoratedChild.Stop(context);

            if (DecoratedChild.LastStatus == RunStatus.Failure)
            {
                yield return RunStatus.Failure;
                yield break;
            }

            if (DateTime.UtcNow > _end)
            {
                _count = 0;
                _end = DateTime.UtcNow + TimeFrame;
            }

            _count++;

            yield return RunStatus.Success;
        }

        private static Composite ChildComposite(params Composite[] children)
        {
            if (children.GetLength(0) == 1)
                return children[0];
            return new PrioritySelector(children);
        }

        #endregion
    }
}
using System;
using System.Collections.Generic;
using Styx.Common;
using Styx.TreeSharp;

namespace SimcBasedCoRo.Utilities
{
    public class DynaWait : Decorator
    {
        #region Fields

        private readonly bool _measure;
        private readonly SimpleTimeSpanDelegate _span;

        private DateTime _begin;
        private DateTime _end;

        #endregion

        #region Constructors

        public DynaWait(SimpleTimeSpanDelegate span, CanRunDecoratorDelegate runFunc, Composite child, bool measure = false)
            : base(runFunc, child)
        {
            _span = span;
            _measure = measure;
        }

        #endregion

        #region Public Methods

        public override void Start(object context)
        {
            _begin = DateTime.UtcNow;
            _end = DateTime.UtcNow + _span(context);
            base.Start(context);
        }

        public override void Stop(object context)
        {
            _end = DateTime.MinValue;
            base.Stop(context);

            if (_measure)
            {
                Logging.Write("Duration: {0:F0} ms", (DateTime.UtcNow - _begin).TotalMilliseconds);
            }
        }

        #endregion

        #region Private Methods

        protected override IEnumerable<RunStatus> Execute(object context)
        {
            while (DateTime.UtcNow < _end)
            {
                if (Runner != null)
                {
                    if (Runner(context))
                    {
                        break;
                    }
                }
                else
                {
                    if (CanRun(context))
                    {
                        break;
                    }
                }

                yield return RunStatus.Running;
            }

            if (DateTime.UtcNow > _end)
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

            yield return RunStatus.Success;
        }

        #endregion
    }
}
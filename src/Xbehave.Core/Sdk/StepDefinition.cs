// <copyright file="StepDefinition.cs" company="xBehave.net contributors">
//  Copyright (c) xBehave.net contributors. All rights reserved.
// </copyright>

namespace Xbehave.Sdk
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;

    [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Step", Justification = "By design.")]
    internal class StepDefinition : IStepDefinition
    {
        public string Text { get; set; }

        public Func<IStepContext, Task> Body { get; set; }

        public ICollection<Func<IStepContext, Task>> Teardowns { get; } = new List<Func<IStepContext, Task>>();

        public string SkipReason { get; set; }

        public RemainingSteps FailureBehavior { get; set; }

        public IStepDefinition Skip(string reason)
        {
            this.SkipReason = reason;
            return this;
        }

        public IStepDefinition Teardown(Func<IStepContext, Task> action)
        {
            if (action != null)
            {
                this.Teardowns.Add(action);
            }

            return this;
        }

        public IStepDefinition OnFailure(RemainingSteps behavior)
        {
            this.FailureBehavior = behavior;
            return this;
        }

        IStepBuilder IStepBuilder.Skip(string reason) => this.Skip(reason);

        IStepBuilder IStepBuilder.Teardown(Func<IStepContext, Task> action) => this.Teardown(action);

        IStepBuilder IStepBuilder.OnFailure(RemainingSteps behavior) => this.OnFailure(behavior);
    }
}

﻿using FluentAssertions;
using ModularPipelines.Context;

namespace TUnit.Pipeline.Modules.Tests;

public class PriorityFilteringTests5 : TestModule
{
    protected override async Task<TestResult?> ExecuteAsync(IPipelineContext context, CancellationToken cancellationToken)
    {
        return await RunTestsWithFilter(context, 
            "/*/*/PriorityFilteringTests/*[Priority=*]",
            [
                result => result.Successful.Should().BeTrue(),
                result => result.Total.Should().Be(6),
                result => result.Passed.Should().Be(6),
                result => result.Failed.Should().Be(0),
                result => result.Skipped.Should().Be(0)
            ], cancellationToken);
    }
}
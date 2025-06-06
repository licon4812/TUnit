﻿using System.Collections.Concurrent;
using TUnit.Assertions;
using TUnit.Assertions.Extensions;
using TUnit.TestProject.Attributes;

namespace TUnit.TestProject;

[EngineTest(ExpectedResult.Pass)]
public class ParallelTests
{
    private static readonly ConcurrentBag<DateTimeRange> TestDateTimeRanges = [];

    [After(Test)]
    public async Task TestOverlaps()
    {
        TestDateTimeRanges.Add(new DateTimeRange(TestContext.Current!.TestStart!.Value.DateTime, TestContext.Current.Result!.End!.Value.DateTime));

        await AssertOverlaps();
    }

    [Test, Repeat(3)]
    public async Task Parallel_Test1()
    {
        await Task.Delay(5000);
    }

    [Test, Repeat(3)]
    public async Task Parallel_Test2()
    {
        await Task.Delay(5000);
    }

    [Test, Repeat(3)]
    public async Task Parallel_Test3()
    {
        await Task.Delay(5000);
    }

    private async Task AssertOverlaps()
    {
        if (TestDateTimeRanges.Count < 5)
        {
            return;
        }
        
        foreach (var testDateTimeRange in TestDateTimeRanges)
        {
            await Assert.That(TestDateTimeRanges
                .Except([testDateTimeRange])
                .Any(x => x.Overlap(testDateTimeRange)))
                .IsTrue();
        }
    }

    private class DateTimeRange(DateTime start, DateTime end)
    {
        public DateTime Start { get; } = start;
        public DateTime End { get; } = end;

        public bool Overlap(DateTimeRange other)
        {
            return Start <= other.End && other.Start <= End;
        }
    }
}
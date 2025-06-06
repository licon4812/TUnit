﻿using TUnit.TestProject.Attributes;
using TUnit.TestProject.Library.Models;

namespace TUnit.TestProject;

[EngineTest(ExpectedResult.Pass)]
[ParallelLimiter<ParallelLimit3>]
public class ParallelLimiterTests
{
    [Test, Repeat(3)]
    public async Task Parallel_Test1()
    {
        await Task.Delay(500);
    }

    [Test, Repeat(3)]
    public async Task Parallel_Test2()
    {
        await Task.Delay(500);
    }

    [Test, Repeat(3)]
    public async Task Parallel_Test3()
    {
        await Task.Delay(500);
    }
}
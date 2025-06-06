﻿using System.Diagnostics.CodeAnalysis;
using TUnit.TestProject.Attributes;

namespace TUnit.TestProject;

[EngineTest(ExpectedResult.Failure)]
[UnconditionalSuppressMessage("Usage", "TUnit0033:Conflicting DependsOn attributes")]
public class ConflictingDependsOnTests3
{
    [Test, DependsOn(nameof(Test5))]
    public async Task Test1()
    {
        await Task.Delay(TimeSpan.FromSeconds(5));
    }

    [Test, DependsOn(nameof(Test1))]
    public async Task Test2()
    {
        await Task.CompletedTask;
    }

    [Test, DependsOn(nameof(Test2))]
    public async Task Test3()
    {
        await Task.CompletedTask;
    }

    [Test, DependsOn(nameof(Test3))]
    public async Task Test4()
    {
        await Task.CompletedTask;
    }

    [Test, DependsOn(nameof(Test4))]
    public async Task Test5()
    {
        await Task.CompletedTask;
    }
}
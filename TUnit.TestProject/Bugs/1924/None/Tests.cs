﻿using TUnit.TestProject.Attributes;

namespace TUnit.TestProject.Bugs._1924.None;

[EngineTest(ExpectedResult.Pass)]
public class BaseClass
{
    [ClassDataSource<DataClass>]
    public required DataClass Data { get; init; }
    
    [Test]
    [Arguments(1)]
    [Arguments(2)]
    [Arguments(3)]
    [Repeat(10)]
    public async Task Test(int value)
    {
        await Data.DoSomething();
        await Task.Delay(TimeSpan.FromMilliseconds(50));
    }
}

[InheritsTests]
public class Tests : BaseClass;

[InheritsTests]
public class Tests2 : BaseClass;

[InheritsTests]
public class Tests3 : BaseClass;
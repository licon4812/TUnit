﻿#if NET
using TUnit.Assertions.Extensions;

namespace TUnit.Assertions.UnitTests;

public class TimeOnlyEqualToAssertionTests
{
    private static readonly TimeOnly TestTime = new TimeOnly(13, 14, 15);
    
    [Test]
    public async Task EqualsTo_Success()
    {
        var value1 = TestTime.Add(TimeSpan.FromSeconds(1.1));
        var value2 = TestTime.Add(TimeSpan.FromSeconds(1.1));
        
        await TUnitAssert.That(value1).IsEqualTo(value2);
    }
    
    [Test]
    public async Task EqualsTo_Failure()
    {
        var value1 = TestTime.Add(TimeSpan.FromSeconds(1.1));
        var value2 = TestTime.Add(TimeSpan.FromSeconds(1.2));
        
        await TUnitAssert.ThrowsAsync<TUnitAssertionException>(async () => await TUnitAssert.That(value1).IsEqualTo(value2));
    }
    
    [Test]
    public async Task EqualsTo__With_Tolerance_Success()
    {
        var value1 = TestTime.Add(TimeSpan.FromSeconds(1.1));
        var value2 = TestTime.Add(TimeSpan.FromSeconds(1.2));
        
        await TUnitAssert.That(value1).IsEqualTo(value2).Within(TimeSpan.FromSeconds(0.1));
    }
    
    [Test]
    public async Task EqualsTo__With_Tolerance_Failure()
    {
        var value1 = TestTime.Add(TimeSpan.FromSeconds(1.1));
        var value2 = TestTime.Add(TimeSpan.FromSeconds(1.3));
        
        await TUnitAssert.ThrowsAsync<TUnitAssertionException>(async () => await TUnitAssert.That(value1).IsEqualTo(value2).Within(TimeSpan.FromSeconds(0.1)));
    }
}
#endif
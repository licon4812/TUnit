﻿namespace TUnit.Assertions.Tests.Old;

public class BoolEqualToAssertionTests
{
    [Test]
    public async Task EqualsTo_Success()
    {
        var value1 = true;
        var value2 = true;
        
        await TUnitAssert.That(value1).IsEqualTo(value2);
    }
    
    [Test]
    public async Task EqualsTo_Success2()
    {
        bool? value1 = null;
        bool? value2 = null;
        
        await TUnitAssert.That(value1).IsEqualTo(value2);
    }
    
    [Test]
    public async Task EqualsTo_Failure()
    {
        var value1 = true;
        var value2 = false;
        
        await TUnitAssert.ThrowsAsync<TUnitAssertionException>(async () => await TUnitAssert.That(value1).IsEqualTo(value2));
    }
    
    [Test]
    public async Task EqualsTo_Failure2()
    {
        bool? value1 = null;
        var value2 = true;
        
        await TUnitAssert.ThrowsAsync<TUnitAssertionException>(async () => await TUnitAssert.That(value1).IsEqualTo(value2));
    }
}
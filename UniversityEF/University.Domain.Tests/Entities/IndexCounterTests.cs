using System;
using University.Domain.Entities;
using Xunit;

namespace University.Domain.Tests.Entities;

public class IndexCounterTests
{
    [Fact]
    public void DefaultValues_AreEmptyAndZero()
    {
        var index = new IndexCounter();

        Assert.Equal(string.Empty, index.Prefix);
        Assert.Equal(0, index.CurrentValue);
    }
}

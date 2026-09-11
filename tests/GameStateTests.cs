using MOMO.Core;
using Xunit;

namespace MOMO.Tests;

public sealed class GameStateTests
{
    [Fact]
    public void NewGameStartsAtVerticalSlice()
    {
        var state = new GameState();
        Assert.Equal("vertical_slice", state.CurrentRegionId);
    }
}

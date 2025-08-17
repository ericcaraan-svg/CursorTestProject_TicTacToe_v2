using TicTacToe.Core;
using Xunit;

namespace TicTacToe.Tests;

public class BoardTests
{
    [Fact]
    public void EmptyBoard_IsInProgress()
    {
        var b = new Board();
        Assert.Equal(GameStatus.InProgress, b.GetStatus());
    }

    [Fact]
    public void XCompletesRow_Wins()
    {
        // PROMPT: Ask Cursor to make Apply(...) work immutably and then make this pass.
        // Arrange a sequence where X wins on top row.
        // b = b.Apply(new Move(0,0,Cell.X)) ... etc.
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using TicTacToe.Core;
using Xunit;

namespace TicTacToe.Tests;

public class BoardTests
{
    [Fact]
    public void Constructor_CreatesEmptyBoard()
    {
        var board = new Board();
        
        // All cells should be empty
        for (int r = 0; r < 3; r++)
        {
            for (int c = 0; c < 3; c++)
            {
                Assert.Equal(Cell.Empty, board[r, c]);
            }
        }
        
        // Current player should be X
        Assert.Equal(Cell.X, board.CurrentPlayer);
    }

    [Fact]
    public void Indexer_ReturnsCorrectCell()
    {
        var board = new Board();
        
        // Test indexer access
        Assert.Equal(Cell.Empty, board[0, 0]);
        Assert.Equal(Cell.Empty, board[2, 2]);
        Assert.Equal(Cell.Empty, board[1, 0]);
    }

    [Fact]
    public void Apply_ValidMove_ReturnsNewBoard()
    {
        var board = new Board();
        var move = new Move(1, 1, Cell.X);
        
        var newBoard = board.Apply(move);
        
        // Original board should remain unchanged
        Assert.Equal(Cell.Empty, board[0, 0]);
        Assert.Equal(Cell.X, board.CurrentPlayer);
        
        // New board should have the move applied
        Assert.Equal(Cell.X, newBoard[0, 0]);
        Assert.Equal(Cell.O, newBoard.CurrentPlayer);
        
        // Other cells should remain empty
        Assert.Equal(Cell.Empty, newBoard[0, 1]);
        Assert.Equal(Cell.Empty, newBoard[1, 0]);
    }

    [Fact]
    public void Apply_ValidMove_FlipsCurrentPlayer()
    {
        var board = new Board();
        
        // X moves first
        var boardAfterX = board.Apply(new Move(1, 1, Cell.X));
        Assert.Equal(Cell.O, boardAfterX.CurrentPlayer);
        
        // O moves next
        var boardAfterO = boardAfterX.Apply(new Move(1, 2, Cell.O));
        Assert.Equal(Cell.X, boardAfterO.CurrentPlayer);
    }

    [Fact]
    public void Apply_InvalidMove_ThrowsArgumentException()
    {
        var board = new Board();
        
        // Test invalid coordinates
        Assert.Throws<ArgumentException>(() => board.Apply(new Move(0, 1, Cell.X)));
        Assert.Throws<ArgumentException>(() => board.Apply(new Move(1, 0, Cell.X)));
        Assert.Throws<ArgumentException>(() => board.Apply(new Move(4, 1, Cell.X)));
        Assert.Throws<ArgumentException>(() => board.Apply(new Move(1, 4, Cell.X)));
    }

    [Fact]
    public void Apply_OccupiedCell_ThrowsArgumentException()
    {
        var board = new Board();
        var firstMove = new Move(1, 1, Cell.X);
        var boardAfterFirstMove = board.Apply(firstMove);
        
        // Try to move to already occupied cell
        Assert.Throws<ArgumentException>(() => boardAfterFirstMove.Apply(new Move(1, 1, Cell.O)));
    }

    [Fact]
    public void Apply_WrongPlayer_ThrowsArgumentException()
    {
        var board = new Board();
        
        // Try to move O when it's X's turn
        Assert.Throws<ArgumentException>(() => board.Apply(new Move(1, 1, Cell.O)));
    }

    [Fact]
    public void GetStatus_EmptyBoard_ReturnsInProgress()
    {
        var board = new Board();
        Assert.Equal(GameStatus.InProgress, board.GetStatus());
    }

    [Fact]
    public void GetStatus_HorizontalWin_ReturnsCorrectWinner()
    {
        var board = new Board();
        
        // X wins on top row
        board = board.Apply(new Move(1, 1, Cell.X));
        board = board.Apply(new Move(2, 1, Cell.O));
        board = board.Apply(new Move(1, 2, Cell.X));
        board = board.Apply(new Move(2, 2, Cell.O));
        board = board.Apply(new Move(1, 3, Cell.X));
        
        Assert.Equal(GameStatus.XWins, board.GetStatus());
    }

    [Fact]
    public void GetStatus_VerticalWin_ReturnsCorrectWinner()
    {
        var board = new Board();
        
        // O wins on left column
        board = board.Apply(new Move(1, 1, Cell.X));
        board = board.Apply(new Move(1, 2, Cell.O));
        board = board.Apply(new Move(2, 1, Cell.X));
        board = board.Apply(new Move(2, 2, Cell.O));
        board = board.Apply(new Move(3, 3, Cell.X));
        board = board.Apply(new Move(3, 2, Cell.O));
        
        Assert.Equal(GameStatus.OWins, board.GetStatus());
    }

    [Fact]
    public void GetStatus_DiagonalWin_ReturnsCorrectWinner()
    {
        var board = new Board();
        
        // X wins on diagonal (top-left to bottom-right)
        board = board.Apply(new Move(1, 1, Cell.X));
        board = board.Apply(new Move(1, 2, Cell.O));
        board = board.Apply(new Move(2, 2, Cell.X));
        board = board.Apply(new Move(2, 1, Cell.O));
        board = board.Apply(new Move(3, 3, Cell.X));
        
        Assert.Equal(GameStatus.XWins, board.GetStatus());
    }

    [Fact]
    public void GetStatus_ReverseDiagonalWin_ReturnsCorrectWinner()
    {
        var board = new Board();
        
        // O wins on reverse diagonal (top-right to bottom-left)
        board = board.Apply(new Move(1, 3, Cell.X));
        board = board.Apply(new Move(1, 1, Cell.O));
        board = board.Apply(new Move(2, 1, Cell.X));
        board = board.Apply(new Move(2, 2, Cell.O));
        board = board.Apply(new Move(3, 1, Cell.X));
        board = board.Apply(new Move(3, 3, Cell.O));
        
        Assert.Equal(GameStatus.OWins, board.GetStatus());
    }

    [Fact]
    public void GetStatus_Draw_ReturnsDraw()
    {
        var board = new Board();
        
        // Create a draw scenario - X starts, then O, alternating
        // This creates a pattern where no player gets 3 in a row
        board = board.Apply(new Move(2, 2, Cell.X));
        board = board.Apply(new Move(2, 1, Cell.O));
        board = board.Apply(new Move(3, 1, Cell.X));
        board = board.Apply(new Move(1, 3, Cell.O));
        board = board.Apply(new Move(1, 1, Cell.X));
        board = board.Apply(new Move(3, 3, Cell.O));
        board = board.Apply(new Move(2, 3, Cell.X));
        board = board.Apply(new Move(1, 2, Cell.O));        
        board = board.Apply(new Move(3, 2, Cell.X));
        
        Assert.Equal(GameStatus.Draw, board.GetStatus());
    }

    [Fact]
    public void GetEmptyCells_EmptyBoard_ReturnsAllCells()
    {
        var board = new Board();
        var emptyCells = board.GetEmptyCells().ToList();
        
        Assert.Equal(9, emptyCells.Count);
        
        // Check that all expected coordinates are present
        var expectedCells = new List<(int r, int c)>
        {
            (0, 0), (0, 1), (0, 2),
            (1, 0), (1, 1), (1, 2),
            (2, 0), (2, 1), (2, 2)
        };
        
        foreach (var expected in expectedCells)
        {
            Assert.Contains(expected, emptyCells);
        }
    }

    [Fact]
    public void GetEmptyCells_PartiallyFilledBoard_ReturnsCorrectEmptyCells()
    {
        var board = new Board();
        board = board.Apply(new Move(1, 1, Cell.X));
        board = board.Apply(new Move(2, 2, Cell.O));
        
        var emptyCells = board.GetEmptyCells().ToList();
        
        Assert.Equal(7, emptyCells.Count);
        
        // Check that occupied cells are not in the list
        Assert.DoesNotContain((0, 0), emptyCells); // X at (1,1) in user coordinates = (0,0) internally
        Assert.DoesNotContain((1, 1), emptyCells); // O at (2,2) in user coordinates = (1,1) internally
        
        // Check that empty cells are in the list
        Assert.Contains((0, 1), emptyCells);
        Assert.Contains((0, 2), emptyCells);
        Assert.Contains((1, 0), emptyCells);
        Assert.Contains((1, 2), emptyCells);
        Assert.Contains((2, 0), emptyCells);
        Assert.Contains((2, 1), emptyCells);
        Assert.Contains((2, 2), emptyCells);
    }

    [Fact]
    public void GetEmptyCells_FullBoard_ReturnsEmptyList()
    {
        var board = new Board();
        
        // Fill the board - X starts, then O, alternating
        // This creates a pattern where no player gets 3 in a row
        board = board.Apply(new Move(1, 1, Cell.X));
        board = board.Apply(new Move(1, 2, Cell.O));
        board = board.Apply(new Move(1, 3, Cell.X));
        board = board.Apply(new Move(2, 1, Cell.O));
        board = board.Apply(new Move(2, 2, Cell.X));
        board = board.Apply(new Move(2, 3, Cell.O));
        board = board.Apply(new Move(3, 2, Cell.X));
        board = board.Apply(new Move(3, 1, Cell.O));        
        board = board.Apply(new Move(3, 3, Cell.X));
        
        var emptyCells = board.GetEmptyCells().ToList();
        Assert.Empty(emptyCells);
    }

    [Fact]
    public void Apply_MultipleMoves_MaintainsImmutability()
    {
        var board = new Board();
        var originalBoard = board;
        
        // Apply several moves
        board = board.Apply(new Move(1, 1, Cell.X));
        board = board.Apply(new Move(1, 2, Cell.O));
        board = board.Apply(new Move(2, 2, Cell.X));
        
        // Original board should remain unchanged
        Assert.Equal(Cell.Empty, originalBoard[0, 0]);
        Assert.Equal(Cell.Empty, originalBoard[0, 1]);
        Assert.Equal(Cell.Empty, originalBoard[1, 1]);
        Assert.Equal(Cell.X, originalBoard.CurrentPlayer);
        
        // New board should have all moves applied
        Assert.Equal(Cell.X, board[0, 0]);
        Assert.Equal(Cell.O, board[0, 1]);
        Assert.Equal(Cell.X, board[1, 1]);
        Assert.Equal(Cell.O, board.CurrentPlayer);
    }
}

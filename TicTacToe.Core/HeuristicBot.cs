namespace TicTacToe.Core;

public class HeuristicBot : IBotStrategy
{
    // PROMPT: Ask Cursor to implement a simple heuristic:
    // 1) If winning move exists, take it.
    // 2) If opponent can win next, block it.
    // 3) Prefer center, then corners, then edges.
    public Move ChooseMove(Board board)
    {
        // 1. Check if bot can win in one move
        var winningMove = FindWinningMove(board, Cell.O);
        if (winningMove.HasValue)
        {
            return new Move(winningMove.Value.r + 1, winningMove.Value.c + 1, Cell.O);
        }
        
        // 2. Check if human can win next move and block it
        var blockingMove = FindWinningMove(board, Cell.X);
        if (blockingMove.HasValue)
        {
            return new Move(blockingMove.Value.r + 1, blockingMove.Value.c + 1, Cell.O);
        }
        
        // 3. Prefer center if available
        if (board[1, 1] == Cell.Empty)
        {
            return new Move(2, 2, Cell.O);
        }
        
        // 3.5. Place bot's piece beside its existing pieces if possible
        var adjacentMove = FindAdjacentToBotPiece(board);
        if (adjacentMove.HasValue)
        {
            return new Move(adjacentMove.Value.r + 1, adjacentMove.Value.c + 1, Cell.O);
        }
        
        // 4. Prefer corners if available
        var corners = new[] { (0, 0), (0, 2), (2, 0), (2, 2) };
        foreach (var (r, c) in corners)
        {
            if (board[r, c] == Cell.Empty)
            {
                return new Move(r + 1, c + 1, Cell.O);
            }
        }
        
        // 5. Take any available edge position
        var edges = new[] { (0, 1), (1, 0), (1, 2), (2, 1) };
        foreach (var (r, c) in edges)
        {
            if (board[r, c] == Cell.Empty)
            {
                return new Move(r + 1, c + 1, Cell.O);
            }
        }
        
        // Fallback: take any empty cell (shouldn't reach here if game is valid)
        var emptyCells = board.GetEmptyCells();
        var firstEmpty = emptyCells.First();
        return new Move(firstEmpty.r + 1, firstEmpty.c + 1, Cell.O);
    }
    
    private (int r, int c)? FindWinningMove(Board board, Cell player)
    {
        var emptyCells = board.GetEmptyCells();
        
        foreach (var (r, c) in emptyCells)
        {
            // Check if placing the player's piece at (r,c) would result in a win
            if (WouldResultInWin(board, r, c, player))
            {
                return (r, c);
            }
        }
        
        return null;
    }
    
    private bool WouldResultInWin(Board board, int row, int col, Cell player)
    {
        // Check horizontal win
        if (col == 0 && board[row, 1] == player && board[row, 2] == player) return true;
        if (col == 1 && board[row, 0] == player && board[row, 2] == player) return true;
        if (col == 2 && board[row, 0] == player && board[row, 1] == player) return true;
        
        // Check vertical win
        if (row == 0 && board[1, col] == player && board[2, col] == player) return true;
        if (row == 1 && board[0, col] == player && board[2, col] == player) return true;
        if (row == 2 && board[0, col] == player && board[1, col] == player) return true;
        
        // Check diagonal wins
        if (row == col) // Main diagonal (0,0), (1,1), (2,2)
        {
            if (row == 0 && board[1, 1] == player && board[2, 2] == player) return true;
            if (row == 1 && board[0, 0] == player && board[2, 2] == player) return true;
            if (row == 2 && board[0, 0] == player && board[1, 1] == player) return true;
        }
        
        if (row + col == 2) // Anti-diagonal (0,2), (1,1), (2,0)
        {
            if (row == 0 && board[1, 1] == player && board[2, 0] == player) return true;
            if (row == 1 && board[0, 2] == player && board[2, 0] == player) return true;
            if (row == 2 && board[0, 2] == player && board[1, 1] == player) return true;
        }
        
        return false;
    }
    
    private (int r, int c)? FindAdjacentToBotPiece(Board board)
    {
        var emptyCells = board.GetEmptyCells();
        var botPositions = new List<(int r, int c)>();
        
        // Find all positions where the bot has pieces
        for (int r = 0; r < 3; r++)
        {
            for (int c = 0; c < 3; c++)
            {
                if (board[r, c] == Cell.O)
                {
                    botPositions.Add((r, c));
                }
            }
        }
        
        // If no bot pieces exist, return null
        if (!botPositions.Any())
        {
            return null;
        }
        
        // Check each empty cell to see if it's adjacent to any bot piece
        foreach (var (emptyR, emptyC) in emptyCells)
        {
            foreach (var (botR, botC) in botPositions)
            {
                // Check if the empty cell is adjacent (including diagonally)
                if (Math.Abs(emptyR - botR) <= 1 && Math.Abs(emptyC - botC) <= 1)
                {
                    // Make sure it's not the same position
                    if (emptyR != botR || emptyC != botC)
                    {
                        return (emptyR, emptyC);
                    }
                }
            }
        }
        
        return null;
    }
}

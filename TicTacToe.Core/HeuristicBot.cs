namespace TicTacToe.Core;

public class HeuristicBot : IBotStrategy
{
    // PROMPT: Ask Cursor to implement a simple heuristic:
    // 1) If winning move exists, take it.
    // 2) If opponent can win next, block it.
    // 3) Prefer center, then corners, then edges.
    /// <summary>
    /// Chooses a move for the bot based on the current game state.
    /// </summary>
    /// <param name="board">The current board state</param>
    /// <returns>The chosen move</returns>
    public Move ChooseMove(Board board)
    {
        if (board.Phase == GamePhase.Placement)
        {
            return ChoosePlacementMove(board);
        }
        else
        {
            return ChooseMovementMove(board);
        }
    }
    
    /// <summary>
    /// Chooses a placement move during the placement phase.
    /// </summary>
    	private Move ChoosePlacementMove(Board board)
	{
		// If this is the bot's first move and center is taken, pick a random empty cell
		if (board.BotPiecesPlaced == 0 && board[1, 1] != Cell.Empty)
		{
			var empties = board.GetEmptyCells().ToList();
			if (empties.Any())
			{
				var randomCell = empties[Random.Shared.Next(empties.Count)];
				return new Move(randomCell.r + 1, randomCell.c + 1, Cell.O, MoveType.Place);
			}
		}

		// 1. Check if bot can win in one move
        var winningMove = FindWinningMove(board, Cell.O);
        if (winningMove.HasValue)
        {
            return new Move(winningMove.Value.r + 1, winningMove.Value.c + 1, Cell.O, MoveType.Place);
        }
        
        // 2. Check if human can win next move and block it
        var blockingMove = FindWinningMove(board, Cell.X);
        if (blockingMove.HasValue)
        {
            return new Move(blockingMove.Value.r + 1, blockingMove.Value.c + 1, Cell.O, MoveType.Place);
        }
        
        // 3. Prefer center if available
        if (board[1, 1] == Cell.Empty)
        {
            return new Move(2, 2, Cell.O, MoveType.Place);
        }
        
        // 4. Place bot's piece beside its existing pieces if possible
        var adjacentMove = FindAdjacentToBotPiece(board);
        if (adjacentMove.HasValue)
        {
            return new Move(adjacentMove.Value.r + 1, adjacentMove.Value.c + 1, Cell.O, MoveType.Place);
        }
        
        // 5. Prefer corners if available
        var corners = new[] { (0, 0), (0, 2), (2, 0), (2, 2) };
        foreach (var (r, c) in corners)
        {
            if (board[r, c] == Cell.Empty)
            {
                return new Move(r + 1, c + 1, Cell.O, MoveType.Place);
            }
        }
        
        // 6. Take any available edge position
        var edges = new[] { (0, 1), (1, 0), (1, 2), (2, 1) };
        foreach (var (r, c) in edges)
        {
            if (board[r, c] == Cell.Empty)
            {
                return new Move(r + 1, c + 1, Cell.O, MoveType.Place);
            }
        }
        
        // Fallback: take any empty cell
        var emptyCells = board.GetEmptyCells();
        var firstEmpty = emptyCells.First();
        return new Move(firstEmpty.r + 1, firstEmpty.c + 1, Cell.O, MoveType.Place);
    }
    
    /// <summary>
    /// Chooses a movement move during the movement phase.
    /// </summary>
    private Move ChooseMovementMove(Board board)
    {
        // Get all valid moves for the bot once to avoid inconsistencies
        var validMoves = board.GetValidMovementMoves(Cell.O).ToList();
        
        if (!validMoves.Any())
        {
            throw new InvalidOperationException("No valid movement moves available");
        }
        
        // 1. Check if bot can win in one move
        var winningMove = FindWinningMovementMove(board, Cell.O, validMoves);
        if (winningMove != null)
        {
            return winningMove;
        }
        
        // 2. Check if human can win next move and block it
        var blockingMove = FindWinningMovementMove(board, Cell.X, validMoves);
        if (blockingMove != null)
        {
            // Create a new move with the correct player (Cell.O) but same coordinates
            return new Move(blockingMove.Row, blockingMove.Col, Cell.O, MoveType.Move, 
                          blockingMove.FromRow, blockingMove.FromCol);
        }
        
        // 3. Try to improve bot's position
        var improvementMove = FindImprovementMove(board, validMoves);
        if (improvementMove != null)
        {
            return improvementMove;
        }
        
        // 4. Return the first valid move
        return validMoves.First();
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
    
    /// <summary>
    /// Finds a winning movement move for a player.
    /// </summary>
    private Move? FindWinningMovementMove(Board board, Cell player, List<Move> validMoves)
    {
        // Filter moves for the specific player
        var playerMoves = validMoves.Where(m => m.Player == player).ToList();
        
        foreach (var move in playerMoves)
        {
            // Check if this move would result in a win without creating a temporary board
            if (WouldResultInWinAfterMove(board, move, player))
            {
                return move;
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Finds a move that improves the bot's position.
    /// </summary>
    private Move? FindImprovementMove(Board board, List<Move> validMoves)
    {
        // Filter moves for the bot
        var botMoves = validMoves.Where(m => m.Player == Cell.O).ToList();
        
        // Prefer moves that get closer to forming a winning line
        foreach (var move in botMoves)
        {
            // Check if this move gets the bot closer to winning without creating a temporary board
            if (WouldImprovePositionAfterMove(board, move, Cell.O))
            {
                return move;
            }
        }
        
        // If no improvement found, return the first valid move for the bot
        return botMoves.FirstOrDefault();
    }
    
    /// <summary>
    /// Checks if a board position would be an improvement for the given player.
    /// </summary>
    private bool WouldImprovePosition(Board board, Cell player)
    {
        // Simple heuristic: check if pieces are closer to forming a line
        var playerPieces = board.GetPlayerPieces(player).ToList();
        
        if (playerPieces.Count < 2) return false;
        
        // Check if any two pieces are adjacent
        for (int i = 0; i < playerPieces.Count; i++)
        {
            for (int j = i + 1; j < playerPieces.Count; j++)
            {
                var (r1, c1) = playerPieces[i];
                var (r2, c2) = playerPieces[j];
                
                // Check if pieces are adjacent (including diagonally)
                if (Math.Abs(r1 - r2) <= 1 && Math.Abs(c1 - c2) <= 1)
                {
                    return true;
                }
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Checks if a move would result in a win without creating a temporary board.
    /// </summary>
    private bool WouldResultInWinAfterMove(Board board, Move move, Cell player)
    {
        // Simulate the move on the current board state
        int fromRow = move.FromRow!.Value - 1;
        int fromCol = move.FromCol!.Value - 1;
        int toRow = move.Row - 1;
        int toCol = move.Col - 1;
        
        // Check if this move would create a win by checking the resulting board state
        // Check horizontal wins
        for (int row = 0; row < 3; row++)
        {
            if (WouldCreateWinInRow(board, row, player, fromRow, fromCol, toRow, toCol))
                return true;
        }
        
        // Check vertical wins
        for (int col = 0; col < 3; col++)
        {
            if (WouldCreateWinInColumn(board, col, player, fromRow, fromCol, toRow, toCol))
                return true;
        }
        
        // Check diagonal wins
        if (WouldCreateWinInDiagonal(board, player, fromRow, fromCol, toRow, toCol))
            return true;
        
        return false;
    }
    
    /// <summary>
    /// Checks if a move would improve position without creating a temporary board.
    /// </summary>
    private bool WouldImprovePositionAfterMove(Board board, Move move, Cell player)
    {
        // Simulate the move and check if it improves the position
        int fromRow = move.FromRow!.Value - 1;
        int fromCol = move.FromCol!.Value - 1;
        int toRow = move.Row - 1;
        int toCol = move.Col - 1;
        
        // Get the player's pieces after the move (excluding the source, including the destination)
        var playerPieces = board.GetPlayerPieces(player).ToList();
        var sourcePiece = (fromRow, fromCol);
        var destinationPiece = (toRow, toCol);
        
        // Remove source piece and add destination piece
        playerPieces.Remove(sourcePiece);
        playerPieces.Add(destinationPiece);
        
        if (playerPieces.Count < 2) return false;
        
        // Check if any two pieces are adjacent
        for (int i = 0; i < playerPieces.Count; i++)
        {
            for (int j = i + 1; j < playerPieces.Count; j++)
            {
                var (r1, c1) = playerPieces[i];
                var (r2, c2) = playerPieces[j];
                
                // Check if pieces are adjacent (including diagonally)
                if (Math.Abs(r1 - r2) <= 1 && Math.Abs(c1 - c2) <= 1)
                {
                    return true;
                }
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Checks if a move would create a win in a specific row.
    /// </summary>
    private bool WouldCreateWinInRow(Board board, int row, Cell player, int fromRow, int fromCol, int toRow, int toCol)
    {
        int playerCount = 0;
        for (int col = 0; col < 3; col++)
        {
            if (row == fromRow && col == fromCol)
                continue; // Skip the source position
            if (row == toRow && col == toCol)
                playerCount++; // Count the destination position
            else if (board[row, col] == player)
                playerCount++; // Count existing pieces
        }
        return playerCount == 3;
    }
    
    /// <summary>
    /// Checks if a move would create a win in a specific column.
    /// </summary>
    private bool WouldCreateWinInColumn(Board board, int col, Cell player, int fromRow, int fromCol, int toRow, int toCol)
    {
        int playerCount = 0;
        for (int row = 0; row < 3; row++)
        {
            if (row == fromRow && col == fromCol)
                continue; // Skip the source position
            if (row == toRow && col == toCol)
                playerCount++; // Count the destination position
            else if (board[row, col] == player)
                playerCount++; // Count existing pieces
        }
        return playerCount == 3;
    }
    
    /// <summary>
    /// Checks if a move would create a win in either diagonal.
    /// </summary>
    private bool WouldCreateWinInDiagonal(Board board, Cell player, int fromRow, int fromCol, int toRow, int toCol)
    {
        // Check main diagonal (0,0), (1,1), (2,2)
        int mainDiagonalCount = 0;
        for (int i = 0; i < 3; i++)
        {
            if (i == fromRow && i == fromCol)
                continue; // Skip the source position
            if (i == toRow && i == toCol)
                mainDiagonalCount++; // Count the destination position
            else if (board[i, i] == player)
                mainDiagonalCount++; // Count existing pieces
        }
        if (mainDiagonalCount == 3) return true;
        
        // Check anti-diagonal (0,2), (1,1), (2,0)
        int antiDiagonalCount = 0;
        for (int i = 0; i < 3; i++)
        {
            if (i == fromRow && (2 - i) == fromCol)
                continue; // Skip the source position
            if (i == toRow && (2 - i) == toCol)
                antiDiagonalCount++; // Count the destination position
            else if (board[i, 2 - i] == player)
                antiDiagonalCount++; // Count existing pieces
        }
        return antiDiagonalCount == 3;
    }
}

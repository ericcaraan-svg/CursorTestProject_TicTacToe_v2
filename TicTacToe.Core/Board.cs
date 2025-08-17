namespace TicTacToe.Core;

public enum Cell { Empty, X, O }
public enum GameStatus { InProgress, XWins, OWins, Draw }

public record Move(int Row, int Col, Cell Player, MoveType Type = MoveType.Place, int? FromRow = null, int? FromCol = null);

public class Board
{
    private readonly Cell[,] _cells = new Cell[3,3];
    public Cell this[int r, int c] => _cells[r,c];
    public Cell CurrentPlayer { get; private set; } = Cell.X;
    
    /// <summary>
    /// Gets the current game phase.
    /// </summary>
    public GamePhase Phase { get; private set; } = GamePhase.Placement;
    
    /// <summary>
    /// Gets the number of pieces placed by player X (Human).
    /// </summary>
    public int HumanPiecesPlaced { get; private set; } = 0;
    
    /// <summary>
    /// Gets the number of pieces placed by player O (Bot).
    /// </summary>
    public int BotPiecesPlaced { get; private set; } = 0;

    /// <summary>
    /// Applies a move to the board and returns a new board instance.
    /// </summary>
    /// <param name="move">The move to apply</param>
    /// <returns>A new board with the move applied</returns>
    /// <exception cref="ArgumentException">Thrown when the move is invalid</exception>
    public Board Apply(Move move)
    {
        // Convert 1-indexed user input to 0-indexed internal representation
        int internalRow = move.Row - 1;
        int internalCol = move.Col - 1;
        
        // Validate the move coordinates
        if (internalRow < 0 || internalRow >= 3 || internalCol < 0 || internalCol >= 3)
        {
            throw new ArgumentException("Move coordinates must be between 1 and 3");
        }
        
        if (move.Player != CurrentPlayer)
        {
            throw new ArgumentException($"Player {move.Player} cannot move when it's {CurrentPlayer}'s turn");
        }

        // Handle placement phase
        if (Phase == GamePhase.Placement)
        {
            return ApplyPlacementMove(move, internalRow, internalCol);
        }
        
        // Handle movement phase
        if (Phase == GamePhase.Movement)
        {
            return ApplyMovementMove(move, internalRow, internalCol);
        }
        
        throw new InvalidOperationException("Game is in an invalid state");
    }
    
    /// <summary>
    /// Applies a placement move during the placement phase.
    /// </summary>
    private Board ApplyPlacementMove(Move move, int internalRow, int internalCol)
    {
        // Check if target cell is empty
        if (_cells[internalRow, internalCol] != Cell.Empty)
        {
            throw new ArgumentException($"Cell at ({move.Row}, {move.Col}) is already occupied");
        }
        
        // Check if player has pieces left to place
        if (move.Player == Cell.X && HumanPiecesPlaced >= 3)
        {
            throw new ArgumentException("Human has already placed all 3 pieces");
        }
        
        if (move.Player == Cell.O && BotPiecesPlaced >= 3)
        {
            throw new ArgumentException("Bot has already placed all 3 pieces");
        }
        
        // Create new board with the move applied
        var newBoard = new Board();
        
        // Copy existing cells
        for (int r = 0; r < 3; r++)
        {
            for (int c = 0; c < 3; c++)
            {
                if (r == internalRow && c == internalCol)
                {
                    newBoard._cells[r, c] = move.Player;
                }
                else
                {
                    newBoard._cells[r, c] = _cells[r, c];
                }
            }
        }
        
        // Copy game state
        newBoard.HumanPiecesPlaced = HumanPiecesPlaced;
        newBoard.BotPiecesPlaced = BotPiecesPlaced;
        newBoard.Phase = Phase;
        
        // Update piece count
        if (move.Player == Cell.X)
        {
            newBoard.HumanPiecesPlaced = HumanPiecesPlaced + 1;
        }
        else
        {
            newBoard.BotPiecesPlaced = BotPiecesPlaced + 1;
        }
        
        // Check if we should transition to movement phase
        if (newBoard.HumanPiecesPlaced == 3 && newBoard.BotPiecesPlaced == 3)
        {
            newBoard.Phase = GamePhase.Movement;
            // Don't flip the player when transitioning to movement phase
            // The current player should get to make the first movement move
        }
        else
        {
            // Only flip the player if we're still in placement phase
            newBoard.CurrentPlayer = CurrentPlayer == Cell.X ? Cell.O : Cell.X;
        }
        
        return newBoard;
    }
    
    /// <summary>
    /// Applies a movement move during the movement phase.
    /// </summary>
    private Board ApplyMovementMove(Move move, int internalRow, int internalCol)
    {
        // Validate movement move has source coordinates
        if (move.Type != MoveType.Move || !move.FromRow.HasValue || !move.FromCol.HasValue)
        {
            throw new ArgumentException("Movement moves must specify source coordinates");
        }
        
        int fromInternalRow = move.FromRow.Value - 1;
        int fromInternalCol = move.FromCol.Value - 1;
        
        // Check if source cell contains the player's piece
        if (_cells[fromInternalRow, fromInternalCol] != move.Player)
        {
            throw new ArgumentException($"Source cell ({move.FromRow}, {move.FromCol}) does not contain player {move.Player}'s piece");
        }
        
        // Check if target cell is empty
        if (_cells[internalRow, internalCol] != Cell.Empty)
        {
            throw new ArgumentException($"Target cell ({move.Row}, {move.Col}) is already occupied");
        }
        
        // Check if move is to an adjacent cell
        if (!IsAdjacentMove(fromInternalRow, fromInternalCol, internalRow, internalCol))
        {
            throw new ArgumentException($"Move from ({move.FromRow}, {move.FromCol}) to ({move.Row}, {move.Col}) is not to an adjacent cell");
        }
        
        // Create new board with the move applied
        var newBoard = new Board();
        
        // Copy existing cells
        for (int r = 0; r < 3; r++)
        {
            for (int c = 0; c < 3; c++)
            {
                if (r == fromInternalRow && c == fromInternalCol)
                {
                    newBoard._cells[r, c] = Cell.Empty; // Remove piece from source
                }
                else if (r == internalRow && c == internalCol)
                {
                    newBoard._cells[r, c] = move.Player; // Place piece at target
                }
                else
                {
                    newBoard._cells[r, c] = _cells[r, c];
                }
            }
        }
        
        // Copy game state
        newBoard.HumanPiecesPlaced = HumanPiecesPlaced;
        newBoard.BotPiecesPlaced = BotPiecesPlaced;
        newBoard.Phase = Phase;
        
        // Flip the current player
        newBoard.CurrentPlayer = CurrentPlayer == Cell.X ? Cell.O : Cell.X;
        
        return newBoard;
    }
    
    /// <summary>
    /// Checks if a move is to an adjacent cell, respecting the restricted diagonal moves.
    /// </summary>
    private bool IsAdjacentMove(int fromRow, int fromCol, int toRow, int toCol)
    {
        int rowDiff = Math.Abs(toRow - fromRow);
        int colDiff = Math.Abs(toCol - fromCol);
        
        // Must be adjacent (distance 1 in at least one direction)
        if (rowDiff > 1 || colDiff > 1)
        {
            return false;
        }
        
        // Check restricted diagonal moves
        if (rowDiff == 1 && colDiff == 1)
        {
            // Restricted moves: (1,2) ↔ (2,1), (1,2) ↔ (2,3), (2,1) ↔ (3,2), (2,3) ↔ (3,2)
            if ((fromRow == 0 && fromCol == 1 && toRow == 1 && toCol == 0) ||
                (fromRow == 1 && fromCol == 0 && toRow == 0 && toCol == 1) ||
                (fromRow == 0 && fromCol == 1 && toRow == 1 && toCol == 2) ||
                (fromRow == 1 && fromCol == 2 && toRow == 0 && toCol == 1) ||
                (fromRow == 1 && fromCol == 0 && toRow == 2 && toCol == 1) ||
                (fromRow == 2 && fromCol == 1 && toRow == 1 && toCol == 0) ||
                (fromRow == 1 && fromCol == 2 && toRow == 2 && toCol == 1) ||
                (fromRow == 2 && fromCol == 1 && toRow == 1 && toCol == 2))
            {
                return false;
            }
        }
        
        return true;
    }

    // PROMPT: Ask Cursor to implement GetStatus() that checks rows, cols, diags, and draw.
    public GameStatus GetStatus()
    {
        // Check for horizontal wins
        for (int row = 0; row < 3; row++)
        {
            if (_cells[row, 0] != Cell.Empty && 
                _cells[row, 0] == _cells[row, 1] && 
                _cells[row, 1] == _cells[row, 2])
            {
                return _cells[row, 0] == Cell.X ? GameStatus.XWins : GameStatus.OWins;
            }
        }
        
        // Check for vertical wins
        for (int col = 0; col < 3; col++)
        {
            if (_cells[0, col] != Cell.Empty && 
                _cells[0, col] == _cells[1, col] && 
                _cells[1, col] == _cells[2, col])
            {
                return _cells[0, col] == Cell.X ? GameStatus.XWins : GameStatus.OWins;
            }
        }
        
        // Check for diagonal wins (top-left to bottom-right)
        if (_cells[0, 0] != Cell.Empty && 
            _cells[0, 0] == _cells[1, 1] && 
            _cells[1, 1] == _cells[2, 2])
        {
            return _cells[0, 0] == Cell.X ? GameStatus.XWins : GameStatus.OWins;
        }
        
        // Check for diagonal wins (top-right to bottom-left)
        if (_cells[0, 2] != Cell.Empty && 
            _cells[0, 2] == _cells[1, 1] && 
            _cells[1, 1] == _cells[2, 0])
        {
            return _cells[0, 2] == Cell.X ? GameStatus.XWins : GameStatus.OWins;
        }
        
        // In the new game mechanics, we don't have a traditional "draw" scenario
        // since pieces can move around. The game continues until someone wins.
        return GameStatus.InProgress;
    }

    /// <summary>
    /// Gets all empty cells on the board.
    /// </summary>
    /// <returns>Enumerable of empty cell coordinates</returns>
    public IEnumerable<(int r, int c)> GetEmptyCells()
    {
        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                if (_cells[row, col] == Cell.Empty)
                {
                    yield return (row, col);
                }
            }
        }
    }
    
    /// <summary>
    /// Gets all valid movement moves for a player during the movement phase.
    /// </summary>
    /// <param name="player">The player to get moves for</param>
    /// <returns>Enumerable of valid movement moves</returns>
    public IEnumerable<Move> GetValidMovementMoves(Cell player)
    {
        if (Phase != GamePhase.Movement)
        {
            yield break;
        }
        
        // Find all pieces belonging to the player
        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                if (_cells[row, col] == player)
                {
                    // Check all adjacent cells for valid moves
                    for (int targetRow = Math.Max(0, row - 1); targetRow <= Math.Min(2, row + 1); targetRow++)
                    {
                        for (int targetCol = Math.Max(0, col - 1); targetCol <= Math.Min(2, col + 1); targetCol++)
                        {
                            // Skip the current cell
                            if (targetRow == row && targetCol == col)
                                continue;
                                
                            // Check if the target cell is empty and the move is valid
                            if (_cells[targetRow, targetCol] == Cell.Empty && 
                                IsAdjacentMove(row, col, targetRow, targetCol))
                            {
                                yield return new Move(
                                    targetRow + 1, targetCol + 1, player, 
                                    MoveType.Move, row + 1, col + 1);
                            }
                        }
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Gets all pieces belonging to a specific player.
    /// </summary>
    /// <param name="player">The player to get pieces for</param>
    /// <returns>Enumerable of piece coordinates</returns>
    public IEnumerable<(int r, int c)> GetPlayerPieces(Cell player)
    {
        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                if (_cells[row, col] == player)
                {
                    yield return (row, col);
                }
            }
        }
    }
}

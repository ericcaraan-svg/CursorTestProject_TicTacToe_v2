namespace TicTacToe.Core;

public enum Cell { Empty, X, O }
public enum GameStatus { InProgress, XWins, OWins, Draw }

public record Move(int Row, int Col, Cell Player);

public class Board
{
    private readonly Cell[,] _cells = new Cell[3,3];
    public Cell this[int r, int c] => _cells[r,c];
    public Cell CurrentPlayer { get; private set; } = Cell.X;

    // PROMPT: Ask Cursor to implement immutable Apply(Move) that returns a NEW Board with the move applied,
    // throws on invalid move, and flips CurrentPlayer.
    public Board Apply(Move move)
    {
        // Convert 1-indexed user input to 0-indexed internal representation
        int internalRow = move.Row - 1;
        int internalCol = move.Col - 1;
        
        // Validate the move (now using 0-indexed coordinates)
        if (internalRow < 0 || internalRow >= 3 || internalCol < 0 || internalCol >= 3)
        {
            throw new ArgumentException("Move coordinates must be between 1 and 3");
        }
        
        if (_cells[internalRow, internalCol] != Cell.Empty)
        {
            throw new ArgumentException($"Cell at ({move.Row}, {move.Col}) is already occupied");
        }
        
        if (move.Player != CurrentPlayer)
        {
            throw new ArgumentException($"Player {move.Player} cannot move when it's {CurrentPlayer}'s turn");
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
        
        // Flip the current player
        newBoard.CurrentPlayer = CurrentPlayer == Cell.X ? Cell.O : Cell.X;
        
        return newBoard;
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
        
        // Check for draw (all cells filled)
        bool hasEmptyCell = false;
        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                if (_cells[row, col] == Cell.Empty)
                {
                    hasEmptyCell = true;
                    break;
                }
            }
            if (hasEmptyCell) break;
        }
        
        return hasEmptyCell ? GameStatus.InProgress : GameStatus.Draw;
    }

    // PROMPT: Ask Cursor to implement GetEmptyCells() helper returning IEnumerable<(int r,int c)>.
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
}

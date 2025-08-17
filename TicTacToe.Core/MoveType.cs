namespace TicTacToe.Core;

/// <summary>
/// Represents the type of move being made in the game.
/// </summary>
public enum MoveType
{
    /// <summary>
    /// Placing a new piece on an empty cell.
    /// </summary>
    Place,
    
    /// <summary>
    /// Moving an existing piece to an adjacent empty cell.
    /// </summary>
    Move
} 
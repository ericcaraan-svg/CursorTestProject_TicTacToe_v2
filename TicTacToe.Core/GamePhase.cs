namespace TicTacToe.Core;

/// <summary>
/// Represents the current phase of the Tic-Tac-Toe game.
/// </summary>
public enum GamePhase
{
    /// <summary>
    /// Players are placing their 3 pieces on the board.
    /// </summary>
    Placement,
    
    /// <summary>
    /// Players are moving their existing pieces to adjacent positions.
    /// </summary>
    Movement
} 
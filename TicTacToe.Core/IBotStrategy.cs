namespace TicTacToe.Core;

public interface IBotStrategy
{
    // PROMPT: Ask Cursor to implement ChooseMove(Board board) that selects a legal move for the current player.
    Move ChooseMove(Board board);
}

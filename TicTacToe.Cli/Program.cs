using TicTacToe.Core;
using TicTacToe.Cli;

Console.WriteLine("Tic-Tac-Toe Version 2 — Human (X) vs Bot (O)");
Console.WriteLine("Place 3 pieces, then move them to adjacent positions!");
var game = new GameLoop();
await game.Run();

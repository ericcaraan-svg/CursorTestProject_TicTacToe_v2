using TicTacToe.Core;
using TicTacToe.Cli;

Console.WriteLine("Tic-Tac-Toe — Human (X) vs Bot (O)");
var game = new GameLoop();
await game.Run();

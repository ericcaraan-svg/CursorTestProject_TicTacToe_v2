using TicTacToe.Core;

namespace TicTacToe.Cli;

public class GameLoop
{
    private Board _board;
    private readonly IBotStrategy _bot;
    
    public GameLoop()
    {
        _board = new Board();
        _bot = new HeuristicBot();
    }
    
    public async Task Run()
    {
        Render(_board);

        while (true)
        {
            // Human move
            if (!ProcessHumanMove())
            {
                break; // User quit
            }
            
            Render(_board);

            // Check game status after human move
            var status = _board.GetStatus();
            if (status != GameStatus.InProgress)
            {
                Render(_board);
                Console.WriteLine($"Game Over! {GetGameStatusMessage(status)}");
                break;
            }
            
            // Bot's turn
            Console.WriteLine("Bot is thinking...");
            
            // Add a delay after showing "Bot is thinking..." so user can see both moves
            await Task.Delay(1000);
            
            ProcessBotMove();
            
            Render(_board);

            // Check game status after bot move
            status = _board.GetStatus();
            if (status != GameStatus.InProgress)
            {
                Render(_board);
                Console.WriteLine($"Game Over! {GetGameStatusMessage(status)}");
                break;
            }
        }
    }
    
    private bool ProcessHumanMove()
    {
        if (_board.Phase == GamePhase.Placement)
        {
            Console.Write($"Enter row,col (1-3,1-3) to place piece {_board.CurrentPlayer} or 'q' to quit: ");
        }
        else
        {
            Console.Write($"Enter fromRow,fromCol,toRow,toCol to move piece {_board.CurrentPlayer} or 'q' to quit: ");
        }
        var input = Console.ReadLine();
        
        if (string.Equals(input, "q", StringComparison.OrdinalIgnoreCase)) 
        {
            Console.WriteLine("Game ended by user.");
            return false;
        }
        
        // Parse and validate human input
        bool moveApplied = false;
        while (!moveApplied)
        {
            try
            {
                // Check if input is null
                if (input == null)
                {
                    throw new FormatException("Input cannot be null");
                }
                
                // Parse input based on game phase
                var parts = input.Split(',');
                
                if (_board.Phase == GamePhase.Placement)
                {
                    // Placement phase: expect "row,col"
                    if (parts.Length != 2)
                    {
                        throw new FormatException("Please enter coordinates in format: row,col (e.g., 2,2)");
                    }
                    
                    if (!int.TryParse(parts[0].Trim(), out int row) || !int.TryParse(parts[1].Trim(), out int col))
                    {
                        throw new FormatException("Please enter valid numbers for row and column");
                    }
                    
                    // Validate range (1-3)
                    if (row < 1 || row > 3 || col < 1 || col > 3)
                    {
                        throw new ArgumentException("Row and column must be between 1 and 3");
                    }
                    
                    // Create and apply the placement move
                    var move = new Move(row, col, Cell.X, MoveType.Place);
                    _board = _board.Apply(move);
                    moveApplied = true;
                    
                    Console.WriteLine($"Piece placed at: ({row},{col})");
                }
                else
                {
                    // Movement phase: expect "fromRow,fromCol,toRow,toCol"
                    if (parts.Length != 4)
                    {
                        throw new FormatException("Please enter coordinates in format: fromRow,fromCol,toRow,toCol (e.g., 1,1,2,2)");
                    }
                    
                    if (!int.TryParse(parts[0].Trim(), out int fromRow) || 
                        !int.TryParse(parts[1].Trim(), out int fromCol) ||
                        !int.TryParse(parts[2].Trim(), out int toRow) || 
                        !int.TryParse(parts[3].Trim(), out int toCol))
                    {
                        throw new FormatException("Please enter valid numbers for all coordinates");
                    }
                    
                    // Validate range (1-3)
                    if (fromRow < 1 || fromRow > 3 || fromCol < 1 || fromCol > 3 ||
                        toRow < 1 || toRow > 3 || toCol < 1 || toCol > 3)
                    {
                        throw new ArgumentException("All coordinates must be between 1 and 3");
                    }
                    
                    // Create and apply the movement move
                    var move = new Move(toRow, toCol, Cell.X, MoveType.Move, fromRow, fromCol);
                    _board = _board.Apply(move);
                    moveApplied = true;
                    
                    Console.WriteLine($"Piece moved from ({fromRow},{fromCol}) to ({toRow},{toCol})");
                }
            }
            catch (Exception ex)
            {
                Render(_board);
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Error: {ex.Message}");
                Console.ResetColor();
                if (_board.Phase == GamePhase.Placement)
                {
                    Console.Write("Please try again (row,col format): ");
                }
                else
                {
                    Console.Write("Please try again (fromRow,fromCol,toRow,toCol format): ");
                }
                input = Console.ReadLine();
                
                if (input == null || string.Equals(input, "q", StringComparison.OrdinalIgnoreCase)) 
                {
                    Console.WriteLine("Game ended by user.");
                    return false;
                }
            }
        }
        
        return true;
    }
    
    private void ProcessBotMove()
    {
        var botMove = _bot.ChooseMove(_board);
        _board = _board.Apply(botMove);
        
        if (botMove.Type == MoveType.Place)
        {
            Console.WriteLine($"Bot placed piece at: ({botMove.Row},{botMove.Col})");
        }
        else
        {
            Console.WriteLine($"Bot moved piece from ({botMove.FromRow},{botMove.FromCol}) to ({botMove.Row},{botMove.Col})");
        }
    }
    
    private static void Render(Board b)
    {
        Console.Clear();
        Console.WriteLine("\n     1   2   3");
        Console.WriteLine("   ┌───┬───┬───┐");
        
        for (int row = 0; row < 3; row++)
        {
            Console.Write($" {row + 1} │");
            for (int col = 0; col < 3; col++)
            {
                var cell = b[row, col];
                var symbol = cell switch
                {
                    Cell.X => " X ",
                    Cell.O => " O ",
                    Cell.Empty => "   ",
                    _ => " ? "
                };
                
                // Color code only the X and O symbols
                switch (cell)
                {
                    case Cell.X:
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.Write(symbol);
                        Console.ResetColor();
                        break;
                    case Cell.O:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write(symbol);
                        Console.ResetColor();
                        break;
                    default:
                        Console.Write(symbol);
                        break;
                }
                
                if (col < 2) Console.Write("│");
            }
            Console.WriteLine("│");
            
            if (row < 2)
            {
                Console.WriteLine("   ├───┼───┼───┤");
            }
        }
        
        Console.WriteLine("   └───┴───┴───┘");
        Console.WriteLine($"Current Player: {b.CurrentPlayer}");
        Console.WriteLine($"Game Phase: {b.Phase}");
        Console.WriteLine($"Human pieces: {b.HumanPiecesPlaced}/3");
        Console.WriteLine($"Bot pieces: {b.BotPiecesPlaced}/3");
        Console.WriteLine();
    }
    
    private static string GetGameStatusMessage(GameStatus status)
    {
        return status switch
        {
            GameStatus.XWins => "Human wins!",
            GameStatus.OWins => "Bot wins!",
            GameStatus.Draw => "It's a draw!",
            GameStatus.InProgress => "Game in progress",
            _ => status.ToString()
        };
    }
    

} 
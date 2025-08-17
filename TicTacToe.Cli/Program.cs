using TicTacToe.Core;

Console.WriteLine("Tic-Tac-Toe — Human (X) vs Bot (O)");
var board = new Board();
IBotStrategy bot = new HeuristicBot();

while (true)
{
    // PROMPT: Ask Cursor to implement Render(board) that draws the 3x3 grid with coordinates.
    Render(board);

    // Human move
    Console.Write("Enter row,col (1-3,1-3) or 'q' to quit: ");
    var input = Console.ReadLine();
    if (string.Equals(input, "q", StringComparison.OrdinalIgnoreCase)) break;

    // PROMPT: Ask Cursor to parse input, validate, and apply move; handle errors gracefully.
    // After human move, check status.
    // If game continues, invoke bot. Then check status again and loop.
    
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
            
            // Parse input (expecting format: "row,col")
            var parts = input.Split(',');
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
            
            // Create and apply the move
            var move = new Move(row, col, Cell.X);
            board = board.Apply(move);
            moveApplied = true;
            
            Console.WriteLine($"Move applied: ({row},{col})");
        }
        catch (Exception ex)
        {
            Render(board);
            Console.WriteLine($"Error: {ex.Message}");            
            Console.Write("Please try again (row,col format): ");
            input = Console.ReadLine();
            
            if (input == null || string.Equals(input, "q", StringComparison.OrdinalIgnoreCase)) 
            {
                Console.WriteLine("Game ended by user.");
                return;
            }
        }
    }
    
    // Check game status after human move
    var status = board.GetStatus();
    if (status != GameStatus.InProgress)
    {
        Render(board);
        Console.WriteLine($"Game Over! {status}");
        break;
    }
    
    // Bot's turn
    Console.WriteLine("Bot is thinking...");
    var botMove = bot.ChooseMove(board);
    board = board.Apply(botMove);
    Console.WriteLine($"Bot moved: ({botMove.Row},{botMove.Col})");
    
    // Check game status after bot move
    status = board.GetStatus();
    if (status != GameStatus.InProgress)
    {
        Render(board);
        Console.WriteLine($"Game Over! {status}");
        break;
    }
}

// PROMPT: Ask Cursor to extract GameLoop into a class and add undo/redo as stretch goals.

static void Render(Board b)
{
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
            
            Console.Write(symbol);
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
    Console.WriteLine();
}

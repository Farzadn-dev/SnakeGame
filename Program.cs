

using SnakeGame;

byte width = 40;
byte height = 35;
Console.WindowWidth = width + 2;
Console.WindowHeight = height;
Console.CursorVisible = false;

GameManagment.Guide();
Console.WriteLine("Press Any Key To Start...");
Console.ReadKey();
while (true)
{
    Console.Clear();
    var game = new GameManagment(width, height, 100, 3);
    game.Play();

    Thread.Sleep(3000);
    Console.Clear();
    Console.WriteLine("Press Any Key To Play Again...");

    Console.ReadKey();
}

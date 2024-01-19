using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SnakeGame
{
    public class GameManagment
    {
        private readonly byte WindowX;
        private readonly byte WindowY;
        private int RenderSpeed;
        private int PrintSpeed;

        private List<PlayerObject> PlayerObjs;
        private PlayerObject LastPartOfPlayer;
        private List<GameObject> BallObjs;

        private readonly char BallChar = '@';
        private readonly char PlayerChar = 'O';
        private readonly char PlayerToRightHeadChar = ')';
        private readonly char PlayerToLeftHeadChar = '(';
        private readonly char PlayerUpwardHeadChar = '^';
        private readonly char PlayerDownwardHeadChar = 'v';


        private readonly byte PlayerSpeed = 2;
        private MoveMode PlayerMoveMode;

        private byte Score = 0;
        private bool IsGameOverDisable = false;
        private bool IsGamePause = false;
        private bool IsAutoPlay = false;

        CancellationTokenSource printCancel = new CancellationTokenSource();
        CancellationTokenSource getMove = new CancellationTokenSource();

        //-------------------------------------------------------
        public GameManagment(byte WindowX, byte WindowY, int GameSpeed, byte PlayerPartNumber)
        {
            this.WindowX = WindowX;
            this.WindowY = WindowY;
            this.RenderSpeed = GameSpeed;
            this.PrintSpeed = 1;

            //Create Objects
            PlayerObjs = CreatePlayer(PlayerPartNumber);

            BallObjs = new List<GameObject>();
            CreateNewBall(ref BallObjs);

            PlayerMoveMode = MoveMode.Left;
        }

        public GameManagment() : this(40, 35, 50, 10)
        {

        }

        public GameManagment(byte WindowX, byte WindowY, byte PlayerPartNumber, byte playerSpeed, byte countOfBonusBalls, byte countOfBonusPartOfPlayer, int GameSpeed, char ball, char wall, char playerObj) : this(WindowX, WindowY, GameSpeed, PlayerPartNumber: PlayerPartNumber)
        {
            BallChar = ball;
            PlayerChar = playerObj;
            PlayerSpeed = playerSpeed;
        }

        //-------------------------------------------------------
        private List<PlayerObject> CreatePlayer(byte partNumber)
        {
            var player = new List<PlayerObject>();
            for (byte i = 0; i < partNumber; i++)
            {
                var temp = new PlayerObject()
                {
                    Y = (byte)(WindowY / 2),
                    X = (byte)((WindowX / 2) + i)
                };
                player.Add(temp);
            }

            return player.ToList();
        }
        private void AddPartToPlayer()
        {
            var Lastpart = PlayerObjs.Last();
            PlayerObjs.Add(new PlayerObject(Lastpart));
        }
        private GameObject CreateNewBall()
        {
            List<GameObject> objects = new List<GameObject>();
            for (int i = 2; i < WindowX; i++)
            {
                for (int j = 2; j < WindowY; j++)
                {
                    objects.Add(new GameObject( (byte)i, (byte)j ));
                }
            }
            foreach (var item in PlayerObjs)
            {
                objects.Remove(item);
            }
            Random random = new Random();
            return objects[random.Next(objects.Count)];
        }
        private void CreateNewBall(ref List<GameObject> balls)
        {
            balls.Add(CreateNewBall());
        }

        //-------------------------------------------------------
        public void Play()
        {
            Task.Run(PrintBord);
            Task.Run(MovePlayer);
            while ((IsGameOver() == false || IsGameOverDisable) && IsPlayerWin() == false)
            {
                if (IsGamePause == false)
                {
                    RenderGameBord();
                    Thread.Sleep(RenderSpeed);
                }

            }
            printCancel.Cancel();
            getMove.Cancel();
        }
        private void RenderGameBord()
        {
            if (IsAutoPlay)
                AutoMove();

            CaculateScore();
            PlayerObjs = UpdatePlayerPosition(PlayerObjs);
        }

        private void PrintBord()
        {
            while (!printCancel.Token.IsCancellationRequested)
            {
                PrintDesignBord();
                Thread.Sleep(PrintSpeed);
                if (LastPartOfPlayer is not null)
                    DeleteLastPart(LastPartOfPlayer);
                PrintHead();
                for (int i = 1; i < PlayerObjs.Count; i++)
                {
                    var player = PlayerObjs[i];
                    Console.SetCursorPosition(player.X, WindowY - player.Y);
                    Console.Write(PlayerChar);
                }
                LastPartOfPlayer = PlayerObjs.Last();

                for (int i = 0; i < BallObjs.Count; i++)
                {
                    var ball = BallObjs[i];
                    Console.SetCursorPosition(ball.X, WindowY - ball.Y);
                    Console.Write(BallChar);
                }
            }
        }
        private void PrintDesignBord()
        {
            for (int i = 0; i < WindowY; i++)
            {
                Console.SetCursorPosition(0, i);
                Console.Write('|');
                Console.SetCursorPosition(WindowX + 1, i);
                Console.Write('|');
            }
            Console.SetCursorPosition(1, 0);
            Console.Write($"Score:{Score}");
        }
        private void PrintHead()
        {
            var head = PlayerObjs[0];
            Console.SetCursorPosition(head.X, WindowY - head.Y);
            Console.Write(GetPlayerHead());
        }
        private void DeleteLastPart(PlayerObject Last)
        {
            Console.SetCursorPosition(Last.X, WindowY - Last.Y);
            Console.Write(' ');
        }

        private char GetPlayerHead()
        {
            switch (PlayerMoveMode)
            {
                case MoveMode.Upward:
                    return PlayerUpwardHeadChar;
                case MoveMode.Downward:
                    return PlayerDownwardHeadChar;
                case MoveMode.Left:
                    return PlayerToLeftHeadChar;
                case MoveMode.Right:
                    return PlayerToRightHeadChar;
                default:
                    return PlayerToRightHeadChar;
            }
        }
        //--------------------------------------------------------
        private void MovePlayer()
        {
            while (!getMove.Token.IsCancellationRequested)
            {
                var k = Console.ReadKey(true).Key;
                if (k == ConsoleKey.D || k == ConsoleKey.RightArrow)
                {
                    if (CanSnakeGo(MoveMode.Right))
                        PlayerMoveMode = MoveMode.Right;

                    continue;
                }
                else if (k == ConsoleKey.A || k == ConsoleKey.LeftArrow)
                {
                    if (CanSnakeGo(MoveMode.Left))
                        PlayerMoveMode = MoveMode.Left;

                    continue;
                }
                else if (k == ConsoleKey.W || k == ConsoleKey.UpArrow)
                {
                    if (CanSnakeGo(MoveMode.Upward))
                        PlayerMoveMode = MoveMode.Upward;

                    continue;
                }
                else if (k == ConsoleKey.S || k == ConsoleKey.DownArrow)
                {
                    if (CanSnakeGo(MoveMode.Downward))
                        PlayerMoveMode = MoveMode.Downward;

                    continue;
                }

                //cheat code
                else if (k == ConsoleKey.F12 || k == ConsoleKey.P)
                    IsGamePause = !IsGamePause;

                else if (k == ConsoleKey.Insert)
                    CreateNewBall(ref BallObjs);

                else if (k == ConsoleKey.End)
                    IsGameOverDisable = !IsGameOverDisable;

                else if (k == ConsoleKey.Home)
                {
                    RenderSpeed = 25;
                    PrintSpeed = 0;
                    IsGameOverDisable = !IsGameOverDisable;
                    IsAutoPlay = !IsAutoPlay;
                    Task.Run(() =>
                    {
                        while (!printCancel.Token.IsCancellationRequested)
                        {
                            Thread.Sleep(1500);
                            Console.Clear();
                        }
                    });
                }
                else if (k == ConsoleKey.C)
                    Console.Clear();
            }
        }

        private bool CanSnakeGo(MoveMode side)
        {
            switch (side)
            {
                case MoveMode.Left:
                    return PlayerObjs[1].X != (PlayerObjs[0].X - 1);

                case MoveMode.Right:
                    return PlayerObjs[1].X != (PlayerObjs[0].X + 1);

                case MoveMode.Upward:
                    return PlayerObjs[1].Y != (PlayerObjs[0].Y + 1);

                case MoveMode.Downward:
                    return PlayerObjs[1].Y != (PlayerObjs[0].Y - 1);

                default:
                    return false;
            }
        }

        //--------------------------------------------------------
        //if ball was under player
        private bool IsGameOver()
        {
            var head = PlayerObjs.First();
            switch (PlayerMoveMode)
            {
                case MoveMode.Upward:
                    return PlayerObjs.Exists(part => (part.X == head.X) && (part.Y == head.Y + 1));

                case MoveMode.Downward:
                    return PlayerObjs.Exists(part => (part.X == head.X) && (part.Y == head.Y - 1));

                case MoveMode.Left:
                    return PlayerObjs.Exists(part => (part.X == head.X - 1) && (part.Y == head.Y));

                case MoveMode.Right:
                    return PlayerObjs.Exists(part => (part.X == head.X + 1) && (part.Y == head.Y));

                default:
                    return false;
            }
        }
        private bool IsPlayerWin()
        {
            return Score == (WindowX - 1) * (WindowY - 1);
        }

        private void CaculateScore()
        {
            if (DeleteCollocatedBall(PlayerObjs.First(), BallObjs))
            {
                Score++;
                CreateNewBall(ref BallObjs);
                AddPartToPlayer();
            }
        }

        //--------------------------------------------------------

        private List<PlayerObject> UpdatePlayerPosition(List<PlayerObject> playerObjects)
        {
            for (int i = playerObjects.Count - 1; i > 0; i--)
                playerObjects[i] = new PlayerObject(playerObjects[i - 1]);

            SetHeadPosition(playerObjects.First(), PlayerMoveMode);

            return playerObjects;
        }
        private void SetHeadPosition(PlayerObject Head, MoveMode MoveMode)
        {
            switch (PlayerMoveMode)
            {
                case MoveMode.Right:
                    {
                        if (Head.X + 1 == WindowX)
                        {
                            Head.X = 1;
                            break;
                        }
                        Head.X++;
                        break;
                    }
                case MoveMode.Left:
                    {
                        if (Head.X - 1 == 0)
                        {
                            Head.X = (byte)(WindowX - 1);
                            break;
                        }
                        Head.X--;
                        break;
                    }
                case MoveMode.Upward:
                    {
                        if (Head.Y + 1 == WindowY)
                        {
                            Head.Y = 1;
                            break;
                        }
                        Head.Y++;
                        break;
                    }
                case MoveMode.Downward:
                    {
                        if (Head.Y - 2 == 0)
                        {
                            Head.Y = (byte)(WindowY - 2);
                            break;
                        }
                        Head.Y--;
                        break;
                    }
            }
        }
        //--------------------------------------------------------

        private void AutoMove()
        {
            var ball = BallObjs.First();
            MovePlayerTo(ball.X, ball.Y);
        }

        private void MovePlayerTo(byte x, byte y)
        {
            var head = PlayerObjs.First();
            if (head.X != x || head.Y != y)
            {
                AutoSetMoveMode(x, y);
            }
        }
        private void AutoSetMoveMode(byte x, byte y)
        {
            var head = PlayerObjs.First();
            switch (PlayerMoveMode)
            {
                case MoveMode.Upward:
                    if (head.Y >= y)
                    {
                        MoveMode ChangedMove = head.X < x ? MoveMode.Right : MoveMode.Left;
                        PlayerMoveMode = CanAutoPlayerGo(
                            ChangedMove) ? ChangedMove : PlayerMoveMode;
                    }

                    break;

                case MoveMode.Downward:
                    if (head.Y <= y)
                    {
                        MoveMode ChangedMove = head.X < x ? MoveMode.Right : MoveMode.Left;
                        PlayerMoveMode = CanAutoPlayerGo(ChangedMove) ? ChangedMove : PlayerMoveMode;
                    }
                    break;

                case MoveMode.Left:
                    if (head.X <= x)
                    {
                        MoveMode ChangedMove = head.Y < y ? MoveMode.Upward : MoveMode.Downward;
                        PlayerMoveMode = CanAutoPlayerGo(ChangedMove) ? ChangedMove : PlayerMoveMode;
                    }

                    break;

                case MoveMode.Right:
                    if (head.X >= x)
                    {
                        MoveMode ChangedMove = head.Y < y ? MoveMode.Upward : MoveMode.Downward;
                        PlayerMoveMode = CanAutoPlayerGo(ChangedMove) ? ChangedMove : PlayerMoveMode;
                    }
                    break;

                default:
                    break;
            }
            if (!CanAutoPlayerGo(PlayerMoveMode))
            {
                if (PlayerMoveMode is MoveMode.Upward || PlayerMoveMode is MoveMode.Downward)
                {
                    MoveMode ChangedMove = head.X < x ? MoveMode.Right : MoveMode.Left;
                    if (CanAutoPlayerGo(ChangedMove))
                        PlayerMoveMode = ChangedMove;
                    else
                        PlayerMoveMode = ChangedMove == MoveMode.Right ? MoveMode.Left : MoveMode.Right;
                    return;
                }
                if (PlayerMoveMode is MoveMode.Right || PlayerMoveMode is MoveMode.Left)
                {
                    MoveMode ChangedMove = head.Y < y ? MoveMode.Upward : MoveMode.Downward;
                    if (CanAutoPlayerGo(ChangedMove))
                        PlayerMoveMode = ChangedMove;
                    else
                        PlayerMoveMode = ChangedMove == MoveMode.Upward ? MoveMode.Downward : MoveMode.Upward;
                    return;
                }
            }
        }
        private bool CanAutoPlayerGo(MoveMode mode)
        {
            var head = PlayerObjs.First();
            switch (mode)
            {
                case MoveMode.Upward:
                    return !PlayerObjs.Exists(part => (part.X == head.X) && (part.Y == head.Y + 1));

                case MoveMode.Downward:
                    return !PlayerObjs.Exists(part => (part.X == head.X) && (part.Y == head.Y - 1));

                case MoveMode.Left:
                    return !PlayerObjs.Exists(part => (part.X == head.X - 1) && (part.Y == head.Y));

                case MoveMode.Right:
                    return !PlayerObjs.Exists(part => (part.X == head.X + 1) && (part.Y == head.Y));

                default:
                    return false;
            }
        }
        //--------------------------------------------------------

        private bool DeleteCollocatedBall(PlayerObject playerHead, List<GameObject> balls)
        {
            switch (PlayerMoveMode)
            {
                case MoveMode.Upward:
                    {
                        var ball = balls.FirstOrDefault(ball => ball.X == playerHead.X & ball.Y == playerHead.Y + 1);
                        if (ball != null)
                        {
                            balls.Remove(ball);
                            return true;
                        }
                        return false;
                    }

                case MoveMode.Downward:
                    {
                        var ball = balls.FirstOrDefault(ball => ball.X == playerHead.X & ball.Y == playerHead.Y - 1);
                        if (ball != null)
                        {
                            balls.Remove(ball);
                            return true;
                        }
                        return false;
                    }


                case MoveMode.Right:
                    {
                        var ball = balls.FirstOrDefault(ball => ball.X == playerHead.X + 1 & ball.Y == playerHead.Y);
                        if (ball != null)
                        {
                            balls.Remove(ball);
                            return true;
                        }
                        return false;
                    }
                case MoveMode.Left:
                    {
                        var ball = balls.FirstOrDefault(ball => ball.X == playerHead.X - 1 & ball.Y == playerHead.Y);
                        if (ball != null)
                        {
                            balls.Remove(ball);
                            return true;
                        }
                        return false;
                    }

                default:
                    return false;
            }
        }

        //---------------------------------------------------------


        public static void Guide()
        {
            Console.WriteLine("Use [d,a,w,s or arrow keys] to control \nsnake");
            Console.WriteLine("Use [p or F12] for pause game");
            Console.WriteLine("Use [Home key] to auto play");
            Console.WriteLine("Use [Insert key] to add food");
            Console.WriteLine("Use [End key] to make game overless");
        }
    }

}

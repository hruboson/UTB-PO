using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using static System.Formats.Asn1.AsnWriter;

///█ ■
///

interface Object
{
    public void draw();
    public bool act();
}

namespace SolidSnakeCode
{
    enum DIRECTION
    {
        LEFT,
        RIGHT,
        UP,
        DOWN
    }

    class Window
    {
        public int height { get; set; }
        public int width { get; set; }

        public Window(int height = 16, int width = 36)
        {
            this.height = height;
            this.width = width;

            Console.WindowHeight = height;
            Console.WindowWidth = width;
        }

    }

    class Position
    {
        public int x { get; set; }
        public int y { get; set; }

        public Position(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public static bool operator ==(Position pos1, Position pos2)
        {
            return (pos1.x == pos2.x && pos1.y == pos2.y);
        }

        public static bool operator !=(Position pos1, Position pos2)
        {
            return !(pos1.x == pos2.x && pos1.y == pos2.y);
        }
    }

    class Snake : Object
    {
        private Window window;

		private int length = 5; // initial snake length
        private bool gameOver = false;
        private bool buttonPressed = false;

        private DIRECTION movement = DIRECTION.UP; // initial snake movement

        private Position head;
        private List<Position> body;

        private Berry currentBerry;

	    private DateTime lastMoveTime = DateTime.Now;

        public Snake(int x, int y, Window window)
        {
            this.window = window;
            this.head = new Position(x, y);

            this.body = new List<Position>();

            currentBerry = new Berry(window);
        }

        public void draw()
        {
			// Draw body
			Console.ForegroundColor = ConsoleColor.Green;
			foreach (Position p in body)
			{
				Console.SetCursorPosition(p.x, p.y);
				Console.Write("■");
			}

			// Draw head
			Console.ForegroundColor = ConsoleColor.Red;
			Console.SetCursorPosition(head.x, head.y);
			Console.Write("■");

			currentBerry.draw();
        }

		
		public bool act()
		{
			if ((DateTime.Now - lastMoveTime).TotalMilliseconds < 500)
			{
				HandleInput();
				return true;
			}

			lastMoveTime = DateTime.Now;
			buttonPressed = false;

			HandleInput();

			// Compute new head position
			Position newHead = new Position(head.x, head.y);

			switch (movement)
			{
				case DIRECTION.UP: newHead.y--; break;
				case DIRECTION.DOWN: newHead.y++; break;
				case DIRECTION.LEFT: newHead.x--; break;
				case DIRECTION.RIGHT: newHead.x++; break;
			}

			// Wall collision
			if (newHead.x <= 0 || newHead.x >= window.width - 1 ||
				newHead.y <= 0 || newHead.y >= window.height - 1)
			{
				return false;
			}

			// Self collision
			foreach (Position p in body)
			{
				if (p == newHead) return false;
			}

			// Move body
			body.Add(new Position(head.x, head.y));

			if (newHead == currentBerry.pos)
			{
				length++;                     // grow
				currentBerry.moveToRandomPos();
			}

			if (body.Count >= length)
			{
				body.RemoveAt(0);
			}

			// Apply movement
			head = newHead;

			return true;
		}


		private void HandleInput()
		{
			if (!Console.KeyAvailable) return;

			ConsoleKeyInfo key = Console.ReadKey(true);

			if (key.Key == ConsoleKey.UpArrow && movement != DIRECTION.DOWN && !buttonPressed)
			{
				movement = DIRECTION.UP;
				buttonPressed = true;
			}
			if (key.Key == ConsoleKey.DownArrow && movement != DIRECTION.UP && !buttonPressed)
			{
				movement = DIRECTION.DOWN;
				buttonPressed = true;
			}
			if (key.Key == ConsoleKey.LeftArrow && movement != DIRECTION.RIGHT && !buttonPressed)
			{
				movement = DIRECTION.LEFT;
				buttonPressed = true;
			}
			if (key.Key == ConsoleKey.RightArrow && movement != DIRECTION.LEFT && !buttonPressed)
			{
				movement = DIRECTION.RIGHT;
				buttonPressed = true;
			}
		}
    }

    class Wall : Object
    {
        private Window window;

        public Wall(Window window)
        {
            this.window = window;
        }

        public void draw()
        {
            //TODO refactor this
            Console.ForegroundColor = ConsoleColor.Green;

            // draw edges
            for (int x = 0; x < this.window.width; x++)
            {
                for (int y = 0; y < this.window.height; y++)
                {
                    if (x == 0 || y == 0 || x == this.window.width - 1 || y == this.window.height - 1)
                    {
                        Console.SetCursorPosition(x, y);
                        Console.Write("■");
                    }
                }
            }
        }

        public bool act() { return true; }
    }

    class Berry : Object
    {
        public Position pos;
        private Random randomGenerator = new Random();
        private int maxWidth, maxHeight;

        public void moveToRandomPos()
        {
            pos = new Position(randomGenerator.Next(0, maxWidth - 1), randomGenerator.Next(0, maxHeight - 1));
        }

        public Berry(Window window)
        {
            this.maxWidth = window.width;
            this.maxHeight = window.height;
            pos = new Position(randomGenerator.Next(0, window.width - 1), randomGenerator.Next(0, window.height - 1));
        }

        public void draw()
        {
            Console.SetCursorPosition(this.pos.x, this.pos.y);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("■");
        }

        public bool act()
        {
            return true;
        }
    }

    class Pixel
    {
        public int xpos { get; set; }
        public int ypos { get; set; }
        public ConsoleColor color { get; set; }
    }

    class Engine
    {
        private Window window = new Window();
        private Snake player;
        private Wall wall;

        private static List<Object> objects = new List<Object>();

        private static int FPS = 10;

        public Engine()
        {
            wall = new Wall(window);
            objects.Add(wall);

            player = new Snake(window.width / 2, window.height / 2, window);
            objects.Add(player);
        }

        public void loop()
        {
            while (true) {
	            DateTime startTime = DateTime.Now;

                Console.Clear();

                foreach (var obj in objects)
                {
                    if (!obj.act()) { 
						Console.Clear();

						Console.SetCursorPosition(window.width / 5, window.height / 2);
						Console.WriteLine("Game Over!");
						Console.ReadKey();
						return; 
					}
                    obj.draw();
                }

				double frameTime = (DateTime.Now - startTime).TotalMilliseconds;
				int delay = (int)(1000 / FPS - frameTime);

				if (delay > 0) Thread.Sleep(delay);
            }
        }

        public void run()
        {
            int screenwidth = window.width; /////
            int screenheight = window.height; /////
            Random randomnummer = new Random(); //////
            int score = 5; //////
            bool gameover = false; /////
            Pixel hoofd = new Pixel(); /////
            hoofd.xpos = screenwidth / 2; //////
            hoofd.ypos = screenheight / 2;  /////
            hoofd.color = ConsoleColor.Red;
            string movement = "RIGHT";  /////
            List<int> xposlijf = new List<int>(); /// list of all xs of rendered pixels
            List<int> yposlijf = new List<int>(); /// list of all ys of rendered pixels
            int berryx = randomnummer.Next(0, screenwidth); //// berry pos
            int berryy = randomnummer.Next(0, screenheight);    //// berry pos
            DateTime tijd = DateTime.Now;   ////
            DateTime tijd2 = DateTime.Now;  ////
            bool buttonpressed = false;
            while (true)
            {
                Console.Clear(); ////
                if (hoofd.xpos == screenwidth - 1 || hoofd.xpos == 0 || hoofd.ypos == screenheight - 1 || hoofd.ypos == 0)
                {
                    gameover = true;
                } //////
                for (int i = 0; i < screenwidth; i++) /// wall drawing???
                {
                    Console.SetCursorPosition(i, 0);
                    Console.Write("■");
                }
                for (int i = 0; i < screenwidth; i++)
                {
                    Console.SetCursorPosition(i, screenheight - 1); /// wall drawing???
                    Console.Write("■");
                }
                for (int i = 0; i < screenheight; i++) /// wall drawing???
                {
                    Console.SetCursorPosition(0, i);
                    Console.Write("■");
                }
                for (int i = 0; i < screenheight; i++) /// wall drawing???
                {
                    Console.SetCursorPosition(screenwidth - 1, i);
                    Console.Write("■");
                }
                Console.ForegroundColor = ConsoleColor.Green;
                if (berryx == hoofd.xpos && berryy == hoofd.ypos)
                {
                    score++;
                    berryx = randomnummer.Next(1, screenwidth - 2);
                    berryy = randomnummer.Next(1, screenheight - 2);
                }/////////

                for (int i = 0; i < xposlijf.Count(); i++)
                {
                    Console.SetCursorPosition(xposlijf[i], yposlijf[i]);
                    Console.Write("■");
                    if (xposlijf[i] == hoofd.xpos && yposlijf[i] == hoofd.ypos)
                    {
                        gameover = true;
                    }
                }/////
                if (gameover == true)
                {
                    break;
                }/////
                Console.SetCursorPosition(hoofd.xpos, hoofd.ypos);
                Console.ForegroundColor = hoofd.color;
                Console.Write("■");
                Console.SetCursorPosition(berryx, berryy);
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("■");
                tijd = DateTime.Now;
                buttonpressed = false;
                while (true)
                {
                    tijd2 = DateTime.Now;
                    if (tijd2.Subtract(tijd).TotalMilliseconds > 500) { break; }
                    if (Console.KeyAvailable)
                    {
                        ConsoleKeyInfo toets = Console.ReadKey(true);
                        //Console.WriteLine(toets.Key.ToString());
                        if (toets.Key.Equals(ConsoleKey.UpArrow) && movement != "DOWN" && buttonpressed == false)
                        {
                            movement = "UP";
                            buttonpressed = true;
                        }
                        if (toets.Key.Equals(ConsoleKey.DownArrow) && movement != "UP" && buttonpressed == false)
                        {
                            movement = "DOWN";
                            buttonpressed = true;
                        }
                        if (toets.Key.Equals(ConsoleKey.LeftArrow) && movement != "RIGHT" && buttonpressed == false)
                        {
                            movement = "LEFT";
                            buttonpressed = true;
                        }
                        if (toets.Key.Equals(ConsoleKey.RightArrow) && movement != "LEFT" && buttonpressed == false)
                        {
                            movement = "RIGHT";
                            buttonpressed = true;
                        }
                    }
                }////////
                xposlijf.Add(hoofd.xpos);////
                yposlijf.Add(hoofd.ypos);////
                switch (movement)
                {
                    case "UP":
                        hoofd.ypos--;
                        break;
                    case "DOWN":
                        hoofd.ypos++;
                        break;
                    case "LEFT":
                        hoofd.xpos--;
                        break;
                    case "RIGHT":
                        hoofd.xpos++;
                        break;
                }////
                if (xposlijf.Count() > score)
                {
                    xposlijf.RemoveAt(0);
                    yposlijf.RemoveAt(0);
                }////
            }
            Console.SetCursorPosition(screenwidth / 5, screenheight / 2);
            Console.WriteLine("Game over, Score: " + score);
            Console.SetCursorPosition(screenwidth / 5, screenheight / 2 + 1);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Engine e = new Engine();
            e.loop();
        }
    }
}
//¦

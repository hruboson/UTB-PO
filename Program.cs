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

        int length = 5; // initial snake length
        bool gameOver = false;
        bool buttonpressed = false;

        DIRECTION movement = DIRECTION.UP; // initial snake movement

        Position head;
        List<Position> body;

        Berry currentBerry;

        public Snake(int x, int y, Window window)
        {
            this.window = window;
            this.head = new Position(x, y);

            this.body = new List<Position>();
            for (int i = 0; i < length; i++)
            {
                Position newPosition = new Position(this.head.x, this.head.y - i);
                body.Append(newPosition);
            }

            currentBerry = new Berry(window);
        }

        public void draw()
        {

        }

        public bool act()
        {
            // check wall collision
            if (this.head.x == window.width - 1 || this.head.x == 0 || this.head.y == window.height - 1 || this.head.y == 0)
            {
                this.gameOver = true;
            }

            // berry eaten
            if (currentBerry.pos.x == this.head.x && currentBerry.pos.y == this.head.y)
            {
                this.length++;
                currentBerry.moveToRandomPos();
            }

            // check body-head collision
            foreach (Position b in body)
            {
                if (b == head)
                {
                    this.gameOver = true;
                }
            }

            // check player input
            if (Console.KeyAvailable)
            {
                //! if (time.Subtract(timeDiff).TotalMilliseconds > 500) { return true; }
                ConsoleKeyInfo toets = Console.ReadKey(true);
                //Console.WriteLine(toets.Key.ToString());
                if (toets.Key.Equals(ConsoleKey.UpArrow) && this.movement != DIRECTION.DOWN && this.buttonpressed == false)
                {
                    this.movement = DIRECTION.UP;
                    this.buttonpressed = true;
                }
                if (toets.Key.Equals(ConsoleKey.DownArrow) && this.movement != DIRECTION.UP && this.buttonpressed == false)
                {
                    this.movement = DIRECTION.DOWN;
                    this.buttonpressed = true;
                }
                if (toets.Key.Equals(ConsoleKey.LeftArrow) && this.movement != DIRECTION.RIGHT && this.buttonpressed == false)
                {
                    this.movement = DIRECTION.LEFT;
                    this.buttonpressed = true;
                }
                if (toets.Key.Equals(ConsoleKey.RightArrow) && this.movement != DIRECTION.LEFT && this.buttonpressed == false)
                {
                    this.movement = DIRECTION.RIGHT;
                    this.buttonpressed = true;
                }
            }

            this.body.Add(this.head);
            switch (this.movement)
            {
                case DIRECTION.UP:
                    this.head.y--;
                    break;
                case DIRECTION.DOWN:
                    this.head.y++;
                    break;
                case DIRECTION.LEFT:
                    this.head.x--;
                    break;
                case DIRECTION.RIGHT:
                    this.head.x++;
                    break;
            }
            if (this.body.Count() > this.length)
            {
                this.body.RemoveAt(0);
            }


            if (gameOver)
            {
                return false;
            }

            return true;
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
            pos = new Position(randomGenerator.Next(0, maxWidth), randomGenerator.Next(0, maxHeight));
        }

        public Berry(Window window)
        {
            this.maxWidth = window.width;
            this.maxHeight = window.height;
            pos = new Position(randomGenerator.Next(0, window.width), randomGenerator.Next(0, window.height));
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
        private DateTime timer = DateTime.Now;

        private static List<Object> objects = new List<Object>();

        System.Timers.Timer engineTime = new System.Timers.Timer(1000/FPS);
        private static int FPS = 2;

        public Engine()
        {
            wall = new Wall(window);
            objects.Add(wall);

            //player = new Snake(window.width / 2, window.height / 2, window);
            //objects.Append(player);
        }

        public static void loop()
        {
            DateTime startTime = DateTime.Now;
            DateTime fireTime = DateTime.Now;


            while (true) {
                fireTime = DateTime.Now;
                if(fireTime.Subtract(startTime).TotalMilliseconds > 1000 / FPS) { break;  }

                foreach (var obj in objects)
                {
                    if (!obj.act()) { return; }
                    obj.draw();
                }

                Console.Clear();
                startTime = DateTime.Now;
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
            Engine.loop();
        }
    }
}
//¦

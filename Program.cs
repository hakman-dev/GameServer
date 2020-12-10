using System;
using System.Threading;

namespace GameServer
{
    class Program
    {
        private static bool isRunning = false;

        static void Main(string[] args)
        {
            Console.Title = "Game Server";
            isRunning = true;

            Thread mainThread = new Thread(new ThreadStart(MainThread));
            mainThread.Start();
            
            string testString;
            Console.Write("Enter start date of game");
            testString = Console.ReadLine();
            Console.WriteLine("You entered '{0}'", testString);
            
            
            char[] spearator = { '-', ':', ' ' }; 
          
            // using the method 
            String[] datetime = testString.Split(spearator); 
          
            // foreach(String s in datetime) 
            // { 
                Console.WriteLine(datetime[0]); 
            // } 
            
            
            Int32 currentDate = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0))).TotalSeconds;
            Int32 gameStartTime = (Int32)(DateTime.UtcNow.Subtract(
                new DateTime(
                    Int32.Parse(datetime[0]),
                    Int32.Parse(datetime[1]),
                    Int32.Parse(datetime[2]),
                    Int32.Parse(datetime[3]),
                    Int32.Parse(datetime[4]),
                    Int32.Parse(datetime[5])
                    ))).TotalSeconds;
            Console.WriteLine("Current date:" + currentDate);
            Console.WriteLine("Game date" + gameStartTime);

            while (gameStartTime < 0) // game date not yet, dont start server yet
            {
                gameStartTime = (Int32)(DateTime.UtcNow.Subtract(
                    new DateTime(
                        Int32.Parse(datetime[0]),
                        Int32.Parse(datetime[1]),
                        Int32.Parse(datetime[2]),
                        Int32.Parse(datetime[3]),
                        Int32.Parse(datetime[4]),
                        Int32.Parse(datetime[5])
                    ))).TotalSeconds;
            }

            Server.Start(5, 6976);
        }

        private static void MainThread()
        {
            Console.WriteLine($"[{DateTime.Now.TimeOfDay}] Main thread started. Running at {Constants.TICKS_PER_SEC} ticks per second.");
            DateTime _nextLoop = DateTime.Now;

            while (isRunning)
            {
                while (_nextLoop < DateTime.Now)
                {
                    GameLogic.Update();

                    _nextLoop = _nextLoop.AddMilliseconds(Constants.MS_PER_TICK);

                    if (_nextLoop > DateTime.Now)
                    {
                        Thread.Sleep(_nextLoop - DateTime.Now);
                    }
                }
            }
        }
    }
}
using System;
using System.Threading;

namespace GameServer
{
    class Program
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private static bool isRunning = false;
        static void Main(string[] args)
        {
            Console.Title = "OverToYou Game Server";
            isRunning = true;

            Thread gameThread = new Thread(new ThreadStart(GameLoopThread));
            gameThread.Start();

            Server.Start(5, 20050);
        }

        private static void GameLoopThread()
        {
            logger.Info($"Game Thread started. Running at {Constants.TICKS_PER_SEC} ticks per second.");
            DateTime nextLoop = DateTime.Now;

            while (isRunning)
            {
                while (nextLoop < DateTime.Now)
                {
                    GameLogic.Update();

                    nextLoop = nextLoop.AddMilliseconds(Constants.MS_PER_TICK);

                    if (nextLoop > DateTime.Now)
                    {
                        Thread.Sleep(nextLoop - DateTime.Now);
                    }
                }
            }
        }
    }
}

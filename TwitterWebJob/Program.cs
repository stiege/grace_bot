using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TwitterWebJob
{
    class Program
    {

        static void Main()
        {
            try
            {
                MonitorTwitterMentions();

                while (true)
                {
                    Thread.Sleep(1000);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e);

            }

        }

        public static async void MonitorTwitterMentions()
        {
            await TwitterManager.GetInstance().StartStreamingMentionsAsync();
        }
    }

}

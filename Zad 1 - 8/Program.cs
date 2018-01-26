using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.IO;
using System.Diagnostics;


namespace ConsoleApplication1
{
    class Program
    {
        public static Object thisLock = new object();
        static void Main(string[] args)
        {
            //Zad5(100000,2);    
            Zad8();
        }

        //############################################################################

        static void ThreadProc(object stateInfo)
        {
            Thread.Sleep((int)stateInfo);
            Console.WriteLine(stateInfo);
        }

        static void Zad1()
        {
            ThreadPool.QueueUserWorkItem(ThreadProc, 500);
            ThreadPool.QueueUserWorkItem(ThreadProc, 500);

            Thread.Sleep(2000);
        }

        //############################################################################

        static void ThreadSerwerZad2(object stateInfo)
        {
            TcpListener server = new TcpListener(IPAddress.Any, 1024);
            server.Start();

            while (true)
            {
                TcpClient client = server.AcceptTcpClient();

                byte[] buffer = new byte[1024];
                client.GetStream().Read(buffer, 0, 1024);
                client.GetStream().Write(buffer, 0, buffer.Length);

                string result = Encoding.UTF8.GetString(buffer);
                Console.WriteLine(result);
            }

        }

        static void ThreadKlientZad2(object stateInfo)
        {
            TcpClient client = new TcpClient();
            client.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1024));

            byte[] message = new ASCIIEncoding().GetBytes("wiadomosc od klienta");
            client.GetStream().Write(message, 0, message.Length);
        }

        static void ThreadConnection(object stateInfo)
        {

        }

        static void Zad2()
        {
            ThreadPool.QueueUserWorkItem(ThreadSerwerZad2);

            ThreadPool.QueueUserWorkItem(ThreadKlientZad2);
            ThreadPool.QueueUserWorkItem(ThreadKlientZad2);

            Thread.Sleep(2000);

        }

        //############################################################################

        static void WriteConsoleMessage(string message, ConsoleColor color)
        {
            lock (thisLock)
            {
                Console.ForegroundColor = color;
                Console.WriteLine(message);
                Console.ResetColor();
            }
        }

        static void ThreadKlientZad3(object stateInfo)
        {
            TcpClient client = new TcpClient();
            client.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2048));

            byte[] message = new ASCIIEncoding().GetBytes("wiadomosc");
            client.GetStream().Write(message, 0, message.Length);
            client.GetStream().Read(message, 0, message.Length);

            string result = Encoding.UTF8.GetString(message);

            WriteConsoleMessage("Klient: " + result, ConsoleColor.Green);
        }

        static void ThreadSerwerZad3(object stateInfo)
        {
            TcpListener server = new TcpListener(IPAddress.Any, 2048);
            server.Start();

            while (true)
            {
                TcpClient client = server.AcceptTcpClient();
                ThreadPool.QueueUserWorkItem(ThreadConnection);

                byte[] buffer = new byte[1024];
                client.GetStream().Read(buffer, 0, 1024);
                client.GetStream().Write(buffer, 0, buffer.Length);

                string result = Encoding.UTF8.GetString(buffer);

                WriteConsoleMessage("Serwer: " + result, ConsoleColor.Red);
            }

        }

        static void Zad34()
        {
            ThreadPool.QueueUserWorkItem(ThreadSerwerZad3);
        
            ThreadPool.QueueUserWorkItem(ThreadKlientZad3);
            ThreadPool.QueueUserWorkItem(ThreadKlientZad3);

            Thread.Sleep(2000);
        }

        //############################################################################

        static int sum = 0;
        public static Object lock1 = new object();

        static void ThreadSumFragment(object stateInfo)
        {
            object[] args = stateInfo as object[];
            int[] arr = args[0] as int[];
            WaitHandle handler = args[1] as WaitHandle;
            AutoResetEvent autoResetSum = (AutoResetEvent)handler;

            lock (lock1)
            {
                foreach (int a in arr)
                {
                    sum += a;
                }
            }
            autoResetSum.Set();
        }

        static void Zad5(int size, int fragment)
        {
            Random rand = new Random();
            int events_num = (size + fragment - 1) / fragment;
            Console.WriteLine(events_num);

            List<int[]> fragments_arr = new List<int[]>();
            List<int> single_frag = new List<int>();

            Stopwatch zhonyas = new Stopwatch();
            Stopwatch hourglass = new Stopwatch();


            for (int i = 0; i < events_num - 1; i++)
            {
                for(int j = 0; j < fragment; j++)
                {
                    single_frag.Add(rand.Next(100));
                }
                fragments_arr.Add(single_frag.ToArray());
                single_frag.Clear();
            }
            int rest = size % fragment;

            for(int i = 0; i < rest; i++)
            {
                single_frag.Add(rand.Next(100));
            }
            fragments_arr.Add(single_frag.ToArray());

            /*
            foreach(var item in fragments_arr)
            {
                foreach(var num in item)
                {
                    Console.WriteLine(num);
                }
                Console.WriteLine();
            }
            */

            WaitHandle[] waitHandles = new WaitHandle[events_num];

            for(int i = 0; i < fragments_arr.Count; i++)
            {
                waitHandles[i] = new AutoResetEvent(false);
            }

            zhonyas.Start();
            for(int i = 0; i < events_num; i++)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(ThreadSumFragment), new object[] { fragments_arr[i], waitHandles[i] });
            }
            zhonyas.Stop();

            var result = fragments_arr.SelectMany(i => i);

            int sum_single = 0;

            hourglass.Start();
            sum_single = result.Sum();
            hourglass.Stop();

            
            Console.WriteLine("single={0}", zhonyas.Elapsed);
            Console.WriteLine(sum_single);

            Console.WriteLine("multi={0}", hourglass.Elapsed);
            Console.WriteLine(sum);

            Console.WriteLine();
            Console.ReadLine();

        }
        /*
        WYNIKI dla rozmiaru 10,000,000 podzielonego po 2:

        single=00:00:04.1119021
        494843692
        multi=00:00:00.3994346
        494843692

        wielowątkowa metoda obliczania wygrywa już przy ponad 100,000 przy fragmentach = 2
        przy znacznym zwiększeniu wielkości fragmentów na prowadzenie wysuwa się sumowanie w pojedyńczym wątku
        */

        //############################################################################

        static AutoResetEvent autoEvent = new AutoResetEvent(false);

        static void MyAsyncCallback(IAsyncResult state)
        {
            object[] args = state.AsyncState as object[];
            FileStream fs = args[0] as FileStream;
            byte[] buffer = args[1] as byte[];
            int len = fs.EndRead(state);
            string file = Encoding.UTF8.GetString(buffer, 0, len);
            Console.WriteLine(file);
        }

        static void ThreadFileRead(object stateInfo)
        {
            FileStream fs = File.OpenRead("plikIO.txt");
            byte[] buffer = new byte[1024];
            fs.BeginRead(buffer, 0, buffer.Length, MyAsyncCallback, new object[] { fs, buffer });

            Console.WriteLine("thread file read");

            ((AutoResetEvent)stateInfo).Set();
        }

        static void Zad6()
        {
            ThreadPool.QueueUserWorkItem(ThreadFileRead, autoEvent);
            autoEvent.WaitOne();

            Console.WriteLine("main");

            Thread.Sleep(3000);
        }

        //############################################################################

        static void Zad7()
        {
            FileStream fs = File.OpenRead("plikIO.txt");
            byte[] buffer = new byte[1024];

            var result = fs.BeginRead(buffer, 0, buffer.Length, null, null);
            int res = fs.EndRead(result);

            string plik = Encoding.UTF8.GetString(buffer, 0, res);
            Console.Write(plik);

            Thread.Sleep(2000);

            fs.Close();
        }
        
        static int SilniaIT(int n)
        {
            int result = 1;
            for (int i = 1; i <= n; i++)
            {
                result *= i;
            }
            return result;
        }

        static int SilniaR(int i)
        {
            if (i < 1)
                return 1;
            else
                return i * SilniaR(i - 1);
        }

        //############################################################################

        delegate int DelegateType1(int arguments);
        static DelegateType1 delegateName1;
        static DelegateType1 delegateName2;

        static void Zad8()
        {
            delegateName1 = new DelegateType1(SilniaIT);
            IAsyncResult ar = delegateName1.BeginInvoke(10, null, null);
            int result = delegateName1.EndInvoke(ar);
            Console.WriteLine("it: " + result);

            delegateName2 = new DelegateType1(SilniaR);
            IAsyncResult ar2 = delegateName1.BeginInvoke(10, null, null);
            int result2 = delegateName1.EndInvoke(ar2);

            Console.WriteLine("rek: " + result2);

            Thread.Sleep(2000);
        }
    }
}
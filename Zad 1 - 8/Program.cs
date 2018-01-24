using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.IO;


namespace ConsoleApplication1
{
    class Program
    {
        public static Object thisLock = new object();
        static void Main(string[] args)
        {
            Zad8();
            
        }

        static void ThreadProc(object stateInfo)
        {
            Thread.Sleep((int)stateInfo);
            Console.WriteLine(stateInfo);
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

        static void WriteConsoleMessage(string message, ConsoleColor color)
        {
            lock (thisLock)
            {
                Console.ForegroundColor = color;
                Console.WriteLine(message);
                Console.ResetColor();
            }
        }

        static void Zad1()
        {
            ThreadPool.QueueUserWorkItem(ThreadProc, 500);
            ThreadPool.QueueUserWorkItem(ThreadProc, 500);

            Thread.Sleep(2000);
        }

        static void Zad2()
        {
            ThreadPool.QueueUserWorkItem(ThreadSerwerZad2);

            ThreadPool.QueueUserWorkItem(ThreadKlientZad2);
            ThreadPool.QueueUserWorkItem(ThreadKlientZad2);

            Thread.Sleep(2000);

        }

        static void Zad34()
        {
            ThreadPool.QueueUserWorkItem(ThreadSerwerZad3);
        
            ThreadPool.QueueUserWorkItem(ThreadKlientZad3);
            ThreadPool.QueueUserWorkItem(ThreadKlientZad3);

            Thread.Sleep(2000);
        }

        static void Zad5()
        {

        }

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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Threads4
{
    public delegate void CallBackFromStartClass(string param);
    // Данные. Предмет и основа взаимодействия двух потоков.
    class CommonData
    {
        private int iValue;
        public int IValue { get => iValue; set => iValue = value; }

        public CommonData(int key)
        {
            iValue = key;
        }
    }
    // Классы Receiver и Sender: основа взаимодействующих потоков. 
    class Receiver
    {
        Queue cdQueue;
        CallBackFromStartClass callBack;

        public Receiver(ref Queue queueKey, CallBackFromStartClass cbKey)
        {
            cdQueue = queueKey;
            callBack = cbKey;
        }

        public void startReceiver()
        {
            DoIt();
        }

        // Тело рабочей функции...
        public void DoIt()
        {
            CommonData cd = null;
            while (true)
            {

                Console.WriteLine("Receiver. notifications in queue: {0}", cdQueue.Count);
                if (cdQueue.Count > 0)
                {
                    cd = (CommonData)cdQueue.Dequeue();
                    if (cd == null)
                        Console.WriteLine("?????");
                    else
                    {
                        Console.WriteLine("Process started ({0}).", cd.IValue);
                        // Выбрать какой-нибудь из способов обработки полученного уведомления.
                        // Заснуть на соответствующее количество тиков.
                        // Thread.Sleep(cd.iValProp);
                        // Заняться элементарной арифметикой. С усыплением потока.
                        while (cd.IValue != 0)
                        {
                            cd.IValue--;
                            Thread.Sleep(cd.IValue);
                            Console.WriteLine("process:{0}", cd.IValue);
                        }
                    }
                }
                else
                    callBack("Receiver");

                Thread.Sleep(100);
            }
        }
    }

    class Sender
    {
        Random rnd;
        int stopVal;
        Queue cdQueue;
        CallBackFromStartClass callBack;

        // Конструктор...
        public Sender(ref Queue queueKey, int key, CallBackFromStartClass cbKey)
        {
            rnd = new Random(key);
            stopVal = key;
            cdQueue = queueKey;
            callBack = cbKey;
        }

        public void startSender()
        {
            sendIt();
        }

        // Тело рабочей функции...
        public void sendIt()
        {//====================================

            while (true)
            {
                if (stopVal > 0)
                {
                    // Размещение в очереди нового члена со случайными характеристиками.
                    cdQueue.Enqueue(new CommonData(rnd.Next(0, stopVal)));
                    stopVal--;
                }
                else
                    callBack("Sender");

                Console.WriteLine("Sender. in queue:{0}, the rest of notifications:{1}.",
                 cdQueue.Count, stopVal);
                Thread.Sleep(100);
            }
        }
    }

    class StartClass
    {
        static Thread th0, th1;
        static Queue NotificationQueue;
        static string[] report = new string[2];

        static void Main(string[] args)
        {
            StartClass.NotificationQueue = new Queue();
            // Конструкторы классов Receiver и Sender несут дополнительную нагрузку.
            // Они обеспечивают необходимыми значениями методы,
            // выполняемые во вторичных потоках.
            Sender sender;
            // По окончании работы отправитель вызывает функцию-терминатор.
            // Для этого используется специально определяемый и настраиваемый делегат.
            sender = new Sender(ref NotificationQueue, 10, new CallBackFromStartClass(StartClass.StopMain));

            Receiver receiver;
            // Выбрав всю очередь, получатель вызывает функцию-терминатор.
            receiver = new Receiver(ref NotificationQueue, new CallBackFromStartClass(StartClass.StopMain));
            // Стартовые функции потоков должны соответствовать сигнатуре
            // класса делегата ThreadStart. Поэтому они не имеют параметров.
            ThreadStart t0, t1;
            t0 = new ThreadStart(sender.startSender);
            t1 = new ThreadStart(receiver.startReceiver);

            // Созданы вторичные потоки.
            StartClass.th0 = new Thread(t0);
            StartClass.th1 = new Thread(t1);

            // Запущены вторичные потоки.
            StartClass.th0.Start();
            StartClass.th1.Start();
            // Еще раз о методе Join(): 
            // Выполнение главного потока приостановлено до завершения
            // выполнения вторичных потоков.
            StartClass.th0.Join();
            StartClass.th1.Join();
            // Потому последнее слово остается за главным потоком приложения.
            Console.WriteLine("Main(): " + report[0] + "..." + report[1] + "... Bye.");
        }

        // Функция - член класса StartClass выполняется во ВТОРИЧНОМ потоке!
        public static void StopMain(string param)
        {
            Console.WriteLine("StopMain: " + param);
            // Остановка рабочих потоков. Ее выполняет функция - член класса StartClass. Этой функции в силу своего определения
            // известно ВСЕ о вторичных потоках. Но выполняется она в ЧУЖИХ (вторичных) потоках.
            if (param.Equals("Sender"))
            {
                report[0] = "Sender all did.";
                StartClass.th0.Abort();
            }

            if (param.Equals("Receiver"))
            {
                report[1] = "Receiver all did.";
                StartClass.th1.Abort();
            }
            // Этот оператор не выполняется! Поток, в котором выполняется
            // метод - член класса StartClass StopMain(), остановлен.
            Console.WriteLine("StopMain(): bye.");
        }
    }
}

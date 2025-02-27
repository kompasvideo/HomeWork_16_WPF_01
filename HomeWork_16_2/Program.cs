﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HomeWork_16_2
{
    class Program
    {
        static int[] mas;
        static Stopwatch stopWatch;
        static Stopwatch stopWatch2;

        static void Main(string[] args)
        {
            Console.WriteLine("Однопоточный режим");
            stopWatch = new Stopwatch();
            stopWatch.Start();
            int i = CalculateOne();
            stopWatch.Stop();
            Console.WriteLine($"Количество чисел {i}");
            TimeSpan ts = stopWatch.Elapsed;

            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
            Console.WriteLine("RunTime " + elapsedTime);

            Console.WriteLine("\n\nМнопоточный режим");
            stopWatch2 = new Stopwatch();
            int i2 = Calculate();
            Console.WriteLine($"Количество чисел {i2}");
            TimeSpan ts2 = stopWatch2.Elapsed;

            string elapsedTime2 = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts2.Hours, ts2.Minutes, ts2.Seconds,
                ts2.Milliseconds / 10);
            Console.WriteLine("RunTime " + elapsedTime2);
            Console.ReadKey();
        }
        static int CalculateOne()
        {
            int numbers = 0;
            for (int i = 1_000_000_000; i < 2_000_000_000; i++)
            {
                int endDigit = i - i / 10 * 10;
                if (endDigit != 0)
                {
                    if ((i % endDigit) == 0)
                    {
                        numbers++;
                    }
                }
            }
            return numbers;
        }

        static int Calculate()
        {
            int coreCount = 0;
            foreach (var item in new ManagementObjectSearcher("Select * from Win32_Processor").Get())
            {
                coreCount += int.Parse(item["NumberOfCores"].ToString());
            }
            Console.WriteLine($"Физических ядер = {coreCount}");
            int potok = Environment.ProcessorCount;
            Console.WriteLine($"Количество логических потоков = {potok}");
            potok--;
            Console.WriteLine($"Количество рабочих потоков = {potok}");
            int n = 1_000_000_000;
            int sub = n / potok;
            mas = new int[potok + 1];
            for (int i = 0; i < (potok + 1); i++)
            {
                mas[i] = n + i * sub;
            }
            if ((n % potok) != 0)
            {
                mas[potok] = 2_000_000_000;
            }
            var tasks = new List<Task<int>>();
            stopWatch2.Start();
            for (int k = 0; k < potok; k++)
            {
                tasks.Add(Task<int>.Factory.StartNew(action, k));
            }
            Task.WaitAll(tasks.ToArray());
            int numbers = 0;
            foreach (var task in tasks)
            {
                numbers += task.Result;
            }
            stopWatch2.Stop();
            return numbers;                      
        }

        static Func<object, int> action = (object obj) =>
        {
            int k2 = (int)obj;
            int result = 0;
            for (int i = mas[k2]; i < mas[k2 + 1]; i++)
            {
                int endDigit = i - i / 10 * 10;
                if (endDigit != 0)
                {
                    if ((i % endDigit) == 0)
                    {
                        result++;
                    }
                }
            }
            return result;
        };
    }
}

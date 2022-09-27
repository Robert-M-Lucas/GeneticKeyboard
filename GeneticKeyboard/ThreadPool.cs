using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticKeyboard
{
    internal class PoolData<T>
    {
        public List<T> Objects;
        public Func<T, Action> GetAction;
        public int Start;
        public int End;

        public PoolData (List<T> objects, Func<T, Action> get_action, int start, int end) 
        {
            Objects = objects;
            GetAction = get_action;
            Start = start;
            End = end;
        }
    }

    internal static class ThreadPool
    {
        public static void Pool<T>(int threads, int min_items_per_thread, List<T> objects, Func<T, Action> get_action)
        {
            List<Thread> thread_list = new List<Thread>();

            if (objects.Count() / threads > min_items_per_thread)
            {
                for (int t = 0; t < threads; t++)
                {
                    Thread th;
                    if (t != threads - 1)
                    {
                        th = new Thread(o => Run((PoolData<T>)o));
                        th.Start(new PoolData<T>(objects, get_action, t * (objects.Count() / threads), (t + 1) * (objects.Count() / threads)));
                    }
                    else
                    {
                        th = new Thread(o => Run((PoolData<T>)o));
                        th.Start(new PoolData<T>(objects, get_action, t * (objects.Count() / threads), objects.Count()));
                    }
                    thread_list.Add(th);
                }
            }
            else
            {
                int i = 0;
                while (i < objects.Count())
                {
                    int i2 = i + min_items_per_thread;
                    if (i2 > objects.Count()) i2 = objects.Count();

                    Thread t = new Thread(o => Run((PoolData<T>)o));
                    t.Start(new PoolData<T>(objects, get_action, i, i2));
                    thread_list.Add(t);

                    i = i2;
                }
            }

            foreach (Thread t in thread_list)
            {
                t.Join();
            }
        }

        public static void Run<T>(PoolData<T> p)
        {
            for (int i = p.Start; i < p.End; i++)
            {
                p.GetAction(p.Objects[i])();
            }
        }
    }
}

using NeathCopyEngine.CopyHandlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;

namespace NeathCopyEngine.Helpers
{
    public class MyThread
    {
        /// <summary>
        /// Action to perform
        /// </summary>
        public Action ActionToPerform { get; protected set; }

        Thread thread;
        readonly ManualResetEventSlim pauseGate = new ManualResetEventSlim(true);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action">Action to perform</param>
        public MyThread(Action action)
        {
            ActionToPerform = action;

            Reset();
        }

        public void Reset()
        {
            thread = new Thread(new ThreadStart(mainMethod));
            pauseGate.Set();
        }

        private void mainMethod()
        {
            try
            {
                pauseGate.Wait();
                ActionToPerform.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show(Error.GetErrorLog(ex.Message,"NeathCopyEngine","MyThread", "mainMethod"));
            }

        }

        /// <summary>
        /// Start the action in async way.
        /// </summary>
        public virtual void Start()
        {
            pauseGate.Set();
            thread.Start();
        }
        /// <summary>
        /// Pause the action by loocking with Monitor class.
        /// </summary>
        public virtual void Pause()
        {
            // Cooperate: only blocks before ActionToPerform starts.
            pauseGate.Reset();
        }
        /// <summary>
        /// Resume the action by Exit method of Monitor class.
        /// </summary>
        public virtual void Resume()
        {
             if (thread.ThreadState == ThreadState.Unstarted)
             {
                 pauseGate.Set();
                 thread.Start();
             }
             else
             {
                 pauseGate.Set();
             }

            //if (thread.ThreadState == ThreadState.Suspended)
            //{
            //    thread.Resume();
            //}
            //else if(thread.ThreadState== ThreadState.Unstarted)
            //    thread.Start();
        }
        /// <summary>
        /// Cancel the action.
        /// </summary>
        public virtual void Cancel()
        {
            Abort();
        }

        public virtual void Abort()
        {
            try
            {
                thread = new Thread(new ThreadStart(mainMethod));
                pauseGate.Set();
            }
            catch(Exception ex)
            {
                MessageBox.Show(Error.GetErrorLog(ex.Message, "NeathCopyEngine", "MyThread", "Abort"));
            }
        }
    }
}

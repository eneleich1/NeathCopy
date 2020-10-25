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
        }

        private void mainMethod()
        {
            try
            {
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
            thread.Start();
        }
        /// <summary>
        /// Pause the action by loocking with Monitor class.
        /// </summary>
        public virtual void Pause()
        {
            thread.Suspend();
            //if (thread.ThreadState == ThreadState.Running)
            //{
            //    thread.Suspend();
            //}
        }
        /// <summary>
        /// Resume the action by Exit method of Monitor class.
        /// </summary>
        public virtual void Resume()
        {
             if (thread.ThreadState == ThreadState.Unstarted)
                thread.Start();
             else thread.Resume();

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
            }
            catch(Exception ex)
            {
                MessageBox.Show(Error.GetErrorLog(ex.Message, "NeathCopyEngine", "MyThread", "Abort"));
            }
        }
    }
}

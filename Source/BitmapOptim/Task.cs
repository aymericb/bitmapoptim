using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace BitmapOptim
{
    public class Task
    {
        // ### 
        /*public class RunContext
        {
            public RunContext(CancellationToken token, Semaphore sem)
            {
                m_token = token;
                m_sem = sem;
            }

            public CancellationToken CancellationToken { get { return m_token; } }              
            public SemaphoreSlim Semaphore { get { return m_sem; } }

            private CancellationToken m_token;
            private Semaphore m_sem;
        }*/

        public string Task { get { return m_name; } }
        public Exception Error { get { return m_error; } }
        public List<Task> SubTasks { get; set; }
        public bool HasError { get { return GetHasError(); } }

        private string m_name;
        private Exception m_error;

        public Task(string name)
        {
            this.m_name = name;
            this.m_error = null;
        }

        public void Run(CancellationToken token)
        {       
            // Run subtasks sequentially
            foreach (Task subtask in this.SubTasks)
            {
                subtask.Run(token);
                if (token.IsCancellationRequested)
                    return;
            }

            // Run the body
            try
            {
                DoTask(token);
            }
            catch (Exception e)
            {
                m_error = e;
            }
        }

        // Override
        protected virtual void DoTask(CancellationToken token)
        {
        }

        private bool GetHasError()
        {
            if (m_error != null)
                return true;
            foreach (Task subtask in this.SubTasks)
            {
                if (subtask.HasError)
                    return true;
            }
            return false;
        }
    }
}

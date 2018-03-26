using Common.Logging;
using Quartz;
using System;
using System.ComponentModel;

namespace SchedulingService.Models
{
    class Listener : IJobListener
    {
        #region "Local properties"

        /// <summary>
        /// Initial setup
        /// </summary>
        private static readonly ILog logger = LogManager.GetLogger(typeof(Listener));

        /// <summary>
        /// The handler
        /// </summary>
        public event EventHandler JobExecutedHandler;

        #endregion

        #region "Actions"

        /// <summary>
        /// Event that is fired when a job is vetoed
        /// </summary>
        /// <param name="context"></param>
        public void JobExecutionVetoed(IJobExecutionContext context)
        {
            logger.Info("JobExecutionVetoed");
        }

        public void JobToBeExecuted(IJobExecutionContext context)
        {
            logger.Info("JobToBeExecuted");
        }

        /// <summary>
        /// After job complete, raise the details.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="jobException"></param>
        public void JobWasExecuted(IJobExecutionContext context, JobExecutionException jobException)
        {
            // This object needs to be updated when you change what a job can store.
            var jobResult = new Job()
            {
                Id = context.JobDetail.Key.Name,
                Url = context.MergedJobDataMap["url"].ToString(),
                BodyCompressed = context.MergedJobDataMap["bodycompressed"] as byte[],
                Status = context.MergedJobDataMap["status"].ToString()
            };
            OnJobExecuted(EventArgs.Empty, jobResult);
            logger.Info("JobWasExecuted");
        }

        /// <summary>
        /// Name of Listener.
        /// </summary>
        public string Name
        {
            get
            {
                return "Listener";
            }
        }

        #endregion

        #region "Local methods"

        /// <summary>
        /// Once a job is complete, this will raise the event to the listener.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="aJob"></param>
        protected virtual void OnJobExecuted(EventArgs args, Job aJob)
        {
            // This code will prevent IllegalThreadContext exceptions
            EventHandler jobExecHandler = JobExecutedHandler;

            if (jobExecHandler != null)
            {
                ISynchronizeInvoke target = jobExecHandler.Target as ISynchronizeInvoke;

                if (target != null && target.InvokeRequired)
                {
                    target.Invoke(jobExecHandler, new object[] { aJob, args });
                }
                else
                {
                    jobExecHandler(aJob, args);
                }
            }
        }

        #endregion
    }
}

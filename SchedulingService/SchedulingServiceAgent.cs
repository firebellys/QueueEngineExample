using SchedulingService.Models;
using System;
using System.Collections.Generic;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;
using System.Linq;
using SchedulingService.Helpers;

namespace SchedulingService
{
    public class SchedulingServiceAgent
    {
        #region "Local properties"

        /// <summary>
        /// Quartz Scheduler
        /// </summary>
        private static IScheduler scheduler;

        /// <summary>
        /// List of completed jobs
        /// </summary>
        private static List<Job> JobsList;

        #endregion

        #region "Contructor"

        /// <summary>
        /// Constructor
        /// </summary>
        static SchedulingServiceAgent()
        {
            // Spin Scheduler
            scheduler = StdSchedulerFactory.GetDefaultScheduler();
            scheduler.Start();

            // Create listener
            Listener jobListener = new Listener();
            jobListener.JobExecutedHandler += new EventHandler((sender, e) => AddToList(sender));
            scheduler.ListenerManager.AddJobListener(jobListener, GroupMatcher<JobKey>.GroupEquals("RequestGroup"));

            JobsList = new List<Job>();
        }

        #endregion

        #region "Actions"

        /// <summary>
        /// Gets all executing Ids
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<string> GetAll()
        {
            var resultsList = new List<string>();
            foreach (var key in scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals("RequestGroup")))
            {
                resultsList.Add(key.Name);
            }
            return resultsList;
        }

        /// <summary>
        /// Gets job details by Id
        /// 
        /// Will check the scheduler first, then proceed to internal storage.
        /// 
        /// TODO: Add a hook into Memcache or Redis or Azure Table Storage.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string GetStatusById(string id)
        {
            // First check if there is something in queue.
            var singleJob = scheduler.GetJobDetail(new JobKey(id, "RequestGroup"));
            if (singleJob != null)
            {
                var jobResult = new Job()
                {
                    Id = singleJob.Key.Name,
                    Url = singleJob.JobDataMap["url"].ToString(),
                    Status = singleJob.JobDataMap["status"].ToString()
                };
                return Newtonsoft.Json.JsonConvert.SerializeObject(jobResult);
            }
            else
            {
                // Check the completed queue for items.
                var completedItem = JobsList.Find(x => x.Id == id);
                if (completedItem != null)
                {
                    // Decompress and clear the binary out.
                    // TODO: This section needs to be redone, so the json is clean
                    completedItem.Body = Utilities.Unzip(completedItem.BodyCompressed);
                    completedItem.BodyCompressed = null;
                    return Newtonsoft.Json.JsonConvert.SerializeObject(completedItem);
                }
                else
                {
                    // Nothing found in system.
                    return null;
                }
            }
        }

        /// <summary>
        /// Request a new job to be scheduled
        /// </summary>
        /// <param name="request">Request body</param>
        /// <returns></returns>
        public static string QueueRequest(string request)
        {
            if (String.IsNullOrEmpty(request))
            {
                throw new Exception("Request must not be null.");
            }

            // See if we have cached a reply already
            var cachedJob = JobsList.Where(c => c.Url == request)
                                .OrderByDescending(t => t.StartTime)
                                .FirstOrDefault();

            if (cachedJob != null)
            {
                // Check how old the archived version is, refresh if more than hour old
                if (cachedJob.StartTime.Subtract(DateTime.Now).TotalMinutes > 60)
                {
                    // If the archive is stale, fetch a new one
                    // TODO: add logic to update existing instead of creating a new
                    return Newtonsoft.Json.JsonConvert.SerializeObject(new Response()
                    {
                        Id = QueueJob(request)
                    }); ;
                }
                else
                {
                    // Deflate the scraped data
                    var response = new Response
                    {
                        Id = cachedJob.Id,
                        CachedResponse = (Job)cachedJob.Clone()
                    };
                    response.CachedResponse.Body = Utilities.Unzip(cachedJob.BodyCompressed);
                    response.CachedResponse.BodyCompressed = null;

                    // Return cached version id instead
                    return Newtonsoft.Json.JsonConvert.SerializeObject(response);
                }
            }
            else
            {
                // Return ID for new job
                return Newtonsoft.Json.JsonConvert.SerializeObject(new Response()
                {
                    Id = QueueJob(request)
                }); ;
            }
        }

        /// <summary>
        /// Removes a job from complete list and scheduler.
        /// </summary>
        public static string RemoveFromList(string id)
        {
            var stuffToRemove = JobsList.SingleOrDefault(s => s.Id == id);
            if (stuffToRemove != null)
            {
                JobsList.Remove(stuffToRemove);
                return "Deleted from Archive";
            }
            else
            {
                scheduler.DeleteJob(new JobKey(id, "RequestGroup"));
                return "Deleted from Scheduler";
            }
        }

        #endregion

        #region "Local methods"

        /// <summary>
        /// Add to complete list.
        /// </summary>
        /// <param name="result"></param>
        static void AddToList(Object result)
        {
            JobsList.Add(result as Job);
        }

        /// <summary>
        /// Enqueues a new job and returns the tracking id
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        static string QueueJob(string request)
        {
            // Create a new id to track the transaction
            var id = Guid.NewGuid().ToString();
            IJobDetail job = JobBuilder.Create<RequestJob>()
                .WithIdentity("RequestJob_" + id, "RequestGroup")
                .Build();
            job.JobDataMap["status"] = JobStatus.InProgress;
            job.JobDataMap["url"] = request;

            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("RequestJobTrigger_" + id, "RequestGroup")
                .StartNow()
                .Build();
            scheduler.ScheduleJob(job, trigger);

            return job.Key.Name.ToString();
        }

        #endregion
    }
}

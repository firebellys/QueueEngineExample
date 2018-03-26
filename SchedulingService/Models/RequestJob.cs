using Quartz;
using SchedulingService.Helpers;
using System;
using System.Net.Http;

namespace SchedulingService.Models
{
    [PersistJobDataAfterExecution]
    class RequestJob : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            try
            {
                // Create storage for the data.
                JobKey key = context.JobDetail.Key;
                JobDataMap dataMap = context.MergedJobDataMap;

                // Mark first attempt
                dataMap["startdate"] = DateTime.Now;

                // Scrape the web site
                using (var client = new HttpClient())
                {
                    var response = client.GetAsync(dataMap["url"].ToString()).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = response.Content;
                        
                        // Capture the last modified to check if we are scraping too soon in future requests
                        // TODO: Find a better metric, some sites don't return a valid response
                        // TODO: Break up the screen scrape here with more advanced logic
                        //dataMap["lastmodified"] = response.Content.Headers.Expires;

                        // Compress the response
                        dataMap["bodycompressed"] = Utilities.Zip(responseContent.ReadAsStringAsync().Result);
                    }
                }                    
                dataMap["status"] = JobStatus.Complete.ToString();
            }
            catch (Exception e)
            {
                // Retry the job if it fails
                JobExecutionException jException = new JobExecutionException(e)
                {
                    RefireImmediately = true
                };
                throw jException;
            }
        }
    }
}

using System;

namespace SchedulingService.Models
{
    enum JobStatus
    {
        Complete,
        Error,
        WaitingForPickup,
        InProgress
    }

    /// <summary>
    /// Main Job Model
    /// </summary>
    public class Job : ICloneable
    {
        /// <summary>
        /// Id of job, 
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Body of web result
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// Body of web result
        /// </summary>
        public byte[] BodyCompressed { get; set; }

        /// <summary>
        /// Request web url
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Current Status of job
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Start time
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// End time
        /// </summary>
        public DateTime LastModified { get; set; }

        // Clone the object
        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}

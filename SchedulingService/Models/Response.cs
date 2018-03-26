using Newtonsoft.Json;

namespace SchedulingService.Models
{
    // Response object for request
    [JsonObject]
    public class Response
    {
        // Tracking Id for request
        [JsonProperty]
        public string Id { get; set; }

        // Returns the cached job if it exists
        [JsonProperty]
        public Job CachedResponse { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public Response()
        {
            this.Id = string.Empty;
            this.CachedResponse = null;
        }
    }
}

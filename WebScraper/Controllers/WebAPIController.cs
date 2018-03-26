using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using SchedulingService;

namespace WebScraper.Controllers
{
    public class WebAPIController : ApiController
    {
        /// <summary>
        /// Returns a list of in process items
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> Get()
        {
            return SchedulingServiceAgent.GetAll();
        }

        /// <summary>
        /// Returns job details such as status or response
        /// </summary>
        /// <param name="id">The job tracking ID</param>
        /// <returns>Formatted HTTP Reponse with payload</returns>
        public HttpResponseMessage Get(string id)
        {
            if (String.IsNullOrEmpty(id))
            {
                HttpError err = new HttpError("Request ID can't be blank.");
                return Request.CreateResponse(HttpStatusCode.BadRequest, err);
            }
            else
            {
                var result = SchedulingServiceAgent.GetStatusById(id);
                if (result == null)
                {
                    HttpError err = new HttpError("No results found.");
                    return Request.CreateResponse(HttpStatusCode.NotFound, err);
                }
                else
                {
                    return new HttpResponseMessage()
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent(result, System.Text.Encoding.UTF8, "application/json")
                    };
                }
            }
        }

        /// <summary>
        /// Request a screen scrape
        /// </summary>
        /// <param name="value">URL Request</param>
        /// <returns>Formatted HTTP Reponse with payload</returns>
        public HttpResponseMessage Post(string value)
        {
            // Check for missing parameters
            if (String.IsNullOrEmpty(value))
            {
                HttpError err = new HttpError("Request URLs can't be blank.");
                return Request.CreateResponse(HttpStatusCode.BadRequest, err);
            }
            else
            {
                // Check if value is a good URI
                if (Uri.IsWellFormedUriString(value, UriKind.Absolute))
                {
                    return new HttpResponseMessage()
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent(SchedulingServiceAgent.QueueRequest(value), System.Text.Encoding.UTF8, "application/json")
                    };
                }
                else
                {
                    HttpError err = new HttpError("The URL is malformed. Please use valid URLs.");
                    return Request.CreateResponse(HttpStatusCode.BadRequest, err);
                }
            }
        }

        /// <summary>
        /// Delete function for Deleting endpoint.
        /// </summary>
        /// <param name="id">Id of requested job</param>
        /// <returns>Formatted HTTP Reponse with payload</returns>
        public HttpResponseMessage Delete(string id)
        {
            // Check for missing parameters
            if (String.IsNullOrEmpty(id))
            {
                HttpError err = new HttpError("Id's can't be blank.");
                return Request.CreateResponse(HttpStatusCode.BadRequest, err);
            }
            else
            {
                return new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(SchedulingServiceAgent.RemoveFromList(id), System.Text.Encoding.UTF8, "application/json")
                };
            }
        }
    }
}

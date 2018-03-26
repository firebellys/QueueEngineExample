## Synopsis
This ASP.Net Web API allows a consumer to request a web scrape of any site. The system will enqueue the request and return the consumer a tracking ID. This ID can be used to check status or retrieve the results.

### Technologies used
List of required frameworks or libraries
* .Net 4.6 (.Net Standard)
* [Quartz Scheduler (For .Net)](https://github.com/quartznet/quartznet)
* [JSON.Net](https://www.newtonsoft.com/json)

## Design
The application is designed around a simple client / service pattern. The crucial portion being the queue system inside the service. The client is the API interface which follows ASP MVC patterns and consumes the reponse from the server layer.

The system will check if the request has already been made within the last hour and if there is no existing request it will queue a new one.

The system provides a means of retrieving the data after it is completed in the background.

There is no data layer since this application runs without any back-end or cloud services.

### API
The API uses OOTB features of an ASP.Net Web API application. It follows a standard MVC pattern. Updates to the way the API is structured can be managed by the [route table configuration](https://docs.microsoft.com/en-us/aspnet/web-api/overview/web-api-routing-and-actions/routing-in-aspnet-web-api).

There are two controllers present. One exists to provide a browser friendly page when not accessing the apis. The WebAPI Controller provides the core HTTP functionality.
* GET, returns all jobs in process. After a job is complete, it will not be returned here.
* GET(id), returns the status of your job or results if complete.
* POST(string), creates a new job and provides you with the tracking id.
* DELETE(id), removes the job from the system.

### Service Layer
The service layer provides the functionality to the platform. The Quartz scheduler manages the threading of jobs internally. This thresh-hold can be managed by the [Quartz configuration files](https://www.quartz-scheduler.net/documentation/quartz-2.x/quick-start.html). When a request comes in, the system uses the scheduler to start a job in the background. When this job is complete (scraping), a listener will put the response into a local in-memory archive. This is volatile storage, so request need to be restarted if the system restarts.

### Use Cases
There are several standard use cases for web scraping. Most of the time, these are timer jobs that run on a schedule such as the start of a day or end of a week or fiscal period.
* Data mining, building analytics based off of a competitor's public facing presence.
* Autonomous merging of disperse data sets for reporting. If a partner wants to feature data on your own site/app, the app could pull it in through scraping.
* Real time analysis. If an app wanted to merge data sets from multiple sources outside the organization in real time, such as the best cheese burger nearby, screen scraping could help build that data set.

## Areas if improvement
The following are a list of comments around future state. These include technical solutions as well as logical solutions.

### Hosting
This project should be hosted in Azure to gain the most useful functionality. One factor is physical proximity to other Azure services such as storage. Another advantage is the ability to use frameworks such as [Application Insights](https://azure.microsoft.com/en-us/services/application-insights/). App Insights provides you powerful the ability to track transactions, track down exceptions and also spot bottle necks in the system.

### Deployment
A Docker container can be used to manage the deployment process. This [link](https://docs.microsoft.com/en-us/aspnet/mvc/overview/deployment/docker-aspnetmvc) can provide information on deploying with .Net Standard vs Core.

### Queue Service
The queue service does not support threading by itself, it relies on the scheduler to manage this. If the scheduler is swapped out, there will need ot be updates made to the concurrency processing, as of now the scheduler manages this.

### Queue Back-end
Quartz is one of many different scheduling solutions for .Net. These are other options that can help improve the ability to scale or compatibility.
* [Hangfire](https://www.hangfire.io/), This is a .Net native background tool that is similar to Quartz in features. It does have a license fee.
* [Azure Queue/Bus](https://docs.microsoft.com/en-us/azure/service-bus-messaging/service-bus-azure-and-service-bus-queues-compared-contrasted), Azure provides a service for quick messages or more robust scheduling. This allows for easier scaling.

Storing the results is also an area where vast improvements could be made. Instead of local, volatile in memory object, a more robust system could be used.
* [Azure Table Storage](https://azure.microsoft.com/en-us/services/storage/tables/), Instead of in memory storage of completed items, table storage could be used to archive the results.
* MongoDB + GraphQL, This combination may not bring immediate benefits, but in an environment where Node.js and .Net are present, it could help with future integrations since the interface is common across all platforms.
* SQLite/SQL, This is a last resort for storing information. Persistent, stable and can be pushed to mobile if using SQLite.

### Auth Services
The API could also use some level of security such as an API Key to manage users and also tie transactions to a particular user.

### Scraping
The scraping process can be improved with adding features that can improve speed, readability and targeted data. Another solution would be to use an off the shelf solution.
* Doing more with DOM elements that are pulled in. Often JavaScript, React elements or garbage is stored in a DOM layout, getting at the important bits is crucial (Key information such as prices or dates). This would be the next step
* Initially batching would help with larger tasks, the ability to queue up multiple requests and then pull them in batches.
* Handling errors needs to be updated so that 404s or other HTTP codes are managed.
* RPA, Blue Prism, Third party applications can manage the automated scraping of web sites. This could include Azure Scheduler or Blue Prism RPA.
* SaaS such as [80legs](http://80legs.com/), this would come at a monetary cost but can reduce development time and application management.
* Use HTMLAgility, This library provides HTML manipulation functionality within .Net and can be used to create smarter scraped objects.
* Use Custom Templates/Filters on results, If the site that is being scraped is defined, templates can be used to filter piece of the web site out. If we are targeting prices, then a template could be used to only bring forward the price results.

### Cache
Caching becomes very important as it will speed up the application by reducing work (and cost, if hosted). 
* Redis/Memcached, These frameworks can run locally or in the cloud. They provide far more flexibility with caching and can scale quicker.

### Intelligent Threading
As the threads spin up, there should be a better way to monitor this. If required, create a new scheduler to manage a new round of requests and maintain those in a list. This will allow the system to spin up or down scheduler services to deal with heavy load. The logic would be as followed:
* On request, check how many requests are being handled by a single scheduler/listener
* If it exceeds a certain amount (to be tuned with load testing)
* Spin up a new instance of a scheduler/listener until the load dies down again

## Testing
Unit Tests are provided in the Test project.

The application was run through a few scenarios of WebSurge Load Tester and showed a steady pace when throttled, although when more than a few hundred requests per second are queued, the system begins to slow from ~15ms to a second. Several factors could cause this, incorrect settings on the web server or the app is not configured correctly for threading because a threshold. The final reason is the logical layout of the application is not conducive to more than a few hundred requests a second and the bottle neck needs to be refined.


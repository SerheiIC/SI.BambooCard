**Steps to run aspnetcore web app:**
  1. Build the solution using msbuild directly from the solution folder ( \SI.BambooCard> msbuild SI.BambooCard.sln) 
     ![image](https://github.com/SerheiIC/SI.BambooCard/assets/159906799/30789e93-2ec6-4d67-9d79-0db2673bf5f2)

  2. Run the application using the Kestrel web server using PowerShell and calling **dotnet run** from the folder containing the SI.BambooCard.Web.Host.csproj file. As on the screenshot below.
     ![image](https://github.com/SerheiIC/SI.BambooCard/assets/159906799/77c62c14-baa3-4b88-8cc6-89a4821aba98)

     <hr>
  **Explanatory note**
  <br>
When implementing this task requirement, which states that the API must be able to efficiently serve a large number of requests without risking overloading the Hacker News API requirement, I used semaforeslim to limit the maximum number of   simultaneously processed threads in the current implementation:
```cs
 var semaphore = new SemaphoreSlim(_maxConcurrency);
```
    
When sending multiple requests, limiting the number of simultaneous http requests to a maximum of 20 ensures that the collections used are thread safe during an external API call. 
Each task is stored within a list where we can keep track of the completion state: 
```cs
IEnumerable<Task<ItemDto>> tasks;
``` 
In this case, the throttling pattern ensures that the synchronization object is not released to perform additional tasks when the number of concurrent tasks exceeds the 20 limit.
Thus, we managed to receive about 200 requests in less than 3 seconds.

This approach is faster than the following possible methods:
- simple asynchronous operations, simply using the async/await keywords;
- parallel execution of await Task.WhenAll(tasks);
- parallel but not all at the same time,  batches for 100 as example;

Suggested next steps for improvement that were not implemented:
- implementation of the Redis cache.

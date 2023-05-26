# Chaining Functions Together

Chaining Functions: No single place to see the whole workflow

## Chaining with Durable Functions

```cs
// Orchestrator Function

// call the first activity
await CallActivityAsync("Activity1");
// call the second activity
await CallActivityAsync("Activity2");
// call the third activity
await CallActivityAsync("Activity3");

```

## Create a function chain with Durable Functions

- Create an orchestrator function
- Create activity functions
- Start a new orchestration with the DurableClient binding
- Test locally

### Create a workflow starter function
- DurableClient binding
- Orchestrator input data
- CreateCheckStatusResponse

### Create an orchestrator function
- Calls each activity in turn
- Receive input data from the starter function

## Orchestrator Function Rules

> Must be deterministic

- The whole function will be “replayed”
> Don’t

- Use current date time
- Generate random numbers or Guids
- Access data stores (e.g. database, configuration)
> Do

- Use IDurableOrchestrationContext.CurrentUtcDateTime
- Pass configuration into your orchestrator function
- Retrieve data in activity functions
> Must be non-blocking

- No I/O to disk or network
- No Thread.Sleep
> Do not initiate async operations

- Except on IDurableOrchestrationContext API
- No Task.Run, Task.Delay, HttpClient.SendAsync
> Do not create infinite loops

- Event history needs to be replayable
- ContinueAsNew should be used instead

## Logging in Orchestrator Functions

-   Use the ILogger interface
-   Log messages get written on every replay

```cs
    log = ctx.CreateReplaySafeLogger(log);
```

## Starter Function

```cs
[FunctionName("ProcessVideoStarter")]
public static async Task<IActionResult> ProcessVideoStarter(
    [HttpTrigger] HttpRequest req,
    [DurableClient] IDurableOrchestrationClient starter)
{
    var video = req.GetQueryParameterDictionary()["video"];
    var instanceId = await starter.StartNewAsync("ProcessVideo", video);
    var payload = starter.CreateHttpManagementPayload(instanceId);
    return new OkObjectResult(payload);
}
```

## Orchestration Function
```cs
[FunctionName("ProcessVideo")]
public static async Task<object> ProcessVideo(
    [OrchestrationTrigger] IDurableOrchestrationContext ctx)
{
    var videoUri = ctx.GetInput<string>();
    var transcodedUri = await ctx.CallActivityAsync<string>("TranscodeVideo", videoUri);
    var thumbnailUri = await ctx.CallActivityAsync<string>("ExtractThumbnail", transcodedUri);
    var withIntroUri = await ctx.CallActivityAsync<string>("PrependIntro", transcodedUri);
    return new { videoUri, thumbnailUri, withIntroUri };
}
```

### Orchestrator Function Rules
- Must be deterministic
- Must be non-blocking
- No async operations
- No infinite loops



## Activity Function

```cs
[FunctionName("TranscodeVideo")]
public static async Task<string> TranscodeVideo(
    [ActivityTrigger] string inputUri)
{
    await Task.Delay(5000); // simulate some work
    return $"{inputUri}-transcoded.mp4";
}
```
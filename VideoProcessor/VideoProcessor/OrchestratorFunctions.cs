using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace VideoProcessor;

public static class OrchestratorFunctions
{
    [FunctionName(nameof(ProcessVideoOrchestrator))]
    public static async Task<object> ProcessVideoOrchestrator(
        [OrchestrationTrigger] IDurableOrchestrationContext context, ILogger log)
    {
        log = context.CreateReplaySafeLogger(log);
        var videoLocation = context.GetInput<string>();

        string transcodedLocation = null;
        string thumbnailLocation = null;
        string withIntroLocation = null;

        try
        {
            //var bitRates = await context.CallActivityAsync<int[]>("GetTranscodeBitrates", null);
            ////var bitRates = new [] { 1000, 2000, 3000, 4000 };

            //var transcodeTasks = new List<Task<VideoFileInfo>>();

            //foreach (var bitRate in bitRates)
            //{
            //    var info = new VideoFileInfo() { Location = videoLocation, BitRate = bitRate };
            //        var task= context.CallActivityAsync<VideoFileInfo>("TranscodeVideo", info);
            //        transcodeTasks.Add(task);
            //    //transcodedLocation = await context.CallActivityAsync<string>("TranscodeVideo", videoLocation);
            //}
            var transcodeResults = await context.CallSubOrchestratorAsync<VideoFileInfo[]>(
                nameof(TranscodeVideoOrchestrator), videoLocation);
           

            transcodedLocation = transcodeResults
                .OrderByDescending(r => r.BitRate)
                .Select(r => r.Location)
                .First();

            log.LogInformation("about to call extract thumbnail activity");
            thumbnailLocation = await context.CallActivityAsync<string>("ExtractThumbnail", transcodedLocation);

            log.LogInformation("about to call prepend intro activity");
            withIntroLocation = await context.CallActivityAsync<string>("PrependIntro", transcodedLocation);


        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        return new
        {
            Transcoded = transcodedLocation,
            Thumbnail = thumbnailLocation,
            WithIntro = withIntroLocation
        };
    }

    [FunctionName(nameof(TranscodeVideoOrchestrator))]
    public static async Task<VideoFileInfo[]> TranscodeVideoOrchestrator(
        [OrchestrationTrigger] IDurableOrchestrationContext context)
    {
        var videoLocation = context.GetInput<string>();
        var bitRates = await context.CallActivityAsync<int[]>("GetTranscodeBitrates", null);
        var transcodeTasks = new List<Task<VideoFileInfo>>();

        foreach (var bitRate in bitRates)
        {
            var info = new VideoFileInfo() { Location = videoLocation, BitRate = bitRate };
            var task = context.CallActivityAsync<VideoFileInfo>("TranscodeVideo", info);
            transcodeTasks.Add(task);
        }
        var transcodeResults = await Task.WhenAll(transcodeTasks);
        return transcodeResults;
    }
}
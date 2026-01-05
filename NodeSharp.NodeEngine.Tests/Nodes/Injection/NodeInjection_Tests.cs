namespace NodeSharp.NodeEngine.Tests.Nodes.Injection;

// public class NodeInjection_Tests
// {
//     private readonly Main test;
//     private const string BaseFilePath = "Nodes\\Injection\\Files";
//     
//     public NodeInjection_Tests()
//     {
//         test = new Main();
//     }
//     
//     [Fact]
//     public async Task Test_BasicFunctionality()
//     {
//         var fileToLoad = $"{BaseFilePath}\\Inject_Basic.json";
//
//         await test.LoadFromFileAsync(fileToLoad);
//
//         await test.Run();
//     }
// }

using System;
using System.Dynamic;
using Newtonsoft.Json;

class Program
{
    public void Main()
    {
        string json = @"{
          ""payload"": {
            ""deviceId"": ""SR4314-F03"",
            ""temperature"": 23.3,
            ""ema"": 23.3
          },
          ""_msgid"": ""7e72d0c68a4535f4""
        }";

        // Deserialize into dynamic ExpandoObject
        dynamic msg = JsonConvert.DeserializeObject<ExpandoObject>(json);

        // Add new fields dynamically
        msg.payload.newField = "hello world";
        msg.payload.calibrationOffset = 1.25;
        msg.extraInfo = "added at runtime";

        // Access them
        Console.WriteLine(msg.payload.newField);        // hello world
        Console.WriteLine(msg.payload.calibrationOffset); // 1.25
        Console.WriteLine(msg.extraInfo);               // added at runtime

        // Serialize back to JSON
        string updatedJson = JsonConvert.SerializeObject(msg, Formatting.Indented);
        Console.WriteLine(updatedJson);
    }
}

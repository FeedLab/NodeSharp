using System.Diagnostics;
using FluentAssertions;
using NodeSharp.NodeEngine.Node;

namespace NodeSharp.NodeEngine.Tests.Nodes.Delay;

public class NodeDelay_Tests
{
    private readonly Main test;
    private const string BaseFilePath = "Nodes\\Delay\\Files";
    
//     public NodeDelay_Tests()
//     {
//         test = new Main();
//     }
//     
//     [Fact]
//     public async Task Test_BasicFunctionality()
//     {
//         var fileToLoad = $"{BaseFilePath}\\Delay_Basic.json";
//
//         await test.LoadFromFileAsync(fileToLoad);
//
//    //     await test.Run();
//
//         var nodeInject = test.FindNodeFromId<NodeInject>("a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d");
//         var nodeDebug = test.FindNodeFromId<NodeDebug>("fdec97b0-c81a-4062-a9c8-76e87c4d6691");
//         var nodeDelay = test.FindNodeFromId<NodeDelay>("04d42414-6992-4201-8d29-da4b1facfbb6");
//
//         nodeInject.Should().NotBeNull("Inject node should be present in the loaded graph");
//         nodeDebug.Should().NotBeNull("Debug node should be present in the loaded graph");
//         nodeDelay.Should().NotBeNull("Delay node should be present in the loaded graph");
//
//         nodeInject!.Id.Should().Be("a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d");
//         nodeDebug!.Id.Should().Be("fdec97b0-c81a-4062-a9c8-76e87c4d6691");
//         nodeDelay!.Id.Should().Be("04d42414-6992-4201-8d29-da4b1facfbb6");
//
//         // Optional: validate the expected wiring (Delay -> Debug)
//         nodeDebug.Inputs.Should().NotBeNull();
//         nodeDebug.Inputs.Should().NotBeEmpty();
//         nodeDebug.Inputs[0].ConnectsToParentNodeId.Should().Contain(nodeDelay.Id);
//         
//         const string delayPayload = "{\"Delay\":{\"Type\":\"Seconds\",\"Value\":2}}";
//
//         var sw = Stopwatch.StartNew();
//         var result = await nodeDelay.RunFromInput(nodeInject, delayPayload);
//         sw.Stop();
//         
//         result.Should().NotBeNullOrEmpty();
//         result.Should().Be(delayPayload);
//         sw.ElapsedMilliseconds.Should().BeGreaterThan(2000);
//     }
//     
//     [Fact]
//     public async Task Test_DelayFromInput_1()
//     {
//         var fileToLoad = $"{BaseFilePath}\\Delay_FromInputJson.json";
//
//         await test.LoadFromFileAsync(fileToLoad);
//
//         // await test.Run();
//
//         var nodeInject = test.FindNodeFromId<NodeInject>("a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d");
//         var nodeDebug = test.FindNodeFromId<NodeDebug>("fdec97b0-c81a-4062-a9c8-76e87c4d6691");
//         var nodeDelay = test.FindNodeFromId<NodeDelay>("04d42414-6992-4201-8d29-da4b1facfbb6");
//
//         nodeInject.Should().NotBeNull("Inject node should be present in the loaded graph");
//         nodeDebug.Should().NotBeNull("Debug node should be present in the loaded graph");
//         nodeDelay.Should().NotBeNull("Delay node should be present in the loaded graph");
//
//         nodeInject!.Id.Should().Be("a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d");
//         nodeDebug!.Id.Should().Be("fdec97b0-c81a-4062-a9c8-76e87c4d6691");
//         nodeDelay!.Id.Should().Be("04d42414-6992-4201-8d29-da4b1facfbb6");
//
//         // Optional: validate the expected wiring (Delay -> Debug)
//         nodeDebug.Inputs.Should().NotBeNull();
//         nodeDebug.Inputs.Should().NotBeEmpty();
//         nodeDebug.Inputs[0].ConnectsToParentNodeId.Should().Contain(nodeDelay.Id);
//         
//         const string delayPayload = "{\"Delay\":{\"Type\":\"Seconds\",\"Value\":1}}";
//
//         var sw = Stopwatch.StartNew();
//         var result = await nodeDelay.RunFromInput(nodeInject, delayPayload);
//         sw.Stop();
//         
//         result.Should().NotBeNullOrEmpty();
//         result.Should().Be(delayPayload);
//         sw.ElapsedMilliseconds.Should().BeGreaterThan(1000);
//     }
//     
//     [Fact]
//     public async Task Test_DelayFromInput_2()
//     {
//         var fileToLoad = $"{BaseFilePath}\\Delay_FromInputJson_2.json";
//
//         await test.LoadFromFileAsync(fileToLoad);
//
// //        await test.Run();
//         
//         var nodeInject = test.FindNodeFromId<NodeInject>("a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d");
//         var nodeDebug = test.FindNodeFromId<NodeDebug>("fdec97b0-c81a-4062-a9c8-76e87c4d6691");
//         var nodeDelay = test.FindNodeFromId<NodeDelay>("04d42414-6992-4201-8d29-da4b1facfbb6");
//
//         nodeInject.Should().NotBeNull("Inject node should be present in the loaded graph");
//         nodeDebug.Should().NotBeNull("Debug node should be present in the loaded graph");
//         nodeDelay.Should().NotBeNull("Delay node should be present in the loaded graph");
//
//         nodeInject!.Id.Should().Be("a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d");
//         nodeDebug!.Id.Should().Be("fdec97b0-c81a-4062-a9c8-76e87c4d6691");
//         nodeDelay!.Id.Should().Be("04d42414-6992-4201-8d29-da4b1facfbb6");
//
//         // Optional: validate the expected wiring (Delay -> Debug)
//         nodeDebug.Inputs.Should().NotBeNull();
//         nodeDebug.Inputs.Should().NotBeEmpty();
//         nodeDebug.Inputs[0].ConnectsToParentNodeId.Should().Contain(nodeDelay.Id);
//         
//         const string delayPayload = "{\"Root\":{\"Delay\":{\"Source\":\"Fixed\",\"Type\":\"Seconds\",\"Value\":2}}}";
//
//         var sw = Stopwatch.StartNew();
//         var result = await nodeDelay.RunFromInput(nodeInject, delayPayload);
//         sw.Stop();
//         
//         result.Should().NotBeNullOrEmpty();
//         result.Should().Be(delayPayload);
//         sw.ElapsedMilliseconds.Should().BeGreaterThan(1000);
//     }
}
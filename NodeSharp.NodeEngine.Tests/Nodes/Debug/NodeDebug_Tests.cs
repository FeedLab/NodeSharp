using FluentAssertions;
using NodeSharp.NodeEngine.Node;

namespace NodeSharp.NodeEngine.Tests.Nodes.Debug;

public class NodeDebugTests
{
    private readonly Main test = new();
    private const string BaseFilePath = @"Nodes\Debug\Files";


    // [Fact]
    // public async Task Test_BasicFunctionality()
    // {
    //     var fileToLoad = $"{BaseFilePath}\\Debug_Basic.json";
    //
    //     await test.LoadFromFileAsync(fileToLoad);
    //
    //     var nodeDebug = test.FindNodeFromId<NodeDebug>("f6e5d4c3-b2a1-4f5e-6d7c-8b9a0e1f2d3c");
    //     var nodeInject = test.FindNodeFromId<NodeInject>("a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d");
    //
    //     nodeDebug.Should().NotBeNull("Debug node should be present in the loaded graph");
    //     nodeInject.Should().NotBeNull("Inject node should be present in the loaded graph");
    //
    //     nodeDebug!.Id.Should().Be("f6e5d4c3-b2a1-4f5e-6d7c-8b9a0e1f2d3c");
    //     nodeInject!.Id.Should().Be("a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d");
    //
    //     // Optional: validate the expected wiring (Inject -> Debug)
    //     nodeDebug.Inputs.Should().NotBeNull();
    //     nodeDebug.Inputs.Should().NotBeEmpty();
    //     nodeDebug.Inputs[0].ConnectsToParentNodeId.Should().Contain(nodeInject.Id);
    //
    //     const string expectedResult = "Hello World!";
    //     var result = await nodeDebug.RunFromInput(nodeInject, expectedResult);
    //
    //     result.Should().NotBeNullOrEmpty();
    //     result.Should().Be(expectedResult);
    // }
}
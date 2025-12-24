namespace NodeSharp.NodeEngine.Tests.Nodes.Injection;

public class NodeInjection_Tests
{
    private readonly Main test;
    private const string BaseFilePath = "Nodes\\Injection\\Files";
    
    public NodeInjection_Tests()
    {
        test = new Main();
    }
    
    [Fact]
    public async Task Test_BasicFunctionality()
    {
        var fileToLoad = $"{BaseFilePath}\\Inject_Basic.json";

        await test.LoadFromFileAsync(fileToLoad);

        await test.Run();
    }
}
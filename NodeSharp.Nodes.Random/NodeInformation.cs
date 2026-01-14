using System.Text;
using NodeSharp.Nodes.Common.Model;
using NodeSharp.Nodes.Random.Component;

namespace NodeSharp.Nodes.Random;

    public class NodePresentationInformation : INodePresentationInformation
    {
        public string OverviewText { get; init; } = "The RandomNumber node generates a random number between a configured Max and Min number";
        public string FontFamilyName { get; init; } = "FontSolid";
        public string Symbol { get; init; } = "\uf566";
    }

    public class NodeInformation : INodeInformation
    {
        private const string DefaultTypeId = "RandomNumber";
        private const string DefaultRuntimeType = "NodeRandomNumber";

        public string TypeId => DefaultTypeId;
        public string RuntimeType => DefaultRuntimeType;
        public bool ActivateOnStart => false;
        public bool IsEnabled => true;
        public int NumberOfInputs => 1;
        public int NumberOfOutputs => 1;

        public bool HasOverviewText => !string.IsNullOrWhiteSpace(PresentationInformation.OverviewText);
        public INodePresentationInformation PresentationInformation { get; init; } = new NodePresentationInformation();

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"{nameof(TypeId)}: {TypeId}");
        sb.AppendLine($"{nameof(ActivateOnStart)}: {ActivateOnStart}");
        sb.AppendLine($"{nameof(IsEnabled)}: {IsEnabled}");
        return sb.ToString();
    }
}
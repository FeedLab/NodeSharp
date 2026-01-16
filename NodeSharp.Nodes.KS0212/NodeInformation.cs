using System.Text;
using NodeSharp.Nodes.Common.Model;

namespace NodeSharp.Nodes.KS0212;

    public class NodePresentationInformation : INodePresentationInformation
    {
        public string OverviewText { get; init; } = "KS0212 is a fully compliant Raspberry Pi HAT designed for seamless integration";
        public string FontFamilyName { get; init; } = "FontSolid";
        public string Symbol { get; init; } = "\uf1de";
    }

    public class NodeInformation : INodeInformation
    {
        private const string DefaultTypeId = "KS0212";
        private const string DefaultRuntimeType = "NodeKS0212";

        public string TypeId => DefaultTypeId;
        public string RuntimeType => DefaultRuntimeType;
        public bool ActivateOnStart => false;
        public bool IsEnabled => true;
        public int NumberOfInputs => 1;
        public int NumberOfOutputs => 6;

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
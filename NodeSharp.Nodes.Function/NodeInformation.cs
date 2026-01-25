using System.Text;
using Microsoft.Maui.Graphics;
using NodeSharp.Nodes.Common.Model;
using NodeSharp.Nodes.Function.Component;

namespace NodeSharp.Nodes.Function;

    public class NodePresentationInformation : INodePresentationInformation
    {
        public string OverviewText { get; init; } = "The Inject node can initiate a flow with a specific payload value";
        public string FontFamilyName { get; init; } = "FontSolid";
        public string Symbol { get; init; } = "\uf669";
    }

    public class NodeInformation : INodeInformation
    {
        private const string DefaultTypeId = "Function";
        private const string DefaultRuntimeType = "NodeFunction";

        public string TypeId => DefaultTypeId;
        public string RuntimeType => DefaultRuntimeType;
        public string Group => "Util";
        public Color Background => Color.FromArgb("#EAF7EE");
        public bool ActivateOnStart => false;
        public bool IsEnabled => true;
        public int NumberOfInputs => 1;
        public int NumberOfOutputs => 1;

        public bool HasOverviewText => !string.IsNullOrWhiteSpace(PresentationInformation.OverviewText);
        public INodePresentationInformation PresentationInformation { get; init; } = new NodePresentationInformation();
    }



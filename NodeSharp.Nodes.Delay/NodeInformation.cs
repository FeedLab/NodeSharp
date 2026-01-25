using System.Text;
using Microsoft.Maui.Graphics;
using NodeSharp.Nodes.Common.Model;
using NodeSharp.Nodes.Delay.Component;

namespace NodeSharp.Nodes.Delay;

    public class NodePresentationInformation : INodePresentationInformation
    {
        public string OverviewText { get; init; } = "Sets the delay, to be applied to the message";
        public string FontFamilyName { get; init; } = "FontSolid";
        public string Symbol { get; init; } = "\uf2f2";
    }

    public class NodeInformation : INodeInformation
    {
        private const string DefaultTypeId = "Delay";
        private const string DefaultRuntimeType = "NodeDelay";

        public string TypeId => DefaultTypeId;
        public string DisplayName => "Delay";
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



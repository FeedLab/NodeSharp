using System.Text;
using Microsoft.Maui.Graphics;
using NodeSharp.Nodes.Common.Model;
using NodeSharp.Nodes.Debug.Component;

namespace NodeSharp.Nodes.Debug;

    public class NodePresentationInformation : INodePresentationInformation
    {
        public string OverviewText { get; init; } = "Displays selected message properties in the debug sidebar";
        public string FontFamilyName { get; init; } = "FontSolid";
        public string Symbol { get; init; } = "\uf717";
    }

    public class NodeInformation : INodeInformation
    {
        private const string DefaultTypeId = "Debug";
        private const string DefaultRuntimeType = "NodeDebug";

        public string TypeId => DefaultTypeId;
        public string RuntimeType => DefaultRuntimeType;
        public string Group => "Core";
        public Color Background => Color.FromUint(0xFFFFEBCD);
        public bool ActivateOnStart => false;
        public bool IsEnabled => true;
        public int NumberOfInputs => 1;
        public int NumberOfOutputs => 0;

        public bool HasOverviewText => !string.IsNullOrWhiteSpace(PresentationInformation.OverviewText);
        public INodePresentationInformation PresentationInformation { get; init; } = new NodePresentationInformation();
    }



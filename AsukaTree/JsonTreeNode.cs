using System.Collections.ObjectModel;

namespace AsukaTree
{
    public sealed class JsonTreeNode
    {
        public string Text { get; init; } = "";
        public string IconKey { get; init; } = "type:default";
        public ObservableCollection<JsonTreeNode> Children { get; } = new();
    }
}

using TombLib.LevelData.VisualScripting;

namespace TombLib.Controls.VisualScripting
{
    public partial class NodeEditor
    {
        /// <summary>
        /// Adds an action node that is already bound to a catalog function.
        /// This keeps effect-box composition on the same graph implementation as the TEN node editor.
        /// </summary>
        public TriggerNode AddFunctionNode(NodeFunction function, bool linkToSelected)
        {
            if (function == null || function.Conditional)
                return null;

            AddActionNode(linkToSelected, false);

            var node = SelectedNode;
            if (node == null)
                return null;

            node.Name = function.Name;
            node.Function = function.Signature;
            node.FixArguments(function);

            UpdateVisibleNodes(true);
            SelectNode(node, false, true);
            ShowNode(node);
            RefreshArgumentUI();
            return node;
        }
    }
}

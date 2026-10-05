using System.Windows.Forms;

namespace XeniaSettings.Utilities
{
    [System.ComponentModel.DesignerCategory("")]
    internal class PatchTreeView : TreeView
    {
        protected override void CreateHandle()
        {
            // TreeView applies checkbox styles after raising HandleCreated; hide headers once creation finishes.
            base.CreateHandle();
            foreach (TreeNode node in Nodes) TreeViewHelper.HideCheckBox(node);
        }

        protected override void WndProc(ref Message message)
        {
            if (message.Msg == 0x0203) // WM_LBUTTONDBLCLK
            {
                int point = message.LParam.ToInt32();
                TreeViewHitTestInfo hit = HitTest((short)(point & 0xFFFF), (short)(point >> 16));
                // A native double-click can change the checkbox without raising AfterCheck.
                if ((hit.Location & TreeViewHitTestLocations.StateImage) != 0)
                    message.Msg = 0x0201; // Use the normal checkbox click path.
            }
            base.WndProc(ref message);
        }
    }
}

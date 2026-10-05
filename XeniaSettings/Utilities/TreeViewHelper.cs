using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace XeniaSettings.Utilities
{
    internal static class TreeViewHelper
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct TreeViewItem
        {
            public uint Mask;
            public IntPtr Item;
            public uint State;
            public uint StateMask;
            public IntPtr Text;
            public int TextLength;
            public int Image;
            public int SelectedImage;
            public int Children;
            public IntPtr Parameter;
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SendMessage(IntPtr window, int message, IntPtr wParam, ref TreeViewItem item);

        public static void HideCheckBox(TreeNode node)
        {
            if (node.TreeView == null || !node.TreeView.IsHandleCreated) return;

            // WinForms applies CheckBoxes to every node. Clear the native state image for game headers.
            var item = new TreeViewItem { Mask = 0x0008, Item = node.Handle, StateMask = 0xF000 };
            SendMessage(node.TreeView.Handle, 0x113F, IntPtr.Zero, ref item); // TVM_SETITEMW
        }

        public static void HighlightNodes(TreeNodeCollection nodes, string searchText)
        {
            foreach (TreeNode node in nodes)
            {
                node.BackColor = Color.Empty;

                if (node.Text.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    node.BackColor = Color.PaleGoldenrod;
                    node.EnsureVisible();
                }

                if (node.Nodes.Count > 0)
                {
                    HighlightNodes(node.Nodes, searchText);
                }
            }
        }

        public static void ResetNodeHighlights(TreeNodeCollection nodes)
        {
            foreach (TreeNode node in nodes)
            {
                node.BackColor = Color.Empty;

                if (node.Nodes.Count > 0)
                {
                    ResetNodeHighlights(node.Nodes);
                }
            }
        }

        public static void ApplySearch(TreeView tree, RichTextBox description, string searchText,
            TreeNode match, Action showSelection)
        {
            tree.CollapseAll();
            ResetNodeHighlights(tree.Nodes);
            if (searchText.Length >= 2)
            {
                HighlightNodes(tree.Nodes, searchText);
                if (match != null)
                {
                    tree.SelectedNode = match;
                    match.EnsureVisible();
                    match.BackColor = Color.PaleGoldenrod;
                }
            }

            showSelection();
            int selectionStart = description.SelectionStart;
            int selectionLength = description.SelectionLength;
            description.SelectAll();
            description.SelectionBackColor = description.BackColor;
            description.Select(selectionStart, selectionLength);
            if (searchText.Length < 2 || match == null) return;

            int start = description.Text.IndexOf(searchText, StringComparison.OrdinalIgnoreCase);
            if (start >= 0)
            {
                description.Select(start, searchText.Length);
                description.SelectionBackColor = Color.PaleGoldenrod;
            }
        }
    }
}

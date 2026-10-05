using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using XeniaSettings.Models;
using XeniaSettings.Utilities;

namespace XeniaSettings.Tests
{
    internal static partial class RegressionTests
    {
        private static void TestApplicationReference()
        {
            Check(typeof(MainForm).Assembly != typeof(RegressionTests).Assembly
                && typeof(ConfigHelper).Assembly == typeof(MainForm).Assembly
                && typeof(PatchHelper).Assembly == typeof(MainForm).Assembly,
                "Regression tests exercise the compiled application assembly.");
        }

        private static void TestPatchBatchSaveFailure()
        {
            string original = "title_name = 'Batch'\ntitle_id = '12345678'\n[[patch]]\nname = 'Toggle'\nis_enabled = false\n";
            string firstPath = Path.Combine(TestDirectory, "batch-first.patch.toml");
            string secondPath = Path.Combine(TestDirectory, "batch-second.patch.toml");
            File.WriteAllText(firstPath, original, new UTF8Encoding(false));
            File.WriteAllText(secondPath, original, new UTF8Encoding(false));
            byte[] originalBytes = File.ReadAllBytes(firstPath);
            PatchFile first = PatchHelper.LoadPatchFile(firstPath);
            PatchFile second = PatchHelper.LoadPatchFile(secondPath);
            first.Patches[0].IsEnabled = true;
            second.Patches[0].IsEnabled = true;
            File.SetAttributes(firstPath, FileAttributes.ReadOnly);
            try
            {
                var errors = PatchHelper.SavePatches(new[] { first, second });
                Check(errors.Count == 1 && errors[0].StartsWith(Path.GetFileName(firstPath) + ":"),
                    "Batch save identifies the patch file that failed.");
                Check(first.IsModified && File.ReadAllBytes(firstPath).SequenceEqual(originalBytes),
                    "Failed patch save retains pending edits and the original file.");
                Check(!second.IsModified && PatchHelper.LoadPatchFile(secondPath).Patches[0].IsEnabled,
                    "Batch save continues with other modified files after a failure.");
                Check(!Directory.GetFiles(TestDirectory, "*.tmp").Any(),
                    "Failed patch replacement leaves no temporary files.");
            }
            finally { File.SetAttributes(firstPath, FileAttributes.Normal); }
            Check(PatchHelper.SavePatches(new[] { first, second }).Count == 0 && !first.IsModified
                && PatchHelper.LoadPatchFile(firstPath).Patches[0].IsEnabled,
                "Batch save can retry the failed file without losing its edits.");
        }

        private static Color BackgroundAt(RichTextBox description, string text)
        {
            int start = description.Text.IndexOf(text, StringComparison.OrdinalIgnoreCase);
            Check(start >= 0, "Search description contains " + text + ".");
            description.Select(start, text.Length);
            return description.SelectionBackColor;
        }

        private static bool HasNoDescriptionHighlight(RichTextBox description)
        {
            if (description.TextLength == 0) return true;
            description.SelectAll();
            return description.SelectionBackColor.ToArgb() == description.BackColor.ToArgb();
        }

        private static void CheckRepeatedSearch(TreeView tree, TextBox search, RichTextBox description,
            TreeNode expected, string firstQuery, string secondQuery, string label)
        {
            search.Text = firstQuery;
            Check(tree.SelectedNode == expected && BackgroundAt(description, firstQuery).ToArgb() == Color.PaleGoldenrod.ToArgb(),
                label + ": search selects and highlights the description match.");
            search.Text = secondQuery;
            Check(tree.SelectedNode == expected && BackgroundAt(description, firstQuery).ToArgb() == description.BackColor.ToArgb()
                && BackgroundAt(description, secondQuery).ToArgb() == Color.PaleGoldenrod.ToArgb(),
                label + ": a new query on the same node clears the old highlight.");
            search.Clear();
            Check(HasNoDescriptionHighlight(description) && expected.BackColor == Color.Empty,
                label + ": clearing search removes description and tree highlights.");
            search.Text = firstQuery;
            search.Text = "query with no matching settings or patches";
            Check(HasNoDescriptionHighlight(description) && expected.BackColor == Color.Empty,
                label + ": an unmatched query clears old description and tree highlighting.");
        }

        private static void TestRepeatedSearch()
        {
            using (var form = new MainForm())
            {
                ShowTestForm(form);
                var tabs = Control<TabControl>(form, "tabControl1");
                var configTree = Control<TreeView>(form, "configTreeView");
                CheckRepeatedSearch(configTree, Control<TextBox>(form, "configSearchTextBox"),
                    Control<RichTextBox>(form, "configDescRichTextBox"), configTree.Nodes[0].Nodes[0],
                    "Audio system", "xaudio2", "Config");
                Check(!Control<ConfigFile>(form, "_config").IsModified,
                    "Searching Config does not modify setting values.");
                tabs.SelectedIndex = 1;
                var patchesTree = Control<TreeView>(form, "patchesTreeView");
                CheckRepeatedSearch(patchesTree, Control<TextBox>(form, "patchesSearchTextBox"),
                    Control<RichTextBox>(form, "patchDescRichTextBox"), patchesTree.Nodes[1].Nodes[3],
                    "protected files", "83610004", "Patches");
                Check(patchesTree.Nodes.Cast<TreeNode>().All(n => !((PatchFile)n.Tag).IsModified),
                    "Searching Patches does not modify patch states.");
            }
        }
    }
}

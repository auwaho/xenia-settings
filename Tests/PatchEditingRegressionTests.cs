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
        private static void TestPatchTextPreservation(Encoding encoding, string newline, string label)
        {
            string path = Path.Combine(TestDirectory, "edit-" + label + ".patch.toml");
            string header = "title_name = 'Test game' # header" + newline + "title_id = '12345678'" + newline;
            string body = "    name = 'Original'" + newline + "    is_enabled = false # toggle" + newline
                + "    [[patch.be8]]" + newline + "        value = 0x01 # 0x00=unlimited; 0x01=60FPS" + newline;
            string neighbor = "[[patch]]" + newline + "name = 'Neighbor'" + newline + "is_enabled = true";
            string original = header + "[[patch]] # keep header" + newline + body + neighbor;
            File.WriteAllText(path, original, encoding);
            byte[] bytes = File.ReadAllBytes(path);
            PatchFile file = PatchHelper.LoadPatchFile(path);
            Patch patch = file.Patches[0];
            string edited = body.Replace("'Original'", "'Edited И'").Replace("false # toggle", "true # toggle").Replace("value = 0x01", "value = 0x00");
            PatchHelper.UpdatePatch(patch, Normalize(edited).TrimEnd('\n'));
            Check(file.IsModified && File.ReadAllBytes(path).SequenceEqual(bytes), label + ": applying patch text stays in memory.");
            Check(patch.Name == "Edited И" && patch.IsEnabled, label + ": patch text reparses name and enabled state.");
            string expected = header + "[[patch]] # keep header" + newline + edited + neighbor;
            Check(file.Content == expected, label + ": editing keeps metadata, adjacent patches and line separators.");
            Check(PatchHelper.SavePatches(new[] { file }).Count == 0 && File.ReadAllBytes(path).SequenceEqual(encoding.GetPreamble().Concat(encoding.GetBytes(expected))),
                label + ": edited text retains encoding, BOM and source newlines.");
            Check(PatchHelper.LoadPatchFile(path).Patches.Count == 2, label + ": saving retains both readable patches.");
            patch.IsEnabled = false;
            Check(patch.Description.Contains("false # toggle"), label + ": checkbox targets the right span after a text edit.");
            string pending = file.Content;
            foreach (string invalid in new[]
            {
                "name = 'Bad'\nis_enabled = invalid",
                "is_enabled = true",
                "name = 'Bad'\nis_enabled = false\n[[patch]]\nname = 'Other'\nis_enabled = false",
                "name = 'Bad'\nis_enabled = false\n[[other]]"
            })
            {
                bool rejected = false;
                try { PatchHelper.UpdatePatch(patch, invalid); }
                catch (FormatException) { rejected = true; }
                Check(rejected && file.Content == pending, label + ": invalid text does not partly change the patch.");
            }
            string mixed = "name = 'Mixed'\r\nis_enabled = false\n[[patch.be8]]\r\nvalue = 0x01";
            File.WriteAllText(path, header + "[[patch]]" + newline + mixed, encoding);
            file = PatchHelper.LoadPatchFile(path);
            PatchHelper.UpdatePatch(file.Patches[0], Normalize(mixed));
            Check(!file.IsModified && file.Patches[0].Description == mixed, label + ": no-op editing retains mixed newlines and absent final newline.");
        }

        private static void TestInlinePatchEditing()
        {
            string path = Directory.GetFiles(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "patches"))
                .First(file => Path.GetFileName(file).Contains("Dead Island"));
            byte[] fixture = File.ReadAllBytes(path);
            try
            {
                using (var form = new TestMainForm())
                {
                    ShowTestForm(form);
                    var tabs = Control<TabControl>(form, "tabControl1");
                    tabs.SelectedIndex = 1;
                    var tree = Control<TreeView>(form, "patchesTreeView");
                    var description = Control<RichTextBox>(form, "patchDescRichTextBox");
                    var menu = Control<ContextMenuStrip>(form, "patchContextMenu");
                    var edit = Control<ToolStripMenuItem>(form, "editPatchToolStripMenuItem");
                    var save = Control<ToolStripMenuItem>(form, "savePatchesToolStripMenuItem");
                    TreeNode game = tree.Nodes[0];
                    TreeNode node = game.Nodes[0];
                    var patch = (Patch)node.Tag;
                    var file = (PatchFile)game.Tag;
                    string original = file.Content;
                    string neighbor = file.Patches[1].Description;
                    game.Expand();
                    Application.DoEvents();
                    tree.SelectedNode = game.Nodes[1];
                    var point = new IntPtr(((node.Bounds.Top + node.Bounds.Height / 2) << 16) | (node.Bounds.Left + 5));
                    PostMessage(tree.Handle, 0x0204, new IntPtr(2), point);
                    PostMessage(tree.Handle, 0x0205, IntPtr.Zero, point);
                    Application.DoEvents();
                    Check(tree.SelectedNode == node && !node.Checked, "Right-click selects a patch without toggling it.");
                    menu.Close();
                    tree.SelectedNode = game;
                    menu.Show(tree, Point.Empty);
                    Check(!menu.Visible && description.ReadOnly, "Game metadata stays read-only and has no Edit menu.");
                    tree.SelectedNode = node;
                    int windows = Application.OpenForms.Count;
                    edit.PerformClick();
                    Check(!description.ReadOnly && description.BackColor == SystemColors.Window && description.Focused && Application.OpenForms.Count == windows,
                        "Edit enables the existing text area without opening a window.");
                    description.Text = description.Text.Replace("value = 0x01", "value = 0x00");
                    string draft = description.Text;
                    var search = Control<TextBox>(form, "patchesSearchTextBox");
                    tree.SelectedNode = game.Nodes[1];
                    search.Text = "value = 0x00";
                    Check(tree.SelectedNode == node && description.Text == draft, "Search finds unsaved patch text without losing it.");
                    search.Clear();
                    tree.SelectedNode = game.Nodes[1];
                    Check(description.ReadOnly && description.BackColor == SystemColors.Control && !description.Text.Contains("value = 0x00"), "Another patch starts in read-only mode.");
                    tree.SelectedNode = node;
                    Check(description.ReadOnly && description.Text == draft, "Returning to a patch retains its unsaved text.");
                    edit.PerformClick();
                    description.Text = description.Text.Replace("value = 0x00", "value = 0x02").Replace("\"60 FPS\"", "\"Custom FPS\"");
                    draft = description.Text;
                    tabs.SelectedIndex = 0;
                    tabs.SelectedIndex = 1;
                    Check(!description.ReadOnly && description.Text == draft, "Editing survives switching Config and Patches tabs.");
                    node.Checked = true;
                    Check(node.Checked && patch.IsEnabled && patch.Description.Contains("value = 0x02")
                        && description.Text.Contains("is_enabled = true") && node.Text == "Custom FPS",
                        "Checkbox preserves edited text and refreshes the patch name.");
                    Check(File.ReadAllBytes(path).SequenceEqual(fixture), "Editing and checkbox changes do not write to disk.");
                    save.PerformClick();
                    Check(description.ReadOnly && description.BackColor == SystemColors.Control && !file.IsModified && form.Errors.Count == 0, "Save patches persists edits and ends editing.");
                    string expected = original.Replace("\"60 FPS\"", "\"Custom FPS\"").Replace("value = 0x01", "value = 0x02");
                    int enabled = expected.IndexOf("is_enabled = false", StringComparison.Ordinal);
                    expected = expected.Remove(enabled, "is_enabled = false".Length).Insert(enabled, "is_enabled = true");
                    Check(File.ReadAllText(path) == expected && file.Patches[1].Description == neighbor,
                        "Inline save preserves the original indentation, comments, metadata and neighboring patch.");
                    byte[] saved = File.ReadAllBytes(path);
                    edit.PerformClick();
                    description.Text = description.Text; // Exercise an unchanged edit.
                    save.PerformClick();
                    Check(File.ReadAllBytes(path).SequenceEqual(saved) && !file.IsModified, "Unchanged inline editing keeps exact bytes.");

                    edit.PerformClick();
                    description.Text = description.Text.Replace("is_enabled = true", "is_enabled = invalid");
                    string invalid = description.Text;
                    tree.SelectedNode = game.Nodes[1];
                    save.PerformClick();
                    Check(form.Errors.Count == 1 && tree.SelectedNode == node && !description.ReadOnly
                        && description.Text == invalid && File.ReadAllBytes(path).SequenceEqual(saved),
                        "Invalid save restores the editable draft and does not write the file.");
                    node.Checked = false;
                    Check(node.Checked && description.Text == invalid, "Invalid draft blocks a checkbox toggle without losing text.");
                    description.Text = invalid.Replace("is_enabled = invalid", "is_enabled = true");
                    save.PerformClick();
                    Check(description.ReadOnly && File.ReadAllBytes(path).SequenceEqual(saved), "Corrected draft can be saved.");

                    edit.PerformClick();
                    description.Text = description.Text.Replace("value = 0x02", "value = 0x00");
                    Control<ToolStripMenuItem>(form, "reloadPatchesToolStripMenuItem").PerformClick();
                    node = tree.Nodes[0].Nodes[0];
                    tree.SelectedNode = node;
                    Check(description.ReadOnly && description.Text.Contains("value = 0x02"), "Reload discards pending text and restores read-only mode.");
                    edit.PerformClick();
                    description.Text = description.Text.Replace("value = 0x02", "value = 0x00");
                    File.AppendAllText(path, "\n# external change");
                    save.PerformClick();
                    Check(!description.ReadOnly && description.Text.Contains("value = 0x00") && File.ReadAllText(path).EndsWith("# external change"),
                        "External-change failure retains editable text without overwriting disk.");
                    File.WriteAllBytes(path, saved);
                    save.PerformClick();
                    Check(description.ReadOnly && File.ReadAllText(path).Contains("value = 0x00"), "Saving can be retried after restoring the original file.");
                }
            }
            finally { File.WriteAllBytes(path, fixture); }
        }
    }
}

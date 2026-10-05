using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using XeniaSettings.Models;

namespace XeniaSettings.Tests
{
    internal static partial class RegressionTests
    {
        private static void PressSaveShortcut(MainForm form)
        {
            var message = Message.Create(form.Handle, 0x0100, new IntPtr((int)Keys.S), IntPtr.Zero);
            bool handled = (bool)typeof(MainForm).GetMethod("ProcessCmdKey", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(form, new object[] { message, Keys.Control | Keys.S });
            Check(handled, "Ctrl+S is handled by the application.");
        }

        private static void TestSaveStateAndShortcuts()
        {
            string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "xenia-canary.config.toml");
            string patchPath = Directory.GetFiles(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "patches"))
                .First(path => Path.GetFileName(path).Contains("Dead Island"));
            byte[] configBytes = File.ReadAllBytes(configPath);
            byte[] patchBytes = File.ReadAllBytes(patchPath);
            try
            {
                using (var form = new TestMainForm())
                {
                    ShowTestForm(form);
                    var tabs = Control<TabControl>(form, "tabControl1");
                    var config = Control<TreeView>(form, "configTreeView");
                    var nodes = config.Nodes.Cast<TreeNode>().SelectMany(n => n.Nodes.Cast<TreeNode>()).ToList();
                    var value = Control<TextBox>(form, "valueTextBox");
                    var number = Control<NumericUpDown>(form, "numericValueInput");
                    var boolean = Control<ComboBox>(form, "booleanValueComboBox");
                    var patches = Control<TreeView>(form, "patchesTreeView");
                    TreeNode patchNode = patches.Nodes[0].Nodes[0];
                    var description = Control<RichTextBox>(form, "patchDescRichTextBox");
                    var edit = Control<ToolStripMenuItem>(form, "editPatchToolStripMenuItem");
                    Check(form.Text == "Xenia Settings", "Unchanged application starts with a clean title.");
                    config.SelectedNode = nodes.First(n => ((Setting)n.Tag).Name == "apu");
                    value.Text = "nop";
                    Check(form.Text.Contains("Unsaved changes"), "String config edit marks the title.");
                    value.Text = "any";
                    Check(form.Text == "Xenia Settings", "Returning a string to its original value clears the title.");
                    config.SelectedNode = nodes.First(n => ((Setting)n.Tag).Name == "ffmpeg_verbose");
                    boolean.SelectedItem = "true";
                    Check(form.Text.Contains("Unsaved changes"), "Boolean config edit marks the title.");
                    boolean.SelectedItem = "false";
                    Check(form.Text == "Xenia Settings", "Returning a boolean to its original value clears the title.");
                    config.SelectedNode = nodes.First(n => ((Setting)n.Tag).Name == "apu_max_queued_frames");
                    if (config.SelectedNode == null) throw new InvalidOperationException("Number fixture not found.");
                    number.Focus();
                    number.Text = "65";
                    Check(form.Text.Contains("Unsaved changes"), "Uncommitted numeric text marks the title.");
                    PressSaveShortcut(form);
                    Check(((Setting)config.SelectedNode.Tag).Value == "65" && File.ReadAllText(configPath).Contains("apu_max_queued_frames = 65")
                        && form.Text == "Xenia Settings", "Ctrl+S commits and saves pending numeric text.");
                    number.Value = 64;
                    Control<ToolStripMenuItem>(form, "reloadConfigToolStripMenuItem").PerformClick();
                    Check(form.Text == "Xenia Settings", "Reload config clears only its pending changes.");

                    config.SelectedNode = config.Nodes.Cast<TreeNode>().SelectMany(n => n.Nodes.Cast<TreeNode>())
                        .First(n => ((Setting)n.Tag).Name == "apu");
                    value.Text = "nop";
                    patchNode.Checked = true;
                    value.Focus();
                    PressSaveShortcut(form);
                    Check(File.ReadAllText(configPath).Contains("apu = \"nop\"")
                        && File.ReadAllBytes(patchPath).SequenceEqual(patchBytes) && form.Text.Contains("Unsaved changes"),
                        "Config Ctrl+S leaves patch edits and their title indicator pending.");
                    tabs.SelectedIndex = 1;
                    patches.SelectedNode = patchNode;
                    patches.Focus();
                    PressSaveShortcut(form);
                    Check(File.ReadAllText(patchPath).Contains("is_enabled = true") && form.Text == "Xenia Settings",
                        "Patches Ctrl+S saves checkboxes and clears the title after both tabs are saved.");

                    edit.PerformClick();
                    Check(form.Text == "Xenia Settings", "Entering Edit alone does not mark the application dirty.");
                    string original = description.Text;
                    description.Text = original.Replace("value = 0x01", "value = 0x00");
                    Check(form.Text.Contains("Unsaved changes"), "Inline patch text marks the title before parsing.");
                    description.Text = original;
                    Check(form.Text == "Xenia Settings", "Undoing an inline patch edit clears the title.");
                    description.Text = original.Replace("is_enabled = true", "is_enabled = invalid");
                    byte[] saved = File.ReadAllBytes(patchPath);
                    description.Focus();
                    PressSaveShortcut(form);
                    Check(form.Errors.Count == 1 && form.Text.Contains("Unsaved changes") && !description.ReadOnly
                        && File.ReadAllBytes(patchPath).SequenceEqual(saved), "Invalid Ctrl+S retains the draft and dirty title.");
                    description.Text = original.Replace("value = 0x01", "value = 0x00");
                    PressSaveShortcut(form);
                    Check(File.ReadAllText(patchPath).Contains("value = 0x00") && description.ReadOnly && form.Text == "Xenia Settings",
                        "Ctrl+S from the patch text area saves corrected text.");
                    patchNode.Checked = false;
                    Check(form.Text.Contains("Unsaved changes"), "Patch checkbox change marks the title.");
                    patchNode.Checked = true;
                    Check(form.Text == "Xenia Settings", "Undoing a checkbox change clears the title.");

                    tabs.SelectedIndex = 0;
                    value.Text = "any";
                    File.SetAttributes(configPath, FileAttributes.ReadOnly);
                    PressSaveShortcut(form);
                    Check(form.Errors.Count == 2 && form.Text.Contains("Unsaved changes"), "Failed config save retains the title indicator.");
                    File.SetAttributes(configPath, FileAttributes.Normal);
                    PressSaveShortcut(form);
                    Check(form.Text == "Xenia Settings" && File.ReadAllText(configPath).Contains("apu = \"any\""),
                        "Retrying Ctrl+S clears the indicator after success.");

                    tabs.SelectedIndex = 1;
                    edit.PerformClick();
                    description.Text = description.Text.Replace("value = 0x00", "value = 0x02");
                    tabs.SelectedIndex = 0;
                    value.Text = "nop";
                    Control<ToolStripMenuItem>(form, "reloadPatchesToolStripMenuItem").PerformClick();
                    Check(form.Text.Contains("Unsaved changes"), "Reload patches retains the config title indicator.");
                    Control<ToolStripMenuItem>(form, "reloadConfigToolStripMenuItem").PerformClick();
                    Check(form.Text == "Xenia Settings", "Reloading both changed tabs clears the title.");
                }

                File.Delete(configPath);
                using (var form = new TestMainForm())
                {
                    ShowTestForm(form);
                    Control<TreeView>(form, "patchesTreeView").Nodes[0].Nodes[0].Checked = false;
                    PressSaveShortcut(form);
                    Check(!File.Exists(configPath) && form.Errors.Count == 0 && form.Text.Contains("Unsaved changes"),
                        "Config Ctrl+S does not save Patches when Config is unavailable.");
                    Control<TabControl>(form, "tabControl1").SelectedIndex = 1;
                    PressSaveShortcut(form);
                    Check(form.Text == "Xenia Settings", "Patches Ctrl+S works while Config is unavailable.");
                }
            }
            finally
            {
                if (File.Exists(configPath)) File.SetAttributes(configPath, FileAttributes.Normal);
                File.WriteAllBytes(configPath, configBytes);
                File.WriteAllBytes(patchPath, patchBytes);
            }
        }
    }
}

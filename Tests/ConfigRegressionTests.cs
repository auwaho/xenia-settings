using System;
using System.Collections.Generic;
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
        private static ConfigFile LoadConfig(string path)
        {
            ConfigFile config;
            string error;
            Check(ConfigHelper.TryLoadConfig(path, out config, out error), "Load test config: " + error);
            return config;
        }

        private static Setting FindSetting(ConfigFile config, string section, string name)
        {
            return config.Sections.Single(s => s.SectionName == section).Settings.Single(s => s.Name == name);
        }

        private static void TestConfigPreservation(Encoding encoding, string newline, string label)
        {
            string path = Path.Combine(TestDirectory, "config-" + label + ".toml");
            string array = "[" + newline + "  { text = ']}#', nested = [1, 2] }, # inside array" + newline + "  3," + newline + "]";
            string basic = "\"\"\"" + newline + "[NotASection]" + newline + "fake = false # text" + newline + "quoted \\\" # И" + newline + "\"\"\"";
            string literal = "'''" + newline + "[AlsoNotASection]" + newline + "fake = 42" + newline + "ends with quotes'''''";
            string original = string.Join(newline, new[]
            {
                "# preserve the preamble",
                "global = 7 # root setting",
                "'quoted key' = 'preserve unknown syntax'",
                "",
                "  [First] # section comment",
                "shared  =  1   # first setting",
                "path = 'C:\\Games\\pack#1' # real comment",
                "escaped = \"a \\\"#b\\\" \\\\ folder\" # actual comment",
                "empty = \"\"",
                "array = " + array + " # array comment",
                "basic = " + basic + " # multiline comment",
                "literal = " + literal + " # literal comment",
                "# continued description",
                "",
                "[Second]",
                "shared = 2 # second setting",
                "last = false" // Exercise a final line without a newline.
            });
            File.WriteAllText(path, original, encoding);
            byte[] originalBytes = File.ReadAllBytes(path);
            ConfigFile config = LoadConfig(path);
            string error;
            Check(!config.IsModified && config.Content == original, label + ": preserve the whole config on load.");
            Check(FindSetting(config, "General", "global").Value == "7", label + ": expose root settings without inserting a section.");
            Check(FindSetting(config, "First", "path").Value == "'C:\\Games\\pack#1'", label + ": hash inside a literal string is part of the value.");
            Check(FindSetting(config, "First", "escaped").Description == "# actual comment", label + ": escaped quotes do not start a comment.");
            Check(FindSetting(config, "First", "empty").Value == "\"\"", label + ": empty quoted string is preserved.");
            Check(FindSetting(config, "First", "array").Value == array, label + ": nested multiline array includes its original comments.");
            Check(FindSetting(config, "First", "basic").Value == basic, label + ": preserve a multiline basic string.");
            Check(FindSetting(config, "First", "literal").Value == literal, label + ": preserve a multiline literal string ending in five quotes.");
            Check(FindSetting(config, "First", "literal").Description.Contains("# continued description"), label + ": continuation comments remain visible.");
            Check(config.Sections.Count == 3 && config.Sections.Sum(s => s.Settings.Count) == 10, label + ": string contents are not parsed as sections or settings.");
            Check(ConfigHelper.TrySaveConfig(config, out error) && File.ReadAllBytes(path).SequenceEqual(originalBytes), label + ": unchanged save preserves exact bytes.");

            Setting first = FindSetting(config, "First", "shared");
            Setting second = FindSetting(config, "Second", "shared");
            first.Value = "12345";
            second.Value = "42";
            string expected = original.Replace("shared  =  1   #", "shared  =  12345   #").Replace("shared = 2 #", "shared = 42 #");
            Check(ConfigHelper.TrySaveConfig(config, out error) && !config.IsModified, label + ": save multiple changed spans.");
            Check(File.ReadAllBytes(path).SequenceEqual(encoding.GetPreamble().Concat(encoding.GetBytes(expected))), label + ": preserve encoding, BOM, line endings and untouched fields.");
            second.Value = "999999";
            first.Value = "1";
            expected = original.Replace("shared = 2 #", "shared = 999999 #");
            Check(ConfigHelper.TrySaveConfig(config, out error) && File.ReadAllText(path) == expected, label + ": repeated save updates span offsets correctly.");
            Setting last = FindSetting(config, "Second", "last");
            last.Value = "true";
            Check(ConfigHelper.TrySaveConfig(config, out error) && File.ReadAllText(path) == expected.Replace("last = false", "last = true"), label + ": edit the final line after multiple saves.");
            second.Value = "2";
            last.Value = "false";
            Check(ConfigHelper.TrySaveConfig(config, out error) && File.ReadAllBytes(path).SequenceEqual(originalBytes), label + ": restore original bytes after repeated saves.");
            first.Value = "0";
            first.Value = "1";
            Check(!config.IsModified, label + ": reverting an unsaved edit is a no-op.");

            // Value validation is deliberately deferred; empty user input must not acquire new restrictions.
            first.Value = "";
            Check(ConfigHelper.TrySaveConfig(config, out error) && File.ReadAllText(path).Contains("shared  =     # first setting"), label + ": save does not introduce value validation.");
            first.Value = "1";
            Check(ConfigHelper.TrySaveConfig(config, out error) && File.ReadAllBytes(path).SequenceEqual(originalBytes), label + ": editing a zero-length span can restore its value.");
        }

        private static void TestConfigErrors()
        {
            string path = Path.Combine(TestDirectory, "config-errors.toml");
            ConfigFile config;
            string error;
            Check(!ConfigHelper.TryLoadConfig(path + ".missing", out config, out error) && config == null && !string.IsNullOrEmpty(error), "Missing config returns an error without exiting.");
            File.WriteAllBytes(path, new byte[] { 0x5B, 0x47, 0x5D, 0x0A, 0xFF });
            Check(!ConfigHelper.TryLoadConfig(path, out config, out error) && config == null, "Invalid UTF-8 reports a load error.");
            File.WriteAllText(path, "[GPU]\nflag = false # original\n", new UTF8Encoding(false));
            config = LoadConfig(path);
            byte[] originalBytes = File.ReadAllBytes(path);
            Setting setting = FindSetting(config, "GPU", "flag");
            setting.Value = "true";
            File.SetAttributes(path, FileAttributes.ReadOnly);
            try
            {
                Check(!ConfigHelper.TrySaveConfig(config, out error) && !string.IsNullOrEmpty(error) && config.IsModified, "Read-only config reports an error and retains edits.");
                Check(File.ReadAllBytes(path).SequenceEqual(originalBytes), "Read-only save preserves the original file.");
            }
            finally { File.SetAttributes(path, FileAttributes.Normal); }

            using (var locked = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                Check(!ConfigHelper.TrySaveConfig(config, out error) && config.IsModified, "Replacement failure returns an error and retains edits.");
                Check(File.ReadAllBytes(path).SequenceEqual(originalBytes), "Replacement failure preserves original bytes.");
            }
            Check(!Directory.GetFiles(TestDirectory, "*.tmp").Any(), "Failed config saves clean up temporary files.");
            using (var locked = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
                ConfigFile failed;
                Check(!ConfigHelper.TryLoadConfig(path, out failed, out error) && failed == null, "Locked config returns a load error.");
            }

            File.AppendAllText(path, "# external edit\n");
            byte[] externalBytes = File.ReadAllBytes(path);
            Check(!ConfigHelper.TrySaveConfig(config, out error) && error.Contains("changed on disk"), "Detect external config edits.");
            Check(config.IsModified && File.ReadAllBytes(path).SequenceEqual(externalBytes), "Conflict preserves disk contents and unsaved model changes.");
            File.Delete(path);
            Check(!ConfigHelper.TrySaveConfig(config, out error) && config.IsModified && !File.Exists(path), "Deleted config is not silently recreated during save.");
            File.WriteAllBytes(path, originalBytes);
            Check(ConfigHelper.TrySaveConfig(config, out error) && !config.IsModified, "Save can be retried after restoring the original file.");
            Check(FindSetting(LoadConfig(path), "GPU", "flag").Value == "true", "Retried save persists the pending value.");
        }

        private class TestMainForm : MainForm
        {
            public readonly List<string> Errors = new List<string>();
            protected override void ShowConfigError(string error, string title) { Errors.Add(title + ": " + error); }
        }

        private static void ShowTestForm(MainForm form)
        {
            form.ShowInTaskbar = false;
            form.StartPosition = FormStartPosition.Manual;
            form.Location = new Point(-32000, -32000);
            form.Show();
            Application.DoEvents();
        }

        private static void TestConfigFormFailures()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "xenia-canary.config.toml");
            byte[] fixture = File.ReadAllBytes(path);
            string source = "[First]\nshared = 1 # first description\narray = [\n 1,\n 2\n]\n[Second]\nshared = 2 # second description\n";
            try
            {
                File.WriteAllText(path, source, new UTF8Encoding(false));
                using (var form = new TestMainForm())
                {
                    ShowTestForm(form);
                    var tree = Control<TreeView>(form, "configTreeView");
                    var value = Control<NumericUpDown>(form, "numericValueInput");
                    var search = Control<TextBox>(form, "configSearchTextBox");
                    var description = Control<RichTextBox>(form, "configDescRichTextBox");
                    var model = Control<ConfigFile>(form, "_config");
                    tree.SelectedNode = tree.Nodes[0].Nodes[1];
                    Check(!model.IsModified && FindSetting(model, "First", "array").Value.Contains("\n"), "Displaying a multiline setting does not modify its source value.");
                    search.Text = "shared";
                    tree.SelectedNode = tree.Nodes[1].Nodes[0];
                    value.Value = 42;
                    Check(FindSetting(model, "Second", "shared").Value == "42" && FindSetting(model, "First", "shared").Value == "1", "Config uses node identity for duplicate setting names.");
                    TreeNode selected = tree.SelectedNode;
                    string previousDescription = description.Text;
                    var patches = Control<TreeView>(form, "patchesTreeView");
                    patches.Nodes[0].Nodes[0].Checked = true;
                    File.Delete(path);
                    Control<ToolStripMenuItem>(form, "reloadConfigToolStripMenuItem").PerformClick();
                    Check(form.Errors.Count == 1 && Control<ConfigFile>(form, "_config") == model, "Failed reload retains the previous config model.");
                    Check(tree.SelectedNode == selected && value.Value == 42 && description.Text == previousDescription && search.Text == "shared", "Failed reload preserves selection, edits, description and search.");
                    Check(((Patch)patches.Nodes[0].Nodes[0].Tag).IsEnabled, "Failed config reload preserves unsaved patch changes.");
                    File.WriteAllText(path, source, new UTF8Encoding(false));
                    File.SetAttributes(path, FileAttributes.ReadOnly);
                    Control<ToolStripMenuItem>(form, "saveConfigToolStripMenuItem").PerformClick();
                    Check(form.Errors.Count == 2 && model.IsModified && File.ReadAllText(path) == source, "Save menu reports errors without losing edits or source data.");
                    File.SetAttributes(path, FileAttributes.Normal);
                    Control<ToolStripMenuItem>(form, "saveConfigToolStripMenuItem").PerformClick();
                    Check(!model.IsModified && File.ReadAllText(path) == source.Replace("shared = 2 #", "shared = 42 #"), "Save menu can retry after a failed write.");
                    value.Value = 43;
                    File.AppendAllText(path, "# external edit\n");
                    Control<ToolStripMenuItem>(form, "saveConfigToolStripMenuItem").PerformClick();
                    Check(form.Errors.Count == 3 && model.IsModified && File.ReadAllText(path).Contains("# external edit"), "Save menu reports an external edit conflict.");
                    Control<ToolStripMenuItem>(form, "reloadConfigToolStripMenuItem").PerformClick();
                    Check(Control<ConfigFile>(form, "_config") != model && !Control<ConfigFile>(form, "_config").IsModified && tree.SelectedNode == null, "Successful reload replaces the model and clears the selection.");
                    Check(((Patch)patches.Nodes[0].Nodes[0].Tag).IsEnabled, "Successful config reload leaves patch changes intact.");
                }

                File.Delete(path);
                using (var form = new TestMainForm())
                {
                    ShowTestForm(form);
                    var tabs = Control<TabControl>(form, "tabControl1");
                    var tree = Control<TreeView>(form, "configTreeView");
                    var description = Control<RichTextBox>(form, "configDescRichTextBox");
                    Check(!tree.Enabled && !Control<ToolStripMenuItem>(form, "saveConfigToolStripMenuItem").Enabled && description.Text.Contains("Could not load config"), "Missing config disables editing and shows a recoverable error.");
                    tabs.SelectedIndex = 1;
                    var patches = Control<TreeView>(form, "patchesTreeView");
                    patches.SelectedNode = patches.Nodes[0].Nodes[0];
                    patches.Nodes[0].Nodes[0].Checked = true;
                    Check(patches.Nodes.Count == 2 && Control<RichTextBox>(form, "patchDescRichTextBox").Text.Contains("is_enabled = true"), "Patches works when config is missing at startup.");
                    tabs.SelectedIndex = 0;
                    Check(description.Text.Contains("Could not load config"), "Switching tabs keeps the config error visible.");
                    File.WriteAllBytes(path, fixture);
                    Control<ToolStripMenuItem>(form, "reloadConfigToolStripMenuItem").PerformClick();
                    Check(tree.Enabled && Control<TextBox>(form, "valueTextBox").Enabled && Control<ToolStripMenuItem>(form, "saveConfigToolStripMenuItem").Enabled && tree.Nodes.Count > 0 && description.Text == "", "Reload activates Config after the missing file is restored.");
                    Check(((Patch)patches.Nodes[0].Nodes[0].Tag).IsEnabled && form.Errors.Count == 0, "Recovering Config preserves patch changes.");
                }
            }
            finally
            {
                if (File.Exists(path)) File.SetAttributes(path, FileAttributes.Normal);
                File.WriteAllBytes(path, fixture);
            }
        }
    }
}

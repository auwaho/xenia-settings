using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using XeniaSettings.Models;
using XeniaSettings.Utilities;

namespace XeniaSettings.Tests
{
    internal static partial class RegressionTests
    {
        private static int _checks;
        private static readonly string TestDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cases");

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr window, int message, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool PostMessage(IntPtr window, int message, IntPtr wParam, IntPtr lParam);

        [STAThread]
        private static int Main(string[] args)
        {
            using (var timeout = new System.Threading.Timer(_ =>
            {
                Console.Error.WriteLine("FAIL: regression checks timed out.");
                Environment.Exit(1);
            }, null, 30000, System.Threading.Timeout.Infinite))
            {
                try
                {
                    Directory.CreateDirectory(TestDirectory);
                    TestApplicationReference();
                    TestFixtures();
                    TestPreservation(new UTF8Encoding(false), "\n", "utf8");
                    TestPreservation(new UTF8Encoding(true), "\r\n", "utf8-bom");
                    TestPreservation(new UnicodeEncoding(false, true), "\r\n", "utf16");
                    TestErrors();
                    TestPatchBatchSaveFailure();
                    TestConfigPreservation(new UTF8Encoding(false), "\n", "utf8");
                    TestConfigPreservation(new UTF8Encoding(true), "\r\n", "utf8-bom");
                    TestConfigPreservation(new UnicodeEncoding(false, true), "\r\n", "utf16");
                    TestConfigErrors();
                    TestForm();
                    TestConfigFormFailures();
                    TestRepeatedSearch();
                    TestSimpleConfigInputs();
                    if (args.Length > 0) TestCorpus(args[0]);
                    Console.WriteLine("PASS: " + _checks + " checks.");
                    return 0;
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                    return 1;
                }
            }
        }

        private static void Check(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
            _checks++;
        }

        private static void TestFixtures()
        {
            List<string> errors;
            List<PatchFile> files = PatchHelper.LoadPatches(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "patches"), out errors);
            Check(errors.Count == 0 && files.Count == 2, "Load both sample games.");
            Check(files.Sum(f => f.Patches.Count) == 6, "Load all six sample patches.");
            Check(files.All(f => !f.IsModified && f.Content == f.OriginalContent), "Untouched samples retain exact text.");
            var forza = files.Single(f => f.TitleId == "4D5309C9");
            Check(forza.Description.Contains("#media_id") && !forza.Description.Contains("[[patch]]"), "Game description stops at the first patch.");
            Check(forza.Patches[0].Description.Contains("[[patch.be32]]") && !forza.Patches[0].Description.Contains("Disable Depth of Field"), "Nested writes belong to the selected patch.");
            Check(forza.Patches[1].Description.Split(new[] { "[[patch.be32]]" }, StringSplitOptions.None).Length == 4, "Keep all nested writes.");
        }

        private static void TestPreservation(Encoding encoding, string newline, string label)
        {
            string path = Path.Combine(TestDirectory, label + ".patch.toml");
            string original = string.Join(newline, new[]
            {
                "title_name = \"Game # \\\"quoted\\\" \\u0418\" # comment",
                "title_id = 'ABCD1234'",
                "# [[patch]] is only a comment",
                "",
                "  [[patch]] # first",
                " name = \"Duplicate\"",
                " author = \"Author\"",
                " is_enabled  =  false # preserve spacing",
                " [[patch.be32]]",
                "   address = 0x12345678",
                "   value = 0x60000000",
                "   is_enabled = false # nested value must stay unchanged",
                "[[patch]]",
                " name = 'Duplicate'",
                " is_enabled = true",
                " [[patch.be8]]",
                "   value = 0x01"
            });
            File.WriteAllText(path, original, encoding);
            byte[] originalBytes = File.ReadAllBytes(path);
            PatchFile file = PatchHelper.LoadPatchFile(path);
            Check(file.TitleName == "Game # \"quoted\" И", label + ": decode quoted metadata.");
            Check(file.Patches.Count == 2 && !file.Patches[0].IsEnabled && file.Patches[1].IsEnabled, label + ": duplicate names have independent states.");
            Check(PatchHelper.SavePatches(new[] { file }).Count == 0 && File.ReadAllBytes(path).SequenceEqual(originalBytes), label + ": no-op save preserves bytes.");
            file.Patches[0].IsEnabled = true;
            Check(file.Patches[0].Description.Contains("is_enabled  =  true #"), label + ": description follows the checkbox.");
            Check(PatchHelper.SavePatches(new[] { file }).Count == 0 && !file.IsModified, label + ": save completes.");
            string expected = original.Replace("is_enabled  =  false #", "is_enabled  =  true #");
            byte[] expectedBytes = encoding.GetPreamble().Concat(encoding.GetBytes(expected)).ToArray();
            Check(File.ReadAllBytes(path).SequenceEqual(expectedBytes), label + ": change only the selected boolean, preserving BOM and line endings.");
            Check(PatchHelper.LoadPatchFile(path).Patches.All(p => p.IsEnabled), label + ": reload saved checkbox states.");
            file.Patches[0].IsEnabled = false;
            Check(PatchHelper.SavePatches(new[] { file }).Count == 0 && File.ReadAllBytes(path).SequenceEqual(originalBytes), label + ": repeated save can restore the original.");
            file.Patches[0].IsEnabled = true;
            file.Patches[0].IsEnabled = false;
            Check(!file.IsModified, label + ": toggling back before save is a no-op.");
            file.Patches[0].IsEnabled = true;
            File.AppendAllText(path, newline + "# external edit", encoding);
            byte[] externalBytes = File.ReadAllBytes(path);
            Check(PatchHelper.SavePatches(new[] { file }).Count == 1 && file.IsModified && File.ReadAllBytes(path).SequenceEqual(externalBytes), label + ": external edits are not overwritten.");
            Check(!Directory.GetFiles(TestDirectory, "*.tmp").Any(), label + ": no temporary files left.");
        }

        private static void TestErrors()
        {
            List<string> errors;
            Check(PatchHelper.LoadPatches(Path.Combine(TestDirectory, "missing"), out errors).Count == 0 && errors.Count == 0, "Missing patches folder is allowed.");
            string directory = Path.Combine(TestDirectory, "mixed");
            Directory.CreateDirectory(directory);
            string header = "title_name = \"Empty\"\ntitle_id = \"12345678\"\n";
            File.WriteAllText(Path.Combine(directory, "good.patch.toml"), header);
            File.WriteAllText(Path.Combine(directory, "bad.patch.toml"), header + "[[patch]]\nname = \"Bad\"\nis_enabled = invalid\n");
            File.WriteAllText(Path.Combine(directory, "duplicate.patch.toml"), header + "[[patch]]\nname = \"Bad\"\nis_enabled = true\nis_enabled = false\n");
            File.WriteAllText(Path.Combine(directory, "ignored.toml"), "Not a patch file");
            var files = PatchHelper.LoadPatches(directory, out errors);
            Check(files.Count == 1 && errors.Count == 2, "Malformed files are reported while valid files remain available.");
            Check(files[0].Patches.Count == 0 && !files[0].IsModified, "A metadata-only patch file is safe.");
        }

        private static T Control<T>(MainForm form, string field) where T : class
        {
            return (T)typeof(MainForm).GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(form);
        }

        private static string Normalize(string value) => value.Replace("\r\n", "\n");

        private static void TestForm()
        {
            Application.EnableVisualStyles();
            string previousDirectory = Environment.CurrentDirectory;
            Environment.CurrentDirectory = TestDirectory;
            try
            {
                using (var form = new MainForm())
                {
                    form.ShowInTaskbar = false;
                    form.StartPosition = FormStartPosition.Manual;
                    form.Location = new Point(-32000, -32000);
                    form.Show();
                    Application.DoEvents();
                    var tabs = Control<TabControl>(form, "tabControl1");
                    var tree = Control<TreeView>(form, "patchesTreeView");
                    var description = Control<RichTextBox>(form, "patchDescRichTextBox");
                    var configDescriptionBox = Control<RichTextBox>(form, "configDescRichTextBox");
                    tabs.SelectedIndex = 1;
                    Application.DoEvents();
                    Check(tree.Nodes.Count == 2, "UI loads files beside the EXE even with another working directory.");
                    TreeNode game = tree.Nodes[1];
                    tree.SelectedNode = game;
                    Check(description.ReadOnly && description.Parent == Control<SplitContainer>(form, "splitContainer3").Panel2, "Patches uses its own read-only description control.");
                    Check(configDescriptionBox.Parent == Control<SplitContainer>(form, "splitContainer2").Panel2, "Config keeps its own description control.");
                    Check(description.Text == Normalize(((PatchFile)game.Tag).Description), "Selecting a game shows all metadata.");
                    Check((SendMessage(tree.Handle, 0x1127, game.Handle, new IntPtr(0xF000)).ToInt64() & 0xF000) == 0, "Game headers have no native checkbox.");
                    game.Checked = true;
                    Check(!game.Checked, "Game headers cannot be toggled.");
                    TreeNode patchNode = game.Nodes[0];
                    tree.SelectedNode = patchNode;
                    string expectedDescription = string.Join("\n", Normalize(((Patch)patchNode.Tag).Description)
                        .Split('\n').Select(line => line.StartsWith("    ") ? line.Substring(4) : line));
                    Check(description.Text == expectedDescription, "Selecting a patch shows its complete body without the common indentation.");
                    patchNode.Checked = true;
                    Check(((Patch)patchNode.Tag).IsEnabled && description.Text.Contains("is_enabled = true"), "Checkbox updates model and description.");
                    Check((SendMessage(tree.Handle, 0x1127, patchNode.Handle, new IntPtr(0xF000)).ToInt64() & 0xF000) == 0x2000, "Patch has a checked native checkbox.");
                    game.Expand();
                    patchNode.EnsureVisible();
                    Application.DoEvents();
                    int y = patchNode.Bounds.Top + patchNode.Bounds.Height / 2;
                    int x = Enumerable.Range(Math.Max(0, patchNode.Bounds.Left - 40), 40)
                        .First(px => (tree.HitTest(px, y).Location & TreeViewHitTestLocations.StateImage) != 0);
                    var point = new IntPtr((y << 16) | x);
                    SendMessage(tree.Handle, 0x0201, new IntPtr(1), point);
                    SendMessage(tree.Handle, 0x0202, IntPtr.Zero, point);
                    Check(!patchNode.Checked && !((Patch)patchNode.Tag).IsEnabled, "Mouse checkbox click updates the model.");
                    SendMessage(tree.Handle, 0x0100, new IntPtr(32), new IntPtr(1));
                    SendMessage(tree.Handle, 0x0101, new IntPtr(32), new IntPtr(unchecked((int)0xC0000001)));
                    Check(patchNode.Checked && ((Patch)patchNode.Tag).IsEnabled, "Space toggles the selected patch.");
                    SendMessage(tree.Handle, 0x0201, new IntPtr(1), point);
                    SendMessage(tree.Handle, 0x0202, IntPtr.Zero, point);
                    PostMessage(tree.Handle, 0x0203, new IntPtr(1), point);
                    PostMessage(tree.Handle, 0x0202, IntPtr.Zero, point);
                    Application.DoEvents();
                    Check(((SendMessage(tree.Handle, 0x1127, patchNode.Handle, new IntPtr(0xF000)).ToInt64() & 0xF000) == 0x2000) == patchNode.Checked,
                        "Double-click leaves the visible checkbox in sync with the model.");
                    patchNode.Checked = true;
                    Check(!((Patch)tree.Nodes[0].Nodes[1].Tag).IsEnabled, "Same patch name in another game stays independent.");
                    Control<ToolStripMenuItem>(form, "savePatchesToolStripMenuItem").PerformClick();
                    Check(PatchHelper.LoadPatchFile(((PatchFile)game.Tag).FilePath).Patches[0].IsEnabled, "Save patches menu persists the checkbox.");
                    patchNode.Checked = false;
                    Control<ToolStripMenuItem>(form, "reloadPatchesToolStripMenuItem").PerformClick();
                    game = tree.Nodes[1];
                    Check(game.Nodes[0].Checked && description.Text == "", "Reload patches discards unsaved edits and stale descriptions.");
                    var search = Control<TextBox>(form, "patchesSearchTextBox");
                    search.Text = "4D5309C9";
                    Check(tree.SelectedNode == game, "Search by title ID.");
                    search.Text = "Disable Depth of Field";
                    Check(tree.SelectedNode == game.Nodes[1], "Search by patch name.");
                    search.Text = "83610004";
                    Check(tree.SelectedNode == game.Nodes[3], "Search in the raw patch description.");
                    search.Clear();
                    tree.SelectedNode = game.Nodes[0];
                    game.Expand();
                    using (var preview = new Bitmap(form.Width, form.Height))
                    {
                        form.DrawToBitmap(preview, new Rectangle(Point.Empty, preview.Size));
                        preview.Save(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "patches-preview.png"));
                    }
                    typeof(System.Windows.Forms.Control).GetMethod("RecreateHandle", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(tree, null);
                    Check((SendMessage(tree.Handle, 0x1127, game.Handle, new IntPtr(0xF000)).ToInt64() & 0xF000) == 0, "Game checkboxes stay hidden after handle recreation.");
                    tabs.SelectedIndex = 0;
                    var config = Control<TreeView>(form, "configTreeView");
                    config.SelectedNode = config.Nodes[0].Nodes[0];
                    var value = Control<TextBox>(form, "valueTextBox");
                    string originalValue = value.Text;
                    value.Text = "nop";
                    string configDescription = configDescriptionBox.Text;
                    string patchDescription = description.Text;
                    tabs.SelectedIndex = 1;
                    tabs.SelectedIndex = 0;
                    Check(value.Text == "nop" && configDescriptionBox.Text == configDescription, "Config edits and descriptions survive switching tabs.");
                    Check(description.Text == patchDescription && description.Parent == Control<SplitContainer>(form, "splitContainer3").Panel2,
                        "Patch description stays independent from Config.");
                    value.Text = originalValue;
                    // Restore only the test fixture changed by the menu test.
                    ((Patch)game.Nodes[0].Tag).IsEnabled = false;
                    Control<ToolStripMenuItem>(form, "savePatchesToolStripMenuItem").PerformClick();
                }
            }
            finally { Environment.CurrentDirectory = previousDirectory; }
        }

        private static void TestSimpleConfigInputs()
        {
            using (var form = new MainForm())
            {
                ShowTestForm(form);
                var tree = Control<TreeView>(form, "configTreeView");
                var model = Control<ConfigFile>(form, "_config");
                var boolean = Control<ComboBox>(form, "booleanValueComboBox");
                var number = Control<NumericUpDown>(form, "numericValueInput");
                var text = Control<TextBox>(form, "valueTextBox");
                var nodes = tree.Nodes.Cast<TreeNode>().SelectMany(n => n.Nodes.Cast<TreeNode>()).ToList();
                foreach (TreeNode node in nodes) tree.SelectedNode = node;
                Check(!model.IsModified, "Displaying every config input retains all original values.");
                tree.SelectedNode = nodes.First(n => ((Setting)n.Tag).Name == "ffmpeg_verbose");
                Check(boolean.Visible && !number.Visible && !text.Visible && boolean.DropDownStyle == ComboBoxStyle.DropDownList && boolean.Items.Count == 2 && (string)boolean.SelectedItem == "false", "Boolean values use a fixed true/false list.");
                boolean.SelectedItem = "true";
                Check(((Setting)tree.SelectedNode.Tag).Value == "true", "Selecting true writes a boolean value.");
                boolean.SelectedItem = "false";
                tree.SelectedNode = nodes.First(n => ((Setting)n.Tag).Name == "apu_max_queued_frames");
                Check(number.Visible && number.DecimalPlaces == 0 && !text.Visible, "Whole numbers use the numeric input.");
                number.Value = -1;
                Check(((Setting)tree.SelectedNode.Tag).Value == "-1", "Numeric input supports negative values.");
                number.Value = 64;
                tree.SelectedNode = nodes.First(n => ((Setting)n.Tag).Name == "postprocess_ffx_fsr_sharpness_reduction");
                Check(number.Visible && number.DecimalPlaces > 0, "Fractional values retain decimal precision.");
                number.Value = 1.5m;
                Check(((Setting)tree.SelectedNode.Tag).Value == "1.5", "Numeric input serializes decimal values with a point.");
                tree.SelectedNode = nodes.First(n => ((Setting)n.Tag).Name == "apu");
                Check(text.Visible && text.Text == "any" && !boolean.Visible && !number.Visible, "Quoted values use the string field without delimiters.");
                text.Text = "a\"b\\c";
                Check(((Setting)tree.SelectedNode.Tag).Value == "\"a\\\"b\\\\c\"", "String input restores quotes and escapes embedded quotes and slashes.");
            }
        }
        private static void TestCorpus(string directory)
        {
            List<string> errors;
            var files = PatchHelper.LoadPatches(directory, out errors);
            foreach (string error in errors) Console.WriteLine(error);
            Check(errors.Count == 0, "All emulator patch files parse successfully.");
            Check(files.Count == Directory.GetFiles(directory, "*.patch.toml").Length, "Load every emulator patch file.");
            Check(files.All(f => !f.IsModified), "Reading emulator files preserves every character.");
            string configPath = Path.Combine(Path.GetDirectoryName(directory.TrimEnd(Path.DirectorySeparatorChar)), "xenia-canary.config.toml");
            if (File.Exists(configPath))
            {
                ConfigFile config;
                string error;
                Check(ConfigHelper.TryLoadConfig(configPath, out config, out error), "Load the emulator config without modifying it.");
                Check(!config.IsModified && config.Content == config.OriginalContent, "Emulator config round-trip preserves every character.");
                Console.WriteLine("Config: " + config.Sections.Count + " sections, " + config.Sections.Sum(s => s.Settings.Count) + " settings (read only).");
            }
            Console.WriteLine("Corpus: " + files.Count + " files, " + files.Sum(f => f.Patches.Count) + " patches (read only).");
        }
    }
}

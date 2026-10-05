using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using XeniaSettings.Models;
using XeniaSettings.Utilities;

namespace XeniaSettings
{
    public partial class MainForm : Form
    {
        private ConfigFile _config;
        private bool _loadingConfigValue;
        private enum ConfigValueKind { String, Boolean, Number }
        private ConfigValueKind _configValueKind;
        private char _configStringQuote;
        private bool _configNumberIsFloat;
        private List<PatchFile> _patchFiles = new List<PatchFile>();
        private bool _loadingPatches;

        public MainForm()
        {
            InitializeComponent();

            ReloadConfig(false);
            ReloadPatches();
        }

        private void ReloadSettingsTreeView()
        {
            configTreeView.Nodes.Clear();
            foreach (SettingsSection section in _config.Sections)
            {
                var sectionNode = new TreeNode(section.SectionName) { Tag = section };
                configTreeView.Nodes.Add(sectionNode);

                foreach (Setting setting in section.Settings)
                {
                    sectionNode.Nodes.Add(new TreeNode(setting.Name) { Tag = setting });
                }
            }
        }

        private void exitToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            Application.Exit();
        }

        private void configSearchTextBox_TextChanged(object sender, EventArgs e)
        {
            string search = configSearchTextBox.Text;
            TreeNode match = null;
            if (search.Length >= 2 && _config != null)
            {
                var settings = _config.Sections.SelectMany(s => s.Settings).ToList();
                Setting setting = settings.FirstOrDefault(s => s.Name.Equals(search, StringComparison.OrdinalIgnoreCase))
                    ?? settings.FirstOrDefault(s => s.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)
                    ?? settings.FirstOrDefault(s => s.Description.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0);
                match = configTreeView.Nodes.Cast<TreeNode>().SelectMany(n => n.Nodes.Cast<TreeNode>())
                    .FirstOrDefault(n => ReferenceEquals(n.Tag, setting));
            }
            TreeViewHelper.ApplySearch(configTreeView, configDescRichTextBox, search, match, ShowSelectedConfigSetting);
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/auwaho/xenia-settings");
        }

        private void reloadConfigToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ReloadConfig(true);
        }

        private void ReloadConfig(bool reportError)
        {
            ConfigFile loaded;
            string error;
            if (!ConfigHelper.TryLoadConfig(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "xenia-canary.config.toml"), out loaded, out error))
            {
                if (_config == null)
                {
                    SetConfigControlsEnabled(false);
                    configDescRichTextBox.Text = "Could not load config:\n" + error + "\n\nRestore the file and select File > Reload config. Patches is still available.";
                }
                if (reportError) ShowConfigError(error, "Could not reload config");
                return;
            }

            configTreeView.SelectedNode = null;
            configDescRichTextBox.Clear();
            configSearchTextBox.Text = string.Empty;
            valueTextBox.Text = string.Empty;
            _config = loaded;
            SetConfigControlsEnabled(true);
            ReloadSettingsTreeView();
        }

        private void SetConfigControlsEnabled(bool enabled)
        {
            configTreeView.Enabled = enabled;
            configSearchTextBox.Enabled = enabled;
            valueTextBox.Enabled = enabled;
            booleanValueComboBox.Enabled = enabled;
            numericValueInput.Enabled = enabled;
            saveConfigToolStripMenuItem.Enabled = enabled;
        }

        protected virtual void ShowConfigError(string error, string title)
        {
            MessageBox.Show(this, error, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void saveConfigToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_config == null) return;
            if (_configValueKind == ConfigValueKind.Number) numericValueInput.Value = numericValueInput.Value;
            string error;
            if (!ConfigHelper.TrySaveConfig(_config, out error)) ShowConfigError(error, "Could not save config");
        }

        private void configTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            ShowSelectedConfigSetting();
        }

        private void ShowSelectedConfigSetting()
        {
            if (tabControl1.SelectedTab != tabPage1 || _config == null) return;
            var setting = configTreeView.SelectedNode?.Tag as Setting;
            configDescRichTextBox.Text = setting?.Description;
            _loadingConfigValue = true;
            try
            {
                string raw = setting?.Value.Trim() ?? string.Empty;
                bool quoted = raw.Length >= 2 && (raw[0] == '"' || raw[0] == '\'') && raw[raw.Length - 1] == raw[0];
                _configValueKind = quoted ? ConfigValueKind.String
                    : raw == "true" || raw == "false" ? ConfigValueKind.Boolean : ConfigValueKind.Number;
                valueTextBox.Visible = setting == null || _configValueKind == ConfigValueKind.String;
                booleanValueComboBox.Visible = setting != null && _configValueKind == ConfigValueKind.Boolean;
                numericValueInput.Visible = setting != null && _configValueKind == ConfigValueKind.Number;
                valueTextBox.Enabled = booleanValueComboBox.Enabled = numericValueInput.Enabled = setting != null;
                if (quoted)
                {
                    _configStringQuote = raw[0];
                    string text = raw.Substring(1, raw.Length - 2);
                    valueTextBox.Text = _configStringQuote == '"' ? Regex.Replace(text, @"\\([""\\])", "$1") : text;
                }
                else if (_configValueKind == ConfigValueKind.Boolean)
                    booleanValueComboBox.SelectedItem = raw;
                else
                {
                    decimal number;
                    decimal.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out number);
                    _configNumberIsFloat = raw.IndexOfAny(new[] { '.', 'e', 'E' }) >= 0;
                    numericValueInput.DecimalPlaces = _configNumberIsFloat ? Math.Max(6, (decimal.GetBits(number)[3] >> 16) & 0xFF) : 0;
                    numericValueInput.Increment = _configNumberIsFloat ? 0.1m : 1m;
                    numericValueInput.Value = number;
                    if (setting == null) valueTextBox.Clear();
                }
            }
            finally { _loadingConfigValue = false; }
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab == tabPage2)
            {
                ShowPatchDescription();
                patchesSearchTextBox.Select();
            }
            else
            {
                ShowSelectedConfigSetting();
                configSearchTextBox.Select();
            }
        }

        private void ReloadPatches()
        {
            List<string> errors;
            _patchFiles = PatchHelper.LoadPatches(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "patches"), out errors);
            _loadingPatches = true;
            patchesTreeView.BeginUpdate();
            try
            {
                patchesTreeView.Nodes.Clear();
                patchesSearchTextBox.Clear();
                foreach (PatchFile file in _patchFiles)
                {
                    var gameNode = new TreeNode(file.TitleName + " (" + file.TitleId + ")") { Tag = file };
                    foreach (Patch patch in file.Patches)
                        gameNode.Nodes.Add(new TreeNode(patch.Name) { Tag = patch, Checked = patch.IsEnabled });
                    patchesTreeView.Nodes.Add(gameNode);
                    TreeViewHelper.HideCheckBox(gameNode);
                }
            }
            finally
            {
                patchesTreeView.EndUpdate();
                _loadingPatches = false;
            }
            if (tabControl1.SelectedTab == tabPage2) ShowPatchDescription();
            ShowPatchErrors(errors, "Some patch files could not be loaded");
        }

        private void ShowPatchErrors(List<string> errors, string title)
        {
            if (errors.Count > 0)
                MessageBox.Show(this, string.Join(Environment.NewLine, errors), title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void reloadPatchesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ReloadPatches();
        }

        private void savePatchesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowPatchErrors(PatchHelper.SavePatches(_patchFiles), "Some patch files could not be saved");
        }

        private void ShowPatchDescription()
        {
            object selected = patchesTreeView.SelectedNode?.Tag;
            var patch = selected as Patch;
            patchDescRichTextBox.Text = patch != null
                ? RemoveCommonIndentation(patch.Description)
                : (selected as PatchFile)?.Description ?? string.Empty;
        }

        private static string RemoveCommonIndentation(string text)
        {
            var lines = Regex.Matches(text, @"^[ \t]*(?=\S)", RegexOptions.Multiline);
            if (lines.Count == 0) return text;

            string indentation = lines[0].Value;
            foreach (Match line in lines)
            {
                while (indentation.Length > 0 && !line.Value.StartsWith(indentation, StringComparison.Ordinal))
                    indentation = indentation.Substring(0, indentation.Length - 1);
                if (indentation.Length == 0) return text;
            }

            // Remove only the shared margin for display; nested indentation and stored text stay intact.
            return Regex.Replace(text, "^" + Regex.Escape(indentation), string.Empty, RegexOptions.Multiline);
        }

        private void patchesTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (tabControl1.SelectedTab == tabPage2) ShowPatchDescription();
        }

        private void patchesTreeView_BeforeCheck(object sender, TreeViewCancelEventArgs e)
        {
            e.Cancel = !(e.Node.Tag is Patch);
        }

        private void patchesTreeView_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (_loadingPatches) return;
            var patch = e.Node.Tag as Patch;
            if (patch == null) return;
            patch.IsEnabled = e.Node.Checked;
            if (tabControl1.SelectedTab == tabPage2 && patchesTreeView.SelectedNode == e.Node)
                ShowPatchDescription();
        }

        private void patchesSearchTextBox_TextChanged(object sender, EventArgs e)
        {
            if (_loadingPatches) return;
            string search = patchesSearchTextBox.Text;
            TreeNode match = null;
            if (search.Length >= 2)
            {
                var nodes = patchesTreeView.Nodes.Cast<TreeNode>()
                    .SelectMany(game => new[] { game }.Concat(game.Nodes.Cast<TreeNode>())).ToList();
                match = nodes.FirstOrDefault(n => n.Text.Equals(search, StringComparison.OrdinalIgnoreCase))
                    ?? nodes.FirstOrDefault(n => n.Text.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)
                    ?? nodes.FirstOrDefault(n => ((n.Tag as PatchFile)?.Description ?? (n.Tag as Patch)?.Description ?? string.Empty)
                        .IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0);
            }
            TreeViewHelper.ApplySearch(patchesTreeView, patchDescRichTextBox, search, match, ShowPatchDescription);
        }

        private void valueTextBox_TextChanged(object sender, EventArgs e)
        {
            UpdateSelectedConfigValue();
        }

        private void configValueInput_Changed(object sender, EventArgs e)
        {
            UpdateSelectedConfigValue();
        }

        private void UpdateSelectedConfigValue()
        {
            if (_loadingConfigValue) return;
            var setting = configTreeView.SelectedNode?.Tag as Setting;
            if (setting == null) return;
            if (_configValueKind == ConfigValueKind.Boolean)
                setting.Value = booleanValueComboBox.SelectedItem?.ToString() ?? setting.Value;
            else if (_configValueKind == ConfigValueKind.Number)
            {
                string number = numericValueInput.Value.ToString(CultureInfo.InvariantCulture);
                setting.Value = _configNumberIsFloat && !number.Contains(".") ? number + ".0" : number;
            }
            else
            {
                string text = valueTextBox.Text;
                setting.Value = _configStringQuote == '\'' && !text.Contains("'") ? "'" + text + "'"
                    : "\"" + text.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t") + "\"";
            }
        }
    }
}

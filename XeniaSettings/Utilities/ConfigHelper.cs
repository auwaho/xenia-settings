using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using XeniaSettings.Models;

namespace XeniaSettings.Utilities
{
    internal static class ConfigHelper
    {
        private static readonly Regex SectionPattern = new Regex(@"^[ \t]*\[(?<name>[^\[\]\r\n]+)\][ \t]*(?:#[^\r\n]*)?\r?$", RegexOptions.Compiled);
        private static readonly Regex SettingPattern = new Regex(@"^[ \t]*(?<name>[A-Za-z0-9_-]+)[ \t]*=[ \t]*", RegexOptions.Compiled);

        public static bool TryLoadConfig(string path, out ConfigFile config, out string error)
        {
            config = null;
            error = null;
            try
            {
                TextFileData data = TextFileStorage.Read(path);

                config = new ConfigFile
                {
                    FilePath = path,
                    Encoding = data.Encoding,
                    OriginalBytes = data.Bytes,
                    OriginalContent = data.Content,
                    Sections = ParseSections(data.Content)
                };
                return true;
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is ArgumentException)
            {
                error = ex.Message;
                return false;
            }
        }

        private static List<SettingsSection> ParseSections(string content)
        {
            var sections = new List<SettingsSection>();
            SettingsSection section = null;
            Setting lastSetting = null;
            int position = 0;
            while (position < content.Length)
            {
                int lineEnd = content.IndexOf('\n', position);
                if (lineEnd < 0) lineEnd = content.Length;
                string line = content.Substring(position, lineEnd - position);
                Match sectionMatch = SectionPattern.Match(line);
                if (sectionMatch.Success)
                {
                    section = new SettingsSection { SectionName = sectionMatch.Groups["name"].Value, Settings = new List<Setting>() };
                    sections.Add(section);
                    lastSetting = null;
                }
                else
                {
                    Match settingMatch = SettingPattern.Match(line);
                    if (settingMatch.Success)
                    {
                        if (section == null)
                        {
                            section = new SettingsSection { SectionName = "General", Settings = new List<Setting>() };
                            sections.Add(section);
                        }
                        int start = position + settingMatch.Length;
                        var comments = new List<string>();
                        int end;
                        position = ReadValue(content, start, comments, out end);
                        string value = content.Substring(start, end - start);
                        lastSetting = new Setting
                        {
                            Name = settingMatch.Groups["name"].Value,
                            Value = value,
                            OriginalValue = value,
                            ValueIndex = start,
                            ValueLength = end - start,
                            Description = string.Join("\n", comments)
                        };
                        section.Settings.Add(lastSetting);
                        continue;
                    }
                    if (lastSetting != null && line.TrimStart().StartsWith("#"))
                    {
                        lastSetting.Description += (lastSetting.Description.Length == 0 ? "" : "\n") + line.Trim();
                    }
                }
                position = lineEnd < content.Length ? lineEnd + 1 : content.Length;
            }
            return sections;
        }

        // Locate a value without interpreting its type. Quotes, comments and nested containers
        // determine its boundary; the original text remains the source for every untouched setting.
        private static int ReadValue(string content, int start, List<string> comments, out int end)
        {
            int depth = 0;
            char quote = '\0';
            bool multiline = false;
            end = start;
            int position = start;
            while (position < content.Length)
            {
                char current = content[position];
                if (quote != '\0')
                {
                    if (quote == '"' && current == '\\')
                    {
                        position = Math.Min(position + 2, content.Length);
                    }
                    else if (current == quote)
                    {
                        int count = 1;
                        while (position + count < content.Length && content[position + count] == quote) count++;
                        if (!multiline)
                        {
                            quote = '\0';
                            position++;
                        }
                        else if (count >= 3)
                        {
                            quote = '\0';
                            position += count;
                        }
                        else position += count;
                    }
                    else if (!multiline && (current == '\r' || current == '\n'))
                    {
                        // Leave value validation to the editor's future validation workflow.
                        break;
                    }
                    else position++;
                    end = position;
                    continue;
                }
                if (current == '#')
                {
                    int commentEnd = content.IndexOf('\n', position);
                    if (commentEnd < 0) commentEnd = content.Length;
                    comments.Add(content.Substring(position, commentEnd - position).TrimEnd());
                    position = commentEnd;
                    if (depth == 0) break;
                    continue;
                }
                if ((current == '\r' || current == '\n') && depth == 0) break;
                if (current == '"' || current == '\'')
                {
                    quote = current;
                    multiline = position + 2 < content.Length && content[position + 1] == quote && content[position + 2] == quote;
                    position += multiline ? 3 : 1;
                    end = position;
                    continue;
                }
                if (current == '[' || current == '{') depth++;
                else if (current == ']' || current == '}') depth = Math.Max(0, depth - 1);
                position++;
                if (!char.IsWhiteSpace(current)) end = position;
            }

            int nextLine = content.IndexOf('\n', position);
            return nextLine < 0 ? content.Length : nextLine + 1;
        }

        public static bool TrySaveConfig(ConfigFile config, out string error)
        {
            error = null;
            if (!config.IsModified) return true;
            string content = config.Content;
            byte[] bytes;
            if (!TextFileStorage.TryWrite(config.FilePath, content, config.Encoding, config.OriginalBytes, out bytes, out error))
                return false;

            config.AcceptChanges(content, bytes);
            return true;
        }
    }
}

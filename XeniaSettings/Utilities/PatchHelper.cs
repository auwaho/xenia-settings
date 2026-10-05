using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using XeniaSettings.Models;

namespace XeniaSettings.Utilities
{
    internal static class PatchHelper
    {
        // Only a root [[patch]] starts a new patch; [[patch.be32]] belongs to its body.
        private static readonly Regex PatchTablePattern = new Regex(
            @"^[ \t]*\[\[patch\]\][ \t]*(?:#[^\r\n]*)?(?:\r?\n|$)", RegexOptions.Multiline);
        private static readonly Regex NestedTablePattern = new Regex(@"^[ \t]*\[", RegexOptions.Multiline);
        private static readonly Regex EnabledPattern = new Regex(
            @"^[ \t]*is_enabled[ \t]*=[ \t]*(?<value>true|false)[ \t]*(?:#[^\r\n]*)?\r?$", RegexOptions.Multiline);

        public static List<PatchFile> LoadPatches(string directory, out List<string> errors)
        {
            var files = new List<PatchFile>();
            errors = new List<string>();
            if (!Directory.Exists(directory)) return files;

            string[] paths;
            try
            {
                paths = Directory.GetFiles(directory, "*.patch.toml");
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                errors.Add(ex.Message);
                return files;
            }

            foreach (string path in paths.OrderBy(p => p, StringComparer.OrdinalIgnoreCase))
            {
                try
                {
                    files.Add(LoadPatchFile(path));
                }
                catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is FormatException || ex is ArgumentException)
                {
                    errors.Add(Path.GetFileName(path) + ": " + ex.Message);
                }
            }

            return files.OrderBy(f => f.TitleName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(f => f.TitleId, StringComparer.OrdinalIgnoreCase).ToList();
        }

        internal static PatchFile LoadPatchFile(string path)
        {
            TextFileData data = TextFileStorage.Read(path);
            string content = data.Content;

            MatchCollection tables = PatchTablePattern.Matches(content);
            string header = tables.Count == 0 ? content : content.Substring(0, tables[0].Index);
            var file = new PatchFile
            {
                FilePath = path,
                TitleName = ReadString(header, "title_name"),
                TitleId = ReadString(header, "title_id"),
                Description = header,
                Encoding = data.Encoding,
                OriginalBytes = data.Bytes,
                OriginalContent = content,
                Patches = new List<Patch>()
            };

            for (int i = 0; i < tables.Count; i++)
            {
                Match table = tables[i];
                int start = table.Index + table.Length;
                int end = i + 1 < tables.Count ? tables[i + 1].Index : content.Length;
                string body = content.Substring(start, end - start);
                file.Patches.Add(ParsePatch(body, table.Value));
            }

            return file;
        }

        private static Patch ParsePatch(string body, string header)
        {
            Match nestedTable = NestedTablePattern.Match(body);
            string metadata = nestedTable.Success ? body.Substring(0, nestedTable.Index) : body;
            MatchCollection enabled = EnabledPattern.Matches(metadata);
            if (enabled.Count != 1)
                throw new FormatException("Expected one is_enabled = true/false field.");
            Group value = enabled[0].Groups["value"];
            return new Patch
            {
                Name = ReadString(metadata, "name"),
                IsEnabled = value.Value == "true",
                TableHeader = header,
                OriginalBody = body,
                EnabledValueIndex = value.Index,
                EnabledValueLength = value.Length
            };
        }

        internal static void UpdatePatch(Patch patch, string body)
        {
            string original = patch.Description;
            body = body.Replace("\r\n", "\n");
            if (body == original.Replace("\r\n", "\n")) return;
            foreach (Match table in NestedTablePattern.Matches(body))
            {
                if (!Regex.IsMatch(body.Substring(table.Index),
                    @"\A[ \t]*\[\[patch\.[A-Za-z0-9_]+\]\][ \t]*(?:#[^\n]*)?(?:\n|$)"))
                    throw new FormatException("Only nested [[patch.type]] tables are allowed inside a patch.");
            }

            // Retain the separator before the next patch and the source newline convention.
            if (original.EndsWith("\n", StringComparison.Ordinal) && !body.EndsWith("\n", StringComparison.Ordinal))
                body += "\n";
            int newline = original.IndexOf('\n');
            if (newline > 0 && original[newline - 1] == '\r') body = body.Replace("\n", "\r\n");
            Patch edited = ParsePatch(body, patch.TableHeader);
            patch.Name = edited.Name;
            patch.IsEnabled = edited.IsEnabled;
            patch.OriginalBody = edited.OriginalBody;
            patch.EnabledValueIndex = edited.EnabledValueIndex;
            patch.EnabledValueLength = edited.EnabledValueLength;
        }

        private static string ReadString(string text, string key)
        {
            string pattern = @"^[ \t]*" + Regex.Escape(key) +
                @"[ \t]*=[ \t]*(?:""(?<basic>(?:\\.|[^""\\\r\n])*)""|'(?<literal>[^'\r\n]*)')[ \t]*(?:#[^\r\n]*)?\r?$";
            MatchCollection matches = Regex.Matches(text, pattern, RegexOptions.Multiline);
            if (matches.Count != 1) throw new FormatException("Expected one quoted " + key + " field.");
            Match match = matches[0];
            if (match.Groups["literal"].Success) return match.Groups["literal"].Value;

            return Regex.Replace(match.Groups["basic"].Value, @"\\(U[0-9a-fA-F]{8}|u[0-9a-fA-F]{4}|.)", escape =>
            {
                string value = escape.Groups[1].Value;
                switch (value)
                {
                    case "b": return "\b";
                    case "t": return "\t";
                    case "n": return "\n";
                    case "f": return "\f";
                    case "r": return "\r";
                    case "\"": return "\"";
                    case "\\": return "\\";
                    default:
                        if (value.StartsWith("u") || value.StartsWith("U"))
                            return char.ConvertFromUtf32(int.Parse(value.Substring(1), NumberStyles.HexNumber, CultureInfo.InvariantCulture));
                        throw new FormatException("Unsupported escape in " + key + ".");
                }
            });
        }

        public static List<string> SavePatches(IEnumerable<PatchFile> files)
        {
            var errors = new List<string>();
            foreach (PatchFile file in files.Where(f => f.IsModified))
            {
                string content = file.Content;
                byte[] bytes;
                string error;
                if (TextFileStorage.TryWrite(file.FilePath, content, file.Encoding, file.OriginalBytes, out bytes, out error))
                {
                    file.OriginalBytes = bytes;
                    file.OriginalContent = content;
                }
                else errors.Add(Path.GetFileName(file.FilePath) + ": " + error);
            }
            return errors;
        }
    }
}

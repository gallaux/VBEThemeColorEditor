using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Xml;

namespace VBEThemeColorEditorLib
{
    public class ThemeEditor
    {
        #region Sequences to look for
        private readonly byte[] PatchFind1 = //1st sequence
        {
            0xff, 0xff, 0xff, 0x00, 0xc0, 0xc0, 0xc0, 0x00, 0x80, 0x80, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0xff, 0x00, 0x00, 0x00, 0x80, 0x00, 0x00, 0x00, 0xff, 0xff, 0x00, 0x00, 0x80, 0x80, 0x00, 0x00, 0x00, 0xff, 0x00, 0x00, 0x00, 0x80, 0x00, 0x00, 0x00, 0xff, 0xff, 0x00, 0x00, 0x80, 0x80, 0x00, 0x00, 0x00, 0xff, 0x00, 0x00, 0x00, 0x80, 0x00, 0xff, 0x00, 0xff, 0x00, 0x80, 0x00, 0x80, 0x00
        };

        private readonly byte[] PatchFind2 = //2nd sequence
        {
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x00, 0x00, 0x80, 0x00, 0x00, 0x00, 0x80, 0x80, 0x00, 0x80, 0x00, 0x00, 0x00, 0x80, 0x00, 0x80, 0x00, 0x80, 0x80, 0x00, 0x00, 0xc0, 0xc0, 0xc0, 0x00, 0x80, 0x80, 0x80, 0x00, 0x00, 0x00, 0xff, 0x00, 0x00, 0xff, 0x00, 0x00, 0x00, 0xff, 0xff, 0x00, 0xff, 0x00, 0x00, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0xff, 0x00, 0x00, 0xff, 0xff, 0xff, 0x00
        };
        #endregion

        #region Variables
        private byte[] PatchFind;
        private int FoundIteration = 0;
        private int FoundSequences = 0;
        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool DetectPatch(byte[] sequence, int position)
        {
            // Look for 1st iteration of the 1st sequence
            // and 2nd iteration of the 2nd sequence

            if (position + PatchFind.Length > sequence.Length)
                return false;
            for (int p = 0; p < PatchFind.Length; p++)
            {
                if (PatchFind[p] != sequence[position + p])
                    return false;
            }

            FoundIteration++;
            if (FoundIteration == 2)
                return false;
            else
            {
                FoundSequences++;
                return true;
            }
        }

        public void PatchFile(string originalFile, byte[] themeColors)
            => PatchFile(originalFile, originalFile + ".BAK", themeColors);

        private void PatchFile(string originalFile, string backupFile, byte[] themeColors)
        {
            // Reset variables
            FoundIteration = 0;
            FoundSequences = 0;

            // Ensure target directory exists.
            var targetDirectory = Path.GetDirectoryName(originalFile);
            if (targetDirectory == null)
                return;
            Directory.CreateDirectory(targetDirectory);

            byte[] fileContent = File.ReadAllBytes(originalFile);

            // Backup original file, unless it has already been done
            if (!File.Exists(backupFile))
            {
                File.WriteAllBytes(backupFile, fileContent);
            }
            else // Load backup file for a clean theme installation
            {
                // Read file bytes.
                fileContent = File.ReadAllBytes(backupFile);
            }

            for (int i = 0; i < 2; i++)
            {
                if (i == 0)
                    // 1st sequence
                    PatchFind = PatchFind1;
                else if (i > 0)
                    // 2nd sequence
                    PatchFind = PatchFind2;

                // Detect and patch file.
                for (int p = 0; p < fileContent.Length; p++)
                {
                    if (!DetectPatch(fileContent, p))
                        continue;

                    for (int w = 0; w < PatchFind.Length; w++)
                    {
                        fileContent[p + w] = themeColors[w];
                    }

                    if (FoundIteration > 2)
                        break;
                }
            }

            if (FoundSequences == 2)
            {
                // Save file
                File.WriteAllBytes(originalFile, fileContent);
            }
            else
            {
                throw new System.Exception("Theme could not be applied");
            }
        }

        public string LoadThemeFromDll(string FileName, byte[] ThemeColors)
        {
            byte[] content = File.ReadAllBytes(FileName);

            byte[] backup;

            if (FileName.EndsWith(".bak", StringComparison.OrdinalIgnoreCase))
                backup = content;
            else if (!File.Exists(FileName + ".BAK"))
                backup = content;
            else
                backup = File.ReadAllBytes(FileName + ".BAK");

            foreach (var pf in new byte[][] { PatchFind1, PatchFind2 })
            {
                PatchFind = pf;

                for (int p = 0; p < content.Length; p++)
                {
                    if (!DetectPatch(backup, p))
                        continue;

                    for (int w = 0; w < PatchFind.Length; w++)
                    {
                        ThemeColors[w] = content[p + w];
                    }

                    if (FoundIteration > 2)
                        break;
                }
            }

            return "Theme loaded: " + FileName;
        }

        public string LoadThemeFromXml(string FileName, byte[] ThemeColors)
        {
            int j = 0;
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(FileName);

            XmlNode nodeTheme = xmlDoc.DocumentElement.SelectSingleNode("/VbeTheme");
            XmlNode nodeThemeColors = nodeTheme.SelectSingleNode("ThemeColors");

            string XMLname = nodeTheme.Attributes["name"]?.InnerText;
            string XMLDescription = nodeTheme.Attributes["desc"]?.InnerText;
            string[] XMLhexColor = new string[16];

            foreach (XmlNode nodeColor in nodeThemeColors)
            {
                XMLhexColor[j] = nodeColor.Attributes["HexColor"]?.InnerText;
                j++;
            }

            // Colors / DLL
            for (int i = 0; i < 16; i++)
            {
                ThemeColors[i * 4] = Convert.ToByte(Int32.Parse(XMLhexColor[i].Substring(0, 2), System.Globalization.NumberStyles.HexNumber));
                ThemeColors[i * 4 + 1] = Convert.ToByte(Int32.Parse(XMLhexColor[i].Substring(2, 2), System.Globalization.NumberStyles.HexNumber));
                ThemeColors[i * 4 + 2] = Convert.ToByte(Int32.Parse(XMLhexColor[i].Substring(4, 2), System.Globalization.NumberStyles.HexNumber));
                ThemeColors[i * 4 + 3] = 0x00;
            }

            return "Theme loaded: " + XMLDescription;
        }
    }
}

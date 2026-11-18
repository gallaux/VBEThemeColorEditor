using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml.Linq;
using VBEThemeColorEditorLib;

namespace VBEThemeColorEditor
{
    public partial class ThemeEditorForm : Form
    {
        #region Preloaded themes
        private readonly byte[] ThemeDefault = //Default VBE Theme
        {
            0xff, 0xff, 0xff, 0x00, 0xc0, 0xc0, 0xc0, 0x00, 0x80, 0x80, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0xff, 0x00, 0x00, 0x00, 0x80, 0x00, 0x00, 0x00, 0xff, 0xff, 0x00, 0x00, 0x80, 0x80, 0x00, 0x00, 0x00, 0xff, 0x00, 0x00, 0x00, 0x80, 0x00, 0x00, 0x00, 0xff, 0xff, 0x00, 0x00, 0x80, 0x80, 0x00, 0x00, 0x00, 0xff, 0x00, 0x00, 0x00, 0x80, 0x00, 0xff, 0x00, 0xff, 0x00, 0x80, 0x00, 0x80, 0x00
        };

        private readonly byte[] ThemeDark = //VS2012 Dark Theme
        {
            0x00, 0x00, 0x00, 0x00, 0x1e, 0x1e, 0x1e, 0x00, 0x34, 0x3a, 0x40, 0x00, 0x3c, 0x42, 0x48, 0x00, 0xd4, 0xd4, 0xd4, 0x00, 0xff, 0xff, 0xff, 0x00, 0x26, 0x4f, 0x78, 0x00, 0x56, 0x9c, 0xd6, 0x00, 0x74, 0xb0, 0xdf, 0x00, 0x79, 0x4e, 0x8b, 0x00, 0x9f, 0x74, 0xb1, 0x00, 0xe5, 0x14, 0x00, 0x00, 0xd6, 0x9d, 0x85, 0x00, 0xce, 0x91, 0x78, 0x00, 0x60, 0x8b, 0x4e, 0x00, 0xb5, 0xce, 0xa8, 0x00
        };
        #endregion

        #region Variables
        ThemeEditor Editor;
        #endregion

        public ThemeEditorForm()
        {
            InitializeComponent();
            Editor = new ThemeEditor();
            UpdateButtonColors(ThemeDefault);
        }




        private void buttonModifyDLL_Click(object sender, EventArgs e)
        {
            // Show the dialog and get result.
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = "VBEx.DLL|VBE7.DLL;VBE6.DLL|VBA6.DLL|VBA6.DLL";
            openFile.InitialDirectory = @"C:\Program Files (x86)\Common Files\Microsoft Shared\VBA\VBA7.1";
            openFile.Title = "Select VBEx.DLL file";

            DialogResult result = openFile.ShowDialog();

            if (result == DialogResult.OK) // Test result.
            {
                string sFileName = openFile.FileName;
                byte[] ThemeColors = LoadColorsFromButtons();

                try
                {
                    Editor.PatchFile(sFileName, ThemeColors);
                    toolStripStatusLabel.Text = "Theme successfully applied";
                }
                catch (IOException)
                {
                    toolStripStatusLabel.Text = "Theme could not be applied";
                    MessageBox.Show("A Microsoft Office Application is opened. Please close it.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    toolStripStatusLabel.Text = ex.Message;
                }
            }
        }

        private void UpdateColor(object sender, EventArgs e)
        {
            var buttonColor = (Button)sender;

            ColorDialog MyColorDialog = new ColorDialog();
            MyColorDialog.Color = buttonColor.BackColor;
            DialogResult pickedColor = MyColorDialog.ShowDialog();

            if (pickedColor == DialogResult.OK)
            {
                // Set button background to the selected color.
                buttonColor.BackColor = MyColorDialog.Color;
            }
        }

        private void UpdateButtonColors(byte[] ThemeTmp)
        {
            for (int i = 1; i < 17; i++)
            {
                Button tmpButton = this.Controls.Find("button" + i.ToString(), true).FirstOrDefault() as Button;
                tmpButton.BackColor = Color.FromArgb
                    (
                        ThemeTmp[(i - 1) * 4],
                        ThemeTmp[(i - 1) * 4 + 1],
                        ThemeTmp[(i - 1) * 4 + 2]
                    );
            }
        }

        private byte[] LoadColorsFromButtons()
        {
            byte[] ThemeColorTmp = new byte[64];

            int tmpButtonID;
            for (int i = 1; i < 17; i++)
            {
                Button tmpButton = this.Controls.Find("button" + i.ToString(), true).FirstOrDefault() as Button;

                tmpButtonID = Int32.Parse(Regex.Match(tmpButton.Name, @"\d+").Value);
                ThemeColorTmp[(i - 1) * 4] = tmpButton.BackColor.R;
                ThemeColorTmp[(i - 1) * 4 + 1] = tmpButton.BackColor.G;
                ThemeColorTmp[(i - 1) * 4 + 2] = tmpButton.BackColor.B;
                ThemeColorTmp[(i - 1) * 4 + 3] = 0x00;
            }

            return ThemeColorTmp;
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Show the dialog and get result.
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = "VBE Theme|*.xml;VBE7.DLL;VBE6.DLL";
            openFile.InitialDirectory = Directory.GetCurrentDirectory();
            openFile.InitialDirectory = openFile.InitialDirectory + @"\Themes";
            openFile.Title = "Select theme";

            DialogResult result = openFile.ShowDialog();

            if (result == DialogResult.OK) // Test result.
            {
                string FileName = openFile.FileName;

                byte[] ThemeColors = new byte[64];

                //try
                //{
                if (FileName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                {
                    toolStripStatusLabel.Text = Editor.LoadThemeFromXml(FileName, ThemeColors);
                }
                else
                {
                    toolStripStatusLabel.Text = Editor.LoadThemeFromDll(FileName, ThemeColors);
                }

                UpdateButtonColors(ThemeColors);
                //}
                //catch
                //{
                //    toolStripStatusLabel.Text = "Theme could not be loaded, invalid file";
                //    MessageBox.Show("Invalid Theme", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //}
            }
        }



        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Show the dialog and get result.
            SaveFileDialog saveFile = new SaveFileDialog();
            saveFile.Filter = "VBE Theme|*.xml";
            saveFile.InitialDirectory = Directory.GetCurrentDirectory();
            saveFile.InitialDirectory = saveFile.InitialDirectory + @"\Themes";
            saveFile.Title = "Save theme";
            saveFile.ShowDialog();

            if (saveFile.FileName.Length > 0) // Test result.
            {
                // Ugly way to write the xml file
                string tmpFileName = Path.GetFileNameWithoutExtension(saveFile.FileName);

                string savedTheme_str =
                @"<?xml version=""1.0""?>
                    <!-- Description: " + tmpFileName + @" -->
                    <VbeTheme name =""" + tmpFileName + @""" desc=""" + tmpFileName + @""">" +
                    @"<ThemeColors>";

                int tmpButtonID;
                string savedColors_str = "";
                for (int i = 1; i < 17; i++)
                {
                    savedColors_str = savedColors_str + @"<Color colorID=""" + i.ToString() + @""" HexColor=""";

                    // Convert button color RGB to Hex
                    Button tmpButton = this.Controls.Find("button" + i.ToString(), true).FirstOrDefault() as Button;

                    tmpButtonID = Int32.Parse(Regex.Match(tmpButton.Name, @"\d+").Value);
                    savedColors_str = savedColors_str + tmpButton.BackColor.R.ToString("X2") + tmpButton.BackColor.G.ToString("X2") + tmpButton.BackColor.B.ToString("X2") + @"""/>";
                }

                savedTheme_str = savedTheme_str + savedColors_str +
                @" </ThemeColors>
                    </VbeTheme>";

                XDocument savedTheme = XDocument.Parse(savedTheme_str);
                savedTheme.Save(saveFile.FileName);

                toolStripStatusLabel.Text = "Theme saved";
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox about = new AboutBox();
            about.Show();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
using CommandLine;
using System;
using System.IO;
using VBEThemeColorEditorLib;

namespace VBEThemeColorEditorCli
{
    class Program
    {
        private class Options
        {
            [Option("Theme", Required = true, HelpText = "Theme to apply.")]
            public string ThemeLocation { get; set; }

            [Option("VBE", Required = true, HelpText = "VBE-Dll to patch with theme.")]
            public string VBELocation { get; set; }
        }

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                   .WithParsed(o =>
                   {
                       if (!File.Exists(o.ThemeLocation))
                           throw new FileNotFoundException("Theme not found", o.ThemeLocation);

                       if (!File.Exists(o.VBELocation))
                           throw new FileNotFoundException("VBE-dll not found", o.VBELocation);


                       var Editor = new ThemeEditor();

                       var ThemeColors = new byte[64];
                       switch (Path.GetExtension(o.ThemeLocation.ToLower()))
                       {
                           case ".dll":
                               Editor.LoadThemeFromDll(o.ThemeLocation, ThemeColors);
                               break;
                           case ".xml":
                               Editor.LoadThemeFromXml(o.ThemeLocation, ThemeColors);
                               break;
                           default:
                               throw new NotImplementedException(Path.GetExtension(o.ThemeLocation.ToLower()));
                       }

                       Editor.PatchFile(o.VBELocation, ThemeColors);
                   });
        }
    }
}

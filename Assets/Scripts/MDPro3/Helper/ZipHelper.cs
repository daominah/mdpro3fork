using System.Collections.Generic;
using System.IO;
using Ionic.Zip;

namespace MDPro3
{
    public class ZipHelper
    {
        public static List<ZipFile> zips = new();

        public static void Initialize()
        {
            Dispose();

            if (!Directory.Exists("Expansions"))
                Directory.CreateDirectory("Expansions");
            foreach (var zip in Directory.GetFiles("Expansions", "*.ypk"))
                zips.Add(new ZipFile(zip));
            foreach (var zip in Directory.GetFiles("Expansions", "*.zip"))
                zips.Add(new ZipFile(zip));

            zips.Add(new ZipFile("Data/script.zip"));//Make "Data/script.zip" the last one to read.
        }

        public static void Dispose()
        {
            foreach (var zip in zips)
                zip.Dispose();
            zips.Clear();
        }

        public static List<string> GetAllCdbTempPath()
        {
            var returnValue = new List<string>();
            foreach (var zip in zips)
            {
                if (zip.Name.ToLower().EndsWith("script.zip"))
                    continue;
                foreach (var file in zip.EntryFileNames)
                {
                    if (file.ToLower().EndsWith(".cdb"))
                    {
                        var e = zip[file];
                        if (!Directory.Exists(Program.PATH_TEMP_FOLDER))
                            Directory.CreateDirectory(Program.PATH_TEMP_FOLDER);
                        var tempFile = Path.Combine(Path.GetFullPath(Program.PATH_TEMP_FOLDER), file);
                        e.Extract(Path.GetFullPath(Program.PATH_TEMP_FOLDER), ExtractExistingFileAction.OverwriteSilently);
                        returnValue.Add(tempFile);
                    }
                }
            }
            return returnValue;
        }
    }
}

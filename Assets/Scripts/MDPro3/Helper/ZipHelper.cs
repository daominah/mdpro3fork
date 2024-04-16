using System.Collections.Generic;
using System.IO;
using Ionic.Zip;

namespace MDPro3
{
    public class ZipHelper
    {
        public static List<ZipFile> zips = new List<ZipFile>();

        public static void Initialize()
        {
            zips.Clear();
            zips.Add(new ZipFile("Data/script.zip"));

            if (!Directory.Exists("Expansions"))
                Directory.CreateDirectory("Expansions");
            foreach (var zip in Directory.GetFiles("Expansions", "*.ypk"))
                zips.Add(new ZipFile(zip));
            foreach (var zip in Directory.GetFiles("Expansions", "*.zip"))
                zips.Add(new ZipFile(zip));
        }
        public static void Dispose()
        {
            foreach (var zip in zips)
                zip.Dispose();
        }
    }
}

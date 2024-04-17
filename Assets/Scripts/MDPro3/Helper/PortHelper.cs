
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SFB;

namespace MDPro3
{
    public class PortHelper
    {
        public static void ImportFiles()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            NativeFilePicker.PickMultipleFiles(MoveFilesToGame, null);
#else
            ChooseFiles();
#endif
        }

        public static void ExportAllDecks()
        {
            if(!Directory.Exists(Program.deckPath))
                Directory.CreateDirectory(Program.deckPath);
            var filePaths = Directory.GetFiles(Program.deckPath);
            Export(filePaths);
        }
        public static void ExportAllReplays()
        {
            if (!Directory.Exists(Program.replayPath))
                Directory.CreateDirectory(Program.replayPath);
            var filePaths = Directory.GetFiles(Program.replayPath);
            Export(filePaths);
        }
        public static void ExportAllPictures()
        {
            if (!Directory.Exists(Program.cardPicPath))
                Directory.CreateDirectory(Program.cardPicPath);
            var filePaths = Directory.GetFiles(Program.cardPicPath);
            Export(filePaths);
        }

        static void Export(string[] filePaths)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            NativeFilePicker.ExportMultipleFiles(filePaths, ExportResult);
#else
            StandaloneFileBrowser.OpenFolderPanelAsync(InterString.Get("请选择导出目录"), "", false, (string[] paths) =>
            {
                ExportFiles(paths, filePaths);
            });
#endif
        }

        private static void ExportFiles(string[] result, string[] filePaths)
        {
            try
            {
                foreach(var file in  filePaths)
                    File.Copy(file, Path.Combine(result.FirstOrDefault(), Path.GetFileName(file)));
                ExportResult(true);
            }
            catch
            {
                ExportResult(false);
            }
        }

        static void ChooseFiles()
        {
            var extensions = new[]
            {
                new ExtensionFilter(InterString.Get("所有文件"), "*"),
                new ExtensionFilter(InterString.Get("卡组码文件"), "ydk"),
                new ExtensionFilter(InterString.Get("回放文件"), "yrp", "yrp3d"),
                new ExtensionFilter(InterString.Get("扩展卡文件"), "ypk"),
                new ExtensionFilter(InterString.Get("数据库文件"), "cdb"),
                new ExtensionFilter(InterString.Get("字段文件"), "conf"),
                new ExtensionFilter(InterString.Get("图片文件"), "png", "jpg")
            };
            StandaloneFileBrowser.OpenFilePanelAsync(InterString.Get("请选择需要导入的文件"), "", extensions, true, (string[] paths) =>
            {
                CopyFilesToGame(paths);
            });
        }

        static void CopyFilesToGame(IEnumerable<string> files)
        {
            bool newDataAdded = false;
            foreach (string path in files)
            {
                var fileName = Path.GetFileName(path);
                try
                {
                    if (path.ToLower().EndsWith(".ydk"))
                    {
                        File.Copy(path, Program.deckPath + Program.slash + fileName, true);
                        MessageManager.Cast(InterString.Get("导入卡组「[?]」成功。", fileName.Replace(".ydk", string.Empty)));
                    }
                    else if (path.ToLower().EndsWith(".yrp") || path.ToLower().EndsWith(".yrp3d"))
                    {
                        File.Copy(path, Program.replayPath + Program.slash + fileName, true);
                        MessageManager.Cast(InterString.Get("导入回放「[?]」成功。", fileName));
                    }
                    else if (path.ToLower().EndsWith(".ypk") || path.ToLower().EndsWith(".zip") || path.ToLower().EndsWith(".cdb") || path.ToLower().EndsWith(".conf"))
                    {
                        File.Copy(path, Program.expansionsPath + Program.slash + fileName, true);
                        newDataAdded = true;
                        if (fileName.ToLower().EndsWith(".ypk") || fileName.ToLower().EndsWith(".zip"))
                            MessageManager.Cast(InterString.Get("导入扩展卡文件「[?]」成功。", fileName));
                        else if (fileName.ToLower().EndsWith(".cdb"))
                            MessageManager.Cast(InterString.Get("导入卡片数据库「[?]」成功。", fileName));
                        else if (fileName.ToLower().EndsWith(".conf"))
                            MessageManager.Cast(InterString.Get("导入字段文件「[?]」成功。", fileName));
                    }
                    else if (path.ToLower().EndsWith(".png") || path.ToLower().EndsWith(".jpg"))
                    {
                        File.Copy(path, Program.altArtPath + Program.slash + Path.GetFileName(path), true);
                        MessageManager.Cast(InterString.Get("导入自定义卡图「[?]」成功。", fileName));
                    }
                }
                catch { }
            }
            if (newDataAdded)
                Program.I().InitializeForDataChange();
        }
        static void MoveFilesToGame(string[] files)
        {
            bool newDataAdded = false;
            foreach (string path in files)
            {
                try
                {
                    if (path.ToLower().EndsWith(".ydk"))
                        File.Move(path, Program.deckPath + Program.slash + Path.GetFileName(path));
                    if (path.ToLower().EndsWith(".yrp") || path.ToLower().EndsWith(".yrp3d"))
                        File.Move(path, Program.replayPath + Program.slash + Path.GetFileName(path));
                    if (path.ToLower().EndsWith(".ypk") || path.ToLower().EndsWith(".zip") || path.ToLower().EndsWith(".cdb") || path.ToLower().EndsWith(".conf"))
                    {
                        File.Move(path, Program.expansionsPath + Program.slash + Path.GetFileName(path));
                        newDataAdded = true;
                    }
                    if (path.ToLower().EndsWith(".png") || path.ToLower().EndsWith(".jpg") || path.ToLower().EndsWith(".jpeg"))
                        File.Move(path, Program.altArtPath + Program.slash + Path.GetFileName(path));
                }
                catch { }
            }
            if (newDataAdded)
                Program.I().InitializeForDataChange();
        }

        static void ExportResult(bool sucess)
        {
            if (sucess)
                MessageManager.Cast(InterString.Get("导出成功。"));
            else
                MessageManager.Cast(InterString.Get("导出失败。"));
        }
    }

}

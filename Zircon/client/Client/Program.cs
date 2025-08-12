using Client.Controls;
using Client.Envir;
using Client.Scenes;
using Client1000.RayDraw;
using Library;
using Raylib_cs;
using Sentry;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace Client
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Console.WriteLine($"当前app路径: {AppInfo.AppPath}");

            ConfigReader.Load(Assembly.GetAssembly(typeof(Config)));

            if (Config.SentryEnabled && !string.IsNullOrEmpty(Config.SentryDSN))
            {
                using (SentrySdk.Init(Config.SentryDSN))
                    Init();
            }
            else
            {
                Init();
            }

            ConfigReader.Save(typeof(Config).Assembly);
        }

        private static void Init()
        {
            Application.EnableVisualStyles();
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.SetCompatibleTextRenderingDefault(false);

            foreach (KeyValuePair<LibraryFile, string> pair in Libraries.LibraryList)
            {
                if (!File.Exists(pair.Value))
                    continue;

                CEnvir.LibraryList[pair.Key] = new MirLibrary(pair.Value);
            }

            //RayFont.LoadFont($"{Config.AppPath}/Data123/Fonts/SourceHanSansSC-Bold.ttf");
            //RayFont.LoadCommChars($"{Config.AppPath}/Data123/Chars/chars3500.txt");

            // 创建窗口
            RayApp app = new RayApp("mir3z", Config.GameSize);

            DXManager.Create();
            DXSoundManager.Create();

            DXControl.ActiveScene = new LoginScene(Config.GameSize);

            app.Run();

            //MessagePump.Run(CEnvir.Target, CEnvir.GameLoop);

            CEnvir.Session?.Save(true);
            CEnvir.Unload();
            DXManager.Unload();
            DXSoundManager.Unload();
        }
    }
}
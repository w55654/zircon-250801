using Client.Controls;
using Client.Envir;
using Client.Scenes;
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

            var form = new Client.TargetForm();   // 为了保持外部引用不改

            DXManager.Create();
            DXSoundManager.Create();

            DXControl.ActiveScene = new LoginScene(Config.IntroSceneSize);

            while (!Raylib.WindowShouldClose())
            {
                form.PumpInput();                 // 轮询输入并转发到 DXControl.ActiveScene
                CEnvir.GameLoop();                // 你的更新
                DXManager.BeginFrame(System.Drawing.Color.Black);
                // 你的渲染逻辑（场景里还是调用 DXManager.SpriteXXX）
                DXManager.PresentToScreen();
            }

            //MessagePump.Run(CEnvir.Target, CEnvir.GameLoop);

            CEnvir.Session?.Save(true);
            CEnvir.Unload();
            DXManager.Unload();
            DXSoundManager.Unload();
        }
    }
}
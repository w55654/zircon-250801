using Client.Controls;
using Client.Envir.Translations;
using Client.Models;
using Client.Scenes;
using Client.Scenes.Views;
using Client.UserModels;
using Library;
using Library.Network;
using Library.SystemModels;
using MirDB;
using Raylib_cs;
using Sentry;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client.Envir
{
    public static class CEnvir
    {
        public static Random Random = new Random();

        private static DateTime _FPSTime;
        private static int FPSCounter;
        private static int FPSCount;

        public static int DPSCounter;
        private static int DPSCount;

        public static bool Shift, Alt, Ctrl;
        public static DateTime Now;
        public static Point MouseLocation;

        public static CConnection Connection;
        public static bool WrongVersion;

        public static Dictionary<LibraryFile, MirLibrary> LibraryList = new Dictionary<LibraryFile, MirLibrary>();

        public static ClientUserItem[] Storage, MainStorage, PartsStorage, MainPartsStorage;

        public static List<ClientBlockInfo> BlockList = new List<ClientBlockInfo>();
        public static DBCollection<KeyBindInfo> KeyBinds;
        public static DBCollection<WindowSetting> WindowSettings;
        public static DBCollection<CastleInfo> CastleInfoList;
        public static MirDB.Session Session;

        public static ConcurrentQueue<string> ChatLog = new ConcurrentQueue<string>();

        public static bool Loaded;
        public static bool Loading { get; private set; }

        public static string BuyAddress;
        public static string C;

        public static bool TestServer;

        public static StringMessages Language { get; set; }

        static CEnvir()
        {
            LoadLanguage();

            Thread workThread = new Thread(SaveChatLoop) { IsBackground = true };
            workThread.Start();

            try
            { A(); }
            catch { }
        }

        public static void CloseWin32()
        {
        }

        public static void LoadLanguage()
        {
            switch (Config.Language.ToUpper())
            {
                case "ENGLISH":
                    Language = (StringMessages)ConfigReader.ConfigObjects[typeof(EnglishMessages)];
                    break;

                case "CHINESE":
                    Language = (StringMessages)ConfigReader.ConfigObjects[typeof(ChineseMessages)];
                    break;
            }
        }

        private static void A()
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "My Games", "Zircon");

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            if (File.Exists(path + "\\CheckSum.bin"))
            {
                using (BinaryReader E = new BinaryReader(File.OpenRead(path + "\\CheckSum.bin")))
                    C = E.ReadString();
            }
            else
            {
                using (BinaryWriter E = new BinaryWriter(File.Create(path + "\\CheckSum.bin")))
                    E.Write(C = Functions.RandomString(Random, 20));
            }
        }

        public static void SaveChatLoop()
        {
            List<string> lines = new List<string>();
            while (true)
            {
                while (ChatLog.IsEmpty)
                    Thread.Sleep(1000);

                while (!ChatLog.IsEmpty)
                {
                    string line;

                    if (!ChatLog.TryDequeue(out line)) continue;

                    lines.Add(line);
                }

                try
                {
                    File.AppendAllLines(@".\Chat Logs.txt", lines);
                    lines.Clear();
                }
                catch
                {
                }
            }
        }

        //public static void GameLoop()
        //{
        //    UpdateGame();
        //    RenderGame();

        //    if (Config.LimitFPS)
        //        Thread.Sleep(1);
        //}

        public static void UpdateGame()
        {
            Now = Time.Now;
            //DXControl.ActiveScene?.OnMouseMove(new MouseEvent(MouseButton.None, 0, MouseLocation.X, MouseLocation.Y, 0));

            if (Time.Now >= _FPSTime)
            {
                _FPSTime = Time.Now.AddSeconds(1);
                FPSCount = FPSCounter;
                FPSCounter = 0;
                DPSCount = DPSCounter;
                DPSCounter = 0;
                DXManager.MemoryClear();
            }

            Connection?.Process();
            DXControl.ActiveScene?.Process();

            string debugText = $"FPS: {FPSCount}";

            if (DXControl.MouseControl != null)
                debugText += $", Mouse Control: {DXControl.MouseControl.GetType().Name}";

            if (DXControl.FocusControl != null)
                debugText += $", Focus Control: {DXControl.FocusControl.GetType().Name}";

            debugText += $", Mouse: {CEnvir.MouseLocation}";

            if (GameScene.Game != null)
            {
                if (DXControl.MouseControl is MapControl)
                    debugText += $", Co Ords: {GameScene.Game.MapControl.MapLocation}";

                debugText += $", Objects: {GameScene.Game.MapControl.Objects.Count}";

                if (MapObject.MouseObject != null)
                    debugText += $", Mouse Object: {MapObject.MouseObject.Name}";
            }
            debugText += $", DPS: {DPSCount}";

            string connectionText = string.Empty;

            if (Connection != null)
            {
                const decimal KB = 1024;
                const decimal MB = KB * 1024;

                string sent, received;

                if (Connection.TotalBytesSent > MB)
                    sent = $"{Connection.TotalBytesSent / MB:#,##0.0}MB";
                else if (Connection.TotalBytesSent > KB)
                    sent = $"{Connection.TotalBytesSent / KB:#,##0}KB";
                else
                    sent = $"{Connection.TotalBytesSent:#,##0}B";

                if (Connection.TotalBytesReceived > MB)
                    received = $"{Connection.TotalBytesReceived / MB:#,##0.0}MB";
                else if (Connection.TotalBytesReceived > KB)
                    received = $"{Connection.TotalBytesReceived / KB:#,##0}KB";
                else
                    received = $"{Connection.TotalBytesReceived:#,##0}B";

                connectionText = $"Ping: {Connection.Ping}, Sent: {sent}, Received: {received}";
            }

            if (Config.FullScreen)
            {
                DXControl.DebugLabel.Text = debugText;

                DXControl.DebugLabel.IsVisible = Config.DebugLabel;
                DXControl.PingLabel.IsVisible = Config.DebugLabel;

                if (Connection != null)
                {
                    DXControl.PingLabel.Text = connectionText;
                    DXControl.PingLabel.Location = new Point(DXControl.DebugLabel.DisplayArea.Right + 5, DXControl.DebugLabel.DisplayArea.Y);
                }
            }
            else
            {
                //CEnvir.Target.Text = $"{Globals.ClientName} - {debugText} - {connectionText}";

                DXControl.PingLabel.Text = string.Empty;
                DXControl.DebugLabel.Text = string.Empty;

                DXControl.DebugLabel.Visible = false;
                DXControl.PingLabel.Visible = false;
            }

            if (DXControl.MouseControl != null && DXControl.ActiveScene != null)
            {
                DXControl.HintLabel.Text = DXControl.MouseControl.Hint;

                var c = DXControl.MouseControl;

                int x;
                int y;

                switch (DXControl.MouseControl.HintPosition)
                {
                    default:
                    case HintPosition.TopLeft:
                        x = c.DisplayArea.Left;
                        y = c.DisplayArea.Top - DXControl.HintLabel.Size.Height;
                        break;

                    case HintPosition.BottomLeft:
                        x = c.DisplayArea.Left;
                        y = c.DisplayArea.Bottom;
                        break;

                    case HintPosition.FixedY:
                        x = MouseLocation.X;
                        y = c.DisplayArea.Top - DXControl.HintLabel.Size.Height;
                        break;

                    case HintPosition.Fluid:
                        x = MouseLocation.X;
                        y = MouseLocation.Y + 17;
                        break;
                }

                Point location = new(x, y);

                if (location.X + DXControl.HintLabel.Size.Width > DXControl.ActiveScene.Size.Width)
                    location.X = DXControl.ActiveScene.Size.Width - DXControl.HintLabel.Size.Width - 1;

                if (location.Y + DXControl.HintLabel.Size.Height > DXControl.ActiveScene.Size.Height)
                    location.Y = DXControl.ActiveScene.Size.Height - DXControl.HintLabel.Size.Height - 1;

                if (location.X < 0) location.X = 0;
                if (location.Y < 0) location.Y = 0;

                DXControl.HintLabel.Location = location;
            }
            else
            {
                DXControl.HintLabel.Text = null;
            }
        }

        public static void RenderUI()
        {
        }

        public static void RenderGame()
        {
            try
            {
                // 你的实际绘制
                DXControl.ActiveScene?.Draw();

                FPSCounter++;
            }
            catch (Exception ex)
            {
                // 没有 Direct3D9Exception 这种鬼东西了，统一收敛到这里
                SaveException(ex);
                // raylib 不需要 AttemptRecovery，留着会误导，别做多余操作
            }
        }

        public static void ReturnToLogin()
        {
            if (DXControl.ActiveScene is LoginScene) return; // handle ?

            DXControl.ActiveScene.Dispose();
            DXSoundManager.StopAllSounds();
            DXControl.ActiveScene = new LoginScene(Config.GameSize);

            BlockList = new List<ClientBlockInfo>();
        }

        public static void LoadDatabase()
        {
            Loading = true;
            Task.Run(() =>
            {
                try
                {
                    Session = new MirDB.Session(SessionMode.Users) { BackUp = false };

                    Session.Initialize(
                        Assembly.GetAssembly(typeof(ItemInfo)),
                        Assembly.GetAssembly(typeof(WindowSetting))
                    );

                    Globals.ItemInfoList = Session.GetCollection<ItemInfo>();
                    Globals.MagicInfoList = Session.GetCollection<MagicInfo>();
                    Globals.MapInfoList = Session.GetCollection<MapInfo>();
                    Globals.CurrencyInfoList = Session.GetCollection<CurrencyInfo>();
                    Globals.InstanceInfoList = Session.GetCollection<InstanceInfo>();
                    Globals.NPCPageList = Session.GetCollection<NPCPage>();
                    Globals.MonsterInfoList = Session.GetCollection<MonsterInfo>();
                    Globals.FishingInfoList = Session.GetCollection<FishingInfo>();
                    Globals.StoreInfoList = Session.GetCollection<StoreInfo>();
                    Globals.NPCInfoList = Session.GetCollection<NPCInfo>();
                    Globals.MovementInfoList = Session.GetCollection<MovementInfo>();
                    Globals.QuestInfoList = Session.GetCollection<QuestInfo>();
                    Globals.QuestTaskList = Session.GetCollection<QuestTask>();
                    Globals.CompanionInfoList = Session.GetCollection<CompanionInfo>();
                    Globals.CompanionLevelInfoList = Session.GetCollection<CompanionLevelInfo>();
                    Globals.DisciplineInfoList = Session.GetCollection<DisciplineInfo>();
                    Globals.FameInfoList = Session.GetCollection<FameInfo>();
                    Globals.BundleInfoList = Session.GetCollection<BundleInfo>();
                    Globals.LootBoxInfoList = Session.GetCollection<LootBoxInfo>();

                    KeyBinds = Session.GetCollection<KeyBindInfo>();
                    WindowSettings = Session.GetCollection<WindowSetting>();
                    CastleInfoList = Session.GetCollection<CastleInfo>();

                    Globals.GoldInfo = Globals.CurrencyInfoList.Binding.First(x => x.Type == CurrencyType.Gold).DropItem;

                    CheckKeyBinds();

                    Loaded = true;
                }
                finally
                {
                    Loading = false;
                }
            });
        }

        public static IEnumerable<KeyBindAction> GetKeyAction(KeyboardKey key)
        {
            if (!Loaded) yield break;

            foreach (KeyBindInfo bind in KeyBinds.Binding)
            {
                if ((bind.Control1 == Ctrl && bind.Alt1 == Alt && bind.Shift1 == Shift && bind.Key1 == key) ||
                    (bind.Control2 == Ctrl && bind.Alt2 == Alt && bind.Shift2 == Shift && bind.Key2 == key))
                    yield return bind.Action;
            }
        }

        public static string GetKeyBindLabel(KeyBindAction action)
        {
            var bind = KeyBinds.Binding.FirstOrDefault(x => x.Action == action);

            if (bind == null)
            {
                return KeyboardKey.Null.ToString();
            }

            string text = "";
            if (bind.Control1)
                text += "Ctrl + ";

            if (bind.Alt1)
                text += "Alt + ";

            if (bind.Shift1)
                text += "Shift + ";

            text += CEnvir.GetText(bind.Key1);

            if (bind.Key2 != KeyboardKey.Null)
            {
                text += ", ";
                if (bind.Control2)
                    text += "Ctrl + ";

                if (bind.Alt2)
                    text += "Alt + ";

                if (bind.Shift2)
                    text += "Shift + ";

                text += CEnvir.GetText(bind.Key2);
            }

            return text;
        }

        public static void FillStorage(List<ClientUserItem> items, bool observer)
        {
            Storage = new ClientUserItem[1000];
            PartsStorage = new ClientUserItem[1000];

            if (!observer)
            {
                MainStorage = Storage;
                MainPartsStorage = PartsStorage;
            }

            foreach (ClientUserItem item in items)
                if (item.Slot >= 2000)
                    PartsStorage[item.Slot - Globals.PartsStorageOffset] = item;
                else
                    Storage[item.Slot] = item;
        }

        public static void Enqueue(Packet packet)
        {
            Connection?.Enqueue(packet);
        }

        public static void ResetKeyBinds()
        {
            for (int i = KeyBinds.Count - 1; i >= 0; i--)
                KeyBinds[i].Delete();

            CheckKeyBinds();
        }

        public static void CheckKeyBinds()
        {
            foreach (KeyBindAction action in Enum.GetValues(typeof(KeyBindAction)).Cast<KeyBindAction>())
            {
                switch (action)
                {
                    case KeyBindAction.None:
                        break;

                    default:
                        if (KeyBinds.Binding.Any(x => x.Action == action)) continue;

                        ResetKeyBind(action);
                        break;
                }
            }
        }

        public static void ResetKeyBind(KeyBindAction action)
        {
            KeyBindInfo bind = KeyBinds.CreateNewObject();
            bind.Action = action;

            switch (action)
            {
                case KeyBindAction.MenuWindow:
                    bind.Category = "Windows";
                    bind.Key1 = KeyboardKey.N;
                    break;

                case KeyBindAction.ConfigWindow:
                    bind.Category = "Windows";
                    bind.Key1 = KeyboardKey.O;
                    break;

                case KeyBindAction.CharacterWindow:
                    bind.Category = "Windows";
                    bind.Key1 = KeyboardKey.Q;
                    break;

                case KeyBindAction.InventoryWindow:
                    bind.Category = "Windows";
                    bind.Key1 = KeyboardKey.W;
                    break;

                case KeyBindAction.MagicWindow:
                    bind.Category = "Windows";
                    bind.Key1 = KeyboardKey.E;
                    break;

                case KeyBindAction.MagicBarWindow:
                    bind.Category = "Windows";
                    bind.Key1 = KeyboardKey.E;
                    bind.Control1 = true;
                    break;

                case KeyBindAction.RankingWindow:
                    bind.Category = "Windows";
                    bind.Key1 = KeyboardKey.R;
                    break;

                case KeyBindAction.GameStoreWindow:
                    bind.Category = "Windows";
                    bind.Key1 = KeyboardKey.Y;
                    break;

                case KeyBindAction.DungeonFinderWindow:
                    bind.Category = "Windows";
                    bind.Key1 = KeyboardKey.D;
                    bind.Shift1 = true;
                    break;

                case KeyBindAction.CompanionWindow:
                    bind.Category = "Windows";
                    bind.Key1 = KeyboardKey.U;
                    break;

                case KeyBindAction.GroupWindow:
                    bind.Category = "Windows";
                    bind.Key1 = KeyboardKey.P;
                    break;

                case KeyBindAction.AutoPotionWindow:
                    bind.Category = "Windows";
                    bind.Key1 = KeyboardKey.A;
                    break;

                case KeyBindAction.StorageWindow:
                    bind.Category = "Windows";
                    bind.Key1 = KeyboardKey.S;
                    break;

                case KeyBindAction.BlockListWindow:
                    bind.Category = "Windows";
                    bind.Key1 = KeyboardKey.F;
                    break;

                case KeyBindAction.GuildWindow:
                    bind.Category = "Windows";
                    bind.Key1 = KeyboardKey.G;
                    break;

                case KeyBindAction.QuestLogWindow:
                    bind.Category = "Windows";
                    bind.Key1 = KeyboardKey.J;
                    break;

                case KeyBindAction.QuestTrackerWindow:
                    bind.Category = "Windows";
                    bind.Key1 = KeyboardKey.L;
                    break;

                case KeyBindAction.BeltWindow:
                    bind.Category = "Windows";
                    bind.Key1 = KeyboardKey.Z;
                    break;

                case KeyBindAction.MarketPlaceWindow:
                    bind.Category = "Windows";
                    bind.Key1 = KeyboardKey.C;
                    break;

                case KeyBindAction.MapMiniWindow:
                    bind.Category = "Windows";
                    bind.Key1 = KeyboardKey.V;
                    break;

                case KeyBindAction.MapBigWindow:
                    bind.Category = "Windows";
                    bind.Key1 = KeyboardKey.B;
                    break;

                case KeyBindAction.MailBoxWindow:
                    bind.Category = "Windows";
                    //bind.Key1 = KeyboardKey.Oemcomma;
                    break;

                case KeyBindAction.MailSendWindow:
                    bind.Category = "Windows";
                    //bind.Key1 = KeyboardKey.OemPeriod;
                    break;

                case KeyBindAction.ChatOptionsWindow:
                    bind.Category = "Windows";
                    bind.Key1 = KeyboardKey.O;
                    bind.Control1 = true;
                    break;

                case KeyBindAction.ExitGameWindow:
                    bind.Category = "Windows";
                    bind.Key1 = KeyboardKey.Q;
                    bind.Alt1 = true;
                    bind.Key2 = KeyboardKey.X;
                    bind.Alt2 = true;
                    break;

                case KeyBindAction.ChangeAttackMode:
                    bind.Category = "Functions";
                    bind.Key1 = KeyboardKey.H;
                    bind.Control1 = true;
                    break;

                case KeyBindAction.ChangePetMode:
                    bind.Category = "Functions";
                    bind.Key1 = KeyboardKey.A;
                    bind.Control1 = true;
                    break;

                case KeyBindAction.GroupAllowSwitch:
                    bind.Category = "Functions";
                    bind.Key1 = KeyboardKey.P;
                    bind.Alt1 = true;
                    break;

                case KeyBindAction.GroupTarget:
                    bind.Category = "Functions";
                    bind.Key1 = KeyboardKey.G;
                    bind.Control1 = true;
                    break;

                case KeyBindAction.TradeRequest:
                    bind.Category = "Functions";
                    bind.Key1 = KeyboardKey.T;
                    break;

                case KeyBindAction.TradeAllowSwitch:
                    bind.Category = "Functions";
                    bind.Key1 = KeyboardKey.T;
                    bind.Control1 = true;
                    break;

                case KeyBindAction.MountToggle:
                    bind.Category = "Functions";
                    bind.Key1 = KeyboardKey.M;
                    break;

                case KeyBindAction.AutoRunToggle:
                    bind.Category = "Functions";
                    bind.Key1 = KeyboardKey.D;
                    break;

                case KeyBindAction.ChangeChatMode:
                    bind.Category = "Functions";
                    bind.Key1 = KeyboardKey.K;
                    break;

                case KeyBindAction.ItemPickUp:
                    bind.Category = "Items";
                    bind.Key1 = KeyboardKey.Tab;
                    break;

                case KeyBindAction.PartnerTeleport:
                    bind.Category = "Items";
                    bind.Key1 = KeyboardKey.Z;
                    bind.Shift1 = true;
                    break;

                case KeyBindAction.ToggleItemLock:
                    bind.Category = "Items";
                    bind.Key1 = KeyboardKey.ScrollLock;
                    break;

                case KeyBindAction.UseBelt01:
                    bind.Category = "Items";
                    bind.Key1 = KeyboardKey.Kp1;
                    bind.Key2 = KeyboardKey.Kp1;
                    bind.Shift2 = true;
                    break;

                case KeyBindAction.UseBelt02:
                    bind.Category = "Items";
                    bind.Key1 = KeyboardKey.Kp2;
                    bind.Key2 = KeyboardKey.Kp2;
                    bind.Shift2 = true;
                    break;

                case KeyBindAction.UseBelt03:
                    bind.Category = "Items";
                    bind.Key1 = KeyboardKey.Kp3;
                    bind.Key2 = KeyboardKey.Kp3;
                    bind.Shift2 = true;
                    break;

                case KeyBindAction.UseBelt04:
                    bind.Category = "Items";
                    bind.Key1 = KeyboardKey.Kp4;
                    bind.Key2 = KeyboardKey.Kp4;
                    bind.Shift2 = true;
                    break;

                case KeyBindAction.UseBelt05:
                    bind.Category = "Items";
                    bind.Key1 = KeyboardKey.Kp5;
                    bind.Key2 = KeyboardKey.Kp5;
                    bind.Shift2 = true;
                    break;

                case KeyBindAction.UseBelt06:
                    bind.Category = "Items";
                    bind.Key1 = KeyboardKey.Kp6;
                    bind.Key2 = KeyboardKey.Kp6;
                    bind.Shift2 = true;
                    break;

                case KeyBindAction.UseBelt07:
                    bind.Category = "Items";
                    bind.Key1 = KeyboardKey.Kp7;
                    bind.Key2 = KeyboardKey.Kp7;
                    bind.Shift2 = true;
                    break;

                case KeyBindAction.UseBelt08:
                    bind.Category = "Items";
                    bind.Key1 = KeyboardKey.Kp8;
                    bind.Key2 = KeyboardKey.Kp8;
                    bind.Shift2 = true;
                    break;

                case KeyBindAction.UseBelt09:
                    bind.Category = "Items";
                    bind.Key1 = KeyboardKey.Kp9;
                    bind.Key2 = KeyboardKey.Kp9;
                    bind.Shift2 = true;
                    break;

                case KeyBindAction.UseBelt10:
                    bind.Category = "Items";
                    bind.Key1 = KeyboardKey.Kp0;
                    bind.Key2 = KeyboardKey.Kp0;
                    bind.Shift2 = true;
                    break;

                case KeyBindAction.SpellSet01:
                    bind.Category = "Magic";
                    bind.Key1 = KeyboardKey.F1;
                    bind.Control1 = true;
                    break;

                case KeyBindAction.SpellSet02:
                    bind.Category = "Magic";
                    bind.Key1 = KeyboardKey.F2;
                    bind.Control1 = true;
                    break;

                case KeyBindAction.SpellSet03:
                    bind.Category = "Magic";
                    bind.Key1 = KeyboardKey.F3;
                    bind.Control1 = true;
                    break;

                case KeyBindAction.SpellSet04:
                    bind.Category = "Magic";
                    bind.Key1 = KeyboardKey.F4;
                    bind.Control1 = true;
                    break;

                case KeyBindAction.SpellUse01:
                    bind.Category = "Magic";
                    bind.Key1 = KeyboardKey.F1;
                    bind.Key2 = KeyboardKey.F1;
                    bind.Shift2 = true;
                    break;

                case KeyBindAction.SpellUse02:
                    bind.Category = "Magic";
                    bind.Key1 = KeyboardKey.F2;
                    bind.Key2 = KeyboardKey.F2;
                    bind.Shift2 = true;
                    break;

                case KeyBindAction.SpellUse03:
                    bind.Category = "Magic";
                    bind.Key1 = KeyboardKey.F3;
                    bind.Key2 = KeyboardKey.F3;
                    bind.Shift2 = true;
                    break;

                case KeyBindAction.SpellUse04:
                    bind.Category = "Magic";
                    bind.Key1 = KeyboardKey.F4;
                    bind.Key2 = KeyboardKey.F4;
                    bind.Shift2 = true;
                    break;

                case KeyBindAction.SpellUse05:
                    bind.Category = "Magic";
                    bind.Key1 = KeyboardKey.F5;
                    bind.Key2 = KeyboardKey.F5;
                    bind.Shift2 = true;
                    break;

                case KeyBindAction.SpellUse06:
                    bind.Category = "Magic";
                    bind.Key1 = KeyboardKey.F6;
                    bind.Key2 = KeyboardKey.F6;
                    bind.Shift2 = true;
                    break;

                case KeyBindAction.SpellUse07:
                    bind.Category = "Magic";
                    bind.Key1 = KeyboardKey.F7;
                    bind.Key2 = KeyboardKey.F7;
                    bind.Shift2 = true;
                    break;

                case KeyBindAction.SpellUse08:
                    bind.Category = "Magic";
                    bind.Key1 = KeyboardKey.F8;
                    bind.Key2 = KeyboardKey.F8;
                    bind.Shift2 = true;
                    break;

                case KeyBindAction.SpellUse09:
                    bind.Category = "Magic";
                    bind.Key1 = KeyboardKey.F9;
                    bind.Key2 = KeyboardKey.F9;
                    bind.Shift2 = true;
                    break;

                case KeyBindAction.SpellUse10:
                    bind.Category = "Magic";
                    bind.Key1 = KeyboardKey.F10;
                    bind.Key2 = KeyboardKey.F10;
                    bind.Shift2 = true;
                    break;

                case KeyBindAction.SpellUse11:
                    bind.Category = "Magic";
                    bind.Key1 = KeyboardKey.F11;
                    bind.Key2 = KeyboardKey.F11;
                    bind.Shift2 = true;
                    break;

                case KeyBindAction.SpellUse12:
                    bind.Category = "Magic";
                    bind.Key1 = KeyboardKey.F12;
                    bind.Key2 = KeyboardKey.F12;
                    bind.Shift2 = true;
                    break;

                case KeyBindAction.SpellUse13:
                    bind.Category = "Magic";
                    break;

                case KeyBindAction.SpellUse14:
                    bind.Category = "Magic";
                    break;

                case KeyBindAction.SpellUse15:
                    bind.Category = "Magic";
                    break;

                case KeyBindAction.SpellUse16:
                    bind.Category = "Magic";
                    break;

                case KeyBindAction.SpellUse17:
                    bind.Category = "Magic";
                    break;

                case KeyBindAction.SpellUse18:
                    bind.Category = "Magic";
                    break;

                case KeyBindAction.SpellUse19:
                    bind.Category = "Magic";
                    break;

                case KeyBindAction.SpellUse20:
                    bind.Category = "Magic";
                    break;

                case KeyBindAction.SpellUse21:
                    bind.Category = "Magic";
                    break;

                case KeyBindAction.SpellUse22:
                    bind.Category = "Magic";
                    break;

                case KeyBindAction.SpellUse23:
                    bind.Category = "Magic";
                    break;

                case KeyBindAction.SpellUse24:
                    bind.Category = "Magic";
                    break;

                case KeyBindAction.FortuneWindow:
                    bind.Category = "Windows";
                    bind.Key1 = KeyboardKey.W;
                    bind.Control1 = true;
                    break;

                case KeyBindAction.CurrencyWindow:
                    bind.Category = "Windows";
                    bind.Key1 = KeyboardKey.C;
                    bind.Control1 = true;
                    break;

                case KeyBindAction.FilterDropWindow:
                    bind.Category = "Windows";
                    bind.Key1 = KeyboardKey.F;
                    bind.Control1 = true;
                    break;
            }
        }

        public static float FontSize(float size)
        {
            return 20;
        }

        public static int ErrorCount;
        private static string LastError;

        public static void SaveError(string ex)
        {
            try
            {
                if (++ErrorCount > 20 || String.Compare(ex, LastError, StringComparison.OrdinalIgnoreCase) == 0) return;

                string LogPath = $"{AppInfo.AppPath}/Errors/";

                LastError = ex;

                if (!Directory.Exists(LogPath))
                    Directory.CreateDirectory(LogPath);

                File.AppendAllText($"{LogPath}{Now.Year}-{Now.Month}-{Now.Day}.txt", LastError + Environment.NewLine + Environment.NewLine);
            }
            catch
            { }
        }

        public static void SaveException(Exception ex)
        {
            SaveError(ex.ToString());
        }

        public static void Unload()
        {
            CConnection con = Connection;

            Connection = null;

            con?.Disconnect();
        }

        public static KeyBindInfo GetKeyBind(KeyBindAction action)
        {
            return KeyBinds.Binding.FirstOrDefault(x => x.Action == action);
        }

        public static string GetText(Raylib_cs.KeyboardKey key)
        {
            switch (key)
            {
                case Raylib_cs.KeyboardKey.Null: return string.Empty;

                // 锁定/导航
                case Raylib_cs.KeyboardKey.Backspace: return "Backspace";
                case Raylib_cs.KeyboardKey.CapsLock: return "Caps Lock";
                case Raylib_cs.KeyboardKey.ScrollLock: return "Scroll Lock";
                case Raylib_cs.KeyboardKey.NumLock: return "Num Lock";
                case Raylib_cs.KeyboardKey.PageUp: return "Page Up";
                case Raylib_cs.KeyboardKey.PageDown: return "Page Down";

                // 小键盘（KP）
                case Raylib_cs.KeyboardKey.KpMultiply: return "Num Pad *";
                case Raylib_cs.KeyboardKey.KpAdd: return "Num Pad +";
                case Raylib_cs.KeyboardKey.KpSubtract: return "Num Pad -";
                case Raylib_cs.KeyboardKey.KpDecimal: return "Num Pad .";
                case Raylib_cs.KeyboardKey.KpDivide: return "Num Pad /";
                case Raylib_cs.KeyboardKey.KpEnter: return "Num Pad Enter";
                case Raylib_cs.KeyboardKey.KpEqual: return "Num Pad =";
                case Raylib_cs.KeyboardKey.Kp0: return "Num Pad 0";
                case Raylib_cs.KeyboardKey.Kp1: return "Num Pad 1";
                case Raylib_cs.KeyboardKey.Kp2: return "Num Pad 2";
                case Raylib_cs.KeyboardKey.Kp3: return "Num Pad 3";
                case Raylib_cs.KeyboardKey.Kp4: return "Num Pad 4";
                case Raylib_cs.KeyboardKey.Kp5: return "Num Pad 5";
                case Raylib_cs.KeyboardKey.Kp6: return "Num Pad 6";
                case Raylib_cs.KeyboardKey.Kp7: return "Num Pad 7";
                case Raylib_cs.KeyboardKey.Kp8: return "Num Pad 8";
                case Raylib_cs.KeyboardKey.Kp9: return "Num Pad 9";

                // 主键区标点（raylib 没有 Oem*，用这组）
                case Raylib_cs.KeyboardKey.Semicolon: return ";";
                case Raylib_cs.KeyboardKey.Equal: return "=";
                case Raylib_cs.KeyboardKey.Comma: return ",";
                case Raylib_cs.KeyboardKey.Minus: return "-";
                case Raylib_cs.KeyboardKey.Period: return ".";
                case Raylib_cs.KeyboardKey.Slash: return "/";
                case Raylib_cs.KeyboardKey.Apostrophe: return "'";
                case Raylib_cs.KeyboardKey.Grave: return "`";
                case Raylib_cs.KeyboardKey.LeftBracket: return "[";
                case Raylib_cs.KeyboardKey.RightBracket: return "]";
                case Raylib_cs.KeyboardKey.Backslash: return "\\";

                // 顶部数字行（不是 D0..D9）
                case Raylib_cs.KeyboardKey.One: return "1";
                case Raylib_cs.KeyboardKey.Two: return "2";
                case Raylib_cs.KeyboardKey.Three: return "3";
                case Raylib_cs.KeyboardKey.Four: return "4";
                case Raylib_cs.KeyboardKey.Five: return "5";
                case Raylib_cs.KeyboardKey.Six: return "6";
                case Raylib_cs.KeyboardKey.Seven: return "7";
                case Raylib_cs.KeyboardKey.Eight: return "8";
                case Raylib_cs.KeyboardKey.Nine: return "9";
                case Raylib_cs.KeyboardKey.Zero: return "0";

                // 顺手补几个常见的（你要显示名字更友好点）
                case Raylib_cs.KeyboardKey.Space: return "Space";
                case Raylib_cs.KeyboardKey.Enter: return "Enter";
                case Raylib_cs.KeyboardKey.Tab: return "Tab";
                case Raylib_cs.KeyboardKey.Escape: return "Esc";
                case Raylib_cs.KeyboardKey.Delete: return "Delete";
                case Raylib_cs.KeyboardKey.Insert: return "Insert";
                case Raylib_cs.KeyboardKey.Home: return "Home";
                case Raylib_cs.KeyboardKey.End: return "End";
                case Raylib_cs.KeyboardKey.PrintScreen: return "Print Screen";
                case Raylib_cs.KeyboardKey.Pause: return "Pause";

                default:
                    return key.ToString(); // 其它直接用枚举名
            }
        }

        public static bool IsCurrencyItem(ItemInfo info)
        {
            return Globals.CurrencyInfoList.Binding.FirstOrDefault(x => x.DropItem == info) != null;
        }

        public static int CurrencyImage(ItemInfo info, long count)
        {
            var currency = Globals.CurrencyInfoList.Binding.FirstOrDefault(x => x.DropItem == info);

            if (currency == null)
                return 0;

            var image = currency.Images.OrderByDescending(x => x.Amount).FirstOrDefault(x => x.Amount <= count);

            if (image != null)
                return image.Image;
            else
                return currency.DropItem.Image;
        }
    }
}
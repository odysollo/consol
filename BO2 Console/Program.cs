using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Threading;
using System.Xml.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.IO;
namespace BO2_Console
{
    class BO2
    {
        #region Mem Functions & Defines

        [Flags]
        public enum ProcessAccessFlags : uint
        {
            All = 0x001F0FFF,
            Terminate = 0x00000001,
            CreateThread = 0x00000002,
            VirtualMemoryOperation = 0x00000008,
            VirtualMemoryRead = 0x00000010,
            VirtualMemoryWrite = 0x00000020,
            DuplicateHandle = 0x00000040,
            CreateProcess = 0x000000080,
            SetQuota = 0x00000100,
            SetInformation = 0x00000200,
            QueryInformation = 0x00000400,
            QueryLimitedInformation = 0x00001000,
            Synchronize = 0x00100000,
        }

        [DllImport("kernel32.dll")]
        static extern IntPtr OpenProcess(ProcessAccessFlags processAccess, bool bInheritHandle, int processId);

        [Flags]
        public enum FreeType
        {
            Decommit = 0x4000,
            Release = 0x8000,
        }

        [Flags]
        public enum AllocationType
        {
            Commit = 0x1000,
            Reserve = 0x2000,
            Decommit = 0x4000,
            Release = 0x8000,
            Reset = 0x80000,
            Physical = 0x400000,
            TopDown = 0x100000,
            WriteWatch = 0x200000,
            LargePages = 0x20000000
        }

        [Flags]
        public enum MemoryProtection
        {
            Execute = 0x10,
            ExecuteRead = 0x20,
            ExecuteReadWrite = 0x40,
            ExecuteWriteCopy = 0x80,
            NoAccess = 0x01,
            ReadOnly = 0x02,
            ReadWrite = 0x04,
            WriteCopy = 0x08,
            GuardModifierflag = 0x100,
            NoCacheModifierflag = 0x200,
            WriteCombineModifierflag = 0x400
        }

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, IntPtr dwSize,
            AllocationType flAllocationType, MemoryProtection flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int nSize,
            out int lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, int dwSize, FreeType dwFreeType);

        [DllImport("kernel32.dll")]
        static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize,
            IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, out IntPtr lpThreadId);

        public byte[] cbuf_addtext_wrapper =
        {
            0x55,
            0x8B, 0xEC,
            0x83, 0xEC, 0x8,
            0xC7, 0x45, 0xF8, 0x0, 0x0, 0x0, 0x0,
            0xC7, 0x45, 0xFC, 0x0, 0x0, 0x0, 0x0,
            0xFF, 0x75, 0xF8,
            0x6A, 0x0,
            0xFF, 0x55, 0xFC,
            0x83, 0xC4, 0x8,
            0x8B, 0xE5,
            0x5D,
            0xC3
        };

        IntPtr hProcess = IntPtr.Zero;
        int dwPID = -1;
        uint cbuf_address;
        uint nop_address;
        byte[] callbytes;
        IntPtr cbuf_addtext_alloc = IntPtr.Zero;
        byte[] commandbytes;
        IntPtr commandaddress;
        byte[] nopBytes = { 0x90, 0x90 };

        #endregion

        public void Send(string command)
        {
            try
            {
                callbytes = BitConverter.GetBytes(cbuf_address);
                if (command == "")
                {
                }
                else
                {
                    if (cbuf_addtext_alloc == IntPtr.Zero)
                    {
                        cbuf_addtext_alloc = VirtualAllocEx(hProcess, IntPtr.Zero,
                            (IntPtr)cbuf_addtext_wrapper.Length,
                            AllocationType.Commit | AllocationType.Reserve, MemoryProtection.ExecuteReadWrite);
                        commandbytes = System.Text.Encoding.ASCII.GetBytes(command);
                        commandaddress = VirtualAllocEx(hProcess, IntPtr.Zero, (IntPtr)(commandbytes.Length),
                            AllocationType.Commit | AllocationType.Reserve, MemoryProtection.ExecuteReadWrite);
                        int bytesWritten = 0;
                        int bytesWritten2 = commandbytes.Length;
                        WriteProcessMemory(hProcess, commandaddress, commandbytes, commandbytes.Length,
                            out bytesWritten2);

                        Array.Copy(BitConverter.GetBytes(commandaddress.ToInt64()), 0, cbuf_addtext_wrapper, 9, 4);
                        Array.Copy(callbytes, 0, cbuf_addtext_wrapper, 16, 4);

                        WriteProcessMemory(hProcess, cbuf_addtext_alloc, cbuf_addtext_wrapper,
                            cbuf_addtext_wrapper.Length, out bytesWritten);

                        IntPtr bytesOut;
                        CreateRemoteThread(hProcess, IntPtr.Zero, 0, cbuf_addtext_alloc, IntPtr.Zero, 0,
                            out bytesOut);

                        if (cbuf_addtext_alloc != IntPtr.Zero && commandaddress != IntPtr.Zero)
                        {
                            VirtualFreeEx(hProcess, cbuf_addtext_alloc, cbuf_addtext_wrapper.Length,
                                FreeType.Release);
                            VirtualFreeEx(hProcess, commandaddress, cbuf_addtext_wrapper.Length, FreeType.Release);
                        }
                    }

                    cbuf_addtext_alloc = IntPtr.Zero;
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Error");
                Console.ReadLine();
            }
        }

        public void FindGame()
        {
            if (Process.GetProcessesByName("t6mp").Length != 0)
            {
                cbuf_address = 0x5BDF70;
                nop_address = 0x8C90DA;
                dwPID = Process.GetProcessesByName("t6mp")[0].Id;
            }
            else if (Process.GetProcessesByName("t6zm").Length != 0)
            {
                cbuf_address = 0x4C7120;
                nop_address = 0x8C768A;
                dwPID = Process.GetProcessesByName("t6zm")[0].Id;
            }
            else if (Process.GetProcessesByName("t6mpv43").Length != 0)
            {
                cbuf_address = 0x5C6F10;
                nop_address = 0x8C923A;
                dwPID = Process.GetProcessesByName("t6mpv43")[0].Id;
            }
            else if (Process.GetProcessesByName("t6zmv41").Length != 0)
            {
                cbuf_address = 0x6B9D20;
                nop_address = 0x8C7E7A;
                dwPID = Process.GetProcessesByName("t6zmv41")[0].Id;
            }
            else
            {
                cbuf_address = 0x0;
                nop_address = 0x0;
                Console.WriteLine("No game found. Please open your game then restart CONSOL.");
                Console.ReadLine();
            }
            hProcess = OpenProcess(ProcessAccessFlags.All, false, dwPID);
            int nopBytesLength = nopBytes.Length;
            WriteProcessMemory(hProcess, (IntPtr)nop_address, nopBytes, nopBytes.Length, out nopBytesLength);
            Program.processMemory = new ProcessMemory(hProcess);
            Program.gmFog = new GMFog(Program.processMemory);
        }
    }
    public class WebConfigReader
    {
        private static string link = "";

        public WebConfigReader(string s)
        {
            link = s;
        }

        public string ReadString()
        {
            return new WebClient().DownloadString(link);
        }

        public float ReadFloat()
        {
            string commandAsString = new WebClient().DownloadString(link);
            return float.Parse(commandAsString);
        }

        public bool ReadBool()
        {
            string commandAsString = new WebClient().DownloadString(link);
            return bool.Parse(commandAsString);
        }

        public int ReadInt()
        {
            string commandAsString = new WebClient().DownloadString(link);
            return int.Parse(commandAsString);
        }
    }
    public class WrongCmd
    {
        private static string link2 = "";

        public WrongCmd(string d)
        {
            link2 = d;
        }

        public string ReadString()
        {
            return new WebClient().DownloadString(link2);
        }

        public float ReadFloat()
        {
            string commandAsString = new WebClient().DownloadString(link2);
            return float.Parse(commandAsString);
        }

        public bool ReadBool()
        {
            string commandAsString = new WebClient().DownloadString(link2);
            return bool.Parse(commandAsString);
        }

        public int ReadInt()
        {
            string commandAsString = new WebClient().DownloadString(link2);
            return int.Parse(commandAsString);
        }
    }
    class Program
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GetDesktopWindow();
        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowRect(IntPtr hWnd, ref Rect rect);

        [StructLayout(LayoutKind.Sequential)]
        private struct Rect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
        static void uninstall()
        {
            string app_name = Application.StartupPath + "\\" + Application.ProductName + ".exe";
            string bat_name = app_name + ".bat";

            string bat = "@echo off\n"
                + ":loop\n"
                + "del \"" + app_name + "\"\n"
                + "if Exist \"" + app_name + "\" GOTO loop\n"
                + "del %0";

            StreamWriter file = new StreamWriter(bat_name);
            file.Write(bat);
            file.Close();

            Process bat_call = new Process();
            bat_call.StartInfo.FileName = bat_name;
            bat_call.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            bat_call.StartInfo.UseShellExecute = true;
            bat_call.Start();

            Application.Exit();
        }
        [DllImport("User32.dll")]
        public static extern short GetAsyncKeyState(System.Windows.Forms.Keys vKey);

        [DllImport("User32.dll")]
        public static extern short GetAsyncKeyState(System.Int32 vKey);

        [DllImport("user32.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern long GetAsyncKeyState(long vKey);

        public static ProcessMemory processMemory;
        public static GMFog gmFog;

        static void Main(string[] args)
        {
            var p = new BO2();
            p.FindGame();
            string cmd;
            string night = "r_lightTweakSunColor 0.8 0.8 0.8 \n" +
                "r_lightTweakSunLight 7 \n " +
                "r_lightTweakSunDirection \n" +
                "sm_enable 1 \n" +
                "sm_maxLights 1 \n" +
                "sm_polygonoffsetscale 5 \n" +
                "sm_polygonoffsetbias 0.2 \n" +
                "sm_polygonoffsetbias 0.8 \n" +
                "sm_sunSampleSizeNear 0.8 \n" +
                "sm_sunShadowScale 1 \n" +
                "r_skyColorTemp \n" +
                "r_sky_intensity_factor0 0 \n" +
                "r_sky_intensity_factor 0 \n";
            string threefps = "com_maxfps 30 \n" + "timescale 0.1";
            string sixfps = "com_maxfps 30 \n" + "timescale 0.05";
            string maxfps = "com_maxfps 30 \n" + "timescale 0.01";
            string quality = " r_lodbiasSkinned -1000\n" +
                "r_lodbiasRigid -1000";
            string hud = "hud_health_pulserate_critical 0\n" +
                        "hud_health_pulserate_injured 0\n" +
                        "hud_health_startpulse_critical 0\n" +
                        "hud_health_startpulse_injured 0\n" +
                        "hud_healthOverlay_phaseEnd_pulseDuration 0\n" +
                        "hud_healthOverlay_phaseEnd_toAlpha 0\n" +
                        "hud_healthOverlay_phaseOne_pulseDuration 0\n" +
                        "hud_healthOverlay_phaseThree_pulseDuration 0\n" +
                        "hud_healthOverlay_phaseThree_toAlphaMultiplier 0\n" +
                        "hud_healthOverlay_phaseTwo_pulseDuration 0\n" +
                        "hud_healthOverlay_phaseTwo_toAlphaMultiplier 0\n" +
                        "hud_healthOverlay_pulseStart 0\n" +
                        "hud_healthOverlay_regenPauseTime 0\n" +
                        "cg_hudDamageIconHeight 0\n" +
                        "cg_overheadnamesfarscale 0\n" +
                        "cg_overheadiconsize 0\n" +
                        "cg_overheadRankSize 0\n" +
                        "cg_overheadnamesfont 6\n" +
                        "cg_constantSizeHeadIcons 0\n" +
                        "cg_overheadnamessize 0\n" +
                        "cg_overheadnamesglow 0 0 0 0\n" +
                        "cg_hudDamageIconHeight 0\n" +
                        "cg_hudDamageIconInScope 0\n" +
                        "cg_hudDamageIconOffset 0\n" +
                        "cg_hudDamageIconTime 0\n" +
                        "cg_hudDamageIconWidth 0\n" +
                        "cg_hudGrenadeIconEnabledFlash 0\n" +
                        "cg_hudGrenadeIconHeight 0\n" +
                        "cg_hudGrenadeIconInScope 0\n" +
                        "cg_hudGrenadeIconMaxHeight 0\n" +
                        "cg_hudGrenadeIconOffset 0\n" +
                        "cg_hudGrenadeIconWidth 0\n" +
                        "cg_hudGrenadePointerHeight 0\n" +
                        "cg_hudGrenadePointerPivot 0\n" +
                        "cg_hudGrenadePointerWidth 0\n" +
                        "cg_hardcore 1\n" +
                        "cg_useColorControl 1\n" +
                        "bg_bobMax 0\n";
            string greenscreen = "r_modellimit 0 \n" + "r_skipPvs 1\n" + "r_lockPvs .875\n" + "r_bloomTweaks 1\n" +
                 "r_bloomHiQuality 0\n" +
                "r_clearColor 0 1 0 0\n" +
                "r_clearColor2 0 1 0 0\n" +
                 "r_znear 10000\n" +
                 "r_zfar 0.000001\n" +
                 "r_glow_allowed 0\n" +
                 "r_glowtweakbloomintensity 0\n" +
                 "r_glowtweakradius 0\n" +
                 "r_glowTweakEnable 0\n" +
                 "r_glowUseTweaks 0\n" +
                 "r_glow 0\n" +
                 "r_glow_allowed 0\n" +
                 "r_seethru_decal_enable 0\n" +
                 "r_drawdecals 0\n" + "cg_drawgun 1";
            string regular = "r_modellimit 1000\n" +
                "r_skipPvs 0\n" +
                "r_lockPvs 0\n" +
                "r_clearColor 0 0 0 0\n" +
                "r_clearColor2 0 0 0 0\n" +
                "r_bloomTweaks 0\n" +
                "r_bloomHiQuality 1\n" +
                "r_znear 10000\n" +
                "r_zfar 0\n" +
                "r_glow_allowed 1\n" +
                "r_glowtweakbloomintensity 1\n" +
                "r_glowtweakradius 1\n" +
                "r_glowTweakEnable 1\n" +
                "r_glowUseTweaks 1\n" +
                "r_glow 1\n" +
                "r_glow_allowed 1\n" +
                "r_seethru_decal_enable 1\n" +
                "r_drawdecals 1\n" +
                "cg_drawgun 1";
            string regular2 = "r_modellimit 1000\n" +
    "r_skipPvs 0\n" +
    "r_lockPvs 0\n" +
    "r_clearColor 0 0 0 0\n" +
    "r_clearColor2 0 0 0 0\n" +
    "r_bloomTweaks 0\n" +
    "r_bloomHiQuality 1\n" +
    "r_znear 10000\n" +
    "r_zfar 0\n" +
    "r_glow_allowed 1\n" +
    "r_glowtweakbloomintensity 1\n" +
    "r_glowtweakradius 1\n" +
    "r_glowTweakEnable 1\n" +
    "r_glowUseTweaks 1\n" +
    "r_glow 1\n" +
    "r_glow_allowed 1\n" +
    "r_seethru_decal_enable 1\n" +
    "r_drawdecals 1\n" + "cg_drawgun 1";
            string greensky = "r_modellimit 0\n" + "r_clearcolor 0 1 0\n" + "r_clearcolor2 0 1 0\n" + "r_bloomtweaks 1\n";
            string depth = "r_Dof_Enable 0\n" +
                "r_dof_bias 0\n" +
                "r_dof_farBlur 0\n" +
                "r_dof_farEnd 5\n" +
                "r_dof_farStart 0\n" +
                 "r_dof_nearBlur 0\n" +
                "r_dof_nearEnd 0\n" +
                "r_dof_nearStart 0\n" +
                "r_dof_tweak 1\n" +
                "r_dof_tweak_enable 1\n" +
                "r_enablePlayerShadow 0\n" +
                "r_exposureTweak 1\n" +
                "r_exposureValue 16\n" +
                "r_sky_intensity_factor 0\n" +
                "r_modellimit 40\n" +
                "r_clearcolor 1 1 1 0\n" +
                "r_clearcolor2 1 1 1 0\n" +
                "r_bloomtweaks 1\n" + "cg_drawgun 0";
            string depthoff = "cg_draw2d 1\n " +
                "r_enablePlayerShadow 0\n " +
                "r_exposureTweak 0\n " +
                "r_exposureValue 12.5\n " +
                "r_sky_intensity_factor 1\n " +
                "r_modellimit 1024\n " +
                "r_clearcolor 1 1 1 0\n " +
                "r_clearcolor2 1 1 1 0\n " +
                "cg_drawgun 1" +
                "r_bloomtweaks 0";
            string greenplayers = "r_zfar 1\n" + "r_lockPvs 0\n" + "r_modelLimit 0\n" + "r_clearColor 0 1 0 1\n" + "r_clearColor2 0 1 0 1\n" + "r_bloomTweaks 1";
            //Curent WIP is savefog command
            string codename = "code";
            string url = "";
            bool disableanim = true;
            int delay2 = 0;
            int delay3 = 0;
            if (File.Exists(Path.GetDirectoryName(Application.ExecutablePath) + "//settings//" + "anim" + ".solset"))
            {
                string yourDirectory = Path.GetDirectoryName(Application.ExecutablePath) + "//settings//";
                string existingFilePath = Path.GetDirectoryName(Application.ExecutablePath) + "//settings//";
                string existingFile = "anim" + ".solset";
                string fullFilePath = Path.Combine(existingFilePath, existingFile);
                string custom = File.ReadAllText(Path.Combine(yourDirectory, "anim" + ".solset"));
                disableanim = false;
            }
            if (disableanim == false)
            {
                delay2 = 0;
                delay3 = 0;
            }
            else if (disableanim == true)
            {
                delay2 = 50;
                delay3 = 100;
            }
            Console.WriteLine(@"      ___           ___           ___           ___           ___           ___ 
     /\  \         /\  \         /\__\         /\  \         /\  \         /\__\
    /::\  \       /::\  \       /::|  |       /::\  \       /::\  \       /:/  /
   /:/\:\  \     /:/\:\  \     /:|:|  |      /:/\ \  \     /:/\:\  \     /:/  / 
  /:/  \:\  \   /:/  \:\  \   /:/|:|  |__   _\:\~\ \  \   /:/  \:\  \   /:/  /  
 /:/__/ \:\__\ /:/__/ \:\__\ /:/ |:| /\__\ /\ \:\ \ \__\ /:/__/ \:\__\ /:/__/   
 \:\  \  \/__/ \:\  \ /:/  / \/__|:|/:/  / \:\ \:\ \/__/ \:\  \ /:/  / \:\  \   
  \:\  \        \:\  /:/  /      |:/:/  /   \:\ \:\__\    \:\  /:/  /   \:\  \  
   \:\  \        \:\/:/  /       |::/  /     \:\/:/  /     \:\/:/  /     \:\  \ 
    \:\__\        \::/  /        /:/  /       \::/  /       \::/  /       \:\__\
     \/__/         \/__/         \/__/         \/__/         \/__/         \/__/");
            Console.WriteLine("");
            System.Threading.Thread.Sleep(250);
            Console.ForegroundColor = ConsoleColor.White;
            string gameText = "Current game found: ";
            for (int i = 0; i < gameText.Length; i++)

            {
                Console.Write(gameText[i]);
                System.Threading.Thread.Sleep(delay2);
            }
            Console.ForegroundColor = ConsoleColor.White;
            string game = "";
            if (Process.GetProcessesByName("t6mp").Length != 0)
            {
                game = "Steam BO2";
            }
            else if (Process.GetProcessesByName("t6zm").Length != 0)
            {
                game = "Steam BO2 Zombies";
            }
            else if (Process.GetProcessesByName("t6mpv43").Length != 0)
            {
                game = "Redacted BO2";
            }
            else if (Process.GetProcessesByName("t6zmv41").Length != 0)
            {
                game = "Redacted BO2 Zombies";
            }
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            string warning = " (Please note fog does not currently work on redacted)";
            if (game == "Redacted BO2")
            {
                game = game + warning;
            }
            for (int i = 0; i < game.Length; i++)

            {
                Console.Write(game[i]);
                System.Threading.Thread.Sleep(delay2);
            }
            Console.Write("\n");
            Console.ForegroundColor = ConsoleColor.White;
            string helpText = "Need help? Enter ";
            for (int i = 0; i < helpText.Length; i++)

            {
                Console.Write(helpText[i]);
                System.Threading.Thread.Sleep(delay2);
            }
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            string helpText2 = "help ";
            for (int i = 0; i < helpText2.Length; i++)

            {
                Console.Write(helpText2[i]);
                System.Threading.Thread.Sleep(delay2);
            }
            Console.ForegroundColor = ConsoleColor.White;
            string helpText3 = "as your code.";
            for (int i = 0; i < helpText3.Length; i++)

            {
                Console.Write(helpText3[i]);
                System.Threading.Thread.Sleep(delay2);
            }
            Console.Write("\n");
            Console.ForegroundColor = ConsoleColor.White;
            System.Threading.Thread.Sleep(200);
            string codeText = "Enter code ";
            for (int i = 0; i < codeText.Length; i++)

            {
                Console.Write(codeText[i]);
                System.Threading.Thread.Sleep(delay3);
            }
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            string codeText2 = "here";
            for (int i = 0; i < codeText2.Length; i++)

            {
                Console.Write(codeText2[i]);
                System.Threading.Thread.Sleep(delay3);
            }
            Console.ForegroundColor = ConsoleColor.White;
            string codeText3 = ": ";
            for (int i = 0; i < codeText3.Length; i++)

            {
                Console.Write(codeText3[i]);
                System.Threading.Thread.Sleep(delay3);
            }
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            bool saved = false;
            if (File.Exists(Path.GetDirectoryName(Application.ExecutablePath) + "//settings//" + codename + ".solset"))
            {
                Console.WriteLine("Saved code loaded.\n");
                string yourDirectory = Path.GetDirectoryName(Application.ExecutablePath) + "//settings//";
                string existingFilePath = Path.GetDirectoryName(Application.ExecutablePath) + "//settings//";
                string existingFile = codename + ".solset";
                string fullFilePath = Path.Combine(existingFilePath, existingFile);
                string custom2 = File.ReadAllText(Path.Combine(yourDirectory, codename + ".solset"));
                url = custom2;
                saved = true;
            }
            if (saved == false)
            {
                url = Console.ReadLine();
            }
            if (url == "help")
            {
                Console.Clear();
                string dashText = "------------------------------------------------------------";
                for (int i = 0; i < dashText.Length; i++)
                {
                    Console.Write(dashText[i]);
                    System.Threading.Thread.Sleep(50);
                }
                System.Threading.Thread.Sleep(100);
                Console.Write("\n");
                Console.ForegroundColor = ConsoleColor.White;
                string hello = "Hello! Thank you for taking interest in CONSOL.\n";
                for (int i = 0; i < hello.Length; i++)
                {
                    Console.Write(hello[i]);
                    System.Threading.Thread.Sleep(50);
                }
                System.Threading.Thread.Sleep(1500);
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                string confusing = "Everything may look very confusing, however, please do-not be intimidated by the command-line-interface.\n";
                for (int i = 0; i < confusing.Length; i++)
                {
                    Console.Write(confusing[i]);
                    System.Threading.Thread.Sleep(50);
                }
                System.Threading.Thread.Sleep(3000);
                Console.ForegroundColor = ConsoleColor.White;
                string hang = "Once you get the hang of it, you will find everything to be much faster and easier than a traditional console.\n";
                for (int i = 0; i < hang.Length; i++)
                {
                    Console.Write(hang[i]);
                    System.Threading.Thread.Sleep(50);
                }
                System.Threading.Thread.Sleep(2500);
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                string explain = "I'll try to explain everything as well as I can, however, if you still have any questions, please join our Discord.\n";
                for (int i = 0; i < explain.Length; i++)
                {
                    Console.Write(explain[i]);
                    System.Threading.Thread.Sleep(50);
                }
                System.Threading.Thread.Sleep(3000);
                Console.ForegroundColor = ConsoleColor.White;
                string linkp1 = "Link is on site (";
                for (int i = 0; i < linkp1.Length; i++)
                {
                    Console.Write(linkp1[i]);
                    System.Threading.Thread.Sleep(50);
                }
                string link = "https://consol.cf";
                for (int i = 0; i < link.Length; i++)
                {
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.Write(link[i]);
                    Console.ForegroundColor = ConsoleColor.White;
                    System.Threading.Thread.Sleep(50);
                }
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(").\n");
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                System.Threading.Thread.Sleep(1000);
                string press = "Press ";
                for (int i = 0; i < press.Length; i++)
                {
                    Console.Write(press[i]);
                    System.Threading.Thread.Sleep(50);
                }
                Console.ForegroundColor = ConsoleColor.White;
                string linelmao = "'";
                for (int i = 0; i < linelmao.Length; i++)
                {
                    Console.Write(linelmao[i]);
                    System.Threading.Thread.Sleep(50);
                }
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                string enter = "enter";
                for (int i = 0; i < enter.Length; i++)
                {
                    Console.Write(enter[i]);
                    System.Threading.Thread.Sleep(50);
                }
                Console.ForegroundColor = ConsoleColor.White;
                for (int i = 0; i < linelmao.Length; i++)
                {
                    Console.Write(linelmao[i]);
                    System.Threading.Thread.Sleep(50);
                }
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                string tocontinue = " to continue.";
                for (int i = 0; i < tocontinue.Length; i++)
                {
                    Console.Write(tocontinue[i]);
                    System.Threading.Thread.Sleep(50);
                }
                Console.ReadLine();
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.White;
                for (int i = 0; i < dashText.Length; i++)
                {
                    Console.Write(dashText[i]);
                    System.Threading.Thread.Sleep(50);
                }
                Console.Write("\n");
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                string toStart = "First things first, I can see that you have got ";
                for (int i = 0; i < toStart.Length; i++)
                {
                    Console.Write(toStart[i]);
                    System.Threading.Thread.Sleep(50);
                }
                Console.ForegroundColor = ConsoleColor.White;
                if (game == "Redacted BO2 (Please note fog does not currently work on redacted)")
                {
                    game = "Redacted BO2";
                }
                for (int i = 0; i < game.Length; i++)
                {
                    Console.Write(game[i]);
                    System.Threading.Thread.Sleep(50);
                }
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                string open = " open\n";
                for (int i = 0; i < open.Length; i++)
                {
                    Console.Write(open[i]);
                    System.Threading.Thread.Sleep(50);
                }
                Console.ForegroundColor = ConsoleColor.White;
                System.Threading.Thread.Sleep(2000);
                string startoff = "To start off with the basics,\n";
                for (int i = 0; i < startoff.Length; i++)
                {
                    Console.Write(startoff[i]);
                    System.Threading.Thread.Sleep(50);
                }
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                string gamedemo = "CODTV";
                if (game == "Redacted BO2")
                {
                    gamedemo = "Redacted";
                }
                Console.ForegroundColor = ConsoleColor.White;
                string goahead = "Go ahead and load into a custom game or a " + gamedemo + " demo.";
                for (int i = 0; i < goahead.Length; i++)
                {
                    Console.Write(goahead[i]);
                    System.Threading.Thread.Sleep(50);
                }
                System.Threading.Thread.Sleep(1500);
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("\n");
                for (int i = 0; i < press.Length; i++)
                {
                    Console.Write(press[i]);
                    System.Threading.Thread.Sleep(50);
                }
                Console.ForegroundColor = ConsoleColor.White;
                for (int i = 0; i < linelmao.Length; i++)
                {
                    Console.Write(linelmao[i]);
                    System.Threading.Thread.Sleep(50);
                }
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                for (int i = 0; i < enter.Length; i++)
                {
                    Console.Write(enter[i]);
                    System.Threading.Thread.Sleep(50);
                }
                Console.ForegroundColor = ConsoleColor.White;
                for (int i = 0; i < linelmao.Length; i++)
                {
                    Console.Write(linelmao[i]);
                    System.Threading.Thread.Sleep(50);
                }
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                string tocontinue2 = " once you are in one of these.";
                for (int i = 0; i < tocontinue2.Length; i++)
                {
                    Console.Write(tocontinue2[i]);
                    System.Threading.Thread.Sleep(50);
                }
                Console.ReadLine();
                string text;
                Console.ForegroundColor = ConsoleColor.White;
                text = "Great, to make this easy the first thing I will have you do is send a basic command into the game.\n";
                for (int i = 0; i < text.Length; i++)
                {
                    Console.Write(text[i]);
                    System.Threading.Thread.Sleep(50);
                }
                System.Threading.Thread.Sleep(1000);
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                text = "You probably already know this command, but if not, this command does something really simple.\n";
                for (int i = 0; i < text.Length; i++)
                {
                    Console.Write(text[i]);
                    System.Threading.Thread.Sleep(50);
                }
                System.Threading.Thread.Sleep(1000);
                Console.ForegroundColor = ConsoleColor.White;
                text = "The command is: ";
                for (int i = 0; i < text.Length; i++)
                {
                    Console.Write(text[i]);
                    System.Threading.Thread.Sleep(50);
                }
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                text = "cg_drawgun 0\n";
                for (int i = 0; i < text.Length; i++)
                {
                    Console.Write(text[i]);
                    System.Threading.Thread.Sleep(50);
                }
                Console.ForegroundColor = ConsoleColor.White;
                System.Threading.Thread.Sleep(1000);
                text = "And I'm pretty sure you can guess what this does. ";
                for (int i = 0; i < text.Length; i++)
                {
                    Console.Write(text[i]);
                    System.Threading.Thread.Sleep(50);
                }
                text = "It hides the gun.\n";
                for (int i = 0; i < text.Length; i++)
                {
                    Console.Write(text[i]);
                    System.Threading.Thread.Sleep(50);
                }
                System.Threading.Thread.Sleep(1000);
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                for (int i = 0; i < press.Length; i++)
                {
                    Console.Write(press[i]);
                    System.Threading.Thread.Sleep(50);
                }
                Console.ForegroundColor = ConsoleColor.White;
                for (int i = 0; i < linelmao.Length; i++)
                {
                    Console.Write(linelmao[i]);
                    System.Threading.Thread.Sleep(50);
                }
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                for (int i = 0; i < enter.Length; i++)
                {
                    Console.Write(enter[i]);
                    System.Threading.Thread.Sleep(50);
                }
                Console.ForegroundColor = ConsoleColor.White;
                for (int i = 0; i < linelmao.Length; i++)
                {
                    Console.Write(linelmao[i]);
                    System.Threading.Thread.Sleep(50);
                }
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                string tocontinue3 = " to send this command to your game.";
                for (int i = 0; i < tocontinue3.Length; i++)
                {
                    Console.Write(tocontinue3[i]);
                    System.Threading.Thread.Sleep(50);
                }
                Console.ReadLine();
                p.Send("cg_drawgun 0");
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.White;
                text = "Success! If you tab into your game, you can see now that the players gun is gone!\n";
                for (int i = 0; i < text.Length; i++)
                {
                    Console.Write(text[i]);
                    System.Threading.Thread.Sleep(50);
                }
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                text = "The next thing you're probably wondering, is what is that code in the beginning all about?\n";
                for (int i = 0; i < text.Length; i++)
                {
                    Console.Write(text[i]);
                    System.Threading.Thread.Sleep(50);
                }
                Console.ForegroundColor = ConsoleColor.White;
                text = "Well, that code is something called a ";
                for (int i = 0; i < text.Length; i++)
                {
                    Console.Write(text[i]);
                    System.Threading.Thread.Sleep(50);
                }
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                text = "config";
                for (int i = 0; i < text.Length; i++)
                {
                    Console.Write(text[i]);
                    System.Threading.Thread.Sleep(50);
                }
                Console.ForegroundColor = ConsoleColor.White;
                text = ".\n";
                for (int i = 0; i < text.Length; i++)
                {
                    Console.Write(text[i]);
                    System.Threading.Thread.Sleep(50);
                }
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                text = "All a config is, is just a long string of commands that makes the game look better.\n";
                for (int i = 0; i < text.Length; i++)
                {
                    Console.Write(text[i]);
                    System.Threading.Thread.Sleep(50);
                }
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                text = "A config looks something like this: \n";
                for (int i = 0; i < text.Length; i++)
                {
                    Console.Write(text[i]);
                    System.Threading.Thread.Sleep(50);
                }
                Console.ForegroundColor = ConsoleColor.White;
                text = "timescale 0.025\ncom_maxfps 30\ncg_fov 90\ncg_gun_x  .2\ncg_gun_z - .2\nr_dof_bias 0.1\nr_dof_enable 1\nr_dof_tweak 1\nr_dof_viewModelStart 1\nr_dof_viewModelend 6\nr_dof_nearBlur 2\nr_dof_nearEnd 1\nr_dof_farBlur 1\nr_dof_farStart 300\nr_dof_farEnd 1300\nr_dofHDR 4\nr_specularmap 3\n\nr_specularcolorscale 2\nr_fxaa 1\nr_ssao 1\nr_ssaointensity 0.89\nr_ssaoscale 55\nr_ssaoradius 39\nr_ssaobias 0.15\nr_ssaoDebug 1\n";
                for (int i = 0; i < text.Length; i++)
                {
                    Console.Write(text[i]);
                    System.Threading.Thread.Sleep(10);
                }
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                text = "Unlike traditional consoles, if you want to use your config, all you need to do is upload it to the site!\n";
                for (int i = 0; i < text.Length; i++)
                {
                    Console.Write(text[i]);
                    System.Threading.Thread.Sleep(50);
                }
                Console.ForegroundColor = ConsoleColor.White;
                text = "Once uploaded, your config will be given a code. This code is permanent and can be used as many times as you like.\n";
                for (int i = 0; i < text.Length; i++)
                {
                    Console.Write(text[i]);
                    System.Threading.Thread.Sleep(50);
                }
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                text = "Already have a config? Go ahead and upload it! Simply type ";
                for (int i = 0; i < text.Length; i++)
                {
                    Console.Write(text[i]);
                    System.Threading.Thread.Sleep(50);
                }
                Console.ForegroundColor = ConsoleColor.White;
                text = "'";
                for (int i = 0; i < text.Length; i++)
                {
                    Console.Write(text[i]);
                    System.Threading.Thread.Sleep(50);
                }
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                text = "upload";
                for (int i = 0; i < text.Length; i++)
                {
                    Console.Write(text[i]);
                    System.Threading.Thread.Sleep(50);
                }
                Console.ForegroundColor = ConsoleColor.White;
                text = "'";
                for (int i = 0; i < text.Length; i++)
                {
                    Console.Write(text[i]);
                    System.Threading.Thread.Sleep(50);
                }
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                text = " to be taken to the upload page right now!\n";
                for (int i = 0; i < text.Length; i++)
                {
                    Console.Write(text[i]);
                    System.Threading.Thread.Sleep(50);
                }
                Console.ForegroundColor = ConsoleColor.White;
                text = "Type anything else or press enter to use one of our configs: ";
                for (int i = 0; i < text.Length; i++)
                {
                    Console.Write(text[i]);
                    System.Threading.Thread.Sleep(50);
                }
                string what = Console.ReadLine();
                if (what == "upload")
                {
                    Process.Start("https://consol.cf/upload");
                }
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                text = "Great! If you uploaded your own code, use that, or use our code! Our code is ";
                for (int i = 0; i < text.Length; i++)
                {
                    Console.Write(text[i]);
                    System.Threading.Thread.Sleep(50);
                }
                Console.ForegroundColor = ConsoleColor.White;
                text = "zxkfE\n";
                for (int i = 0; i < text.Length; i++)
                {
                    Console.Write(text[i]);
                    System.Threading.Thread.Sleep(50);
                }
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                text = "Please note that codes are case sensitive.\n";
                for (int i = 0; i < text.Length; i++)
                {
                    Console.Write(text[i]);
                    System.Threading.Thread.Sleep(50);
                }
                Console.ForegroundColor = ConsoleColor.White;
                text = "Enter code here: ";
                for (int i = 0; i < text.Length; i++)
                {
                    Console.Write(text[i]);
                    System.Threading.Thread.Sleep(50);
                }
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                url = Console.ReadLine();
                string urlprefix2 = "http://consol.cf/upload/configs/";
                string urlsuffix2 = ".cfg";
                WebConfigReader conf =
                new WebConfigReader(urlprefix2 + url + urlsuffix2);
                Console.ForegroundColor = ConsoleColor.White;
                text = "Great! Now, press ";
                for (int i = 0; i < text.Length; i++)
                {
                    Console.Write(text[i]);
                    System.Threading.Thread.Sleep(50);
                }
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                text = "'";
                for (int i = 0; i < text.Length; i++)
                {
                    Console.Write(text[i]);
                    System.Threading.Thread.Sleep(50);
                }
                Console.ForegroundColor = ConsoleColor.White;
                text = "enter";
                for (int i = 0; i < text.Length; i++)
                {
                    Console.Write(text[i]);
                    System.Threading.Thread.Sleep(50);
                }
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                text = "'";
                for (int i = 0; i < text.Length; i++)
                {
                    Console.Write(text[i]);
                    System.Threading.Thread.Sleep(50);
                }
                Console.ForegroundColor = ConsoleColor.White;
                text = " to send the config!";
                for (int i = 0; i < text.Length; i++)
                {
                    Console.Write(text[i]);
                    System.Threading.Thread.Sleep(50);
                }
                string[] tokens = Regex.Split(conf.ReadString(), @"\r?\n|\r");
                foreach (string s in tokens)
                p.Send(s);
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                text = "As you can see, your game looks different now. The time may be slower, the lighting may be different, the sky may be darker, ect.\n";
                for (int i = 0; i < text.Length; i++)
                {
                    Console.Write(text[i]);
                    System.Threading.Thread.Sleep(50);
                }
                Console.ForegroundColor = ConsoleColor.White;
                text = "And that is the main premise of CONSOL! All you have to do is upload your config, enter your code, then use commands!\n";
                for (int i = 0; i < text.Length; i++)
                {
                    Console.Write(text[i]);
                    System.Threading.Thread.Sleep(50);
                }
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                text = "All the commands you need to know can be found on the website ";
                for (int i = 0; i < text.Length; i++)
                {
                    Console.Write(text[i]);
                    System.Threading.Thread.Sleep(50);
                }
                Console.ForegroundColor = ConsoleColor.White;
                text = "(";
                for (int i = 0; i < text.Length; i++)
                {
                    Console.Write(text[i]);
                    System.Threading.Thread.Sleep(50);
                }
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                text = "http://consol.cf";
                for (int i = 0; i < text.Length; i++)
                {
                    Console.Write(text[i]);
                    System.Threading.Thread.Sleep(50);
                }
                Console.ForegroundColor = ConsoleColor.White;
                text = ")";
                for (int i = 0; i < text.Length; i++)
                {
                    Console.Write(text[i]);
                    System.Threading.Thread.Sleep(50);
                }
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                text = " however, I will tell you some important ones now.\n";
                for (int i = 0; i < text.Length; i++)
                {
                    Console.Write(text[i]);
                    System.Threading.Thread.Sleep(50);
                }
                Console.ForegroundColor = ConsoleColor.White;
                text = "Executing your config can be done in 3 ways. You can simply press ";
                for (int i = 0; i < text.Length; i++)
                {
                    Console.Write(text[i]);
                    System.Threading.Thread.Sleep(50);
                }
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                text = "'";
                for (int i = 0; i < text.Length; i++)
                {
                    Console.Write(text[i]);
                    System.Threading.Thread.Sleep(50);
                }
                Console.ForegroundColor = ConsoleColor.White;
                text = "enter";
                for (int i = 0; i < text.Length; i++)
                {
                    Console.Write(text[i]);
                    System.Threading.Thread.Sleep(50);
                }
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                text = "'\n";
                for (int i = 0; i < text.Length; i++)
                {
                    Console.Write(text[i]);
                    System.Threading.Thread.Sleep(50);
                }
                Console.ForegroundColor = ConsoleColor.White;
                text = "Or, you can use the commands ";
                for (int i = 0; i < text.Length; i++)
                {
                    Console.Write(text[i]);
                    System.Threading.Thread.Sleep(50);
                }
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                text = "'";
                for (int i = 0; i < text.Length; i++)
                {
                    Console.Write(text[i]);
                    System.Threading.Thread.Sleep(50);
                }
                Console.ForegroundColor = ConsoleColor.White;
                text = "e";
                for (int i = 0; i < text.Length; i++)
                {
                    Console.Write(text[i]);
                    System.Threading.Thread.Sleep(50);
                }
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                text = "' ";
                for (int i = 0; i < text.Length; i++)
                {
                    Console.Write(text[i]);
                    System.Threading.Thread.Sleep(50);
                }
                Console.ForegroundColor = ConsoleColor.White;
                text = "or ";
                for (int i = 0; i < text.Length; i++)
                {
                    Console.Write(text[i]);
                    System.Threading.Thread.Sleep(50);
                }
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                text = "'";
                for (int i = 0; i < text.Length; i++)
                {
                    Console.Write(text[i]);
                    System.Threading.Thread.Sleep(50);
                }
                Console.ForegroundColor = ConsoleColor.White;
                text = "exec\n";
                for (int i = 0; i < text.Length; i++)
                {
                    Console.Write(text[i]);
                    System.Threading.Thread.Sleep(50);
                }
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                text = "Want to use streams? Simply type ";
                for (int i = 0; i < text.Length; i++)
                {
                    Console.Write(text[i]);
                    System.Threading.Thread.Sleep(50);
                }
                Console.ForegroundColor = ConsoleColor.White;
                text = "streams ";
                for (int i = 0; i < text.Length; i++)
                {
                    Console.Write(text[i]);
                    System.Threading.Thread.Sleep(50);
                }
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                text = "as a command!\n";
                for (int i = 0; i < text.Length; i++)
                {
                    Console.Write(text[i]);
                    System.Threading.Thread.Sleep(50);
                }
                Console.ForegroundColor = ConsoleColor.White;
                text = "Want to animate any command? Simply use the ";
                for (int i = 0; i < text.Length; i++)
                {
                    Console.Write(text[i]);
                    System.Threading.Thread.Sleep(50);
                }
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                text = "'";
                for (int i = 0; i < text.Length; i++)
                {
                    Console.Write(text[i]);
                    System.Threading.Thread.Sleep(50);
                }
                Console.ForegroundColor = ConsoleColor.White;
                text = "animate";
                for (int i = 0; i < text.Length; i++)
                {
                    Console.Write(text[i]);
                    System.Threading.Thread.Sleep(50);
                }
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                text = "' ";
                for (int i = 0; i < text.Length; i++)
                {
                    Console.Write(text[i]);
                    System.Threading.Thread.Sleep(50);
                }
                Console.ForegroundColor = ConsoleColor.White;
                text = "command!\n";
                for (int i = 0; i < text.Length; i++)
                {
                    Console.Write(text[i]);
                    System.Threading.Thread.Sleep(50);
                }
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                text = "Want to create your own command? Simply use the ";
                for (int i = 0; i < text.Length; i++)
                {
                    Console.Write(text[i]);
                    System.Threading.Thread.Sleep(50);
                }
                Console.ForegroundColor = ConsoleColor.White;
                text = "'";
                for (int i = 0; i < text.Length; i++)
                {
                    Console.Write(text[i]);
                    System.Threading.Thread.Sleep(50);
                }
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                text = "custom";
                for (int i = 0; i < text.Length; i++)
                {
                    Console.Write(text[i]);
                    System.Threading.Thread.Sleep(50);
                }
                Console.ForegroundColor = ConsoleColor.White;
                text = "' ";
                for (int i = 0; i < text.Length; i++)
                {
                    Console.Write(text[i]);
                    System.Threading.Thread.Sleep(50);
                }
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                text = "command!\n";
                for (int i = 0; i < text.Length; i++)
                {
                    Console.Write(text[i]);
                    System.Threading.Thread.Sleep(50);
                }
                Console.ForegroundColor = ConsoleColor.White;
                text = "And that's it! Simple, right? Press enter to conclude the tutorial, and open up the full commands list on the site!\nThanks for using CONSOL!";
                for (int i = 0; i < text.Length; i++)
                {
                    Console.Write(text[i]);
                    System.Threading.Thread.Sleep(50);
                }
                Console.WriteLine("");
                Console.ReadLine();
                Process.Start("https://consol.cf/commands");
                Environment.Exit(1);
            }
            Console.Clear();
            Console.WriteLine("Loading...");
            string urlprefix = "http://consol.cf/upload/configs/";
            string urlsuffix = ".cfg";
            int cVersion = 24;
            int oVersion;
            bool depthon = false;
            string XMLFileLocation = "https://github.com/odysollo/consol/raw/master/version.xml";
            bool debug = false;
            XDocument doc = XDocument.Load(XMLFileLocation);
            var VersionElement = doc.Descendants("Version");
            oVersion = Convert.ToInt32(string.Concat(VersionElement.Nodes()));
            if (saved == false)
            {
                Console.WriteLine("Would you like to save this code? It can later be changed with the 'unsave' command. (enter yes or no)");
                string yesorno = Console.ReadLine();
                string yourDirectory5 = Path.GetDirectoryName(Application.ExecutablePath) + "//settings//";
                string existingFilePath5 = Path.GetDirectoryName(Application.ExecutablePath) + "//settings//";
                string fullFilePath5 = Path.Combine(existingFilePath5);
                if (yesorno == "yes")
                {
                    if (!Directory.Exists(existingFilePath5))
                    {
                        Directory.CreateDirectory(existingFilePath5);
                    }
                    if (!File.Exists(fullFilePath5))
                    {
                        File.WriteAllText(Path.Combine(yourDirectory5, codename + ".solset"), url);
                    }
                    Console.WriteLine("Success! Starting shortly.");
                    System.Threading.Thread.Sleep(2000);
                }
            }
            if (cVersion < oVersion)
            {
                Process.Start("https://consol.cf/update.php");
                uninstall();
                return;
            }
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            if (saved == true)
            {
                Console.WriteLine("Saved code loaded.");
            }
            Console.ForegroundColor = ConsoleColor.White;
            for (; ; )
            {
                {
                    Console.Write("Command: ");
                    cmd = Console.ReadLine();
                    p.Send(cmd);
                    if (cmd == "night")
                    {
                        p.Send(night);
                        Console.WriteLine("Custom command successfully executed\n");
                    }
                    else if (File.Exists(Path.GetDirectoryName(Application.ExecutablePath) + "//commands//" + cmd + ".solcom"))
                    {
                        Console.WriteLine("Custom command successfully executed\n");
                        string yourDirectory = Path.GetDirectoryName(Application.ExecutablePath) + "//commands//";
                        string existingFilePath = Path.GetDirectoryName(Application.ExecutablePath) + "//commands//";
                        string existingFile = cmd + ".solcom";
                        string fullFilePath = Path.Combine(existingFilePath, existingFile);
                        string custom = File.ReadAllText(Path.Combine(yourDirectory, cmd + ".solcom"));
                        p.Send(custom);
                    }
                    else if (cmd == "fps 300")
                    {
                        p.Send(threefps);
                        Console.WriteLine("Custom command successfully executed\n");
                    }
                    else if (cmd == "reload")
                    {
                        Console.WriteLine("Custom command successfully executed\n");
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        Console.WriteLine("Please enter your config's code.\n");
                        url = Console.ReadLine();
                    }
                    else if (cmd == "fps 600")
                    {
                        p.Send(sixfps);
                        Console.WriteLine("Custom command successfully executed\n");
                    }
                    else if (cmd == "fps max")
                    {
                        p.Send(maxfps);
                        Console.WriteLine("Custom command successfully executed\n");
                    }
                    else if (cmd == "quality")
                    {
                        p.Send(quality);
                        Console.WriteLine("Custom command successfully executed\n");
                    }
                    else if (cmd == "hud")
                    {
                        p.Send(hud);
                        Console.WriteLine("Custom command successfully executed\n");
                    }
                    else if (cmd == "greenplayers")
                    {
                        Console.WriteLine("Custom command successfully executed\n");
                        p.Send(greenplayers);
                        Console.WriteLine("Okay, as you can see everything looks all fucked up right now. This is normal.\nJust follow these instructions to get the greenscreened players\nPress enter to continue");
                        Console.ReadLine();
                        Console.WriteLine("Theres only one thing you have to do! Find a part of your game where the player is the only thing that isnt green, essentially making it look like how you want it to, looking at the floor helps with this.\nDon't understand what im trying to say? Type help and a video tutorial will open up.\nOnce this is done press enter.");
                        string needhelp = Console.ReadLine();
                        if (needhelp == "help")
                        {
                            Console.WriteLine("A video by M53H will open shortly explaining how to do this. Ignore the bits about sending a  config, and typing in anything manually. This has all been done for you.");
                            System.Threading.Thread.Sleep(10000);
                            Process.Start("https://www.youtube.com/watch?v=MB9aqxur1Vg");
                        }
                        p.Send("r_lockpvs 1");
                        Console.WriteLine("Success! You are free to move the camera around, play the demo, make your cine, do whatever.\nPress enter when you would like to return to normal");
                        Console.ReadLine();
                        p.Send("r_lockpvs 0");
                        p.Send("r_zfar 0");
                        p.Send(regular);
                    }
                    else if (cmd == "depth")
                    {
                        Console.WriteLine("Custom command successfully executed\n");
                        depthon = !depthon;
                        if (depthon == true)
                        {
                            float dist = 0;
                            gmFog.FogStartDist = dist;
                            float dist2 = 2500;
                            gmFog.FogFadeDist = dist2;
                            float dist3 = 20000;
                            gmFog.FogHeightDist = dist3;
                            float dist4 = 1;
                            gmFog.FogBiasDist = dist4;
                            float R = 1000;
                            float G = 1000;
                            float B = 1000;
                            float A = 100;
                            gmFog.FogBaseColor = new ProcessMemory.Float4(R, G, B, A);
                            float R2 = 1000;
                            float G2 = 1000;
                            float B2 = 1000;
                            float A2 = 103;
                            gmFog.FogFarColor = new ProcessMemory.Float4(R2, G2, B2, A2);
                            p.Send(depth);
                        }
                        else
                        {
                            float dist = 200;
                            gmFog.FogStartDist = dist;
                            float dist2 = 36631;
                            gmFog.FogFadeDist = dist2;
                            float dist3 = 10702;
                            gmFog.FogHeightDist = dist3;
                            float dist4 = 199;
                            gmFog.FogBiasDist = dist4;
                            float R = 5;
                            float G = 5;
                            float B = 5;
                            float A = 1;
                            gmFog.FogBaseColor = new ProcessMemory.Float4(R, G, B, A);
                            float R2 = 5;
                            float G2 = 5;
                            float B2 = 5;
                            float A2 = 1;
                            gmFog.FogFarColor = new ProcessMemory.Float4(R2, G2, B2, A2);
                            p.Send(depthoff);
                        }
                    }
                    else if (cmd == "fogdist")
                    {
                        Console.WriteLine("Custom command successfully executed\n");
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine("Please enter fog distance");
                        float dist = float.Parse(Console.ReadLine());
                        gmFog.FogStartDist = dist;
                    }
                    else if (cmd == "fogfade")
                    {
                        Console.WriteLine("Custom command successfully executed\n");
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine("Please enter fog fade distance");
                        float dist = float.Parse(Console.ReadLine());
                        gmFog.FogFadeDist = dist;
                    }
                    else if (cmd == "fogheight")
                    {
                        Console.WriteLine("Custom command successfully executed\n");
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine("Please enter fog height");
                        float dist = float.Parse(Console.ReadLine());
                        gmFog.FogHeightDist = dist;
                    }
                    else if (cmd == "fogbias")
                    {
                        Console.WriteLine("Custom command successfully executed\n");
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine("Please enter fog bias");
                        float dist = float.Parse(Console.ReadLine());
                        gmFog.FogBiasDist = dist;
                    }
                    else if (cmd == "fogcolor")
                    {
                        Console.WriteLine("Custom command successfully executed\n");
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine("Please enter fog color RED");
                        float R = float.Parse(Console.ReadLine());
                        Console.WriteLine("Now green");
                        float G = float.Parse(Console.ReadLine());
                        Console.WriteLine("Now blue");
                        float B = float.Parse(Console.ReadLine());
                        Console.WriteLine("Now alpha");
                        float A = float.Parse(Console.ReadLine());
                        gmFog.FogBaseColor = new ProcessMemory.Float4(R, G, B, A);
                    }
                    else if (cmd == "fogfarcolor")
                    {
                        Console.WriteLine("Custom command successfully executed\n");
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine("Please enter fog far color RED");
                        float R = float.Parse(Console.ReadLine());
                        Console.WriteLine("Now green");
                        float G = float.Parse(Console.ReadLine());
                        Console.WriteLine("Now blue");
                        float B = float.Parse(Console.ReadLine());
                        Console.WriteLine("Now alpha");
                        float A = float.Parse(Console.ReadLine());
                        gmFog.FogFarColor = new ProcessMemory.Float4(R, G, B, A);
                    }
                    else if (cmd == "greenscreen")
                    {
                        Console.WriteLine("Custom command successfully executed\n");
                        p.Send(greenscreen);
                    }
                    else if (cmd == "regular")
                    {
                        Console.WriteLine("Custom command successfully executed\n");
                        p.Send(regular);
                    }
                    else if (cmd == "animoff")
                    {
                        Console.WriteLine("Command successfully executed");
                        string cmdname = "anim";
                        string disabled = "disabled";
                        string yourDirectory = Path.GetDirectoryName(Application.ExecutablePath) + "//settings//";
                        string existingFilePath = Path.GetDirectoryName(Application.ExecutablePath) + "//settings//";
                        string existingFile = "anim.solset";
                        string fullFilePath = Path.Combine(existingFilePath, existingFile);
                        if (!Directory.Exists(existingFilePath))
                        {
                            Directory.CreateDirectory(existingFilePath);
                        }
                        if (!File.Exists(fullFilePath))
                        {
                            File.WriteAllText(Path.Combine(yourDirectory, cmdname + ".solset"), disabled);
                        }
                    }
                    else if (cmd == "animon")
                    {
                        string yourDirectory = Path.GetDirectoryName(Application.ExecutablePath) + "//settings//";
                        Console.WriteLine("Command successfully executed");
                        System.IO.File.Move(Path.Combine(yourDirectory) + "anim.solset", "animsdisabled");
                    }
                    else if (cmd == "unsave")
                    {
                        string yourDirectory = Path.GetDirectoryName(Application.ExecutablePath) + "//settings//";
                        Console.WriteLine("Command successfully executed");
                        System.IO.File.Move(Path.Combine(yourDirectory) + "code.solset", "codeunsaved");
                    }
                    else if (cmd == "animate")
                    {
                        Console.WriteLine("Custom command successfully executed\n");
                        float startfov;
                        float endfov;
                        Console.WriteLine("What command would you like to use? Please enter the command with no space or number at the end.");
                        string cmdlol = Console.ReadLine();
                        Console.WriteLine("Please choose your starting value:");
                        startfov = Convert.ToSingle(Console.ReadLine());
                        Console.WriteLine("Now the end:");
                        endfov = Convert.ToSingle(Console.ReadLine());
                        Console.WriteLine("What would you like the numbers to increment in?");
                        float inc = Convert.ToSingle(Console.ReadLine());
                        Console.WriteLine("What delay would you like ? (this is the amount of time in miliseconds between each change.)\nWARNING: Having this below a value of 5 may cause your game to crash.");
                        int delay = Convert.ToInt32(Console.ReadLine());
                        Console.WriteLine("Great! Hold F11 to begin the animation, even while still ingame!\nThe animation will stop once it reaces your end value.");
                        for (; ; )
                        {
                            if (startfov == endfov)
                            {
                                break;
                            }
                            if (Convert.ToBoolean((long)GetAsyncKeyState(System.Windows.Forms.Keys.F11) & 0x8000))
                            {
                                {
                                    p.Send(cmdlol + " " + startfov);
                                    if (startfov > endfov)
                                    {
                                        startfov = startfov - inc;
                                    }
                                    else
                                    {
                                        startfov = startfov + inc;
                                    }
                                    System.Threading.Thread.Sleep(delay);
                                }
                            }
                        }
                    }
                    else if (cmd == "greensky")
                    {
                        Console.WriteLine("Custom command successfully executed\n");
                        p.Send(greensky);
                    }
                    else if (cmd == "streams")
                    {
                        Console.WriteLine("Custom command successfully executed\n");
                        Console.WriteLine("Before starting, it is very important that your read ALL text shown.\n");
                        string streamsfps = "";
                        int xres = int.Parse(Screen.PrimaryScreen.Bounds.Width.ToString());
                        int yres = int.Parse(Screen.PrimaryScreen.Bounds.Height.ToString());
                        Console.WriteLine("Please put your game into fullscreen windowed, and have its resolution match the resolution of your monitor.\nPress enter once you have done this.");
                        Console.ReadLine();
                        string folder1 = Path.GetDirectoryName(Application.ExecutablePath) + "//regular//";
                        string folder2 = Path.GetDirectoryName(Application.ExecutablePath) + "//green//";
                        string folder3 = Path.GetDirectoryName(Application.ExecutablePath) + "//depth//";
                        Console.WriteLine("What com_maxfps would you like? (recommended 30, however enter the amount your pc is able to handle)");
                        streamsfps = Console.ReadLine();
                        Console.WriteLine("What timescale would you like? (recommended 0.02)");
                        string streamstimescale = Console.ReadLine();
                        p.Send("com_maxfps " + streamsfps + "\n" + "timescale " + streamstimescale);
                        if (!Directory.Exists(folder1))
                        {
                            Directory.CreateDirectory(folder1);
                        }
                        if (!Directory.Exists(folder2))
                        {
                            Directory.CreateDirectory(folder2);
                        }
                        if (!Directory.Exists(folder3))
                        {
                            Directory.CreateDirectory(folder3);
                        }
                        Console.WriteLine("Would you like to record depth, greenscreen, or both along side the regular?\n(enter either depth, greenscreen, or both. Depth works on cines too!)");
                        string yes = Console.ReadLine();
                        Console.WriteLine("Go ingame and press F11 to start recording. Once finished, press ALT+TAB, then close the CONSOLE to stop recording.\nPlease start the recording WHILE the demo is playing\nAll recordings will be saved to three folders in your exe's directory, named regular, green, and depth.\nDo NOT tab out of your game while recording.");
                        Bitmap memoryImage;
                        memoryImage = new Bitmap(xres, yres);
                        Size s = new Size(memoryImage.Width, memoryImage.Height);
                        if (yes == "both")
                        {
                            for (; ; )
                            {
                                if (Convert.ToBoolean((long)GetAsyncKeyState(System.Windows.Forms.Keys.F11) & 0x8000))
                                {
                                    for (var i = 0; ; i++)
                                    {
                                        SendKeys.SendWait(" ");
                                        p.Send(greenscreen);
                                        string str = "";
                                        Graphics memoryGraphics2 = Graphics.FromImage(memoryImage);
                                        memoryGraphics2.CopyFromScreen(0, 0, 0, 0, s);
                                        str = string.Format(Path.GetDirectoryName(Application.ExecutablePath) + "//regular//" +
                                        $@"\regular{i}.png");
                                        memoryImage.Save(str);
                                        System.Threading.Thread.Sleep(5);
                                        float dist = 200;
                                        gmFog.FogStartDist = dist;
                                        float dist2 = 36631;
                                        gmFog.FogFadeDist = dist2;
                                        float dist3 = 10702;
                                        gmFog.FogHeightDist = dist3;
                                        float dist4 = 199;
                                        gmFog.FogBiasDist = dist4;
                                        float R = 5;
                                        float G = 5;
                                        float B = 5;
                                        float A = 1;
                                        gmFog.FogBaseColor = new ProcessMemory.Float4(R, G, B, A);
                                        float R2 = 5;
                                        float G2 = 5;
                                        float B2 = 5;
                                        float A2 = 1;
                                        gmFog.FogFarColor = new ProcessMemory.Float4(R2, G2, B2, A2);
                                        p.Send(depthoff);
                                        p.Send(regular2);
                                        string str2 = "";
                                        Graphics memoryGraphics = Graphics.FromImage(memoryImage);
                                        memoryGraphics.CopyFromScreen(0, 0, 0, 0, s);
                                        str2 = string.Format(Path.GetDirectoryName(Application.ExecutablePath) + "//green//" +
                                        $@"\green{i}.png");
                                        memoryImage.Save(str2);
                                        System.Threading.Thread.Sleep(5);
                                        dist = 0;
                                        gmFog.FogStartDist = dist;
                                        dist2 = 2500;
                                        gmFog.FogFadeDist = dist2;
                                        dist3 = 20000;
                                        gmFog.FogHeightDist = dist3;
                                        dist4 = 1;
                                        gmFog.FogBiasDist = dist4;
                                        R = 1000;
                                        G = 1000;
                                        B = 1000;
                                        A = 100;
                                        gmFog.FogBaseColor = new ProcessMemory.Float4(R, G, B, A);
                                        R2 = 1000;
                                        G2 = 1000;
                                        B2 = 1000;
                                        A2 = 103;
                                        gmFog.FogFarColor = new ProcessMemory.Float4(R2, G2, B2, A2);
                                        p.Send(depth);
                                        System.Threading.Thread.Sleep(266);
                                        string str4 = "";
                                        Graphics memoryGraphics4 = Graphics.FromImage(memoryImage);
                                        memoryGraphics4.CopyFromScreen(0, 0, 0, 0, s);
                                        str4 = string.Format(Path.GetDirectoryName(Application.ExecutablePath) + "//depth//" +
                                        $@"\depth{i}.png");
                                        memoryImage.Save(str4);
                                        SendKeys.SendWait(" ");
                                        dist = 200;
                                        gmFog.FogStartDist = dist;
                                        dist2 = 36631;
                                        gmFog.FogFadeDist = dist2;
                                        dist3 = 10702;
                                        gmFog.FogHeightDist = dist3;
                                        dist4 = 199;
                                        gmFog.FogBiasDist = dist4;
                                        R = 5;
                                        G = 5;
                                        B = 5;
                                        A = 1;
                                        gmFog.FogBaseColor = new ProcessMemory.Float4(R, G, B, A);
                                        R2 = 5;
                                        G2 = 5;
                                        B2 = 5;
                                        A2 = 1;
                                        gmFog.FogFarColor = new ProcessMemory.Float4(R2, G2, B2, A2);
                                        p.Send(depthoff);
                                        p.Send(regular2);
                                        System.Threading.Thread.Sleep(250);

                                    }
                                }
                            }
                        }
                        else if (yes == "greenscreen")
                        {
                            Console.WriteLine("Custom command successfully executed\n");
                            for (; ; )
                            {
                                if (Convert.ToBoolean((long)GetAsyncKeyState(System.Windows.Forms.Keys.F12) & 0x8000))
                                {
                                    break;
                                }
                                if (Convert.ToBoolean((long)GetAsyncKeyState(System.Windows.Forms.Keys.F11) & 0x8000))
                                {
                                    for (var i = 0; ; i++)
                                    {
                                        SendKeys.SendWait(" ");
                                        p.Send(greenscreen);
                                        string str = "";
                                        Graphics memoryGraphics2 = Graphics.FromImage(memoryImage);
                                        memoryGraphics2.CopyFromScreen(0, 0, 0, 0, s);
                                        str = string.Format(Path.GetDirectoryName(Application.ExecutablePath) + "//regular//" +
                                        $@"\regular{i}.png");
                                        memoryImage.Save(str);
                                        System.Threading.Thread.Sleep(4);
                                        p.Send(regular2);
                                        string str2 = "";
                                        Graphics memoryGraphics = Graphics.FromImage(memoryImage);
                                        memoryGraphics.CopyFromScreen(0, 0, 0, 0, s);
                                        str2 = string.Format(Path.GetDirectoryName(Application.ExecutablePath) + "//green//" +
                                        $@"\green{i}.png");
                                        memoryImage.Save(str2);
                                        SendKeys.SendWait(" ");
                                        System.Threading.Thread.Sleep(250);

                                    }
                                }
                            }
                        }
                        else if (yes == "depth")
                        {
                            Console.WriteLine("Custom command successfully executed\n");
                            for (; ; )
                            {
                                if (Convert.ToBoolean((long)GetAsyncKeyState(System.Windows.Forms.Keys.F12) & 0x8000))
                                {
                                    break;
                                }
                                if (Convert.ToBoolean((long)GetAsyncKeyState(System.Windows.Forms.Keys.F11) & 0x8000))
                                {
                                    for (var i = 0; ; i++)
                                    {
                                        SendKeys.SendWait(" ");
                                        float dist = 200;
                                        gmFog.FogStartDist = dist;
                                        float dist2 = 36631;
                                        gmFog.FogFadeDist = dist2;
                                        float dist3 = 10702;
                                        gmFog.FogHeightDist = dist3;
                                        float dist4 = 199;
                                        gmFog.FogBiasDist = dist4;
                                        float R = 5;
                                        float G = 5;
                                        float B = 5;
                                        float A = 1;
                                        gmFog.FogBaseColor = new ProcessMemory.Float4(R, G, B, A);
                                        float R2 = 5;
                                        float G2 = 5;
                                        float B2 = 5;
                                        float A2 = 1;
                                        gmFog.FogFarColor = new ProcessMemory.Float4(R2, G2, B2, A2);
                                        p.Send(depthoff);
                                        p.Send(regular2);
                                        string str2 = "";
                                        Graphics memoryGraphics = Graphics.FromImage(memoryImage);
                                        memoryGraphics.CopyFromScreen(0, 0, 0, 0, s);
                                        str2 = string.Format(Path.GetDirectoryName(Application.ExecutablePath) + "//regular//" +
                                        $@"\regular{i}.png");
                                        memoryImage.Save(str2);
                                        System.Threading.Thread.Sleep(5);
                                        dist = 0;
                                        gmFog.FogStartDist = dist;
                                        dist2 = 2500;
                                        gmFog.FogFadeDist = dist2;
                                        dist3 = 20000;
                                        gmFog.FogHeightDist = dist3;
                                        dist4 = 1;
                                        gmFog.FogBiasDist = dist4;
                                        R = 1000;
                                        G = 1000;
                                        B = 1000;
                                        A = 100;
                                        gmFog.FogBaseColor = new ProcessMemory.Float4(R, G, B, A);
                                        R2 = 1000;
                                        G2 = 1000;
                                        B2 = 1000;
                                        A2 = 103;
                                        gmFog.FogFarColor = new ProcessMemory.Float4(R2, G2, B2, A2);
                                        p.Send(depth);
                                        p.Send("cg_drawgun 0");
                                        System.Threading.Thread.Sleep(225);
                                        string str4 = "";
                                        Graphics memoryGraphics4 = Graphics.FromImage(memoryImage);
                                        memoryGraphics4.CopyFromScreen(0, 0, 0, 0, s);
                                        str4 = string.Format(Path.GetDirectoryName(Application.ExecutablePath) + "//depth//" +
                                        $@"\depth{i}.png");
                                        memoryImage.Save(str4);
                                        SendKeys.SendWait(" ");
                                        dist = 200;
                                        gmFog.FogStartDist = dist;
                                        dist2 = 36631;
                                        gmFog.FogFadeDist = dist2;
                                        dist3 = 10702;
                                        gmFog.FogHeightDist = dist3;
                                        dist4 = 199;
                                        gmFog.FogBiasDist = dist4;
                                        R = 5;
                                        G = 5;
                                        B = 5;
                                        A = 1;
                                        gmFog.FogBaseColor = new ProcessMemory.Float4(R, G, B, A);
                                        R2 = 5;
                                        G2 = 5;
                                        B2 = 5;
                                        A2 = 1;
                                        gmFog.FogFarColor = new ProcessMemory.Float4(R2, G2, B2, A2);
                                        p.Send(depthoff);
                                        p.Send(regular2);
                                        System.Threading.Thread.Sleep(250);

                                    }
                                }
                            }
                        }
                    }
                    else if (cmd == "avidemo")
                    {
                        Console.WriteLine("Custom command successfully executed\n");
                        Console.WriteLine("");
                        Console.WriteLine("Hello, thank you for using AVI demo. This also works with cines!");
                        Console.WriteLine("Please enter your monitors resolution (RECORD WITH YOUR GAME IN FULLSCREEN WINDOWED)");
                        Console.WriteLine("X:");
                        int xres = Convert.ToInt32(Console.ReadLine());
                        Console.WriteLine("Y:");
                        int yres = Convert.ToInt32(Console.ReadLine());
                        Console.WriteLine("What com_maxfps would you like to record at? (use what your pc can run at)");
                        string avifps = Console.ReadLine();
                        Console.WriteLine("What timescale would you like to record at?(recommened 0.02)");
                        string avitimescale = Console.ReadLine();
                        Console.WriteLine("Thank you, please put your game into fullscreen windowed, and have its resolution match the resolution of your monitor.\nPress enter once you have done this.");
                        Console.ReadLine();
                        string folder1 = Path.GetDirectoryName(Application.ExecutablePath) + "//avidemo//";
                        p.Send("com_maxfps " + avifps + "\n" + "timescale " + avitimescale);
                        if (!Directory.Exists(folder1))
                        {
                            Directory.CreateDirectory(folder1);
                        }
                        else if (Directory.Exists(folder1))
                        {
                            Directory.CreateDirectory(folder1);
                        }
                        Console.WriteLine("Go ingame and press F11 to start recording. Once finished, press ALT+TAB, then close CONSOL to stop.\nPlease start the recording while the demo is PLAYING\nAll recordings will be saved to a folder in your exe's directory, named avidemo.\nOnce done recording, please rename the 'avidemo' folder if you wish to record again.\nDo NOT tab out of your game while recording.");
                        Bitmap memoryImage;
                        memoryImage = new Bitmap(xres, yres);
                        Size s = new Size(memoryImage.Width, memoryImage.Height);
                        WebConfigReader conf =
                        new WebConfigReader(urlprefix + url + urlsuffix);
                        string[] tokens = Regex.Split(conf.ReadString(), @"\r?\n|\r");
                        foreach (string s2 in tokens)
                            p.Send(s2);
                        for (; ; )
                        {
                            if (Convert.ToBoolean((long)GetAsyncKeyState(System.Windows.Forms.Keys.F12) & 0x8000))
                            {
                                break;
                            }
                            if (Convert.ToBoolean((long)GetAsyncKeyState(System.Windows.Forms.Keys.F11) & 0x8000))
                            {
                                for (var i = 0; ; i++)
                                {
                                    SendKeys.SendWait(" ");
                                    string str = "";
                                    Graphics memoryGraphics2 = Graphics.FromImage(memoryImage);
                                    memoryGraphics2.CopyFromScreen(0, 0, 0, 0, s);
                                    str = string.Format(Path.GetDirectoryName(Application.ExecutablePath) + "//avidemo//" +
                                    $@"\avidemo{i}.png");
                                    memoryImage.Save(str);
                                    SendKeys.SendWait(" ");
                                    System.Threading.Thread.Sleep(250);
                                }
                            }
                        }
                    }
                    else if (cmd == "exec")
                    {
                        Console.WriteLine("CFG successfully executed\n");
                        WebConfigReader conf =
                        new WebConfigReader(urlprefix + url + urlsuffix);
                        string[] tokens = Regex.Split(conf.ReadString(), @"\r?\n|\r");
                        foreach (string s in tokens)
                            p.Send(s);
                    }
                    else if (cmd == "e")
                    {
                        Console.WriteLine("CFG successfully executed\n");
                        WebConfigReader conf =
                        new WebConfigReader(urlprefix + url + urlsuffix);
                        string[] tokens = Regex.Split(conf.ReadString(), @"\r?\n|\r");
                        foreach (string s in tokens)
                            p.Send(s);
                    }
                    else if (cmd == "")
                    {
                        Console.WriteLine("CFG successfully executed\n");
                        WebConfigReader conf =
                        new WebConfigReader(urlprefix + url + urlsuffix);
                        string[] tokens = Regex.Split(conf.ReadString(), @"\r?\n|\r");
                        foreach (string s in tokens)
                            p.Send(s);
                    }
                    else if (cmd == "help")
                    {
                        Console.WriteLine("Custom command successfully executed\n");
                        Process.Start("https://consol.cf/tutorial/");
                    }
                    else if (cmd == "commands")
                    {
                        Console.WriteLine("Custom command successfully executed\n");
                        Process.Start("http://consol.cf/commands");
                    }
                    else if (cmd == "ingame")
                    {
                        Console.WriteLine("Custom command successfully executed\n");
                        Console.WriteLine("");
                        Console.WriteLine("Press F11 to enter config even while tabbed out. Tab back into CONSOL then press F12 to go back to normal.");

                        for (; ; )
                        {
                            WebConfigReader conf =
                        new WebConfigReader(urlprefix + url + urlsuffix);
                            if (Convert.ToBoolean((long)GetAsyncKeyState(System.Windows.Forms.Keys.F12) & 0x8000))
                            {
                                break;
                            }

                            if (Convert.ToBoolean((long)GetAsyncKeyState(System.Windows.Forms.Keys.F11) & 0x8000))
                            {
                                string[] tokens = Regex.Split(conf.ReadString(), @"\r?\n|\r");
                                foreach (string s in tokens)
                                {
                                    p.Send(s);
                                }
                                debug = !debug;
                                Task.Delay(300);
                            }

                            Task.Delay(100);
                        }
                    }
                    else if (cmd == "custom")
                    {
                        Console.WriteLine("Custom command successfully executed\n");
                        Console.WriteLine("Please enter your command like this: cmd1; cmd2; cmd3");
                        string customcmd = Console.ReadLine();
                        Console.WriteLine("Please name your command");
                        string cmdname = Console.ReadLine();
                        Console.WriteLine("Thank you, press enter to confirm");
                        string yourDirectory = Path.GetDirectoryName(Application.ExecutablePath) + "//commands//";
                        string existingFilePath = Path.GetDirectoryName(Application.ExecutablePath) + "//commands//";
                        string existingFile = Console.ReadLine() + ".solcom";
                        string fullFilePath = Path.Combine(existingFilePath, existingFile);
                        if (!Directory.Exists(existingFilePath))
                        {
                            Directory.CreateDirectory(existingFilePath);
                        }

                        if (File.Exists(fullFilePath))
                        {
                            MessageBox.Show("Cannot create a command that already exists!", "CONSOL");
                        }

                        if (!File.Exists(fullFilePath))
                        {
                            File.WriteAllText(Path.Combine(yourDirectory, cmdname + ".solcom"), customcmd);
                        }
                    }
                }
            }
        }
    }
}
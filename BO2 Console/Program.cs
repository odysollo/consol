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
            catch (Exception ex)
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
                cbuf_address = 0x5BDF70;
                nop_address = 0x8C90DA;
                dwPID = Process.GetProcessesByName("t6mpv43")[0].Id;
            }
            else
            {
                cbuf_address = 0x0;
                nop_address = 0x0;
                Console.WriteLine("No game found.");
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
            string hud = "hud_health_pulserate_critical 0\n " +
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
                "r_blur 0\n" +
                "r_detail 1\n" +
                "r_distortion 1\n" +
                "r_drawWater 1\n" +
                "r_forceLod 0\n" +
                "r_lodBiasRigid - 1000\n" +
                "r_lodBiasSkinned - 1000\n" +
                "r_lodScaleRigid 1\n" +
                "r_lodScaleSkinned 1\n" +
                "r_normalMap 1\n" +
                "r_rendererinuse shader model 3.0\n" +
                "r_rendererPreference shader model 3.0";
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
                "r_drawdecals 1\n";
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
    "r_drawdecals 1\n" + "cg_drawgun 0";
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
                "r_bloomtweaks 1";
            string depthoff = "cg_draw2d 1\n " +
                "r_enablePlayerShadow 0\n " +
                "r_exposureTweak 0\n " +
                "r_exposureValue 12.5\n " +
                "r_sky_intensity_factor 1\n " +
                "r_modellimit 1024\n " +
                "r_clearcolor 1 1 1 0\n " +
                "r_clearcolor2 1 1 1 0\n " +
                "r_bloomtweaks 0";
            string greenplayers = "r_zfar 1\n" + "r_lockPvs 0\n" + "r_modelLimit 0\n" + "r_clearColor 0 1 0 1\n" + "r_clearColor2 0 1 0 1\n" + "r_bloomTweaks 1";
            Console.WriteLine("Please enter your config's code.\n" +
                "Inorder to get your code, please upload your config on http://consol.cf\n" +
                "Need help? Enter 0000 as your code, then type 'help' for a how-to-use\nAnd 'commands' for a full commands list.\n" +
                "Enter code Here: ");
            string url = Console.ReadLine();
            string urlprefix = "http://consol.cf/configs/";
            string urlsuffix = ".cfg";
            int cVersion = 14;
            int oVersion;
            string cmd2 = "xd";
            string XMLFileLocation = "https://github.com/odysollo/consol/raw/master/version.xml";
            bool debug = false;
            XDocument doc = XDocument.Load(XMLFileLocation);
            var VersionElement = doc.Descendants("Version");
            oVersion = Convert.ToInt32(string.Concat(VersionElement.Nodes()));
            if (cVersion < oVersion)
            {
                Process.Start("https://consol.cf/update.php");
                uninstall();
                return;
            }
            for (; ; )
            {
                {
                    Console.WriteLine("Please type in a command, or press ENTER to execute your config.");
                    cmd = Console.ReadLine();
                    p.Send(cmd);
                    Console.WriteLine("Command successfully executed\n");
                    if (cmd == "switch")
                    {
                        debug = !debug;
                    }
                    //else if (cmd != cmd2)
                    //{
                        //Console.WriteLine("incorrect command");
                        //Console.ReadLine();
                    //}
                    else if (cmd == "night")
                    {
                        p.Send(night);
                    }
                    else if (File.Exists(Path.GetDirectoryName(Application.ExecutablePath) + "//commands//" + cmd + ".solcom"))
                    {
                        string yourDirectory = Path.GetDirectoryName(Application.ExecutablePath) + "//commands//";
                        string existingFilePath = Path.GetDirectoryName(Application.ExecutablePath) + "//commands//";
                        string existingFile = cmd + ".solcom";
                        string fullFilePath = Path.Combine(existingFilePath, existingFile);
                        string chirag = File.ReadAllText(Path.Combine(yourDirectory, cmd + ".solcom"));
                        p.Send(chirag);
                    }
                    else if (cmd == "fps 300")
                    {
                        p.Send(threefps);
                    }
                    else if (cmd == "fps 600")
                    {
                        p.Send(sixfps);
                    }
                    else if (cmd == "fps max")
                    {
                        p.Send(maxfps);
                    }
                    else if (cmd == "quality")
                    {
                        p.Send(quality);
                    }
                    else if (cmd == "hud")
                    {
                        p.Send(hud);
                    }
                    else if (cmd == "greenplayers2")
                    {

                    }
                    else if (cmd == "greenplayers")
                    {
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
                    }
                    else if (cmd == "depth")
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
                    else if (cmd == "fogdist")
                    {
                        Console.WriteLine("Please enter fog distance");
                        float dist = float.Parse(Console.ReadLine());
                        gmFog.FogStartDist = dist;
                    }
                    else if (cmd == "fogfade")
                    {
                        Console.WriteLine("Please enter fog fade distance");
                        float dist = float.Parse(Console.ReadLine());
                        gmFog.FogFadeDist = dist;
                    }
                    else if (cmd == "fogheight")
                    {
                        Console.WriteLine("Please enter fog height");
                        float dist = float.Parse(Console.ReadLine());
                        gmFog.FogHeightDist = dist;
                    }
                    else if (cmd == "fogbias")
                    {
                        Console.WriteLine("Please enter fog bias");
                        float dist = float.Parse(Console.ReadLine());
                        gmFog.FogBiasDist = dist;
                    }
                    else if (cmd == "fogcolor")
                    {
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
                    else if (cmd == "depthoff")
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
                    else if (cmd == "greenscreen")
                    {
                        p.Send(greenscreen);
                    }
                    else if (cmd == "regular")
                    {
                        p.Send(regular);
                    }
                    else if (cmd == "greensky")
                    {
                        p.Send(greensky);
                    }
                    else if (cmd == "streams")
                    {
                        string streamsfps = "";
                        Console.WriteLine("Hello, thank you for testing out the streams beta. Please note, your PC must be able to run the game at atleast 10 FPS to use.\nThis will be recorded to roughly 1000fps.");
                        Console.WriteLine("Please enter your monitors resolution (RECORD WITH YOUR GAME IN FULLSCREEN WINDOWED)");
                        Console.WriteLine("X:");
                        int xres = Convert.ToInt32(Console.ReadLine());
                        Console.WriteLine("Y:");
                        int yres = Convert.ToInt32(Console.ReadLine());
                        //Console.WriteLine("Please enter the command you would like to use for the first stream (i.e. greenscreen, or a custom command)");
                        //string streamscmd1 = Console.ReadLine();
                        //Console.WriteLine("Now the second (i.e. regular, or a custom command)");
                        //string streamscmd2 = Console.ReadLine();
                        Console.WriteLine("Thank you, please put your game into fullscreen windowed, and have its resolution match the resolution of your monitor.\nPress enter once you have done this.");
                        Console.ReadLine();
                        string folder1 = Path.GetDirectoryName(Application.ExecutablePath) + "//regular//";
                        string folder2 = Path.GetDirectoryName(Application.ExecutablePath) + "//green//";
                        string folder3 = Path.GetDirectoryName(Application.ExecutablePath) + "//depth//";
                        Console.WriteLine("What com_maxfps would you like? (recommended 10-30)");
                        streamsfps = Console.ReadLine();
                        Console.WriteLine("What timescale would you like? (recommended 0.03-0.05)");
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
                        Console.WriteLine("Go ingame and press F11 to start recording. Once finished, press ALT+TAB, then close the console. to stop.\nPlease start the recording WHILE the demo is playing\nAll recordings will be saved to three folders in your exe's directory, named regular, green, and depth.\nDo NOT tab out of your game while recording.");
                        Bitmap memoryImage;
                        memoryImage = new Bitmap(xres, yres);
                        Size s = new Size(memoryImage.Width, memoryImage.Height);
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
                    /*
                    else if (cmd == "avidemo")
                    {
                        Console.WriteLine("Hello, thank you for using AVI demo. Please note, your PC must be able to run the game at atleast 6 FPS to use. 600 fps footage will be produced.\nThis also works with cines!");
                        Console.WriteLine("Please enter your monitors resolution (RECORD WITH YOUR GAME IN FULLSCREEN WINDOWED)");
                        Console.WriteLine("X:");
                        int xres = Convert.ToInt32(Console.ReadLine());
                        Console.WriteLine("Y:");
                        int yres = Convert.ToInt32(Console.ReadLine());
                        Console.WriteLine("What com_maxfps would you like to record at?");
                        string avifps = Console.ReadLine();
                        Console.WriteLine("What timescale would you like to record at?");
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
                                        string str = "";
                                        Graphics memoryGraphics2 = Graphics.FromImage(memoryImage);
                                        memoryGraphics2.CopyFromScreen(0, 0, 0, 0, s);
                                        str = string.Format(Path.GetDirectoryName(Application.ExecutablePath) + "//avidemo//" +
                                        $@"\avidemo{i}.png");
                                        memoryImage.Save(str);
                                    }
                                }
                            }
                    }
                    */
                    else if (cmd == "exec")
                    {
                        WebConfigReader conf =
                        new WebConfigReader(urlprefix + url + urlsuffix);
                        string[] tokens = Regex.Split(conf.ReadString(), @"\r?\n|\r");
                        foreach (string s in tokens)
                            p.Send(s);
                    }
                    else if (cmd == "e")
                    {
                        WebConfigReader conf =
                        new WebConfigReader(urlprefix + url + urlsuffix);
                        string[] tokens = Regex.Split(conf.ReadString(), @"\r?\n|\r");
                        foreach (string s in tokens)
                            p.Send(s);
                    }
                    else if (cmd == "")
                    {
                        WebConfigReader conf =
                        new WebConfigReader(urlprefix + url + urlsuffix);
                        string[] tokens = Regex.Split(conf.ReadString(), @"\r?\n|\r");
                        foreach (string s in tokens)
                            p.Send(s);
                    }
                    else if (cmd == "help")
                    {
                        Process.Start("http://consol.cf/tutorial.php");
                    }
                    else if (cmd == "commands")
                    {
                        Process.Start("http://consol.cf/commands.php");
                    }
                    else if (cmd == "ingame")
                    //this currenly works, all thats needed is the ability to enter a command to stop loop.
                    {


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
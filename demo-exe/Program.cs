using NLog;
using NDesk.Options;
using NLog.Config;
using System.Diagnostics;


namespace demo
{
    public class Program
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        static bool showHelp = false;
        static bool demo = false;
        static string loglevel = "info";
        static string sourcePath = "";
        static string end = "";

        public static void Help(OptionSet p)
        {
            Console.WriteLine("Usage: dotnet run [OPTIONS]");
            Console.WriteLine();
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
        }

        public static void Demo()
        {
            var emu = new RV32I.Emulator();
            emu.Reset();

            // test binary in sample-binary-001.txt
            // 80000000 <boot>:
            // 80000000: 93 00 00 00   li      ra, 0
            // # x1 == ra == 0
            // 80000004: 13 04 00 00   li      s0, 0
            // # x8 == s0/fp == 0
            // 80000008: 37 45 00 00   lui     a0, 4
            // # x10 == a0 == 4<<12 == 16384
            emu.Load("./data/sample-binary-001.txt");

            emu.Step();
            emu.Step();
            emu.Step();
            emu.Dump();

            logger.Info("* Converting txt to bin");

            var loader = new RV32I.Loader();
            loader.TextToBinary("./data/sample-binary-003.txt", "./data/sample-binary-003.bin");

            logger.Info("* Running the bin");

            emu.Reset();
            emu.Load("./data/sample-binary-003.bin");

            logger.Info("*** code ***");

            for (int i = 0; i < 0x108; i += 4)
            {
                logger.Info($"0x{i:X8}: 0x{emu.Memory[i + 3]:X2}{emu.Memory[i + 2]:X2}{emu.Memory[i + 1]:X2}{emu.Memory[i]:X2},");
            }

            logger.Info("*** code end ***");

            emu.StepUntil(0x18);
            emu.Dump();
            emu.StepUntil(0x1c);
        }

        public static void Run(string sourcePath, uint end)
        {
            var emu = new RV32I.Emulator();
            emu.Reset();

            logger.Info($"* Loading {sourcePath}");
            emu.Load(sourcePath);

            logger.Info($"* Running until 0x{end:X8}");
            Stopwatch sw = new Stopwatch();
            sw.Reset();
            sw.Start();
            emu.StepUntil(end);
            sw.Stop();

            emu.Dump();
            long t = sw.ElapsedMilliseconds;
            logger.Info("* Elapsed time: {0} ms", t);
        }

        public static void Main(string[] args)
        {
            var p = new OptionSet() {
            { "l|loglevel=", "loglevel: info, trace, debug, warn, error", v => loglevel = v },
            { "s|sourcepath=", "source file path", v => sourcePath = v },
            { "e|end=", "address of the source to stop execution", v => end = v },
            { "d|demo",  "run a demo program", v => demo = v != null },
            { "h|help",  "show help", v => showHelp = v != null },
        };
            List<string> extra;
            try
            {
                extra = p.Parse(args);
            }
            catch (OptionException e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `dotnet run -- --help' for more information.");
                return;
            }

            if (showHelp)
            {
                Help(p);
                return;
            }

            LogManager.Configuration = new LoggingConfiguration();
            LogManager.Configuration.LoggingRules.Clear();

            var ll = LogLevel.Info;
            if (loglevel.Length > 0)
            {
                ll = LogLevel.FromString(loglevel);
            }

            LogManager.Configuration.LoggingRules.Add(new LoggingRule("*", ll, new NLog.Targets.ConsoleTarget()));
            LogManager.ReconfigExistingLoggers();

            logger.Info("* Started");
            logger.Info("* Running a small program");

            if (demo)
            {
                Demo();
            }
            else
            {
                uint uintEnd;
                if (end.StartsWith("0x"))
                {
                    uintEnd = Convert.ToUInt32(end, 16);
                }
                else
                {
                    uintEnd = Convert.ToUInt32(end, 10);
                }
                Run(sourcePath, uintEnd);
            }

            logger.Info("* Completed");
        }
    }
}
using System;
using System.IO;
using NLog;
using NLog.Config;

public class Program
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    public static void Main()
    {
        LogManager.Configuration = new LoggingConfiguration();
        LogManager.Configuration.LoggingRules.Clear();
        LogManager.Configuration.LoggingRules.Add(new LoggingRule("*", LogLevel.Info, new NLog.Targets.ConsoleTarget()));
        // LogManager.Configuration.LoggingRules.Add(new LoggingRule("*", LogLevel.Trace, new NLog.Targets.ConsoleTarget()));
        LogManager.ReconfigExistingLoggers();

        logger.Info("* Started");
        logger.Info("* Running a small program");

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

        logger.Info("* Completed");
    }
}
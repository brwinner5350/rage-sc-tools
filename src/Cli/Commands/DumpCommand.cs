﻿namespace ScTools.Cli.Commands;

using System;
using System.CommandLine;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CodeWalker.GameFiles;
using GameFiles;
using ScriptLang.Workspace;

internal static class DumpCommand
{
    public static readonly Argument<FileGlob[]> Input = new Argument<FileGlob[]>(
            "input",
            parse: Parsers.ParseFileGlobs,
            description: "The input script files. Supports glob patterns.")
            .AtLeastOne();
    public static  readonly Option<DirectoryInfo> Output = new Option<DirectoryInfo>(
            new[] { "--output", "-o" },
            () => new DirectoryInfo(".\\"),
            "The output directory.")
            .ExistingOnly();
    public static readonly Option<BuildTarget> Target = new Option<BuildTarget>(
            new[] { "--target", "-t" },
            parseArgument: Parsers.ParseBuildTarget,
            description: "The target game.");
    public static Command Command { get; } = BuildCommand();

    private static Command BuildCommand()
    {
        var cmd = new Command("dump")
        {
            Input,
            Output,
            Target,
        };
        cmd.SetHandler(InvokeAsync, Input, Output, Target);
        return cmd;
    }

    public static async Task InvokeAsync(FileGlob[] input, DirectoryInfo output, BuildTarget target)
    {
        static void Print(string str)
        {
            lock (Console.Out)
            {
                Console.WriteLine(str);
            }
        }

        var totalTime = Stopwatch.StartNew();

        var inputFiles = input.SelectMany(i => i.Matches);
        await Parallel.ForEachAsync(inputFiles, async (inputFile, cancellationToken) =>
        {
            var extension = "dump.txt";
            
            var outputFile = new FileInfo(Path.Combine(output.FullName, Path.ChangeExtension(inputFile.Name, extension)));
            try
            {
                Print($"Dumping '{inputFile}'...");
                var source = await File.ReadAllBytesAsync(inputFile.FullName, cancellationToken);

                switch (target)
                {
                    case (Game.GTAV, Platform.x64):
                        throw new NotImplementedException("GTAV x64 not supported");

                    case (Game.MC4, Platform.Xenon):
                    case (Game.GTAIV, Platform.x86):
                    {
                        byte[] aesKey = null!;
                        if (target.Game == Game.GTAIV)
                        {
                            Keys.NY.Load("D:\\programs\\SteamLibrary\\steamapps\\common\\Grand Theft Auto IV\\GTAIV\\GTAIV.exe");
                            aesKey = Keys.NY.AesKeyPC;
                        }
                        else if (target.Game == Game.MC4)
                        {
                            Keys.MC4.Load("");
                            aesKey = Keys.MC4.AesKeyXenon;
                        }

                        var sc = new ScriptNY();
                        sc.Read(new DataReader(new MemoryStream(source)), aesKey);
                        await using var outputStream = outputFile.OpenWrite();
                        await using var outputWriter = new StreamWriter(outputStream);
                        sc.Dump(DumpOptions.Default(outputWriter));
                        break;
                    }
                    
                    default:
                        throw new InvalidOperationException($"Unsupported build target '{target}'");
                }
            }
            catch (Exception e)
            {
                lock (Console.Error)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Error.Write(e.ToString());
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }
        });

        totalTime.Stop();
        Print($"Total time: {totalTime.Elapsed}");
    }
}

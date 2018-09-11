﻿using System;
using System.CodeDom.Compiler;
using TouchStar.Resgenx.ResXGenerators;
using Mono.Options;

namespace TouchStar.Resgenx
{
    internal class Resgenx
    {
        public static int Main(string[] args)
        {
            if (!TryParseArgs(args, out var generatorOptions))
                return 1;

            var result = new ResXGeneratorResult();
            ResXFileCodeGenerator.GenerateFile(generatorOptions, result).GetAwaiter().GetResult();

            var exitCode = (result.Success || result.SuccessWithWarnings) ? 0 : 1;
            if (result.UnhandledException != null)
            {
                Console.Error.WriteLine(result.UnhandledException.Message);
                Console.Error.WriteLine(result.UnhandledException.StackTrace);
            }

            foreach (CompilerError error in result.Errors)
            {
                Console.Error.WriteLine(error);
            }

            return exitCode;
        }

        static bool TryParseArgs(string[] args, out ResXGeneratorOptions generatorOptions)
        {
            var showHelp = false;
            var generatePublicClass = false;
            var targetPcl2Framework = false;
            var className = "";
            var outFilePath = "";

            var cmdLineOptions = new OptionSet {
                {"h|help|?", "show this message and exit", h => showHelp = h != null },
                {"publicClass", "generate a 'public' class instead of an 'internal' class", publicClass => generatePublicClass = publicClass != null },
                {"pcl2", "generate a class that can be used in projects that target the PCL 2 framework, .Net Core 1.x or .Net Standard 1.x", pcl2 => targetPcl2Framework = pcl2 != null },
                {"n|className=", "name of the class to be generated", n => className = n },
                {"o|outputPath=", "path to write the generated file", o => outFilePath = o }
            };

            generatorOptions = null;
            if (args.Length < 2)
            {
                ShowHelp(cmdLineOptions);
                return false;
            }

            try
            {
                var extras = cmdLineOptions.Parse(args);
                if (showHelp || extras.Count < 2)
                {
                    ShowHelp(cmdLineOptions);
                    return false;
                }

                generatorOptions = new ResXGeneratorOptions(extras[0], extras[1])
                {
                    ResultClassName = className,
                    OutputFilePath = outFilePath,
                    GenerateInternalClass = !generatePublicClass,
                    TargetPcl2Framework = targetPcl2Framework
                };

                return true;
            }
            catch (OptionException e)
            {
                Console.Error.WriteLine(e.Message);
            }
            return false;
        }

        static void ShowHelp(OptionSet options)
        {
            Console.WriteLine(@"
USAGE: resgenx [OPTIONS] input_file namespace
Converts the given XML based resource file (.resx) to a strongly typed resource class file in C#.

OPTIONS:");
            options.WriteOptionDescriptions(Console.Out);
        }
    }
}
// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

namespace VersionFileGenerator
{
    public class Program
    {
        private class Configuration
        {
            [CommandLine.Option("solution", Required = true, HelpText = "$(SolutionDir)")]
            public String SolutionDirectoryPath { get; set; }

            public static Configuration Create(String[] commandLineArguments)
            {
                var defaultParser = CommandLine.Parser.Default;
                if (defaultParser == null)
                    throw new NullReferenceException("defaultParser");

                var configuration = new Configuration();
                defaultParser.ParseArgumentsStrict(commandLineArguments, configuration);

                return configuration;
            }
        }

        static void Main(String[] commandLineArguments)
        {
            var configuration = Configuration.Create(commandLineArguments);
            var destinationPath = GetDestinationPath(configuration.SolutionDirectoryPath);
            var contents = GenerateFileContents();
            File.WriteAllText(destinationPath, contents);
        }

        private static String GenerateFileContents()
        {
            var assemblyLocation = Assembly.GetExecutingAssembly().Location;
            var fileVersion = FileVersionInfo.GetVersionInfo(assemblyLocation).FileVersion;
            var version = new Version(fileVersion);
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(@"// Generated by the MSBuild target CreateGitVersion defined in NewRelic.Common.proj.");
            stringBuilder.AppendLine(@"// It is recommended that this file be left out of versioning as it changes after every commit.");
            stringBuilder.AppendLine(String.Format(@"#define NEWRELIC_VERSION {0},{1},{2},{3}", version.Major, version.Minor, version.Build, version.Revision));
            stringBuilder.AppendLine(String.Format(@"#define NEWRELIC_VERSION_STRING ""{0}.{1}.{2}.{3}""", version.Major, version.Minor, version.Build, version.Revision));
            return stringBuilder.ToString();
        }

        private static String GetDestinationPath(String solutionDirectoryPath)
        {
            return Path.Combine(solutionDirectoryPath, Path.Combine("Profiler", "VersionInfo.h"));
        }
    }
}

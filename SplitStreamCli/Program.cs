using SplitStreamLib;
using SplitStreamLib.Recording;
using SplitStreamLib.Streaming;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SplitStreamCli
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                try
                {
                    var options = OptionsFromArgs();
                    var recorder = new Recorder(
                        options
                    );

                    var mp3stream = new Mp3Stream(
                        options.Url
                    );

                    recorder.ProcessFrames(
                        mp3stream.GetFrames()
                    );
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    Console.WriteLine("Restarting...");
                }
            }
        }

        static string[] GetCommandLineArgs()
        {
            var args = new List<string>();
            var commandLine = Environment.CommandLine;
            var offset = 0;
            var length = commandLine.Length;
            var chars = commandLine.ToCharArray();

            if (commandLine.StartsWith("\""))
                offset = commandLine.IndexOf("\"", 1)+2;
            else
                offset = commandLine.IndexOf(" ")+1;

            for (; offset < commandLine.Length; offset++)
            {
                var endOffset = 0;
                var value = string.Empty;

                if(chars[offset] == '"')
                {
                    endOffset = commandLine.IndexOf("\"", ++offset);
                    if (endOffset == -1)
                        endOffset = commandLine.Length;
                    value = commandLine.Substring(offset, endOffset - offset);
                    offset = endOffset + 1;
                }
                else
                {
                    endOffset = commandLine.IndexOf(" ", offset);
                    if (endOffset == -1)
                        endOffset = commandLine.Length;
                    value = commandLine.Substring(offset, endOffset - offset);
                    offset = endOffset;
                }
                args.Add(value);

            }

            return args.ToArray();
        }

        static RecorderOptions OptionsFromArgs()
        {
            var args = GetCommandLineArgs();
            var options = new RecorderOptions();


            for (int i = 0; i < args.Length; i++)
            {
                var command = args[i].ToLower();

                if(command == "--outdir")
                {
                    options.OutputDirectory = args[++i];
                }
                else if (command == "--triggerduration")
                {
                    options.TriggerDuration = TimeSpan.FromMilliseconds(double.Parse(args[++i]));
                }
                else if (command == "--triggervolume")
                {
                    options.TriggerVolume = double.Parse(args[++i]);
                }
                else if(command =="--endtriggerduration")
                {
                    options.EndTriggerDuration = TimeSpan.FromMilliseconds(double.Parse(args[++i]));
                }
                else if(command == "--endtriggervolume")
                {
                    options.EndTriggerVolume = double.Parse(args[++i]);
                }
                else if(command == "--minclipseparation")
                {
                    options.MinClipSeparation = TimeSpan.FromMilliseconds(double.Parse(args[++i]));
                }
                else if(command == "--url")
                {
                    options.Url = args[++i];
                }
                else if(command == "--debug")
                {
                    options.Debug = true;
                }
                else
                {
                    if(!string.IsNullOrEmpty(command))
                        Console.WriteLine($"Unrecognized command: {command}");
                }
            }
            
            return options;
        }

    }
}

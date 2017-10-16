using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Speech.Synthesis;
using System.IO;

namespace VoiceAtTime
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var runFunction = args.Length > 0 && args[0].StartsWith("-run:")
                ? args[0].Substring(args[0].IndexOf(':') + 1)
                : null;

            try
            {
                using (SpeechSynthesizer synthesizer = new SpeechSynthesizer())
                {
                    if (!string.IsNullOrEmpty(runFunction)
                        && TimedDialogs.RunFunction(synthesizer, (TimeType.Relative, new TimeSpan(), $"^{runFunction}", true), DateTime.Now))
                    {
                        return;
                    }

                    var filename = args.Length > 0 && args[0].StartsWith("-inputfile:")
                        ? args[0].Substring(args[0].IndexOf(':') + 1)
                        : "TimedDialogs.txt";

                    if (!File.Exists(filename)) return;


                    var speaker = new TimedDialogs(synthesizer);
                    speaker.Speak(filename);
                }
            }
            catch(Exception ex)
            {
                ShowUsage();
            }
        }

        private static void ShowUsage()
        {
            Console.WriteLine(@"Usage: VoiceAtTime.exe [-run:<function with params>|-inputfile:c:\dialog.txt]");
        }
    }
}

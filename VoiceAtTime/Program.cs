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
            if (args.Length > 1)
            {
                ShowUsage();
                return;
            }

            var filename = args.Length > 0 && args[0].StartsWith("-inputfile:")
                ? args[0].Substring(args[0].IndexOf(':') + 1)
                : "TimedDialogs.txt";

            if (!File.Exists(filename)) return;

            using (SpeechSynthesizer synthesizer = new SpeechSynthesizer())
            {
                var speaker = new TimedDialogs(synthesizer);
                speaker.Speak(filename);
            }
        }

        private static void ShowUsage()
        {
            Console.WriteLine(@"Usage: VoiceAtTime.exe -inputfile:c:\dialog.txt");
        }
    }
}

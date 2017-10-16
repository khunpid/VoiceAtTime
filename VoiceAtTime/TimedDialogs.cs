using System;
using System.IO;
using System.Linq;
using System.Speech.Synthesis;
using System.Threading;

namespace VoiceAtTime
{
    public class TimedDialogs
    {
        private string[] lines;
        private readonly SpeechSynthesizer synthesizer;

        public TimedDialogs(SpeechSynthesizer synthesizer)
        {
            this.synthesizer = synthesizer;
        }

        private (TimeType TimeType, TimeSpan AtTime, string Speech, bool IsSync) Parse(string line)
        {
            var separatorIndex = line.IndexOf(' ');
            var timeOffset = line.Substring(0, separatorIndex);

            if (string.IsNullOrWhiteSpace(timeOffset)) throw new InvalidDataException(line);

            var (timeType, isSync) = ParseForTimeTypeAndSync(timeOffset, out string newTimeOffset);

            if (!TimeSpan.TryParse(newTimeOffset, out TimeSpan result)) throw new InvalidDataException(timeOffset);

            var speech = line.Substring(separatorIndex + 1);

            return (timeType, result, speech, isSync);
        }

        private (TimeType tt, bool sync) ParseForTimeTypeAndSync(string timeOffset, out string newTimeOffset)
        {
            TimeType timeType;
            var isSync = false;

            if (timeOffset.EndsWith("x"))
            {
                isSync = true;
                timeOffset = timeOffset.Substring(0, timeOffset.Length - 1);
            }

            timeType = timeOffset.StartsWith("+") ? TimeType.Relative : TimeType.Absolute;
            newTimeOffset = timeOffset.Replace("+", "");

            return (timeType, isSync);
        }

        public void Speak(string filename, VoiceGender genderHint = VoiceGender.Female, VoiceAge ageHint = VoiceAge.Child)
        {
            synthesizer.SelectVoiceByHints(genderHint, ageHint);
            synthesizer.Volume = 80;
            synthesizer.Rate = 0;

            lines = File.ReadAllLines(filename);

            var startTime = DateTime.Now;
            lines.ToList().ForEach(line =>
            {
                if (line.StartsWith("#")) return;

                try
                {
                    var parsed = Parse(line);
                    var parsedWithVariables = InsertVariable(parsed);

                    WaitForTime(startTime, parsedWithVariables.TimeType, parsedWithVariables.AtTime);
                    var functionRun = RunFunction(parsedWithVariables, DateTime.Now);

                    if (functionRun) return;

                    Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff")} Speaking: {parsedWithVariables.Speech}");
                    if (parsedWithVariables.IsSync)
                        synthesizer.Speak(parsedWithVariables.Speech);
                    else
                        synthesizer.SpeakAsync(parsedWithVariables.Speech);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff")} Skipping: {line}");
                    Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff")} {ex.Message}");
                    Console.WriteLine(ex.StackTrace);
                }

            });

            Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff")} Speaking: Bye");
            synthesizer.Speak("Bye");
        }

        private bool RunFunction((TimeType TimeType, TimeSpan AtTime, string Speech, bool IsSync) parsedWithVariables, DateTime startDate)
        {
            if (!parsedWithVariables.Speech.StartsWith("^")) return false;

            var functionWithParam = parsedWithVariables.Speech.Substring(1);
            var ff = functionWithParam.Split(' ');
            switch (ff[0])
            {
                case "Countdown":
                    if (ff.Length != 4 
                        || !int.TryParse(ff[1], out int countdownFrom)
                        || !int.TryParse(ff[2], out int countdownTo)
                        || !int.TryParse(ff[3], out int step)) return false;
                    if (countdownFrom < countdownTo) return false;
                    if (step < 1) return false;

                    Functions.Countdown(countdownFrom, countdownTo, step, startDate, synthesizer);

                    return true;

                default:
                    return false;
            }
        }

        private static (TimeType TimeType, TimeSpan AtTime, string Speech, bool IsSync) InsertVariable((TimeType TimeType, TimeSpan AtTime, string Speech, bool IsSync) parsed)
        {
            if (parsed.Speech.Contains("$TIME"))
            {
                parsed.Speech = parsed.Speech.Replace("$TIME", DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss"));
            }

            return parsed;
        }

        private static void WaitForTime(DateTime startTime, TimeType timeType, TimeSpan atTime)
        {
            if (timeType == TimeType.Absolute)
            {
                SpinWait.SpinUntil(() => DateTime.Now - startTime >= atTime);

                return;
            }

            Thread.Sleep(atTime);
        }
    }

    public enum TimeType
    {
        Absolute,
        Relative,
    }
}

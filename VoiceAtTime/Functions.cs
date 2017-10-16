using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Synthesis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VoiceAtTime
{
    public static class Functions
    {
        public static  void Countdown(int countdownFrom, int countdownTo, int step, DateTime startDate, SpeechSynthesizer synthesizer)
        {
            Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff")} Speaking: {countdownFrom}");
            synthesizer.SpeakAsync(countdownFrom.ToString());

            using (var timer = new System.Timers.Timer { Interval = 1000 * step })
            {
                var oldrate = synthesizer.Rate;
                synthesizer.Rate = 5;
                timer.AutoReset = true;
                timer.Elapsed += (sender, e) =>
                {
                    var x = e.SignalTime - startDate;
                    var passed = (int)Math.Round(countdownFrom - x.TotalSeconds);
                    if (passed <= countdownTo)
                    {
                        timer.AutoReset = false;
                        timer.Stop();
                    }

                    Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff")} Speaking: {passed}");
                    synthesizer.Speak(passed.ToString());
                };

                timer.Start();
                synthesizer.Rate = oldrate;
                SpinWait.SpinUntil(() => !timer.AutoReset);
            }
        }

    }
}

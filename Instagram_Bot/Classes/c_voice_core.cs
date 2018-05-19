using System.Speech.Synthesis;
using System.Threading.Tasks;

namespace Instagram_Bot
{
    /// <summary>
    /// API for generating text to voice.
    /// </summary>
    public static class C_voice_core
    {

        private static SpeechSynthesizer synth;
        /// <summary>
        /// Output text to the speakers as computer generated voice.
        /// </summary>
        /// <param name="phrase">What to say</param>
        /// <param name="async">Run in non blocking mode, default is no so the caller will wait for speach to finish.</param>
        public static void speak(string phrase = "I can speak, but have nothing to say.", bool async = false)
        {
            if (async)
            {
                speakAsync(phrase);
                return;
            }
            synth = new SpeechSynthesizer();
            synth.SetOutputToDefaultAudioDevice();
            synth.SelectVoiceByHints(VoiceGender.NotSet);
            synth.SpeakCompleted += Synth_SpeakCompleted;
            synth.Speak(new Prompt(phrase, SynthesisTextFormat.Text));
        }

        private static void speakAsync(string phrase)
        {
            var t = new Task(() =>
            {
                speak(phrase);
            });
            t.Start();         
        }

        private static void Synth_SpeakCompleted(object sender, SpeakCompletedEventArgs e) => synth.Dispose();
    }
}

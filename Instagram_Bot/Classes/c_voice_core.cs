using System.Speech.Synthesis;

namespace Instagram_Bot
{
    public static class c_voice_core
    {

        // do not use a using statement around SpeechSynthesizer when using async speak as it just won't work! synth will be disposed instantly and no voice will be heard.
        private static SpeechSynthesizer synth;
        public static void speak(string phrase = "I can speak, but have nothing to say.")
        {
            synth = new SpeechSynthesizer();
            synth.SetOutputToDefaultAudioDevice();
            synth.SelectVoiceByHints(VoiceGender.NotSet);
            synth.SpeakCompleted += Synth_SpeakCompleted;
            //synth.SpeakAsync(new Prompt(phrase, SynthesisTextFormat.Text)); 
            synth.Speak(new Prompt(phrase, SynthesisTextFormat.Text));
        }

        private static void Synth_SpeakCompleted(object sender, SpeakCompletedEventArgs e)
        {
            synth.Dispose();
        }
    }
}

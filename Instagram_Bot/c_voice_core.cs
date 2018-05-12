using System.Speech.Synthesis;

namespace Instagram_Bot
{
    public static class c_voice_core
    {
        public static void speak(string phrase = "I can speak, but have nothing to say.")
        {
            using (SpeechSynthesizer synth = new SpeechSynthesizer())
            {
                try
                {
                    synth.SetOutputToDefaultAudioDevice();
                    synth.Speak(phrase);
                }
                catch { }
                //TODO: log audio errors, these can occur when the sound card blips (when disconnecting an RDP session for example).
            }
        }
    }
}

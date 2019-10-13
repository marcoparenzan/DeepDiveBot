using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace BotWpfClient.CognitiveServices
{
    public class SpeechManager
    {
        private IConfiguration config;
        private SpeechRecognizer recognizer;
        private SpeechSynthesizer syntetizer;
        private bool isRecognizing;
        private string wakeupKeyword;
        private int awakeTimeout;
        private Action<bool> awakeningHandler;
        private Task awakening;

        public SpeechManager(IConfiguration config)
        {
            this.config = config;
        }

        public void WakeDown()
        {
            awakeningHandler(false);
            this.awakening = default;
        }

        public void WakeUp()
        {
            awakeningHandler(true);
            this.awakening = Task.Factory.StartNew(async () => {
                await Task.Delay(awakeTimeout);
                WakeDown();
            });
        }

        // https://docs.microsoft.com/en-us/azure/cognitive-services/speech-service/how-to-recognize-speech-csharp

        public void Start(string culture, Action<string> recognized, string wakeupKeyword = default, int awakeTimeout = 6000, Action<bool> awakeningHandler = default, Action<string> recognizing = default)
        {
            this.wakeupKeyword = wakeupKeyword;
            this.awakeTimeout = awakeTimeout;
            this.awakeningHandler = awakeningHandler;

            var audioInput = AudioConfig.FromDefaultMicrophoneInput();
            var speechInputConfig = SpeechConfig.FromSubscription(config["SpeechToText.SubscriptionKey"], config["SpeechToText.SpeechRegion"]);
            speechInputConfig.OutputFormat = OutputFormat.Detailed;
            speechInputConfig.SpeechRecognitionLanguage = culture;
            recognizer = new SpeechRecognizer(speechInputConfig, audioInput);

            recognizer.Recognizing += (s, e) =>
            {
                isRecognizing = true;
                if (recognizing == default) return;
                var text = e.Result.Text.Clean();
                if (text.IsEmpty()) return;
                recognizing(text);
            };

            recognizer.Recognized += (s, e) => {
                isRecognizing = false;
                if (e.Result.Reason != ResultReason.RecognizedSpeech) return;
                var text = e.Result.Text.Clean();

                if (text.IsEmpty()) return;
                if (this.awakening != null)
                {
                    recognized(text);
                    WakeDown();
                }
                else
                {
                    var wakeupKeywordIndex = text.IndexOf(wakeupKeyword, 0, StringComparison.InvariantCultureIgnoreCase);
                    if (wakeupKeywordIndex < 0) return;

                    var beforeText = string.Empty;
                    if (wakeupKeywordIndex > 0)
                    {
                        beforeText = text.Substring(0, wakeupKeywordIndex - 1).Clean();
                    }

                    var afterText = text.Substring(wakeupKeywordIndex + wakeupKeyword.Length).Clean();
                    if (afterText.IsEmpty())
                    {
                        if (beforeText.IsEmpty())
                        {
                            WakeUp();
                        }
                        else
                        {
                            recognized(beforeText);
                        }
                    }
                    else if (beforeText.IsEmpty())
                    {
                        recognized(afterText);
                    }
                    else
                    {
                        recognized($"{beforeText} {afterText}");
                    }
                }
            };

            recognizer.StartContinuousRecognitionAsync().Wait();

            var audioOutput = AudioConfig.FromDefaultSpeakerOutput();
            var speechOutputConfig = SpeechConfig.FromSubscription(config["SpeechToText.SubscriptionKey"], config["SpeechToText.SpeechRegion"]);
            speechOutputConfig.SpeechSynthesisLanguage = culture;
            speechOutputConfig.SpeechSynthesisVoiceName = config[$"TextToSpeech.VoiceName.{culture}"];
            syntetizer = new SpeechSynthesizer(speechOutputConfig, audioOutput);
        }

        public async Task Text(string text)
        {
            while (isRecognizing) await Task.Delay(100);

            await recognizer.StopContinuousRecognitionAsync();
            var result = await syntetizer.SpeakTextAsync(text);
            await recognizer.StartContinuousRecognitionAsync();
        }
    }
}


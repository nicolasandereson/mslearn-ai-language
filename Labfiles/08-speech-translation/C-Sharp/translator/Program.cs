using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Text;

// Import namespaces
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech.Translation;


namespace speech_translation
{
    class Program
    {
        // Declare a static variable for speech configuration
        private static SpeechConfig speechConfig;
        // Declare a static variable for speech translation configuration
        private static SpeechTranslationConfig translationConfig;

        /// <summary>
        /// Entry point of the program. Configures translation and speech settings and handles user input for translation.
        /// </summary>
        static async Task Main(string[] args)
        {
            try
            {
                /* Get config settings from AppSettings */
                // Create a configuration builder and add the appsettings.json file
                IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
                // Build the configuration
                IConfigurationRoot configuration = builder.Build();
                // Retrieve the SpeechKey from the configuration
                string aiSvcKey = configuration["SpeechKey"];
                // Retrieve the SpeechRegion from the configuration
                string aiSvcRegion = configuration["SpeechRegion"];

                /* Set input and output encoding to unicode */
                Console.InputEncoding = Encoding.Unicode;
                Console.OutputEncoding = Encoding.Unicode;


                /* Configure translation */
                // Create a translation configuration using the subscription key and region
                translationConfig = SpeechTranslationConfig.FromSubscription(aiSvcKey, aiSvcRegion);
                // Set the speech recognition language to English (US)
                translationConfig.SpeechRecognitionLanguage = "en-US";
                // Add French, Spanish and Hindi as target languages
                translationConfig.AddTargetLanguage("fr");
                translationConfig.AddTargetLanguage("es");
                translationConfig.AddTargetLanguage("hi");
                Console.WriteLine("Ready to translate from " + translationConfig.SpeechRecognitionLanguage);


                // Create a speech configuration using the subscription key and region
                speechConfig = SpeechConfig.FromSubscription(aiSvcKey, aiSvcRegion);

                // Initialize the target language variable
                string targetLanguage = "";
                // Loop until the user enters "quit"
                while (targetLanguage != "quit")
                {
                    // Prompt the user to enter a target language
                    Console.WriteLine("\nEnter a target language\n fr = French\n es = Spanish\n hi = Hindi\n Enter anything else to stop\n");
                    targetLanguage = Console.ReadLine().ToLower();
                    // Check if the entered language is a valid target language
                    if (translationConfig.TargetLanguages.Contains(targetLanguage))
                    {
                        // Call the Translate method with the target language
                        await Translate(targetLanguage);
                    }
                    else
                    {
                        // Set the target language to "quit" to exit the loop
                        targetLanguage = "quit";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Translates spoken input to the specified target language and synthesizes the translated text.
        /// </summary>
        /// <param name="targetLanguage">The target language code (e.g., "fr" for French).</param>
        static async Task Translate(string targetLanguage)
        {
            // Initialize the translation variable
            string translation = "";

            /*Translate speech*/
            // Create audio configuration from default microphone input
            using AudioConfig audioConfig = AudioConfig.FromDefaultMicrophoneInput();
            // Create translation recognizer with the specified configuration
            using TranslationRecognizer translator = new TranslationRecognizer(translationConfig, audioConfig);
            // Prompt user to speak
            Console.WriteLine("Speak now...");
            // Recognize speech input once and get the result
            TranslationRecognitionResult result = await translator.RecognizeOnceAsync();
            // Output the recognized text
            Console.WriteLine($"Translating '{result.Text}'");
            // Get the translation for the target language
            translation = result.Translations[targetLanguage];
            // Set the console output encoding to UTF-8
            Console.OutputEncoding = Encoding.UTF8;
            // Output the translation
            Console.WriteLine(translation);


            // Synthesize translation
            // Create a dictionary of voices for different languages
            var voices = new Dictionary<string, string>
            {
                ["fr"] = "fr-FR-HenriNeural",
                ["es"] = "es-ES-ElviraNeural",
                ["hi"] = "hi-IN-MadhurNeural"
            };
            // Set the speech synthesis voice based on the target language
            speechConfig.SpeechSynthesisVoiceName = voices[targetLanguage];
            // Create a speech synthesizer with the specified configuration
            using SpeechSynthesizer speechSynthesizer = new SpeechSynthesizer(speechConfig);
            // Synthesize the translation text to speech
            SpeechSynthesisResult speak = await speechSynthesizer.SpeakTextAsync(translation);
            // Check if the synthesis was completed successfully
            if (speak.Reason != ResultReason.SynthesizingAudioCompleted)
            {
                // Output the reason for failure
                Console.WriteLine(speak.Reason);
            }
        }
    }
}

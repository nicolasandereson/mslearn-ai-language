using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;


namespace speaking_clock
{
    class Program
    {
        private static SpeechConfig speechConfig;
        /// <summary>
        /// Main method to configure speech service and handle user command.
        /// </summary>
        static async Task Main(string[] args)
        {
            try
            {
                // Get config settings from AppSettings
                IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
                // Build the configuration
                IConfigurationRoot configuration = builder.Build();
                // Retrieve the SpeechRegion from configuration
                string aiSvcKey = configuration["SpeechKey"];
                // Configure speech service using the retrieved key and region
                string aiSvcRegion = configuration["SpeechRegion"];

                // Configure speech service using the retrieved key and region
                speechConfig = SpeechConfig.FromSubscription(aiSvcKey, aiSvcRegion);
                // Output the region to the console
                Console.WriteLine("Ready to use speech service in " + speechConfig.Region);
                // Set the voice for speech synthesis
                speechConfig.SpeechSynthesisVoiceName = "en-US-AriaNeural";


                // Initialize command variable
                string command = "";
                // Get spoken input and transcribe it to text
                command = await TranscribeCommand();
                // Check if the transcribed command is "what time is it?"
                if (command.ToLower() == "what time is it?")
                {
                    // Respond with the current time
                    await TellTime();
                }

            }
            catch (Exception ex)
            {
                // Output any exceptions to the console
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Transcribes spoken command from the user.
        /// </summary>
        static async Task<string> TranscribeCommand()
        {
            // Initialize command variable
            string command = "";

            /*Process speech input*/
            // Create audio configuration from default microphone input
            using AudioConfig audioConfig = AudioConfig.FromDefaultMicrophoneInput();
            // Create speech recognizer with the specified configuration
            using SpeechRecognizer speechRecognizer = new SpeechRecognizer(speechConfig, audioConfig);
            // Prompt user to speak
            Console.WriteLine("Speak now...");


            /* Process speech input*/
            // Recognize speech input once
            SpeechRecognitionResult speech = await speechRecognizer.RecognizeOnceAsync();
            // Check if speech was recognized
            if (speech.Reason == ResultReason.RecognizedSpeech)
            {
                // Assign recognized text to command variable
                command = speech.Text;
                // Output recognized command to console
                Console.WriteLine(command);
            }
            else
            {
                // Output the reason for failure
                Console.WriteLine(speech.Reason);
                // Check if recognition was canceled
                if (speech.Reason == ResultReason.Canceled)
                {
                    /*output cancellion details*/
                    var cancellation = CancellationDetails.FromResult(speech);
                    Console.WriteLine(cancellation.Reason);
                    Console.WriteLine(cancellation.ErrorDetails);
                }
            }


            // Return the command
            return command;
        }

        /// <summary>
        /// Tells the current time using speech synthesis.
        /// </summary>
        static async Task TellTime()
        {
            // Get the current date and time
            var now = DateTime.Now;
            // Create a response text with the current time
            string responseText = "The time is " + now.Hour.ToString() + ":" + now.Minute.ToString("D2");

            // Configure speech synthesis synthesis with a specific voice
            speechConfig.SpeechSynthesisVoiceName = "en-GB-LibbyNeural";
            // Create a speech synthesizer with the specified configuration
            using SpeechSynthesizer speechSynthesizer = new SpeechSynthesizer(speechConfig);


            // Synthesize spoken output
            // SpeechSynthesisResult speak = await speechSynthesizer.SpeakTextAsync(responseText);
            // if (speak.Reason != ResultReason.SynthesizingAudioCompleted)
            // {
            //     Console.WriteLine(speak.Reason);
            // }

            // Synthesize spoken output using SSML (Speech Synthesis Markup Language)
            string responseSsml = $@"
            <speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis' xml:lang='en-US'>
                <voice name='en-GB-LibbyNeural'>
                    {responseText}
                    <break strength='weak'/>
                    Time to end this lab!
                </voice>
            </speak>";
            // Synthesize the SSML response
            SpeechSynthesisResult speak = await speechSynthesizer.SpeakSsmlAsync(responseSsml);
            // Check if the synthesis was completed successfully
            if (speak.Reason != ResultReason.SynthesizingAudioCompleted)
            {
                // Output the reason for failure
                Console.WriteLine(speak.Reason);
            }

            // Print the response
            Console.WriteLine(responseText);
        }

    }
}

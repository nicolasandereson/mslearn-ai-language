using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using Azure;
using Azure.AI.Translation.Text;



namespace translate_text
{
    class Program
    {
        /// <summary>
        /// This method initializes the Azure Translator service client, configures console encoding,
        /// retrieves translation settings from the app configuration, and translates input text from the user
        /// to a specified target language. The program continuously prompts the user for text input until 'quit' is entered.
        /// </summary>
        static async Task Main(string[] args)
        {
            try
            {
                // Set console encoding to unicode for handling a wide range for characters 
                Console.InputEncoding = Encoding.Unicode;
                Console.OutputEncoding = Encoding.Unicode;

                //Build configuration from appsettings.json
                IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
                // Load configuration settings 
                IConfigurationRoot configuration = builder.Build();
                // Retrieve Translator settings from configuration
                string translatorRegion = configuration["TranslatorRegion"];
                string translatorKey = configuration["TranslatorKey"];


                // Create AzureKeyCredential using the translaotr key
                AzureKeyCredential credential = new(translatorKey);
                // Initialize TextTranslationClient with the credential and region
                TextTranslationClient client = new(credential, translatorRegion);



                // Request supported languages for translation
                Response<GetLanguagesResult> languagesResponse = await client.GetLanguagesAsync(scope: "translation").ConfigureAwait(false);
                // Get the result containing supported languages
                GetLanguagesResult languages = languagesResponse.Value;
                // Display the number of available languages and a reference link
                Console.WriteLine($"{languages.Translation.Count} languages available.\n(See https://learn.microsoft.com/azure/ai-services/translator/language-support#translation)");
                // Prompt user to enter a target language code
                Console.WriteLine("Enter a target language code for translation (for example, 'en'):");
                // Initialize target language variable
                string targetLanguage = "xx";
                // Initialize flag to check if the language is supported
                bool languageSupported = false;
                // Loop until a supported language code is entered
                while (!languageSupported)
                {
                    // Read target language code from console input
                    targetLanguage = Console.ReadLine();
                    // Check if the entered language code is supported
                    if (languages.Translation.ContainsKey(targetLanguage))
                    {
                        // Set flag to true if language is supported
                        languageSupported = true;
                    }
                    else
                    {
                        // Inform user if the language is not supported
                        Console.WriteLine($"{targetLanguage} is not a supported language.");
                    }

                }


                // Initialize input text variable
                string inputText = "";
                // Loop until user enters 'quit'
                while (inputText.ToLower() != "quit")
                {
                    // Prompt user to enter text for translation
                    Console.WriteLine("Enter text to translate ('quit' to exit)");
                    // Read input text from console
                    inputText = Console.ReadLine();
                    // Check if user entered 'quit'
                    if (inputText.ToLower() != "quit")
                    {
                        // Translate input text to target language
                        Response<IReadOnlyList<TranslatedTextItem>> translationResponse = await client.TranslateAsync(targetLanguage, inputText).ConfigureAwait(false);
                        // Get the list of translations
                        IReadOnlyList<TranslatedTextItem> translations = translationResponse.Value;
                        // Get the first translation item
                        TranslatedTextItem translation = translations[0];
                        //  Get the detected source language
                        string sourceLanguage = translation?.DetectedLanguage?.Language;
                        // Display the translation result
                        Console.WriteLine($"'{inputText}' translated from {sourceLanguage} to {translation?.Translations[0].To} as '{translation?.Translations?[0]?.Text}'.");
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

    }
}
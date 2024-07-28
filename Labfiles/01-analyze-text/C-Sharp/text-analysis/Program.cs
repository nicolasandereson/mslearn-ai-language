using System;
using System.IO;
using Microsoft.Extensions.Configuration;

// import namespaces
using Azure;
using Azure.AI.TextAnalytics;


namespace text_analysis
{
    class Program
    {
        /// <summary>
        /// Main program class for text analysis using Azure Text Analytics API.
        /// This program reads text files from the "reviews" folder, and performs language detection,
        /// sentiment analysis, key phrase extraction, and entity recognition (both categorized and linked entities).
        /// The results of each analysis are printed to the console.
        /// </summary>
        static void Main(string[] args)
        {
            try
            {
                // Get config settings from AppSettings
                IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile("appsettings.json"); // Create a configuration builder and add the appsettings.json file
                IConfigurationRoot configuration = builder.Build(); // Build the configuration
                string aiSvcEndpoint = configuration["AIServicesEndpoint"]; // Retrieve the AI services endpoint from the configuration
                string aiSvcKey = configuration["AIServicesKey"]; // Retrieve the AI services key from the configuration

                // Create client using endpoint and key
                AzureKeyCredential credentials = new AzureKeyCredential(aiSvcKey); // Create Azure credentials using the AI services key
                Uri endpoint = new Uri(aiSvcEndpoint); // Create a URI for the AI services endpoint
                TextAnalyticsClient aiClient = new TextAnalyticsClient(endpoint, credentials); // Create a Text Analytics client using the endpoint and credentials

                // Analyze each text file in the reviews folder
                var folderPath = Path.GetFullPath("./reviews"); // Get the full path to the reviews folder
                DirectoryInfo folder = new DirectoryInfo(folderPath); // Create a DirectoryInfo object for the reviews folder
                foreach (var file in folder.GetFiles("*.txt")) // Iterate through each text file in the folder
                {
                    // Read the file contents
                    Console.WriteLine("\n-------------\n" + file.Name); // Output the file name
                    StreamReader sr = file.OpenText(); // Open the file for reading
                    var text = sr.ReadToEnd(); // Read the entire file contents
                    sr.Close(); // Close the file
                    Console.WriteLine("\n" + text); // Output the file contents

                    // Get language
                    DetectedLanguage detectedLanguage = aiClient.DetectLanguage(text); // Detect the language of the text
                    Console.WriteLine($"\nLanguage: {detectedLanguage.Name}"); // Output the detected language

                    // Get sentiment
                    DocumentSentiment sentimentAnalysis = aiClient.AnalyzeSentiment(text); // Analyze the sentiment of the text
                    Console.WriteLine($"\nSentiment: {sentimentAnalysis.Sentiment}"); // Output the sentiment analysis result

                    // Get key phrases
                    KeyPhraseCollection phrases = aiClient.ExtractKeyPhrases(text); // Extract key phrases from the text
                    if (phrases.Count > 0) // Check if any key phrases were found
                    {
                        Console.WriteLine("\nKey Phrases:"); // Output the key phrases header
                        foreach (string phrase in phrases) // Iterate through each key phrase
                        {
                            Console.WriteLine($"\t{phrase}"); // Output the key phrase
                        }
                    }

                    // Get entities
                    CategorizedEntityCollection entities = aiClient.RecognizeEntities(text); // Recognize entities in the text
                    if (entities.Count > 0) // Check if any entities were found
                    {
                        Console.WriteLine("\nEntities:"); // Output the entities header
                        foreach (CategorizedEntity entity in entities) // Iterate through each entity
                        {
                            Console.WriteLine($"\t{entity.Text} ({entity.Category})"); // Output the entity text and category
                        }
                    }

                    // Get linked entities
                    LinkedEntityCollection linkedEntities = aiClient.RecognizeLinkedEntities(text); // Recognize linked entities in the text
                    if (linkedEntities.Count > 0) // Check if any linked entities were found
                    {
                        Console.WriteLine("\nLinks:"); // Output the linked entities header
                        foreach (LinkedEntity linkedEntity in linkedEntities) // Iterate through each linked entity
                        {
                            Console.WriteLine($"\t{linkedEntity.Name} ({linkedEntity.Url})"); // Output the linked entity name and URL
                        }
                    }
                }
            }
            catch (Exception ex) 
            {
                Console.WriteLine(ex.Message); // Output the exception message
            }
        }
    }
}

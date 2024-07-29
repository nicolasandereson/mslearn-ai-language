using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;
// import namespaces
using Azure;
using Azure.AI.TextAnalytics;


namespace classify_text
{
    class Program
    {
        /// <summary>
        /// Main method to classify text documents using Azure AI Text Analytics.
        /// - Loads configuration settings from appsettings.json.
        /// - Creates a TextAnalyticsClient using the configured endpoint and key.
        /// - Reads text files from the "articles" folder.
        /// - Classifies the text documents using SingleLabelClassifyAsync method.
        /// - Outputs the classification results including category and confidence score.
        /// - Handles and displays any errors that occur during the process.
        /// </summary>
        static async Task Main(string[] args) 
        {
            try
            {
                // Get config settings from AppSettings
                IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile("appsettings.json"); // Create a configuration builder and add the JSON file
                IConfigurationRoot configuration = builder.Build(); // Build the configuration
                string aiSvcEndpoint = configuration["AIServicesEndpoint"]; // Get the AI services endpoint from the configuration
                string aiSvcKey = configuration["AIServicesKey"]; // Get the AI services key from the configuration
                string projectName = configuration["Project"]; // Get the project name from the configuration
                string deploymentName = configuration["Deployment"]; // Get the deployment name from the configuration

                // Create client using endpoint and key
                AzureKeyCredential credentials = new AzureKeyCredential(aiSvcKey); // Create Azure credentials using the AI services key
                Uri endpoint = new Uri(aiSvcEndpoint); // Create a URI object for the AI services endpoint
                TextAnalyticsClient aiClient = new TextAnalyticsClient(endpoint, credentials); // Create a Text Analytics client using the endpoint and credentials

                // Read each text file in the articles folder
                List<string> batchedDocuments = new List<string>(); // Initialize a list to hold the contents of the text files

                var folderPath = Path.GetFullPath("./articles"); // Get the full path to the articles folder
                DirectoryInfo folder = new DirectoryInfo(folderPath); // Create a DirectoryInfo object for the articles folder
                FileInfo[] files = folder.GetFiles("*.txt"); // Get all text files in the articles folder
                foreach (var file in files) // Iterate through each file
                {
                    // Read the file contents
                    StreamReader sr = file.OpenText(); // Open the file for reading
                    var text = sr.ReadToEnd(); // Read the entire contents of the file
                    sr.Close(); // Close the StreamReader
                    batchedDocuments.Add(text); // Add the file contents to the list
                }

                // Get Classifications
                ClassifyDocumentOperation operation = await aiClient.SingleLabelClassifyAsync(WaitUntil.Completed, batchedDocuments, projectName, deploymentName); // Perform single-label classification on the batched documents

                int fileNo = 0; // Initialize a counter for the file number
                await foreach (ClassifyDocumentResultCollection documentsInPage in operation.Value) // Iterate through each page of classification results
                {
                    foreach (ClassifyDocumentResult documentResult in documentsInPage) // Iterate through each document result in the page
                    {
                        Console.WriteLine(files[fileNo].Name); // Print the name of the current file
                        if (documentResult.HasError) // Check if there was an error in the classification
                        {
                            Console.WriteLine($"  Error!"); // Print an error message
                            Console.WriteLine($"  Document error code: {documentResult.Error.ErrorCode}"); // Print the error code
                            Console.WriteLine($"  Message: {documentResult.Error.Message}"); // Print the error message
                            continue; // Skip to the next document result
                        }

                        Console.WriteLine($"  Predicted the following class:"); // Print a message indicating the predicted class
                        Console.WriteLine(); // Print an empty line

                        foreach (ClassificationCategory classification in documentResult.ClassificationCategories) // Iterate through each classification category
                        {
                            Console.WriteLine($"  Category: {classification.Category}"); // Print the category
                            Console.WriteLine($"  Confidence score: {classification.ConfidenceScore}"); // Print the confidence score
                            Console.WriteLine(); // Print an empty line
                        }
                        fileNo++; // Increment the file number counter
                    }
                }
            }
            catch (Exception ex) // Catch any exceptions that occur
            {
                Console.WriteLine(ex.Message); // Print the exception message
            }
        }
    }
}

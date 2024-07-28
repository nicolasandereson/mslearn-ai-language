using System;
using Microsoft.Extensions.Configuration;

// import namespaces
using Azure;
using Azure.AI.Language.QuestionAnswering;


namespace qna_app
{
    class Program
    {
        /// <summary>
        /// This program demonstrates how to use the Azure AI Language Question Answering client to submit questions and retrieve answers.
        /// It reads configuration settings from an "appsettings.json" file, including the AI services endpoint, key, project name, and deployment name.
        /// The user can enter questions in the console, and the program will display the answers along with their confidence scores and sources.
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
                string projectName = configuration["QAProjectName"]; // Retrieve the QA project name from the configuration
                string deploymentName = configuration["QADeploymentName"]; // Retrieve the QA deployment name from the configuration

                // Create client using endpoint and key
                AzureKeyCredential credentials = new AzureKeyCredential(aiSvcKey); // Create Azure credentials using the AI services key
                Uri endpoint = new Uri(aiSvcEndpoint); // Create a URI for the AI services endpoint
                QuestionAnsweringClient aiClient = new QuestionAnsweringClient(endpoint, credentials); // Create a Question Answering client using the endpoint and credentials

                // Submit a question and display the answer
                string user_question = ""; // Initialize the user question variable
                while (user_question.ToLower() != "quit") // Loop until the user types "quit"
                {
                    Console.Write("Question: "); // Prompt the user for a question
                    user_question = Console.ReadLine(); // Read the user's question
                    QuestionAnsweringProject project = new QuestionAnsweringProject(projectName, deploymentName); // Create a Question Answering project using the project and deployment names
                    Response<AnswersResult> response = aiClient.GetAnswers(user_question, project); // Get answers from the Question Answering client
                    foreach (KnowledgeBaseAnswer answer in response.Value.Answers) // Iterate through each answer
                    {
                        Console.WriteLine(answer.Answer); // Output the answer
                        Console.WriteLine($"Confidence: {answer.Confidence:P2}"); // Output the confidence level of the answer
                        Console.WriteLine($"Source: {answer.Source}"); // Output the source of the answer
                        Console.WriteLine(); // Output a blank line for readability
                    }
                }
            }
            catch (Exception ex) // Catch any exceptions that occur
            {
                Console.WriteLine(ex.Message); // Output the exception message
            }
        }
    }
}

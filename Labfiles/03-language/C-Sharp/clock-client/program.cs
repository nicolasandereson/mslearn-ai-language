using System;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Core.Serialization;

// import namespaces
using Azure;
using Azure.AI.Language.Conversations;

namespace clock_client
{
    class Program
    {
        /// <summary>
        /// Main method: Entry point for the application. It reads configuration, initializes the client for the Language service model, 
        /// and processes user input to provide time, date, or day based on intent recognition.
        /// </summary>
        static async Task Main(string[] args)
        {
            try
            {
                // Get config settings from AppSettings
                IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile("appsettings.json"); // Build configuration from appsettings.json
                IConfigurationRoot configuration = builder.Build(); // Build the configuration root
                //Guid lsAppId = Guid.Parse(configuration["LSAppID"]); // (Commented out) Parse the Language Service App ID from configuration
                string predictionEndpoint = configuration["AIServicesEndpoint"]; // Get the AI Services endpoint from configuration
                string predictionKey = configuration["AIServicesKey"]; // Get the AI Services key from configuration

                // Create a client for the Language service model
                Uri endpoint = new Uri(predictionEndpoint); // Create a URI object for the endpoint
                AzureKeyCredential credential = new AzureKeyCredential(predictionKey); // Create a credential object using the prediction key

                ConversationAnalysisClient client = new ConversationAnalysisClient(endpoint, credential); // Initialize the Conversation Analysis client

                // Get user input (until they enter "quit")
                string userText = ""; // Initialize user input variable
                while (userText.ToLower() != "quit") // Loop until user enters "quit"
                {
                    Console.WriteLine("\nEnter some text ('quit' to stop)"); // Prompt user for input
                    userText = Console.ReadLine(); // Read user input
                    if (userText.ToLower() != "quit") // If user input is not "quit"
                    {
                        // Call the Language service model to get intent and entities
                        var projectName = "Clock"; // Define the project name
                        var deploymentName = "production"; // Define the deployment name
                        var data = new // Create the data object for the request
                        {
                            analysisInput = new
                            {
                                conversationItem = new
                                {
                                    text = userText, // User input text
                                    id = "1", // Conversation item ID
                                    participantId = "1", // Participant ID
                                }
                            },
                            parameters = new
                            {
                                projectName, // Project name
                                deploymentName, // Deployment name
                                // Use Utf16CodeUnit for strings in .NET.
                                stringIndexType = "Utf16CodeUnit", // String index type
                            },
                            kind = "Conversation", // Kind of analysis
                        };
                        // Send request
                        Response response = await client.AnalyzeConversationAsync(RequestContent.Create(data)); // Send the request and get the response
                        dynamic conversationalTaskResult = response.Content.ToDynamicFromJson(JsonPropertyNames.CamelCase); // Parse the response content
                        dynamic conversationPrediction = conversationalTaskResult.Result.Prediction; // Get the prediction result
                        var options = new JsonSerializerOptions { WriteIndented = true }; // Set JSON serializer options
                        Console.WriteLine(JsonSerializer.Serialize(conversationalTaskResult, options)); // Print the result
                        Console.WriteLine("--------------------\n"); // Print separator
                        Console.WriteLine(userText); // Print user input
                        var topIntent = ""; // Initialize top intent variable
                        if (conversationPrediction.Intents[0].ConfidenceScore > 0.5) // If the confidence score of the top intent is greater than 0.5
                        {
                            topIntent = conversationPrediction.TopIntent; // Set the top intent
                        }

                        // Apply the appropriate action
                        switch (topIntent) // Switch based on the top intent
                        {
                            case "GetTime":
                                var location = "local"; // Default location
                                // Check for a location entity
                                foreach (dynamic entity in conversationPrediction.Entities) // Iterate through entities
                                {
                                    if (entity.Category == "Location") // If entity is a location
                                    {
                                        //Console.WriteLine($"Location Confidence: {entity.ConfidenceScore}");
                                        location = entity.Text; // Set the location
                                    }
                                }
                                // Get the time for the specified location
                                string timeResponse = GetTime(location); // Get the time response
                                Console.WriteLine(timeResponse); // Print the time response
                                break;
                            case "GetDay":
                                var date = DateTime.Today.ToShortDateString(); // Default date
                                // Check for a Date entity
                                foreach (dynamic entity in conversationPrediction.Entities) // Iterate through entities
                                {
                                    if (entity.Category == "Date") // If entity is a date
                                    {
                                        //Console.WriteLine($"Location Confidence: {entity.ConfidenceScore}");
                                        date = entity.Text; // Set the date
                                    }
                                }
                                // Get the day for the specified date
                                string dayResponse = GetDay(date); // Get the day response
                                Console.WriteLine(dayResponse); // Print the day response
                                break;
                            case "GetDate":
                                var day = DateTime.Today.DayOfWeek.ToString(); // Default day
                                // Check for entities            
                                // Check for a Weekday entity
                                foreach (dynamic entity in conversationPrediction.Entities) // Iterate through entities
                                {
                                    if (entity.Category == "Weekday") // If entity is a weekday
                                    {
                                        //Console.WriteLine($"Location Confidence: {entity.ConfidenceScore}");
                                        day = entity.Text; // Set the day
                                    }
                                }
                                // Get the date for the specified day
                                string dateResponse = GetDate(day); // Get the date response
                                Console.WriteLine(dateResponse); // Print the date response
                                break;
                            default:
                                // Some other intent (for example, "None") was predicted
                                Console.WriteLine("Try asking me for the time, the day, or the date."); // Print default message
                                break;
                        }

                    }

                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message); // Print exception message
            }
        }


        /// <summary>
        /// Gets the current time for a specified location. 
        /// Returns the time as a string formatted as "HH:mm".
        /// </summary>
        /// <param name="location">The location for which the time is requested.</param>
        /// <returns>The current time as a string.</returns>
        static string GetTime(string location) // Define a method that returns the time as a string for a given location
        {
            var timeString = ""; // Initialize an empty string to hold the time
            var time = DateTime.Now; // Get the current local time

            /* Note: To keep things simple, we'll ignore daylight savings time and support only a few cities.
               In a real app, you'd likely use a web service API (or write more complex code!)
               Hopefully this simplified example is enough to get the idea that you
               use LU to determine the intent and entities, then implement the appropriate logic */

            switch (location.ToLower()) // Convert the location to lowercase and switch based on its value
            {
                case "local": // If the location is "local"
                    timeString = time.Hour.ToString() + ":" + time.Minute.ToString("D2"); // Format the local time as "HH:mm"
                    break;
                case "london": // If the location is "london"
                    time = DateTime.UtcNow; // Get the current UTC time
                    timeString = time.Hour.ToString() + ":" + time.Minute.ToString("D2"); // Format the UTC time as "HH:mm"
                    break;
                case "sydney": // If the location is "sydney"
                    time = DateTime.UtcNow.AddHours(11); // Get the current UTC time and add 11 hours
                    timeString = time.Hour.ToString() + ":" + time.Minute.ToString("D2"); // Format the time as "HH:mm"
                    break;
                case "new york": // If the location is "new york"
                    time = DateTime.UtcNow.AddHours(-5); // Get the current UTC time and subtract 5 hours
                    timeString = time.Hour.ToString() + ":" + time.Minute.ToString("D2"); // Format the time as "HH:mm"
                    break;
                case "nairobi": // If the location is "nairobi"
                    time = DateTime.UtcNow.AddHours(3); // Get the current UTC time and add 3 hours
                    timeString = time.Hour.ToString() + ":" + time.Minute.ToString("D2"); // Format the time as "HH:mm"
                    break;
                case "tokyo": // If the location is "tokyo"
                    time = DateTime.UtcNow.AddHours(9); // Get the current UTC time and add 9 hours
                    timeString = time.Hour.ToString() + ":" + time.Minute.ToString("D2"); // Format the time as "HH:mm"
                    break;
                case "delhi": // If the location is "delhi"
                    time = DateTime.UtcNow.AddHours(5.5); // Get the current UTC time and add 5.5 hours
                    timeString = time.Hour.ToString() + ":" + time.Minute.ToString("D2"); // Format the time as "HH:mm"
                    break;
                default: // If the location is not recognized
                    timeString = "I don't know what time it is in " + location; // Set the time string to an error message
                    break;
            }

            return timeString; // Return the formatted time string
        }


        /// <summary>
        /// Gets the date for a specified day of the week.
        /// Returns the date as a string in short date format.
        /// </summary>
        /// <param name="day">The day of the week for which the date is requested.</param>
        /// <returns>The date as a string.</returns>
        static string GetDate(string day) // Define a method that returns the date as a string for a given day of the week
        {
            string date_string = "I can only determine dates for today or named days of the week."; // Default message if the day is not recognized

            // To keep things simple, assume the named day is in the current week (Sunday to Saturday)
            DayOfWeek weekDay; // Declare a variable to hold the parsed day of the week
            if (Enum.TryParse(day, true, out weekDay)) // Try to parse the input string to a DayOfWeek enum, ignoring case
            {
                int weekDayNum = (int)weekDay; // Get the integer value of the parsed day of the week
                int todayNum = (int)DateTime.Today.DayOfWeek; // Get the integer value of today's day of the week
                int offset = weekDayNum - todayNum; // Calculate the offset between the parsed day and today
                date_string = DateTime.Today.AddDays(offset).ToShortDateString(); // Calculate the date for the parsed day and format it as a short date string
            }
            return date_string; // Return the formatted date string or the default message
        }


        /// <summary>
        /// Gets the day of the week as a string for a given date.
        /// </summary>
        /// <param name="date">The date for which the day of the week is requested.</param>
        /// <returns>The day of the week as a string.</returns>
        static string GetDay(string date) // Define a method that returns the day of the week as a string for a given date
        {
            // Note: To keep things simple, dates must be entered in US format (MM/DD/YYYY)
            string day_string = "Enter a date in MM/DD/YYYY format."; // Default message if the date is not in the correct format
            DateTime dateTime; // Declare a variable to hold the parsed date

            if (DateTime.TryParse(date, out dateTime)) // Try to parse the input string to a DateTime object
            {
                day_string = dateTime.DayOfWeek.ToString(); // If parsing is successful, get the day of the week and convert it to a string
            }

            return day_string; // Return the day of the week or the default message
        }
    }
}
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.Relativity.Client;
using Relativity.Services.ServiceProxy;

namespace UpdateClientOnUsers
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                Caller();
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occured:");
                Console.WriteLine(e);
                if (e.InnerException != null)
                {
                    Console.WriteLine("Inner exception:");
                    Console.WriteLine(e.InnerException);
                }
            }

            Pause();
        }


        private static void Caller()
        {
            // introduction
            const string usersFileName = "users.txt";
            Console.WriteLine("This console app will update the Client for a list of Users regardless of their current Client.");
            Console.WriteLine($"Please ensure that there is a text file named '{usersFileName}'.");
            Console.WriteLine("Each line in the file should indicate a user's Artifact ID");

            const string clientIdFile = "client.txt";
            Console.WriteLine($"Please include a text file named '{clientIdFile}' with the Artifact ID of the new client we want to associate with the users.");
            string url = String.Empty;
            string user = String.Empty;
            string pw = String.Empty;
            bool successfulLogin = false;
            while (!successfulLogin)
            {
                successfulLogin = ReadUserInput(out url, out user, out pw);
                if (!successfulLogin)
                {
                    Console.WriteLine("Failed to login! Try again.");
                    Console.WriteLine("---");
                }
            }
            var connHelper = new ConnectionHelper(url, user, pw);

            string currDir = Environment.CurrentDirectory;
            
            // first get user IDs
            string usersFilePath = currDir + @"\" + usersFileName;
            string[] userIdsAsStr;
            try
            {
                userIdsAsStr = File.ReadAllLines(usersFilePath);
            }
            catch (IOException)
            {
                Console.WriteLine($"Failed to read file at {usersFilePath}.");
                Console.WriteLine($"Ensure that {usersFileName} exists in the same folder as the executable.");
                return;
            }
            // user IDs found
            List<int> userIds = userIdsAsStr.Select(Int32.Parse).ToList();

            // then get the client ID
            string clientFilePath = currDir + @"\" + clientIdFile;
            string clientIdAsStr;
            try
            {
                string[] linesInClientFile = File.ReadAllLines(clientFilePath);
                if (linesInClientFile.Length > 0)
                {
                    clientIdAsStr = linesInClientFile.FirstOrDefault();
                }
                else
                {
                    Console.WriteLine($"Need to specify Client Artifact ID in {clientFilePath}.");
                    return;
                }
            }
            catch (IOException)
            {
                Console.WriteLine($"Failed to read file at {clientFilePath}.");
                Console.WriteLine($"Ensure that {clientIdFile} exists in the same folder as the executable.");
                return;
            }
            if (clientIdAsStr == null)
                return;
            int clientId = Int32.Parse(clientIdAsStr);

            // write logs to file
            string logPath = currDir + @"\" + "log.txt";
            // write current date
            string dateAsStr = Environment.NewLine + DateTime.Now.ToString(CultureInfo.CurrentCulture) + Environment.NewLine;
            WriteToFile(logPath, dateAsStr);


            // instantiate IRSAPIClient
            using (IRSAPIClient rsapi = connHelper.GetRsapiClient())
            {
                int successCount = 0;
                int total = userIds.Count;
                foreach (int userId in userIds)
                {
                    bool success = Users.UpdateClientForUser(rsapi, userId, clientId);
                    string message;
                    if (success)
                    {
                        successCount++;
                        message = $"Successfully updated {successCount} of {total} Users. (Artifact ID: {userId})";
                        
                    }
                    else
                    {
                        message = $"Failed to update user with ID {userId}.";
                    }
                    Console.WriteLine(message);
                    WriteToFile(logPath, message);
                }
            }
        }


        /// <summary>
        /// Write log message to file path
        /// </summary>
        /// <param name="file"></param>
        /// <param name="content"></param>
        private static void WriteToFile(string file, string content)
        {
            using (var sw = new StreamWriter(path: file, append: true))
            {
                sw.WriteLine(content);
            }
        }


        /// <summary>
        /// Reads in and validates the user credentials by attempting to log in
        /// </summary>
        /// <param name="url"></param>
        /// <param name="user"></param>
        /// <param name="pw"></param>
        /// <returns></returns>
        private static bool ReadUserInput(out string url, out string user, out string pw)
        {
            Console.WriteLine("Please enter your Relativity instance URL (e.g. https://my-instance.com).");
            url = Console.ReadLine();
            Console.WriteLine("Please enter your Relativity username (e.g. albert.einstein@relativity.com).");
            user = Console.ReadLine();
            Console.WriteLine("Please enter your Relativity password. The cursor will not move.");
            StringBuilder pwBuilder = new StringBuilder();
            // hide password
            while (true)
            {
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    break;
                }
                if (key.Key == ConsoleKey.Backspace && pwBuilder.Length > 0)
                {
                    // remove last element
                    pwBuilder.Remove(pwBuilder.Length - 1, 1);
                }
                else
                {
                    pwBuilder.Append(key.KeyChar);
                }
            }
            pw = pwBuilder.ToString();

            var connHelper = new ConnectionHelper(url, user, pw);
            return connHelper.TestLogin();
        }


        private static void Pause()
        {
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey(true);
        }
    }
}

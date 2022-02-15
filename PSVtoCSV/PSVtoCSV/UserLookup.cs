using System;
using System.Collections.Generic;
using System.IO;

namespace PSVtoCSV
{
    public class UserLookup
    {
        Dictionary<string, bool> s = new Dictionary<string, bool>();

        public void Run(string readfilepath)
        {
            Console.WriteLine("User ID");
            string userID = Console.ReadLine();

            List<string> checkoutIDs = new List<string>();
            List<string> bookNames = new List<string>();

            Program.VerifyFiles(readfilepath);

            float value = 18500000.0f;
            int lines = 0;

            try
            {
                using StreamReader sr = new StreamReader(readfilepath);
                String line;

                Console.WriteLine("Reading contents");

                while ((line = sr.ReadLine()) != null)
                {
                    string tidy = line;
                    string[] tidyParts = tidy.Split(',', StringSplitOptions.RemoveEmptyEntries);

                    if (s.ContainsKey($"{tidyParts[3]},{tidyParts[^1]}"))
                        continue;
                    else s.Add($"{tidyParts[3]},{tidyParts[^1]}", true);

                    // if (tidyParts[3] == userID)
                    // {
                    // checkoutIDs.Add(tidyParts[^1]);
                    // bookNames.Add(tidyParts[0]);
                    // }

                    lines++;
                }

                sr.Close();

                Console.WriteLine($"{lines.Beautify()} lines");
                Console.WriteLine();

                for (int i = 0; i < checkoutIDs.Count; i++)
                {
                    Console.WriteLine($"{checkoutIDs[i]} - {bookNames[i]}");
                }

                Console.WriteLine();
                Console.WriteLine($"Found {checkoutIDs.Count.Beautify()} checkouts for user {userID}");
                Console.WriteLine();
            }
            catch (Exception e)
            {
                Console.WriteLine($"The file could not be read:\n{e}\n{e.Message}");
            }
        }
    }
}
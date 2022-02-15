using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PSVtoCSV
{
    public class GenerateUserCheckoutCountSheet
    {
        private Dictionary<string, User> userDict = new Dictionary<string, User>();
        private List<User> userList = new List<User>();

        public void Run(string rFilePath, string wFilePath, int userIDIndex, int itemIDIndex, int maxUsers = -1, bool removeSingleCheckouts = false)
        {
            Program.VerifyFiles(rFilePath, wFilePath);

            float value = 19300000.0f;
            int lines = 0;

            try
            {
                using StreamReader sr = new StreamReader(rFilePath);
                using StreamWriter sw = new StreamWriter(wFilePath);
                String line;

                Console.WriteLine("Reading");

                while ((line = sr.ReadLine()) != null)
                {
                    lines++;
                    string tidy = line;
                    string[] tidyParts = tidy.Split(',', StringSplitOptions.RemoveEmptyEntries);

                    if (string.IsNullOrWhiteSpace(tidyParts[userIDIndex]))
                        continue;
                    if (string.IsNullOrWhiteSpace(tidyParts[itemIDIndex]))
                        continue;

                    CheckUser(tidyParts, userIDIndex, itemIDIndex);
                    CheckBook(tidyParts, userIDIndex, itemIDIndex);
                }

                sr.Close();

                Console.WriteLine("Writing");
                string delimiter = ",";

                int x = 0;

                userList = userList.OrderByDescending(x => x.checkouts.Count).ToList();

                for (int i = 0; i < userList.Count; i++)
                {
                    if (removeSingleCheckouts && userList[i].checkouts.Count <= 1) continue;

                    x++;

                    if (maxUsers > -1 && x >= maxUsers)
                        break;

                    if (i < userList.Count - 1)
                        sw.WriteLine($"{userList[i].id},{userList[i].checkouts.Count}");
                    else
                        sw.Write($"{userList[i].id},{userList[i].checkouts.Count}");
                }

                sw.Close();

                Console.WriteLine($"Wrote {x.Beautify()} lines");
                Console.WriteLine("Finished");
            }
            catch (Exception e)
            {
                Console.WriteLine($"The file could not be read:\n{e}\n{e.Message}");
            }
        }

        public void CheckHighestCount(string rFilePath, int userIDIndex, int itemIDIndex)
        {
            Program.VerifyFiles(rFilePath);

            int lines = 0;

            try
            {
                using StreamReader sr = new StreamReader(rFilePath);
                String line;

                Console.WriteLine("Reading");

                while ((line = sr.ReadLine()) != null)
                {
                    lines++;
                    string tidy = line;
                    string[] tidyParts = tidy.Split(',', StringSplitOptions.RemoveEmptyEntries);

                    CheckUser(tidyParts, userIDIndex, itemIDIndex);
                    CheckBook(tidyParts, userIDIndex, itemIDIndex);
                }

                sr.Close();

                userList = userList.OrderByDescending(x => x.checkouts.Count).ToList();

                if (userList.Count > 0)
                    Console.WriteLine($"Highest count of checkouts by user is [{userList[0].checkouts.Count}]");
                else
                    Console.WriteLine($"Highest count of checkouts by user is [0]");
            }
            catch (Exception e)
            {
                Console.WriteLine($"The file could not be read:\n{e}\n{e.Message}");
            }
        }

        private void CheckBook(string[] tidyParts, int userIDIndex, int itemIDIndex)
        {
            if (!userDict[tidyParts[userIDIndex]].checkouts.Contains(tidyParts[itemIDIndex]))
            {
                userDict[tidyParts[userIDIndex]].checkouts.Add(tidyParts[itemIDIndex]);
            }
        }

        private void CheckUser(string[] tidyParts, int userIDIndex, int itemIDIndex)
        {
            if (userDict.ContainsKey(tidyParts[userIDIndex]) == false)
            {
                User user = new User(tidyParts[userIDIndex]);
                userDict.Add(tidyParts[userIDIndex], user);

                userList.Add(user);
            }
        }

        public class User
        {
            public string id;
            public List<string> checkouts;

            public User(string id)
            {
                this.id = id;
                checkouts = new List<string>();
            }

            public string GetCheckoutsConcat()
            {
                return $"\"{string.Join(",", checkouts)}\"";
            }
        }
    }
}
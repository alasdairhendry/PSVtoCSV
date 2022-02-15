using System;
using System.Collections.Generic;
using System.IO;

namespace PSVtoCSV
{
    public class ConvertUsersToDuoLookup
    {
        private Dictionary<string, User> userDict = new Dictionary<string, User>();
        private List<User> userList = new List<User>();

        public void Run(string rFilePath, string wFilePath, int maxUsers = -1, bool removeSingleCheckouts = false)
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

                    if (string.IsNullOrWhiteSpace(tidyParts[3]))
                        continue;

                    CheckUser(tidyParts);
                    CheckBook(tidyParts);
                }

                sr.Close();

                Console.WriteLine("Writing");
                string delimiter = ",";

                int x = 0;

                for (int i = 0; i < userList.Count; i++)
                {
                    if (removeSingleCheckouts && userList[i].checkouts.Count <= 1) continue;

                    x++;

                    if (maxUsers > -1 && x >= maxUsers)
                        break;

                    string id = userList[i].id;
                    string books = userList[i].GetCheckoutsConcat();

                    if (i < userList.Count - 1)
                        sw.WriteLine(string.Join(delimiter, id, books));
                    else
                        sw.Write(string.Join(delimiter, id, books));
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

        private void CheckBook(string[] tidyParts)
        {
            if (!userDict[tidyParts[3]].checkouts.Contains(tidyParts[^1]))
            {
                userDict[tidyParts[3]].checkouts.Add(tidyParts[^1]);
            }
        }

        private void CheckUser(string[] tidyParts)
        {
            if (userDict.ContainsKey(tidyParts[3]) == false)
            {
                User user = new User(tidyParts[3]);
                userDict.Add(tidyParts[3], user);

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
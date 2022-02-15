using System;
using System.Collections.Generic;
using System.IO;

namespace PSVtoCSV
{
    public class CheckoutsToItemMatrix
    {
        private Dictionary<string, User> userDict = new Dictionary<string, User>();
        private List<User> userList = new List<User>();

        public void Run(string rFilePath, string wFilePath, int maxUsers = -1, int minCheckouts = -1, int maxCheckouts = -1)
        {
            Program.VerifyFiles(rFilePath, wFilePath);

            float value = 19300000.0f;
            int lines = 0;
            int maxUserCheckoutsFound = 0;

            try
            {
                using StreamReader sr = new StreamReader(rFilePath);
                using StreamWriter sw = new StreamWriter(wFilePath);
                String line;

                Console.WriteLine("Reading");

                while ((line = sr.ReadLine()) != null)
                {
                    string tidy = line;
                    string[] tidyParts = tidy.Split(',', StringSplitOptions.RemoveEmptyEntries);

                    if (string.IsNullOrWhiteSpace(tidyParts[3]))
                        continue;

                    CheckUser(tidyParts);
                    CheckBook(tidyParts);
                    lines++;
                }

                sr.Close();

                Console.WriteLine("Writing");

                int x = 0;

                for (int i = userList.Count - 1; i >= 0; i--)
                {
                    if (minCheckouts > -1 && userList[i].checkouts.Count < minCheckouts)
                    {
                        userList.RemoveAt(i);
                        continue;
                    }

                    if (maxCheckouts > -1 && userList[i].checkouts.Count > maxCheckouts)
                    {
                        userList.RemoveAt(i);
                        continue;
                    }

                    if (userList[i].checkouts.Count >= maxUserCheckoutsFound) maxUserCheckoutsFound = userList[i].checkouts.Count;
                }

                List<string> headers = new List<string>();

                for (int i = 0; i < maxUserCheckoutsFound; i++)
                {
                    headers.Add($"Item{i + 1}");
                }

                sw.WriteLine(string.Join(",", headers));

                for (int i = 0; i < userList.Count; i++)
                {
                    if (maxUsers > 0 && x >= maxUsers)
                        break;

                    string books = userList[i].GetCheckoutsConcat();

                    if (i < userList.Count - 1)
                        sw.WriteLine(books);
                    else
                        sw.Write(books);

                    x++;
                }

                sw.Close();

                Console.WriteLine($"Max user checkouts found is  {maxUserCheckoutsFound.Beautify()}");
                Console.WriteLine($"Wrote {x.Beautify()} lines to file");
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
                return string.Join(",", checkouts);
            }
        }

        public void FindHeadingCount(string readfilepath)
        {
            Program.VerifyFiles(readfilepath);

            float value = 19300000.0f;

            try
            {
                using StreamReader sr = new StreamReader(readfilepath);
                String line;

                Console.WriteLine("Reading");
                int maxColumns = 0;
                string largestRowContents = "";
                Dictionary<int, int> di = new Dictionary<int, int>();

                while ((line = sr.ReadLine()) != null)
                {
                    string tidy = line;
                    string[] tidyParts = tidy.Split(',', StringSplitOptions.RemoveEmptyEntries);

                    if (di.ContainsKey(tidyParts.Length))
                    {
                        di[tidyParts.Length]++;
                    }
                    else
                    {
                        di.Add(tidyParts.Length, 1);
                    }

                    // if (tidyParts.Length > maxColumns)
                    // {
                    // maxColumns = tidyParts.Length;
                    // largestRowContents = tidy;
                    // }
                }

                sr.Close();


                Console.Write($"Found {maxColumns.Beautify()} maximum columns");
                Console.Write($"Found {di.Keys.Count.Beautify()} uniques");
                // Console.Write($"Largest Row Contents [{largestRowContents}]");
            }
            catch (Exception e)
            {
                Console.WriteLine($"The file could not be read:\n{e}\n{e.Message}");
            }
        }
    }
}
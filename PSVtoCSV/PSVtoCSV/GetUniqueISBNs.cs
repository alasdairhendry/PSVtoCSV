using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace PSVtoCSV
{
    public class GetUniqueISBNs
    {
        public Dictionary<string, int> dict = new Dictionary<string, int>();
        public List<string> list = new List<string>();

        public void Run(string readfilepath, string writefilepath, int isbnIndex)
        {
            Program.VerifyFiles(readfilepath, writefilepath);

            string[] regexii = new string[]
            {
                // ISBN formats
                @"^978\d{9}[0-9Xx]$",
                @"^\d{9}[0-9Xx]$",
            };

            try
            {
                using StreamReader sr = new StreamReader(readfilepath);
                using StreamWriter sw = new StreamWriter(writefilepath);
                String line;
                int x = 0;

                while ((line = sr.ReadLine()) != null)
                {
                    string[] tidyParts = line.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    x++;

                    // Begin check to verify ISBN
                    bool isValid = false;

                    for (int i = 0; i < regexii.Length; i++)
                    {
                        if (Regex.IsMatch(tidyParts[isbnIndex], regexii[i]))
                        {
                            isValid = true;
                            break;
                        }
                    }

                    if (!isValid)
                    {
                        continue;
                    }

                    if (!dict.ContainsKey(tidyParts[isbnIndex]))
                    {
                        dict.Add(tidyParts[isbnIndex], 1);
                        list.Add(tidyParts[isbnIndex]);
                    }
                    else
                    {
                        dict[tidyParts[isbnIndex]]++;
                    }
                }

                sr.Close();

                for (int i = 0; i < list.Count; i++)
                {
                    sw.WriteLine($"{list[i]},{dict[list[i]]}");
                }

                sw.Close();

                Console.WriteLine($"Wrote {list.Count.Beautify()} Books");
                Console.WriteLine("---- DONE ---");
            }
            catch (Exception e)
            {
                Console.WriteLine($"The file could not be read:\n{e}\n{e.Message}");
            }
        }
    }
}
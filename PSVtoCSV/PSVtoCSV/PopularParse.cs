using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PSVtoCSV
{
    public class PopularParse
    {
        private Dictionary<string, BookEntry> dictionary = new Dictionary<string, BookEntry>();
        private List<BookEntry> list = new List<BookEntry>();

        public void Run(string filepath)
        {
            Program.VerifyFiles(filepath);

            int lines = 0;

            try
            {
                using StreamReader sr = new StreamReader(filepath);
                // using StreamWriter sw = new StreamWriter(NewFilePathLimitedFirstTrimmed);
                String line;

                Console.WriteLine("Reading contents");
                int removed = 0;

                while ((line = sr.ReadLine()) != null)
                {
                    lines++;
                    string tidy = line;
                    string[] tidyParts = tidy.Split(',', StringSplitOptions.RemoveEmptyEntries);

                    AddEntry(tidyParts);
                }

                sr.Close();
                ReadEntries();

                Console.WriteLine($"Removed entries due to date length of [{removed}]");
                Console.WriteLine();
                Console.WriteLine("Program Finished - Press enter to exit");
                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine($"The file could not be read:\n{e}\n{e.Message}");
            }

            Console.WriteLine("Action complete");
        }

        private void AddEntry(string[] tidyParts)
        {
            if (dictionary.ContainsKey(tidyParts[^1]))
            {
                dictionary[tidyParts[^1]].count++;
            }
            else
            {
                dictionary.Add(tidyParts[^1], new BookEntry(tidyParts[0], tidyParts[^1], 1));
                list.Add(dictionary[tidyParts[^1]]);
            }
        }

        private void ReadEntries()
        {
            Console.WriteLine("Sorting contents");
            list = list.OrderByDescending(x => x.count).ToList();
            Console.WriteLine();

            for (int i = 0; i < list.Count; i++)
            {
                Console.WriteLine($"{list[i].count},{list[i].name.Replace(",", "")}");
                if (i >= 100) break;
            }

            Console.WriteLine();
            Console.WriteLine($"{list.Count.Beautify()} entries in list");
        }

        public class BookEntry
        {
            public string name = "";
            public string id = "";
            public int count = 0;

            public BookEntry(string name, string id, int count)
            {
                this.name = name;
                this.id = id;
                this.count = count;
            }
        }
    }
}
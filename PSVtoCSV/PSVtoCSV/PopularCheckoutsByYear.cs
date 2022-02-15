using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PSVtoCSV
{
    public class PopularCheckoutsByYear
    {
        private Dictionary<string, Checkout> idToNameDictionary = new Dictionary<string, Checkout>();
        private List<Checkout> list = new List<Checkout>();

        private int year;

        public void Run(string readfilepath, string writefilepath, int year)
        {
            this.year = year;
            writefilepath = Path.Combine(writefilepath, year.ToString() + ".csv");
            Program.VerifyFiles(readfilepath, writefilepath);

            int lines = 0;

            try
            {
                using StreamReader sr = new StreamReader(readfilepath);
                String line;

                Console.WriteLine("Reading contents");
                int removed = 0;

                while ((line = sr.ReadLine()) != null)
                {
                    lines++;
                    string tidy = line;
                    string[] tidyParts = tidy.Split(',', StringSplitOptions.RemoveEmptyEntries);

                    string date = tidyParts[2];
                    if (date.Length != 12)
                    {
                        removed++;
                        continue;
                    }

                    string[] pieces = new[]
                    {
                        date.Substring(0, 4),
                        date.Substring(4, 2),
                        date.Substring(6, 2),
                        date.Substring(8, 2),
                        date.Substring(10, 2)
                    };

                    AddEntry(tidyParts, pieces);

                    // if (lines >= 100000) break;
                }

                sr.Close();
                // ReadEntries();
                WriteEntries(writefilepath);

                Console.WriteLine($"Removed entries due to date length of [{removed}]");
                Console.WriteLine();
            }
            catch (Exception e)
            {
                Console.WriteLine($"The file could not be read:\n{e}\n{e.Message}");
            }

            Console.WriteLine($"[Action Complete] = Find popular checkouts by year {year}");
        }

        private void AddEntry(string[] tidyParts, string[] datePieces)
        {
            DateTime dt = PiecesToDateTime(datePieces);

            if (dt.Year == year)
            {
                if (!idToNameDictionary.ContainsKey(tidyParts[^1]))
                {
                    idToNameDictionary.Add(tidyParts[^1], new Checkout(tidyParts[0], tidyParts[^1], 1));
                    list.Add(idToNameDictionary[tidyParts[^1]]);
                }
                else
                {
                    idToNameDictionary[tidyParts[^1]].count++;
                }
            }
        }

        private DateTime PiecesToDateTime(string[] pieces)
        {
            return new DateTime(int.Parse(pieces[0]), int.Parse(pieces[1]), int.Parse(pieces[2]), int.Parse(pieces[3]), int.Parse(pieces[4]), 0);
        }

        private void ReadEntries()
        {
            Console.WriteLine("Sorting contents");
            list = list.OrderByDescending(x => x.count).ToList();
            Console.WriteLine();

            for (int i = 0; i < list.Count; i++)
            {
                Console.WriteLine($"{i},{list[i].count},{idToNameDictionary[list[i].id].name.Replace(",", "")}");
                if (i >= 100) break;
            }

            Console.WriteLine();
            Console.WriteLine($"{list.Count.Beautify()} entries in list");
        }

        private void WriteEntries(string writefilepath)
        {
            try
            {
                Console.WriteLine("Sorting contents");
                list = list.OrderByDescending(x => x.count).ToList();
                Console.WriteLine();

                using StreamWriter sw = new StreamWriter(writefilepath);

                sw.WriteLine($"Rank,Checkouts,ID,Title [{list.Count}]");

                for (int i = 0; i < list.Count; i++)
                {
                    sw.WriteLine($"{i},{list[i].count},{list[i].id},{idToNameDictionary[list[i].id].name.Replace(",", "")}");
                    // if (i >= 100) break;
                }

                Console.WriteLine();
                Console.WriteLine($"Finished writing file");
            }
            catch (Exception e)
            {
                Console.WriteLine($"The file could not be wrote:\n{e}\n{e.Message}");
            }
        }

        public class Checkout
        {
            public string name;
            public string id;
            public int count;

            public Checkout(string name, string id, int count)
            {
                this.name = name;
                this.id = id;
                this.count = count;
            }
        }
    }
}
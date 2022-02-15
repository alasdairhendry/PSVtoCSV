using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace PSVtoCSV
{
    public class PopularCheckoutsByMonth
    {
        private Dictionary<string, Checkout> idToCheckoutDictionary = new Dictionary<string, Checkout>();
        private List<Checkout> list = new List<Checkout>();

        private int year = 0;

        // private int currentMonth = 0;

        public void Run(string readfilepath, string writefilepath, int year)
        {
            this.year = year;
            writefilepath = Path.Combine(writefilepath, year.ToString() + " Monthly" + ".csv");
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

                    // if (lines >= 150000) break;
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
                if (!idToCheckoutDictionary.ContainsKey(tidyParts[^1]))
                {
                    idToCheckoutDictionary.Add(tidyParts[^1], new Checkout(tidyParts[0], tidyParts[^1]));
                    list.Add(idToCheckoutDictionary[tidyParts[^1]]);
                }

                idToCheckoutDictionary[tidyParts[^1]].count[dt.Month - 1]++;
            }
        }

        private DateTime PiecesToDateTime(string[] pieces)
        {
            return new DateTime(int.Parse(pieces[0]), int.Parse(pieces[1]), int.Parse(pieces[2]), int.Parse(pieces[3]), int.Parse(pieces[4]), 0);
        }

        private void ReadEntries()
        {
            Console.WriteLine("Sorting contents");

            for (int i = 0; i < 12; i++)
            {
                list = list.OrderByDescending(x => x.count[i]).ToList();

                if (list[0].count[i] == 0)
                    Console.WriteLine($"{CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(i + 1).ToUpper()} - [{list[0].count[i]}] - No Listings");
                else
                    Console.WriteLine($"{CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(i + 1).ToUpper()} - [{list[0].count[i]}] - {idToCheckoutDictionary[list[0].id].name.Replace(",", "")}");
            }

            Console.WriteLine();
            Console.WriteLine($"{list.Count.Beautify()} entries in list");
        }

        private void WriteEntries(string writefilepath)
        {
            try
            {
                Console.WriteLine("Writing contents");

                list = list.OrderByDescending(x => x.count[0]).ToList();
                Console.WriteLine();

                using StreamWriter sw = new StreamWriter(writefilepath);

                sw.WriteLine($"Month,Checkouts,ID,Title");

                for (int i = 0; i < 12; i++)
                {
                    list = list.OrderByDescending(x => x.count[i]).ToList();

                    if (list[0].count[i] == 0)
                        sw.WriteLine($"{CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(i + 1).ToUpper().Substring(0, 3)},{list[0].count[i]},No ID,No Listings");
                    else
                        sw.WriteLine($"{CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(i + 1).ToUpper().Substring(0, 3)},{list[0].count[i]},{list[0].id},{idToCheckoutDictionary[list[0].id].name.Replace(",", "")}");
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
            public List<int> count = new List<int>();

            public Checkout(string name, string id)
            {
                this.name = name;
                this.id = id;

                for (int i = 0; i < 12; i++)
                {
                    count.Add(0);
                }
            }
        }
    }
}
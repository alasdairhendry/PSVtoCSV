using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PSVtoCSV
{
    public class CheckoutsByYear
    {
        private Dictionary<string, string> idToNameDictionary = new Dictionary<string, string>();
        private List<Checkout> list = new List<Checkout>();

        public CheckoutsByYear()
        {
        }

        public void Run(string filepath)
        {
            Program.VerifyFiles(filepath);

            int lines = 0;

            try
            {
                using StreamReader sr = new StreamReader(filepath);
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

        private void AddEntry(string[] tidyParts, string[] datePieces)
        {
            DateTime dt = PiecesToDateTime(datePieces);

            // if (dt.Year == year)
            // {
            if (!idToNameDictionary.ContainsKey(tidyParts[^1]))
            {
                idToNameDictionary.Add(tidyParts[^1], tidyParts[0]);
            }

            Checkout checkout = new Checkout(tidyParts[^1], dt);
            list.Add(checkout);
            // }
        }

        private DateTime PiecesToDateTime(string[] pieces)
        {
            return new DateTime(int.Parse(pieces[0]), int.Parse(pieces[1]), int.Parse(pieces[2]), int.Parse(pieces[3]), int.Parse(pieces[4]), 0);
        }

        private void ReadEntries()
        {
            Console.WriteLine("Sorting contents");
            // list = list.OrderByDescending(x => x.datetime).ToList();
            Console.WriteLine();


            for (int i = 1950; i < 2021; i++)
            {
                Console.WriteLine($"{i},{list.Count(x => x.datetime.Year == i)}");
            }

            // for (int i = 0; i < list.Count; i++)
            // {
            // Console.WriteLine($"{i},{list[i].datetime.ToShortDateString()},{idToNameDictionary[list[i].id].Replace(",", "")}");
            // if (i >= 100) break;
            // }

            Console.WriteLine();
            Console.WriteLine($"{list.Count.Beautify()} entries in list");
        }

        public class Checkout
        {
            public string id;
            public DateTime datetime;

            public Checkout(string id, DateTime datetime)
            {
                this.id = id;
                this.datetime = datetime;
            }
        }
    }
}
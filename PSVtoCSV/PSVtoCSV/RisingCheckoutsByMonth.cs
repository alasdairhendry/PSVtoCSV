using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace PSVtoCSV
{
    public class RisingCheckoutsByMonth
    {
        private Dictionary<string, Checkout> idToCheckoutDictionary = new Dictionary<string, Checkout>();
        private List<Checkout> list = new List<Checkout>();

        private int year = 0;
        private int month = 0;

        private int lastMonthTotalCheckouts = 0;
        private int thisMonthTotalCheckouts = 0;

        public void Run(string readfilepath, string writefilepath, int year, int month)
        {
            idToCheckoutDictionary = new Dictionary<string, Checkout>();
            list = new List<Checkout>();

            lastMonthTotalCheckouts = 0;
            thisMonthTotalCheckouts = 0;

            this.year = year;
            this.month = month;

            writefilepath = Path.Combine(writefilepath, $"{year.ToString()}_{month:00}_{CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month).ToUpper().Substring(0, 3)}.csv");
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
                }

                sr.Close();

                Console.WriteLine("Updating Ratios");
                UpdateRatios();

                Console.WriteLine("Writing File");
                WriteEntries(writefilepath);

                Console.WriteLine($"Removed entries due to date length of [{removed}]");
                Console.WriteLine();
            }
            catch (Exception e)
            {
                Console.WriteLine($"The file could not be read:\n{e}\n{e.Message}");
            }

            Console.WriteLine($"[Action Complete] = Find rising checkouts by year {year:0000} and month {month:00}");
        }

        private void AddEntry(string[] tidyParts, string[] datePieces)
        {
            DateTime entry = PiecesToDateTime(datePieces);
            DateTime last = new DateTime(year, month, 1).AddMonths(-1);
            DateTime current = new DateTime(year, month, 1);

            if (entry.Year == last.Year && entry.Month == last.Month)
            {
                if (!idToCheckoutDictionary.ContainsKey(tidyParts[^1]))
                {
                    idToCheckoutDictionary.Add(tidyParts[^1], new Checkout(tidyParts[0], tidyParts[^1]));
                    list.Add(idToCheckoutDictionary[tidyParts[^1]]);
                }

                idToCheckoutDictionary[tidyParts[^1]].checkoutsByLastMonth++;
                lastMonthTotalCheckouts++;
            }
            else if (entry.Year == current.Year && entry.Month == current.Month)
            {
                if (!idToCheckoutDictionary.ContainsKey(tidyParts[^1]))
                {
                    idToCheckoutDictionary.Add(tidyParts[^1], new Checkout(tidyParts[0], tidyParts[^1]));
                    list.Add(idToCheckoutDictionary[tidyParts[^1]]);
                }

                idToCheckoutDictionary[tidyParts[^1]].checkoutsByThisMonth++;
                thisMonthTotalCheckouts++;
            }
        }

        private void UpdateRatios()
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (list[i].checkoutsByLastMonth <= 0 || list[i].checkoutsByThisMonth <= 0)
                {
                    list.RemoveAt(i);
                    continue;
                }

                list[i].ratioByLastMonth = (float) list[i].checkoutsByLastMonth / (float) lastMonthTotalCheckouts;
                list[i].ratioByThisMonth = (float) list[i].checkoutsByThisMonth / (float) thisMonthTotalCheckouts;
                list[i].increase = list[i].ratioByThisMonth - list[i].ratioByLastMonth;
            }
        }

        private DateTime PiecesToDateTime(string[] pieces)
        {
            return new DateTime(int.Parse(pieces[0]), int.Parse(pieces[1]), int.Parse(pieces[2]), int.Parse(pieces[3]), int.Parse(pieces[4]), 0);
        }

        private void WriteEntries(string writefilepath)
        {
            try
            {
                list = list.OrderByDescending(x => x.increase).ToList();

                using StreamWriter sw = new StreamWriter(writefilepath);

                sw.WriteLine($"Rank,Checkouts Prev Month,Checkouts Curr Month,Increase,ID,Title");

                for (int i = 0; i < list.Count; i++)
                {
                    sw.WriteLine($"{i:00},{list[i].checkoutsByLastMonth},{list[i].checkoutsByThisMonth},{list[i].increase},{list[i].id},{list[i].name}");
                    if (i > 250) break;
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
            public int checkoutsByLastMonth = 0;
            public int checkoutsByThisMonth = 0;
            public float ratioByLastMonth = 0.0f;
            public float ratioByThisMonth = 0.0f;
            public float increase = 0.0f;

            public Checkout(string name, string id)
            {
                this.name = name;
                this.id = id;
            }
        }
    }
}
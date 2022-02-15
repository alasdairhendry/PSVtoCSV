using System;
using System.IO;
using System.Linq;

namespace PSVtoCSV
{
    public class MatchUserCheckouts
    {
        public void Run(string rFilePath)
        {
            Program.VerifyFiles(rFilePath);

            Console.WriteLine("Input ISBNs separated by comma");
            string input = Console.ReadLine();
            string[] isbns = input.Split(",", StringSplitOptions.RemoveEmptyEntries);

            try
            {
                using StreamReader sr = new StreamReader(rFilePath);
                String line;

                Console.WriteLine("Reading");

                int matchingUsers = 0;

                while ((line = sr.ReadLine()) != null)
                {
                    string[] checkoutEntry = line.Split("\t", StringSplitOptions.RemoveEmptyEntries);
                    string[] checkouts = checkoutEntry[1].Split(",");

                    bool containedAll = true;

                    for (int i = 0; i < isbns.Length; i++)
                    {
                        if (!checkouts.Contains(isbns[i]))
                        {
                            containedAll = false;
                            break;
                        }
                    }

                    if (containedAll)
                    {
                        Console.WriteLine($"User [{checkoutEntry[0]}] contains all ISBNs");
                        matchingUsers++;
                    }
                }

                Console.WriteLine($"{matchingUsers.Beautify()} Users have checked out the inputted ISBNs");
                sr.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine($"The file could not be read:\n{e}\n{e.Message}");
            }
        }
    }
}
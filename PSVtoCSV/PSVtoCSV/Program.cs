using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace PSVtoCSV
{
    class Program
    {
        static string[] regexii = new string[]
        {
            // ISBN formats
            @"^978\d{9}[0-9Xx]$",
            @"^\d{9}[0-9Xx]$",
            // ISSN formats
            @"^[0-9]{4}-?[0-9]{3}[0-9Xx]$",
            // UPC formats
            @"^[0-9]{12}$"
        };

        private static string dateRegex = @"^\d{12}$";

        private static string folderPath => "/Users/alasdairhendry/Downloads/";
        private static string FilePath => folderPath + "User Data Read Only";
        private static string FilePathCheckoutsParsed => folderPath + "User Checkouts Validated.csv";
        private static string FilePathUnder50s => folderPath + "Checkouts Under 50s.csv";
        private static string FilePathUnder50sUniqueISBNs => folderPath + "Checkouts Under 50s Unique ISBNs.csv";

        private static PopularParse popularParse = new PopularParse();
        private static CheckoutsByYear checkoutsByYear = new CheckoutsByYear();
        private static PopularCheckoutsByYear popularCheckoutsByYear = new PopularCheckoutsByYear();
        private static PopularCheckoutsByMonth popularCheckoutsByMonth = new PopularCheckoutsByMonth();
        private static RisingCheckoutsByMonth risingCheckoutsByMonth = new RisingCheckoutsByMonth();
        private static ConvertUsersToDuoLookup convertUsersToDuoLookup = new ConvertUsersToDuoLookup();
        private static GetUniqueISBNs generateUniqueISBNs = new GetUniqueISBNs();
        private static MatchUserCheckouts matchUserCheckouts = new MatchUserCheckouts();
        private static CheckoutsToItemMatrix checkoutsToItemMatrix = new CheckoutsToItemMatrix();
        private static UserLookup userLookup = new UserLookup();
        private static GenerateFakeRatings generateFakeRatings = new GenerateFakeRatings();
        private static GenerateUserCheckoutCountSheet generateUserCheckoutCountSheet = new GenerateUserCheckoutCountSheet();

        static void Main(string[] args)
        {
            // TrimTheBigBoy(true, ',', 0, 50, 3, 5, 6, FilePathCheckoutsParsed, FilePathUnder50s);

            // VerifyUniqueEntries(FilePathUnder50s, "Users", 0, 3);
            VerifyUniqueEntries(FilePathUnder50s, "ISBNs", 1, 3);

            // generateUniqueISBNs.Run(FilePathUnder50s, FilePathUnder50sUniqueISBNs, 1);

            // while (true)
            // {
            // string s = Console.ReadLine();
            // VerifyISBN(s, true);
            // }

            // while (true)
            // {
            // userLookup.Run(FilePathCheckoutsParsed);
            // }

            Console.WriteLine("--- APPLICATION FINISH ---");
            Console.ReadLine();
        }

        private static string ToAlphanumeric(string text)
        {
            Regex rgx = new Regex("[^a-zA-Z0-9 ]");
            return rgx.Replace(text, "");
        }

        public static void VerifyFiles(params string[] filepath)
        {
            for (int i = 0; i < filepath.Length; i++)
            {
                if (Directory.Exists(Path.GetDirectoryName(filepath[i])) == false)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(filepath[i]));
                    Console.WriteLine($"{Path.GetDirectoryName(filepath[i])} directory did not exist and has been created");
                }

                if (File.Exists(filepath[i]) == false)
                {
                    File.Create(filepath[i]).Close();
                    Console.WriteLine($"{Path.GetFileName(filepath[i])} file did not exist and has been created");
                }

                Console.WriteLine($"Verified path [{filepath[i]}]");
            }

            Console.WriteLine();
        }

        private static void VerifyUniqueEntries(string readpath, string entryName, int targetIndex, int targetColumnCount = 6, char seperator = ',')
        {
            VerifyFiles(readpath);
            int lines = 0;

            Dictionary<string, bool> allEntries = new Dictionary<string, bool>();

            try
            {
                using StreamReader sr = new StreamReader(readpath);
                String line;
                int unique = 0;
                int skippedDueToLength = 0;

                while ((line = sr.ReadLine()) != null)
                {
                    lines++;
                    string[] tidyParts = line.Split(seperator, StringSplitOptions.RemoveEmptyEntries);

                    if (tidyParts.Length != targetColumnCount)
                    {
                        skippedDueToLength++;
                        Console.WriteLine(string.Join(" - ", tidyParts));
                        continue;
                    }

                    string entry = tidyParts[targetIndex].Trim();

                    if (!allEntries.ContainsKey(entry))
                    {
                        allEntries.Add(entry, false);
                        unique++;

                        // if (unique < 10)
                        // Console.WriteLine($"Found [{entryName}] [{entry}] at index [{lines + 1:00}]");
                    }
                }

                sr.Close();
                Console.WriteLine($"Skipped [{skippedDueToLength.Beautify()}] entries due to their column count.");
                Console.WriteLine($"Found [{allEntries.Count.Beautify()}] unique [{entryName}] in the [{lines.Beautify()}] entries.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"The file could not be read:\n{e}\n{e.Message}");
            }
        }

        private static Dictionary<string, bool> duplicateUserItems = new Dictionary<string, bool>();

        private static Dictionary<string, List<string>> userCheckoutCount = new Dictionary<string, List<string>>();

        private static List<string> userIDs = new List<string>();

        private static void TrimTheBigBoy(bool validate, char seperator, int deleteUserBeforeXCheckouts, int deleteUserAfterXCheckouts, int userIndex, int isbnIndex, int targetColumns, string readpath, string writepath)
        {
            VerifyFiles(readpath, writepath);

            duplicateUserItems = new Dictionary<string, bool>();
            userCheckoutCount = new Dictionary<string, List<string>>();
            userIDs = new List<string>();


            try
            {
                Console.WriteLine();
                Console.WriteLine("Reading");
                Console.WriteLine();
                using StreamReader sr = new StreamReader(readpath);
                using StreamWriter sw = new StreamWriter(writepath);

                String line;

                int removedDueToColumns = 0;
                int removedDueToDuplicate = 0;
                int removedDueToInvalidDate = 0;
                List<string> removedDueToInvalidISBN = new List<string>();
                int wroteLines = 0;
                int readLines = 0;

                int userCheckoutsAdded = 0;

                while ((line = sr.ReadLine()) != null)
                {
                    readLines++;
                    if (readLines % 1000000 == 0) Console.WriteLine($"Read {readLines.Beautify()} lines");
                    string[] tidyParts = line.Split(seperator, StringSplitOptions.RemoveEmptyEntries);

                    if (validate)
                    {
                        if (tidyParts.Length != targetColumns)
                        {
                            removedDueToColumns++;
                            continue;
                        }

                        tidyParts[isbnIndex] = tidyParts[isbnIndex].Trim().Split(' ')[0];

                        for (int i = 0; i < tidyParts.Length; i++)
                        {
                            tidyParts[i] = tidyParts[i].Replace(",", "");
                        }

                        //Begin check to verify ISBN, ISSN, or UPC formats
                        bool isbnValid = VerifyISBN(tidyParts[isbnIndex], false);
                        bool dateValid = VerifyDate(tidyParts[2], false);

                        if (!isbnValid)
                        {
                            removedDueToInvalidISBN.Add(tidyParts[isbnIndex]);
                            continue;
                        }

                        if (!dateValid)
                        {
                            removedDueToInvalidDate++;
                            continue;
                        }
                    }

                    if (duplicateUserItems.ContainsKey($"{tidyParts[userIndex]},{tidyParts[isbnIndex]}"))
                    {
                        removedDueToDuplicate++;
                        continue;
                    }

                    duplicateUserItems.Add($"{tidyParts[userIndex]},{tidyParts[isbnIndex]}", true);

                    if (!userCheckoutCount.ContainsKey(tidyParts[userIndex]))
                    {
                        userCheckoutCount.Add(tidyParts[userIndex], new List<string>());
                        userIDs.Add(tidyParts[userIndex]);
                    }

                    // if (userCheckoutCount[tidyParts[userIndex]].Count < deleteUserAfterXCheckouts + 1)
                    // {
                    // userCheckoutCount[tidyParts[userIndex]].Add($"{string.Join(",", tidyParts)}");
                    userCheckoutCount[tidyParts[userIndex]].Add($"{tidyParts[userIndex]},{tidyParts[isbnIndex]},{tidyParts[2]}");
                    userCheckoutsAdded++;
                    // }

                    // x++;
                }

                Console.WriteLine();
                Console.WriteLine("Writing");
                Console.WriteLine();

                for (int i = 0; i < userIDs.Count; i++)
                {
                    if (userCheckoutCount.ContainsKey(userIDs[i]))
                    {
                        if (userCheckoutCount[userIDs[i]].Count >= deleteUserBeforeXCheckouts)
                        {
                            if (userCheckoutCount[userIDs[i]].Count <= deleteUserAfterXCheckouts)
                            {
                                for (int j = 0; j < userCheckoutCount[userIDs[i]].Count; j++)
                                {
                                    sw.WriteLine(userCheckoutCount[userIDs[i]][j]);
                                    wroteLines++;
                                    if (wroteLines % 1000000 == 0) Console.WriteLine($"Wrote {wroteLines.Beautify()} lines");
                                }
                            }
                        }
                    }
                }

                sr.Close();
                sw.Close();

                Console.WriteLine();
                Console.WriteLine($"Removed {removedDueToDuplicate.Beautify()} due to duplicates");
                Console.WriteLine($"Removed {removedDueToColumns.Beautify()} due to column count");
                Console.WriteLine($"Removed {removedDueToInvalidDate.Beautify()} due to invalid date");
                Console.WriteLine($"Removed {removedDueToInvalidISBN.Count.Beautify()} due to invalid ISBN");
                Console.WriteLine();
                Console.WriteLine($"Found {userIDs.Count.Beautify()} users");
                Console.WriteLine($"Found {userCheckoutsAdded.Beautify()} users checkouts added");

                // for (int i = 0; i < removedDueToInvalidISBN.Count; i++)
                // {
                // Console.WriteLine($"Removed invalid ISBN: [{removedDueToInvalidISBN[i]}]");
                // }

                Console.WriteLine();
                Console.WriteLine($"Finalised - Read {readLines.Beautify()} lines");
                Console.WriteLine($"Finalised - Wrote {wroteLines.Beautify()} lines");
                Console.WriteLine("Finished Trimming The Big Boi");
            }
            catch (Exception e)
            {
                Console.WriteLine($"The file could not be read:\n{e}\n{e.Message}");
            }
        }

        private static bool VerifyISBN(string isbn, bool output)
        {
            for (int i = 0; i < regexii.Length; i++)
            {
                if (Regex.IsMatch(isbn, regexii[i]))
                {
                    if (output) Console.WriteLine($"[{isbn}] verified against regex [{regexii[i]}");
                    return true;
                    break;
                }
            }

            if (output) Console.WriteLine($"[{isbn}] unverified");
            return false;
        }

        private static bool VerifyDate(string date, bool output)
        {
            if (Regex.IsMatch(date, dateRegex))
            {
                if (output) Console.WriteLine($"[{date}] verified against regex [{dateRegex}]");
                return true;
            }

            if (output) Console.WriteLine($"[{date}] unverified");
            return false;
        }

        private static void CountFileLines(string filepath)
        {
            VerifyFiles(filepath);
            int lines = 0;

            try
            {
                using StreamReader sr = new StreamReader(filepath);
                String line;

                while ((line = sr.ReadLine()) != null)
                {
                    lines++;
                }

                sr.Close();

                Console.WriteLine($"Read {lines.Beautify()} line(s) in the document {filepath}.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"The file could not be read:\n{e.Message}");
            }
        }
    }
}
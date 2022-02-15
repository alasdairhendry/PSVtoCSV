using System;
using System.Collections.Generic;
using System.IO;

namespace PSVtoCSV
{
    public class GenerateFakeRatings
    {
        public List<KeyValuePair<int, int>> ratingRatios = new List<KeyValuePair<int, int>>()
        {
            new KeyValuePair<int, int>(0, 1),
            new KeyValuePair<int, int>(1, 2),
            new KeyValuePair<int, int>(2, 4)
        };

        private Random prng;
        private int total = 0;

        public void Run(string readpath, string writepath)
        {
            Program.VerifyFiles(readpath, writepath);

            int lines = 0;
            prng = new Random();

            for (int i = 0; i < ratingRatios.Count; i++)
            {
                total += ratingRatios[i].Value;
            }

            Console.WriteLine($"Total is {total}");

            try
            {
                using StreamReader sr = new StreamReader(readpath);
                using StreamWriter sw = new StreamWriter(writepath);
                String line;

                sw.WriteLine($"User ID,Item ID,Rating");

                while ((line = sr.ReadLine()) != null)
                {
                    lines++;
                    string[] tidyParts = line.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    int random = prng.Next(0, total);
                    int chosenRating = -1;

                    // Console.WriteLine($"random number is {random}");

                    for (int j = 0; j < ratingRatios.Count; j++)
                    {
                        if (random < ratingRatios[j].Value)
                        {
                            chosenRating = ratingRatios[j].Key;
                            // Console.WriteLine($"Chosen rating found to be {chosenRating}");
                            break;
                        }
                        else
                        {
                            random -= ratingRatios[j].Value;
                            // Console.WriteLine($"{random} is greater or equal to current rating ratio of value {ratingRatios[j].Value}");
                        }
                    }

                    sw.WriteLine($"{tidyParts[3]},{tidyParts[^1]},{chosenRating}");
                    // if (lines > 100000) break;
                }

                sr.Close();
                sw.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine($"The file could not be read:\n{e}\n{e.Message}");
            }
        }
    }
}
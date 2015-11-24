using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Upwork.Challenge
{
    class Program
    {
        static string RootPath = "";

        static void Main(string[] args)
        {
            //Console.WriteLine("Enter File Path");
            //string filepath = Console.ReadLine();
            string filepath = @"C:\City\Citys.txt";
            RootPath = Path.GetDirectoryName(filepath);
            string[] fileText = GetFileText(filepath);
            List<City> cities = ConvertTextToCities(fileText);
            CreateCityByPopulationFile(cities);
            CreateInterStatesByCity(cities);
            DegreesFromChicago(cities);
            Console.Read();
        }

        private static void DegreesFromChicago_Old(List<City> cities)
        {
            List<string> interStateNameCollection = cities.SelectMany(c => c.InterStates).Distinct().ToList();
            Dictionary<int, City> paths = new Dictionary<int, City>();
            List<InterState> interStateCollection = new List<InterState>();
            interStateNameCollection.ForEach(i =>
            {
                interStateCollection.Add(new InterState() { Name = i, ConnectedCities = cities.Where(c => c.InterStates.Contains(i)).Select(c => c.Name).ToArray() });
            });
            var chicago = cities.FirstOrDefault(c => c.Name == "Chicago");
            var interStatesOfChicago = chicago.InterStates;
            Dictionary<string, int> foundCities = new Dictionary<string, int>();
            cities.Where(c => c.Name != "Chicago").ToList().ForEach(c =>
              {
                  string endpoint = c.Name;
                  int cost = int.MaxValue;
                  if (c.InterStates.Any(i => interStatesOfChicago.Contains(i)))
                  {
                      cost = 1;
                  }
                  else
                  {
                      c.InterStates.ToList().ForEach(i =>
                      {
                          var connectedCities = interStateCollection.FirstOrDefault(isc => isc.Name == i).ConnectedCities;
                          if (connectedCities.Contains(c.Name))
                          {
                              var alreadyConnectedCities = foundCities.Where(cc => connectedCities.Contains(cc.Key));
                              if (alreadyConnectedCities.Count() > 0)
                              {
                                  cost = alreadyConnectedCities.Min(kp => kp.Value);
                              }

                          }
                      });
                  }
                  foundCities.Add(c.Name, cost == int.MaxValue ? 0 : cost);
              });
        }

        private static void DegreesFromChicago(List<City> cities)
        {
            List<string> interStateNameCollection = cities.SelectMany(c => c.InterStates).Distinct().ToList();
            Dictionary<string, string[]> insWithConnection = new Dictionary<string, string[]>();
            Dictionary<int, City> paths = new Dictionary<int, City>();
            List<InterState> interStateCollection = new List<InterState>();
            Dictionary<string, int> foundCities = new Dictionary<string, int>();
            foundCities.Add("Chicago", 0);
            interStateNameCollection.ForEach(i =>
            {
                insWithConnection[i] = cities.Where(c => c.InterStates.Contains(i)).Select(c => c.Name).ToArray();
            });
            var chicago = cities.FirstOrDefault(c => c.Name == "Chicago");
            foreach (string interstate in chicago.InterStates)
            {
                cities.Where(c => c.Name != "Chicago" && c.InterStates.Contains(interstate)).ToList().ForEach(c => foundCities.Add(c.Name, 1));
            }
            cities.Where(c => !foundCities.ContainsKey(c.Name)).ToList().ForEach(city =>
            {
                foreach (string interstate in city.InterStates)
                {
                    cities.Where(c => c.InterStates.Contains(interstate)).ToList().ForEach(c =>
                    {
                        if (foundCities.ContainsKey(c.Name))
                        {

                        }
                        else
                        {
                            foundCities.Add(c.Name, 1 + 1);
                        }                        
                    });
                }
            });

        }

        private static void CreateInterStatesByCity(List<City> cities)
        {
            string filename = "Interstates_By_City.txt";
            var istatesWithCount = new SortedDictionary<string, int>(new InterStateComparer());
            cities.ForEach(c =>
            {
                foreach (string ins in c.InterStates)
                {
                    if (!istatesWithCount.ContainsKey(ins))
                    {
                        istatesWithCount.Add(ins, 1);
                    }
                    else
                    {
                        istatesWithCount[ins] += 1;
                    }
                }
            });
            StringBuilder builder = new StringBuilder();
            istatesWithCount.ToList().ForEach(d => builder.AppendLine(string.Format("{0} {1}", d.Key, d.Value)));
            WriteToFile(builder, filename);
        }

        private static void WriteToFile(StringBuilder builder, string filename)
        {
            Console.WriteLine(builder.ToString());
            File.WriteAllText(Path.Combine(RootPath, filename), builder.ToString());
        }

        private static void CreateCityByPopulationFile(List<City> cities)
        {
            string filename = "Cities_By_Population.txt";
            var groupedCities = cities.OrderByDescending(c => c.Population).GroupBy(p => p.Population);
            StringBuilder citiesByPopulation = new StringBuilder();
            foreach (var gc in groupedCities)
            {
                citiesByPopulation.AppendLine(gc.Key.ToString());
                citiesByPopulation.AppendLine();
                var orederCities = gc.OrderBy(c => c.State).ThenBy(c => c.Name);
                foreach (var city in orederCities)
                {
                    citiesByPopulation.AppendLine(FormatCityToString(city));
                    citiesByPopulation.AppendLine();
                }
                citiesByPopulation.AppendLine();
            }
            WriteToFile(citiesByPopulation, filename);
        }

        /// <summary>
        /// Gets the file text into a city list collection.
        /// </summary>
        /// <param name="fileText">Text read from the file.</param>

        private static string FormatCityToString(City city)
        {
            Array.Sort(city.InterStates, new InterStateComparer());
            return string.Format("{0},{1}\nInterstates:{2}", city.Name, city.State, string.Join(",", city.InterStates));
        }

        /// <summary>
        /// Gets the file text into a city list collection.
        /// </summary>
        /// <param name="fileText">Text read from the file.</param>

        private static List<City> ConvertTextToCities(string[] lines)
        {
            var Cities = new List<City>();
            foreach (string line in lines)
            {
                Cities.Add(ConvertStringToCity(line));
            }
            return Cities;
        }

        /// <summary>
        /// Gets the file text into a city list collection.
        /// </summary>
        /// <param name="fileText">Text read from the file.</param>

        private static City ConvertStringToCity(string line)
        {
            var city = new City();
            var sl = line.Split(separator: '|');
            city.Population = Convert.ToInt32(sl[0]);
            city.Name = sl[1];
            city.State = sl[2];
            city.InterStates = sl[3].Split(separator: ';');
            return city;
        }

        /// <summary>
        /// Converts the file input to a string array.
        /// </summary>
        /// <param name="filepath">Path of the file.</param>
        /// <returns></returns>
        private static string[] GetFileText(string filepath)
        {
            if (File.Exists(filepath))
            {
                return File.ReadAllLines(filepath);
            }
            return null;
        }

        class InterStateComparer : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                int isx = Convert.ToInt16(x.Replace("I-", ""));
                int isy = Convert.ToInt16(y.Replace("I-", ""));
                return isx.CompareTo(isy);
            }
        }

        private class City
        {
            public long Population { get; set; }

            public string Name { get; set; }

            public string State { get; set; }

            public string[] InterStates { get; set; }
        }

        class InterState
        {
            public string Name { get; set; }

            public string[] ConnectedCities { get; set; }
        }

    }
}

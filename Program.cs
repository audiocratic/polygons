using System;
using System.Collections.Generic;
using System.Linq;
using Polygons;

namespace TerritoryAPI
{
    class Program
    {
        static void Main(string[] args)
        {
            var territories = 
                Territories.JsonImport.GetTerritoriesFromJsonFile(
                    @"C:\users\audio\development\territories\Champaign East Territories.json"
                );

            Console.WriteLine(territories.FirstOrDefault(t => t.Name == "C56").BoundariesAreValid());
            Console.WriteLine("\r\nTerritories with invalid borders:");
            territories
                .Where(t => !t.BoundariesAreValid())
                .ToList()
                .ForEach(t => {
                    Console.WriteLine(t.Name);
                });

            Console.WriteLine("Territories that contain point:");

            var point = new Point((decimal)40.058580, -(decimal)88.253240);

            territories
                .Where(t => 
                    t.ContainsCoordinates(point.X, point.Y))
                .ToList()
                .ForEach(t => {
                    Console.WriteLine(t.Name);
                });
        

            Console.ReadKey();
        }
    }
}

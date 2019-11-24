using System;
using System.Collections.Generic;
//using static json.reader;
using QuickType;
using System.IO;

namespace json
{
    class Program
    {
        static void Main(string[] args)
        {
            string jsonString = File.ReadAllText("zhepa.json");
            var welcome = Welcome.FromJson(jsonString);
            Console.WriteLine(welcome.Data.Persons[0].FirstName.Ru);
        }
    }
}

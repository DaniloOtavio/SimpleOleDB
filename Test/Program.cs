using System;
using Simple.OleDB;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var db = new OleDb("empty.accdb");
            
            Console.WriteLine("All Tables:");
            foreach (var t in db.GetAllTables())
            {
                Console.WriteLine($"> {t}");
            }




        }
    }
}

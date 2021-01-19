using System;
using Simple.OleDB;
using System.Data;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            OleDb DB = new OleDb();

            DB.CreateDatabase("teste", "123");

            var conn = DB.getConnection();

            if (conn.State == ConnectionState.Open)
            {
                Console.WriteLine("The connection is open!");
                conn.Close();
                Console.WriteLine("The connection was closed!");
            }

            //var db = new OleDb("empty.accdb");
            
            //Console.WriteLine("All Tables:");
            //foreach (var t in db.GetAllTables())
            //{
            //    Console.WriteLine($"> {t}");
            //}

        }
    }
}

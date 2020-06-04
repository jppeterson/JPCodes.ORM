using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace JPCodes.ORM.Examples
{
    class Program
    {
        static void Main(string[] args)
        {
            //Example data model
            User data = new User
            {
                UserID = int.MaxValue,
                FirstName = "Tony",
                LastName = "Tiger"
            };

            //Output example queries to the console window
            WriteDefinition(data);

            //Insert the model into the database
            Task.Run(async () => 
            {
                using (MySqlConnection connection = new MySqlConnection("ConnString"))
                {
                    await connection.InsertAsync(data);
                    //await connection.UpdateAsync(data);
                    //await connection.DeleteAsync(data);
                }
            }).Wait();

            //Select a model from the database
            User model = Task.Run(async () =>
            {
                using (MySqlConnection connection = new MySqlConnection("ConnString"))
                {
                    return await connection.ExecuteOneAsync<User>("SELECT TOP 1 * FROM dbo.[User]");
                }
            }).Result;

            //Select multiple models from the database
            List<User> models = Task.Run(async () =>
            {
                using (MySqlConnection connection = new MySqlConnection("ConnString"))
                {
                    return await connection.ExecuteManyAsync<User>("SELECT * FROM dbo.[User]");
                }
            }).Result;

            Console.ReadLine();
        }

        public static void WriteDefinition<T>(T item)
        {
            DataDefinition def = DataDefinition.FromType(typeof(T));

            Console.WriteLine("Tablename:\r\n    " + def.TableName);
            Console.WriteLine();
            Console.WriteLine("Fields:");
            foreach (DataField field in def.Fields)
            {
                Console.WriteLine($"    PropertyName: {field.PropertyName}");
                Console.WriteLine($"       Fieldname: {field.FieldName}");
                Console.WriteLine($"           IsKey: {field.IsKey}");
                Console.WriteLine();
            }
            Console.WriteLine();
            Console.WriteLine("Insert:\r\n    " + def.GenerateInsertSQL());
            Console.WriteLine();
            Console.WriteLine("Update:\r\n    " + def.GenerateUpdateSQL());
            Console.WriteLine();
            Console.WriteLine("Delete:\r\n    " + def.GenerateDeleteSQL());
            Console.WriteLine();
            Console.WriteLine("Where:\r\n    " + def.GenerateWhereSQL());
            Console.WriteLine();
            Console.WriteLine("Params:\r\n    " + string.Join(Environment.NewLine + "    ", def.GenerateParameters(item).Select(P => $"{P.ParameterName, 15} : {P.Value}")));
        }
    }
}

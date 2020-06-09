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
            User user = new User
            {
                UserID = int.MaxValue,
                FirstName = "Tony",
                LastName = "Tiger"
            };

            UserPKSelect pkSelect = new UserPKSelect
            {
                UserID = int.MaxValue
            };

            UserFirstNameSelect fnSelect = new UserFirstNameSelect
            {
                First_Name = "Tony"
            };

            //Output example queries to the console window
            WriteDefinition(user);

            //Output example queries to the console window
            WriteDefinition(pkSelect);

            //Output example queries to the console window
            WriteDefinition(fnSelect);

            //Insert the model into the database
            Task.Run(async () => 
            {
                using (MySqlConnection connection = new MySqlConnection("ConnString"))
                {
                    await connection.InsertAsync(user);
                    //await connection.UpdateAsync(data);
                    //await connection.DeleteAsync(data);
                }
            }).Wait();

            //Select a model from the database
            User model = Task.Run(async () =>
            {
                using (MySqlConnection connection = new MySqlConnection("ConnString"))
                {
                    return await connection.SelectOneAsync<UserPKSelect, User>(new UserPKSelect
                    {
                        User_ID = int.MaxValue
                    });
                }
            }).Result;

            //Select a model from the database
            List<User> models = Task.Run(async () =>
            {
                using (MySqlConnection connection = new MySqlConnection("ConnString"))
                {
                    return await connection.SelectManyAsync<UserFirstNameSelect, User>(new UserFirstNameSelect
                    {
                        First_Name = "Tony"
                    });
                }
            }).Result;

            Console.ReadLine();
        }

        public static void WriteDefinition<T>(T item)
        {
            DataDefinition def = DataDefinition.FromType(typeof(T));

            Console.WriteLine("Tablename:\r\n    " + def.TableName);
            Console.WriteLine();
            Console.WriteLine("Typename:\r\n    " + def.Type.Name);
            Console.WriteLine();
            Console.WriteLine("Fields:");
            foreach (DataField field in def.Fields)
            {
                Console.WriteLine($"    PropertyName: {field.PropertyName}");
                Console.WriteLine($"       Fieldname: {field.FieldName}");
                Console.WriteLine($"           IsKey: {field.IsWhere}");
                Console.WriteLine();
            }
            Console.WriteLine();
            Console.WriteLine("Select:\r\n    " + def.GenerateSelectSQL());
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

    public class MySqlTableAttribute : TableAttribute
    {
        public MySqlTableAttribute() : base() { }
        public MySqlTableAttribute(string tableName) : base(tableName) { }

        public override string ParameterPrefix => "?";
        public override string EncloseObject(string item) => $"`{item}`";
        public override DbParameter CreateParameter(string name, object value) => new MySqlParameter(name, value);
    }
}

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
            WriteDefinition(user);

            UserInsert insert = new UserInsert
            {
                UserID = int.MaxValue,
                FirstName = "Tony",
                LastName = "Tiger"
            };
            WriteDefinition(insert);
            WriteInsert(insert);

            UserPKSelect pkSelect = new UserPKSelect
            {
                UserIDWhere = int.MaxValue
            };
            WriteDefinition(pkSelect);
            WriteSelect(pkSelect);

            UserFirstNameSelect fnSelect = new UserFirstNameSelect
            {
                FirstNameWhere = "Tony"
            };
            WriteDefinition(fnSelect);
            WriteSelect(fnSelect);

            //Insert the model into the database
            Task.Run(async () =>
            {
                using (MySqlConnection connection = new MySqlConnection("ConnString"))
                {
                    await connection.InsertAsync(insert);
                    //await connection.UpdateAsync(data);
                    //await connection.DeleteAsync(data);
                }
            }).Wait();

            //Select a model from the database
            User model = Task.Run(async () =>
            {
                using (MySqlConnection connection = new MySqlConnection("ConnString"))
                {
                    return await connection.SelectOneAsync<UserPKSelect, User>(pkSelect);
                }
            }).Result;

            //Select models from the database
            List<User> models = Task.Run(async () =>
            {
                using (MySqlConnection connection = new MySqlConnection("ConnString"))
                {
                    return await connection.SelectManyAsync<UserFirstNameSelect, User>(fnSelect);
                }
            }).Result;

            Console.ReadLine();
        }

        public static void WriteDefinition<T>(T item)
        {
            DataDefinition def = DataDefinition.FromType(typeof(T));

            Console.WriteLine("Tablename:\r\n" + def.TableName);
            Console.WriteLine("Typename:\r\n" + def.Type.Name);
            Console.WriteLine("Params:\r\n" + string.Join(Environment.NewLine + "", def.GenerateParameters(item).Select(P => $"{P.ParameterName,15} : {P.Value}")));
        }
        public static void WriteSelect<T>(T item)
        {
            DataDefinition def = DataDefinition.FromType(item.GetType());
            Console.WriteLine("Select:\r\n    " + def.GenerateSelectSQL());
            Console.WriteLine();
        }
        public static void WriteInsert<T>(T item)
        {
            DataDefinition def = DataDefinition.FromType(item.GetType());
            Console.WriteLine("Insert:\r\n    " + def.GenerateInsertSQL());
            Console.WriteLine();
        }
        public static void WriteUpdate<T>(T item)
        {
            DataDefinition def = DataDefinition.FromType(item.GetType());
            Console.WriteLine("Update:\r\n    " + def.GenerateUpdateSQL());
            Console.WriteLine();
        }
        public static void WriteDelete<T>(T item)
        {
            DataDefinition def = DataDefinition.FromType(item.GetType());
            Console.WriteLine("Delete:\r\n    " + def.GenerateDeleteSQL());
            Console.WriteLine();
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

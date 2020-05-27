using MySql.Data.MySqlClient;
using System;
using System.Data.Common;
using System.Linq;

namespace JPCodes.ORM.Examples
{
    class Program
    {
        static void Main(string[] args)
        {
            //This only hurts the first time you call FromType
            DataDefinition def = DataDefinition.FromType(typeof(User));

            WriteDefinition(def, new User
            {
                UserID = int.MaxValue,
                FirstName = "Tony",
                LastName = "Tiger"
            });

            Console.ReadKey();
        }

        public static void WriteDefinition<T>(DataDefinition def, T item)
        {
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

    //Required to specify SQL formatting
    [MySqlDataRecord]
    public class User
    {
        //Set a property as Key. This makes it a WHERE parameter in dynamic SQL
        [DataField(IsKey = true)]
        public int UserID { get; set; }

        //Change the default field name
        [DataField(FieldName = "first_name")]
        public string FirstName { get; set; }
        
        //FieldName matches PropertyName
        public string LastName { get; set; }
    }

    public class MySqlDataRecordAttribute : DataRecordAttribute
    {
        public override string ParameterPrefix => "?";
        public override string EncloseObject(string item) => $"`{item}`";
        public override DbParameter CreateParameter(string name, object value) => new MySqlParameter(name, value);
    }
}

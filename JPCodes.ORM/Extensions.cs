using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPCodes.ORM
{
    public static class Extensions
    {
        public static async Task<List<T>> Query<T>(this DbConnection dbConnection, string sql, params (string Name, object Value)[] parameters) where T : new()
        {
            bool opened = dbConnection.State == ConnectionState.Closed;
            try
            {
                DataDefinition definition = DataDefinition.FromType(typeof(T));

                if (opened)
                {
                    await dbConnection.OpenAsync();
                }

                using (DbCommand command = dbConnection.CreateCommand())
                {
                    foreach ((string name, object value) param in parameters)
                    {
                        DbParameter dbparam = command.CreateParameter();
                        dbparam.ParameterName = param.name;
                        dbparam.Value = param.value;
                        command.Parameters.Add(dbparam);
                    }
                    using (DbDataReader rdr = await command.ExecuteReaderAsync())
                    {
                        List<DataField> resultFields = definition.GetFields(
                            Enumerable.Range(0, rdr.FieldCount).Select(IDX => rdr.GetName(IDX))
                        ).ToList();

                        try
                        {
                            List<T> results = new List<T>();
                            while (await rdr.ReadAsync())
                            {
                                T item = new T();
                                foreach (DataField field in resultFields)
                                {
                                    field.Set(item, rdr[field.FieldName]);
                                }
                                results.Add(item);
                            }
                            return results;
                        }
                        finally
                        {
                            rdr.Close();
                        }
                    }
                }
            }
            finally
            {
                if (opened)
                {
                    dbConnection.Close();
                }
            }
        }
    }
}

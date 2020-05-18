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
        private static ConcurrentDictionary<string, List<(int IDX, DataField DF)>> _mapCache = new ConcurrentDictionary<string, List<(int IDX, DataField DF)>>();

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
                    command.CommandText = sql;
                    foreach ((string name, object value) param in parameters)
                    {
                        DbParameter dbparam = command.CreateParameter();
                        dbparam.ParameterName = param.name;
                        dbparam.Value = param.value;
                        command.Parameters.Add(dbparam);
                    }
                    using (DbDataReader rdr = await command.ExecuteReaderAsync())
                    {
                        try
                        {
                            if (!_mapCache.TryGetValue(sql, out List<(int index, DataField datafield)> resultFields))
                            {
                                List<string> fields = Enumerable.Range(0, rdr.FieldCount).Select(IDX => rdr.GetName(IDX).ToUpper()).ToList();

                                _mapCache[sql] = resultFields = definition.Fields
                                    .Select((datafield, index) =>
                                    {
                                        int idx = fields.IndexOf(datafield.FieldName.ToUpper());
                                        if (idx >= 0)
                                        {
                                            return (idx, datafield);
                                        }
                                        return (-1, null);
                                    })
                                    .Where(CPL => CPL.idx >= 0)
                                    .ToList();
                            }

                            List<T> results = new List<T>();
                            while (await rdr.ReadAsync())
                            {
                                T item = new T();
                                foreach (var (index, datafield) in resultFields)
                                {
                                    datafield.Set(item, rdr[index]);
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

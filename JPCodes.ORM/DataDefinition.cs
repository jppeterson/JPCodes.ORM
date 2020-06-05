using FastMember;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace JPCodes.ORM
{
    public class DataDefinition
    {
        #region Instance Members
        public string TableName { get; private set; }
        public Type Type { get; private set; }
        public IDatabase Database { get; private set; }
        public TypeAccessor Accessor { get; private set; }
        public List<DataField> Fields { get; private set; } = new List<DataField>();
        #endregion

        #region Instance Methods
        public IEnumerable<DataField> GetFields(IEnumerable<string> fields)
        {
            return Fields.Where(FLD => fields.Any(STR => STR.Equals(FLD.FieldName, StringComparison.OrdinalIgnoreCase)));
        }
        public List<DbParameter> GenerateParameters<T>(T item) 
            => Fields.Select(F => Database.CreateParameter(F.PropertyName, F.Get(item))).ToList();
        public string GenerateInsertSQL()
        {
            if (Fields.Count == 0)
            {
                throw new NotSupportedException("Type must contain public properties.");
            }

            StringBuilder sb = new StringBuilder($"INSERT INTO {Database.EncloseObject(TableName)} (");
            StringBuilder sb2 = new StringBuilder();
            foreach (DataField field in Fields.Where(F => !F.IsKey))
            {
                sb.AppendFormat("{0},", Database.EncloseObject(field.FieldName));
                sb2.AppendFormat("{0}{1},", Database.ParameterPrefix, field.PropertyName);
            }
            sb.Length--;
            sb.Append(") VALUES (");
            sb.Append(sb2);
            sb.Length--;
            sb.Append(")");

            return sb.ToString();
        }
        public string GenerateUpdateSQL()
        {
            if (Fields.Count == 0)
            {
                throw new NotSupportedException("Type must contain public properties.");
            }

            StringBuilder sb = new StringBuilder($"UPDATE {Database.EncloseObject(TableName)} SET ");
            foreach (DataField field in Fields.Where(F => !F.IsKey))
            {
                sb.AppendFormat("{0}={1}{2},", Database.EncloseObject(field.FieldName), Database.ParameterPrefix, field.PropertyName);
            }
            sb.Append($" WHERE ");
            sb.Append(GenerateWhereSQL());

            return sb.ToString(); 
        }
        public string GenerateDeleteSQL()
        {
            if (Fields.Count == 0)
            {
                throw new NotSupportedException("Type must contain public properties.");
            }

            StringBuilder sb = new StringBuilder($"DELETE FROM {Database.EncloseObject(TableName)} WHERE ");
            sb.Append(GenerateWhereSQL());
            return sb.ToString();
        }
        public string GenerateSelectSQL()
        {
            if (Fields.Count == 0)
            {
                throw new NotSupportedException("Type must contain public properties.");
            }

            StringBuilder sb = new StringBuilder($"SELECT "); 
            foreach (DataField field in Fields.Where(F => !F.IsKey))
            {
                sb.AppendFormat("{0},", Database.EncloseObject(field.FieldName));
            }
            sb.Length--;

            sb.Append(" FROM ");
            sb.Append(Database.EncloseObject(TableName));
            sb.Append(" WHERE ");
            sb.Append(GenerateWhereSQL());

            return sb.ToString();
        }
        public string GenerateWhereSQL()
        {
            List<DataField> keyFields = Fields.Where(F => F.IsKey).ToList();
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < keyFields.Count; i++)
            {
                sb.Append($"{Database.EncloseObject(keyFields[i].FieldName)}={Database.ParameterPrefix}{keyFields[i].PropertyName} AND ");
            }
            sb.Length -= 5;
            return sb.ToString();
        }
        #endregion

        #region Static Accessors
        private static ConcurrentDictionary<Type, DataDefinition> _definitions = new ConcurrentDictionary<Type, DataDefinition>();
        public static DataDefinition FromType(Type t)
        {
            if (_definitions.TryGetValue(t, out DataDefinition temp))
            {
                return temp;
            }
            else
            {
                DataRecordAttribute drAttribute = t.GetCustomAttribute<DataRecordAttribute>();

                DataDefinition def = new DataDefinition
                {
                    TableName = drAttribute?.TableName ?? t.Name,
                    Type = t,
                    Accessor = TypeAccessor.Create(t),
                    Database = drAttribute
                };

                foreach (PropertyInfo info in t.GetProperties())
                {
                    DataFieldAttribute dfAttribute = info.GetCustomAttribute<DataFieldAttribute>();
                    def.Fields.Add(new DataField
                    {
                        ParentDefinition = def,
                        FieldName = dfAttribute?.FieldName ?? info.Name,
                        PropertyName = info.Name,
                        DisplayName = dfAttribute?.DisplayName ?? info.Name,
                        DataType = info.PropertyType,
                        SafeDataType = info.PropertyType.IsConstructedGenericType 
                            ? Nullable.GetUnderlyingType(info.PropertyType) 
                            : info.PropertyType,
                        IsKey = dfAttribute?.IsKey ?? false,
                    });
                }

                _definitions[t] = def;

                return def;
            }
        }
        public static DataDefinition FromType(string typeName)
        {
            return _definitions.FirstOrDefault(DEF => DEF.Key.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase)).Value;
        }
        #endregion
    }
}

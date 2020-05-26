using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace JPCodes.ORM
{
    public class DataField
    {
        public DataDefinition ParentDefinition { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool IsUnique { get; set; }
        public Type DataType { get; set; }
        public Type SafeDataType { get; set; }
        public DbType DbType
        {
            get
            {
                switch (SafeDataType.Name)
                {
                    case "String":
                        return DbType.String;
                    case "Int16":
                        return DbType.Int16;
                    case "Int32":
                        return DbType.Int16;
                    case "Int64":
                        return DbType.Int16;
                    case "Boolean":
                        return DbType.Boolean;
                    case "Decimal":
                        return DbType.Decimal;
                    case "Double":
                        return DbType.Double;
                    case "DateTime":
                        return DbType.DateTime;
                    case "Guid":
                        return DbType.Guid;
                    default:
                        return default;
                }
            }
        }
        public bool Sensitive { get; set; }
        public string FieldName { get; set; }
        public string PropertyName { get; set; }
        public string DisplayName { get; set; }
        public string ParamName { get; set; }

        public void Set(object instance, object value)
        {
            ParentDefinition.Accessor[instance, PropertyName] = value;
        }

        public object Get(object instance)
        {
            return ParentDefinition.Accessor[instance, PropertyName];
        }
    }
}

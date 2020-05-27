using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace JPCodes.ORM
{
    /// <summary>
    /// Specify DB formatting
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class DataRecordAttribute : Attribute, IDatabase
    {
        public string TableName { get; set; }

        public virtual string ParameterPrefix => throw new NotImplementedException();
        public virtual string EncloseObject(string item) => throw new NotImplementedException();
        public virtual DbParameter CreateParameter(string name, object value) => throw new NotImplementedException();

        public DataRecordAttribute() : base() { }
        public DataRecordAttribute(string tableName) : base()
            => TableName = tableName;
    }

    /// <summary>
    /// Specify field setup
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class DataFieldAttribute : Attribute
    {
        public string DisplayName { get; set; }
        public string FieldName { get; set; }
        public bool IsKey { get; set; }

        public DataFieldAttribute() : base() { }
        public DataFieldAttribute(bool isKey) : this() 
        {
            IsKey = isKey;
        }
        public DataFieldAttribute(string fieldName) : this(false)
            => FieldName = DisplayName = fieldName;
        public DataFieldAttribute(string fieldName, bool isKey) : this(isKey)
            => FieldName = DisplayName = fieldName;
        public DataFieldAttribute(string fieldName, string displayName, bool isKey) : this(fieldName, isKey) 
            => DisplayName = displayName;
        public DataFieldAttribute(string fieldName, string displayName) : this(fieldName, displayName, false) { }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace JPCodes.ORM
{
    /// <summary>
    /// Set a table name to be different than the class name.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class DataRecordAttribute : Attribute
    {
        public string TableName { get; set; }
        public DataRecordAttribute() : base() { }
        public DataRecordAttribute(string tableName) : base()
            => TableName = tableName;
    }

    /// <summary>
    /// Set a DisplayName and FieldName to be different than the property name.
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

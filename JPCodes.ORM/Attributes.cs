using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace JPCodes.ORM
{
    /// <summary>
    /// Specify DB formatting
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class TableAttribute : Attribute, IDatabase
    {
        public string TableName { get; set; }

        public virtual string ParameterPrefix => throw new NotImplementedException();
        public virtual string EncloseObject(string item) => throw new NotImplementedException();
        public virtual DbParameter CreateParameter(string name, object value) => throw new NotImplementedException();

        public TableAttribute() : base() { }
        public TableAttribute(string tableName) : base()
            => TableName = tableName;
    }

    /// <summary>
    /// Field to insert, update, or select.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class FieldAttribute : Attribute
    {
        public string DisplayName { get; set; }
        public string FieldName { get; set; }

        public FieldAttribute() : base() { }
        public FieldAttribute(string fieldName) : this()
            => FieldName = DisplayName = fieldName;
        public FieldAttribute(string fieldName, string displayName) : this(fieldName)
            => DisplayName = displayName;
    }

    /// <summary>
    /// Field to specify condition for insert, update, delete, or select
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class FieldWhereAttribute : FieldAttribute
    {
        public FieldWhereAttribute() : base() { }
        public FieldWhereAttribute(string fieldName) : base()
            => FieldName = DisplayName = fieldName;
        public FieldWhereAttribute(string fieldName, string displayName) : this(fieldName)
            => DisplayName = displayName;
    }
}

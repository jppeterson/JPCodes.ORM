using JPCodes.ORM.MySql;
using System;
using System.Collections.Generic;
using System.Text;

namespace JPCodes.ORM.Examples
{
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
}

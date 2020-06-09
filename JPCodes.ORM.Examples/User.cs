using System;
using System.Collections.Generic;
using System.Text;

namespace JPCodes.ORM.Examples
{
    //Required to specify SQL formatting
    [MySqlTable("user")]
    public class User
    {
        //This makes it a WHERE parameter in dynamic SQL
        [Field("user_id")]
        public virtual int UserID { get; set; }

        //Change the default field name
        [Field("first_name")]
        public virtual string FirstName { get; set; }

        [Field("last_name")]
        public virtual string LastName { get; set; }
    }

    public class UserPKSelect : User
    {
        //Where
        [FieldWhere]
        public int User_ID { get; set; }
    }

    public class UserFirstNameSelect : User
    {
        //Where
        [FieldWhere]
        public string First_Name { get; set; }
    }
}

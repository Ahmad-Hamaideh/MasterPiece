//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace test.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class review
    {
        public int review_id { get; set; }
        public int product_id { get; set; }
        public string user_id { get; set; }
        public Nullable<double> rating { get; set; }
        public string comment { get; set; }
        public System.DateTime created_at { get; set; }
        public Nullable<int> is_verified { get; set; }
    
        public virtual AspNetUser AspNetUser { get; set; }
        public virtual Product Product { get; set; }
    }
}

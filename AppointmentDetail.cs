using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreBot
{
    public class AppointmentDetail
    {
        public AppointmentDetail()
        {

        }

        public string Purpose { get; set; }
        public string Type { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Branch { get; set; }
        public string DateTime { get; set; }

        public override string ToString()
        {
            return string.Format("{0} - {1} - {2} - {3} - {4} - {5} - {6}", Purpose, Type, FullName, Phone, Email, Branch, DateTime);
        }
    }
}

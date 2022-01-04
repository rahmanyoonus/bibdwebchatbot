using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreBot
{
    public class Branch
    {
        public Branch()
        {

        }

        public Branch(int id, string name)
        {
            Id = id.ToString();
            Name = name;
        }

        public string Id { get; set; }
        public string Name { get; set; }
    }
}

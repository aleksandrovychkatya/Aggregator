using System;
using System.Collections.Generic;
using System.Text;

namespace Aggregator
{
    public class Category
    {
        public int ID;
        public string Name;
        public string russianName;
        public List<Product> Products;

        public Category(string name)
        {
            Name = name;
            Products = new List<Product>();

        }
    }
}

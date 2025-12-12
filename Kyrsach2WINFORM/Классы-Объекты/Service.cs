using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kyrsach2WINFORM
{
    public class Service
    {
        public Service(string ID, string Name, string Cost, string Duration, string Description = null, string Id_Category = null, string NameCategory = null)
        {
            this.IdService = ID;
            this.Name = Name;
            this.Description = Description;
            this.Id_Category = Id_Category;
            this.Cost = Cost;
            this.Duration = Duration;
            this.NameCategory = NameCategory;
        }

        public string IdService, Name, Description, Id_Category, Cost, Duration, NameCategory;
    }
}

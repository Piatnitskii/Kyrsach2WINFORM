using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kyrsach2WINFORM
{
    public class Client
    {
        public Client(string ID, string Name, string Surname, string Patronymic, string Phone, string FIO = null, string CurrentRowIndex = null)
        {
            this.IdClient = ID;
            this.Name = Name;
            this.Surname = Surname;
            this.Patronymic = Patronymic;
            this.Phone = Phone;
            this.FIO = FIO;
            this.CurrentRowIndex = CurrentRowIndex;
        }

        public string IdClient, Name, Surname, Patronymic, Phone, FIO, CurrentRowIndex;    
    }
}

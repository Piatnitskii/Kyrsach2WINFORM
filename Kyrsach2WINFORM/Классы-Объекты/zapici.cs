using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kyrsach2WINFORM
{
    public class zapici
    {
        public zapici(string ID, string IDClient, string IDMaster, string FIOClient, string FIOMaster, string Phone, string Status, string Price, string Time, string Date, string Dration, string PhoneMaster = null)
        {
            this.ID = ID;
            this.FIOClient = FIOClient;
            this.FIOMaster = FIOMaster;
            this.Phone = Phone;
            this.Status = Status;
            this.Price = Price;

            this.Time = Time;
            this.Date = Date;
            this.Dration = Dration;

            this.PhoneMaster = PhoneMaster;
        }

        public string ID, IDClient, IDMaster, FIOClient,  FIOMaster,  Phone, Status, Time,  Price,  Date, Dration, PhoneMaster;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kyrsach2WINFORM
{
    public class UserSystem
    {
        public UserSystem(string IdUser, string Name, string Surname, string Patronymic, string Login, string Id_Role = null, string NameRole = null, string Password = null)
        {
            this.IdUser = IdUser;
            this.Name = Name;
            this.Surname = Surname;
            this.Patronymic = Patronymic;
            this.Login = Login;
            this.Id_Role = Id_Role;
            this.NameRole = NameRole;
            this.Password = Password;
            
        }
        public string IdUser, Name, Surname, Patronymic, Password, Login, Id_Role, NameRole;
    }
}

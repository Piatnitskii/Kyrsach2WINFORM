using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kyrsach2WINFORM
{
    public class UserSystem
    {
        public UserSystem(string IdUser, string Id_Employe, string Login, string Id_Role = null, string NameRole = null, string Password = null)
        {
            this.IdUser = IdUser;
            this.Id_Employe = Id_Employe;
            this.Login = Login;
            this.Id_Role = Id_Role;
            this.NameRole = NameRole;
            this.Password = Password;
            
        }
        public string IdUser, Id_Employe, Password, Login, Id_Role, NameRole;
    }
}

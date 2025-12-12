using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Kyrsach2WINFORM
{
    //Менеджер для удаленного закрытия формы
    public static class FormManager
    {
        private static Dictionary<string, Form> forms = new Dictionary<string, Form>();

        public static void RegisterForm(string key, Form form)
        {
            forms[key] = form;
        }

        public static void CloseForm(string key)
        {
            if (forms.ContainsKey(key))
            {
                forms[key].Close();
                forms.Remove(key);
            }
        }
    }
}

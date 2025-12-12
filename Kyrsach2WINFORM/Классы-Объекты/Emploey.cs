using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kyrsach2WINFORM
{
    public class Emploey
    {

        public Emploey(string ID, string Name, string Surname, string Patronymic, string Phone, string Birthday, string Post, string Photo = null, string FIO = null, string CurrentRowIndex = null, string Id_Post = null)
        {
            this.IdEmploey = ID;
            this.Name = Name;
            this.Surname = Surname;
            this.Patronymic = Patronymic;
            this.Phone = Phone;
            this.Birthday = Birthday;
            this.Post = Post;
            this.Photo = Photo;
            this.FIO = FIO;
            this.CurrentRowIndex = CurrentRowIndex;
            this.Id_Post = Id_Post;
        }

        public string IdEmploey, Name, Surname, Patronymic, Phone, Birthday, Post, Photo, FIO, CurrentRowIndex, Id_Post;
    }


}

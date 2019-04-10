using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DataModel;

namespace DataAccess
{
    public class UserRepository
    {
        DbModel model = new DbModel();

        public User Get(Guid id)
        {
            return model.Users.FirstOrDefault(p => p.Id == id);
        }

        public User GetByName(string name)
        {
            return model.Users.FirstOrDefault(p => p.Name == name);
        }

        public List<Meet> GetCreatedMeets(Guid userId)
        {
            return model.Users.FirstOrDefault(p => p.Id == userId).CreatedMeets.ToList();
        }

        public List<Meet> GetTakenMeets(Guid userId)
        {
            return model.Users.FirstOrDefault(p => p.Id == userId).TakenMeets.ToList();
        }

        public List<User> GetAll()
        {
            return model.Users.ToList();
        }

        public void Add(User item)
        {
            model.Users.Add(item);

            model.SaveChanges();
        }

        public void Delete(Guid id)
        {
            var entity = model.Users.FirstOrDefault(p => p.Id == id);

            model.Users.Remove(entity);

            model.SaveChanges();
        }

        public void Edit(User item)
        {
            var entity = model.Users.FirstOrDefault(p => p.Id == item.Id);

            entity.Password = item.Password;

            model.SaveChanges();
        }
    }
}

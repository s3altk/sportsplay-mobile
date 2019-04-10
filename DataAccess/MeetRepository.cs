using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DataModel;

namespace DataAccess
{
    public class MeetRepository
    {
        DbModel model = new DbModel();

        public Meet Get(Guid id)
        {
            return model.Meets.FirstOrDefault(p => p.Id == id);
        }

        public Meet GetByName(string name)
        {
            return model.Meets.FirstOrDefault(p => p.Name == name);
        }

        public List<Meet> GetByPlayground(Guid playgroundId)
        {
            return model.Meets.Where(p => p.PlaygroundId == playgroundId).ToList();
        }

        public List<Meet> GetByFounder(Guid founderId)
        {
            return model.Meets.Where(p => p.FounderId == founderId).ToList();
        }

        public List<Meet> GetByDate(DateTime date)
        {
            return model.Meets.Where(p => p.Date == date).ToList();
        }

        public List<User> GetPartakers(Guid meetId)
        {
            return model.Meets.FirstOrDefault(p => p.Id == meetId).Partakers.ToList();
        }

        public void AddPartaker(Match item)
        {
            var meet = model.Meets.FirstOrDefault(p => p.Id == item.MeetId);

            var user = model.Users.FirstOrDefault(p => p.Id == item.UserId);

            meet.Partakers.Add(user);

            model.SaveChanges();
        }

        public void DeletePartaker(Guid meetId, Guid userId)
        {
            var meet = model.Meets.FirstOrDefault(p => p.Id == meetId);

            var user = model.Users.FirstOrDefault(p => p.Id == userId);

            meet.Partakers.Remove(user);

            model.SaveChanges();
        }

        public List<Meet> GetAll()
        {
            return model.Meets.ToList();
        }

        public void Add(Meet item)
        {
            model.Meets.Add(item);

            model.SaveChanges();
        }

        public void Delete(Guid id)
        {
            var entity = model.Meets.FirstOrDefault(p => p.Id == id);

            model.Meets.Remove(entity);

            model.SaveChanges();
        }

        public void Edit(Meet item)
        {
            var entity = model.Meets.FirstOrDefault(p => p.Id == item.Id);

            entity.Name = item.Name;
            entity.Date = item.Date;

            model.SaveChanges();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DataModel;

namespace DataAccess
{
    public class PlaygroundRepository
    {
        DbModel model = new DbModel();

        public Playground Get(Guid id)
        {
            return model.Playgrounds.FirstOrDefault(p => p.Id == id);
        }

        public Playground GetByLocation(double locX, double locY)
        {
            return model.Playgrounds.FirstOrDefault(p => p.LocationX == locX && p.LocationY == locY);
        }

        public Playground GetByAddress(string address)
        {
            return model.Playgrounds.FirstOrDefault(p => p.Address == address);
        }

        public List<Playground> GetAll()
        {
            return model.Playgrounds.ToList();
        }

        public void Add(Playground item)
        {
            model.Playgrounds.Add(item);

            model.SaveChanges();
        }

        public void Delete(Guid id)
        {
            var entity = model.Playgrounds.FirstOrDefault(p => p.Id == id);

            model.Playgrounds.Remove(entity);

            model.SaveChanges();
        }

        public void Edit(Playground item)
        {
            var entity = model.Playgrounds.FirstOrDefault(p => p.Id == item.Id);

            entity = item;

            model.SaveChanges();
        }
    }
}

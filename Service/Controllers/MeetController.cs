using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using Newtonsoft.Json;
using DataAccess;
using DataModel;

namespace Service.Controllers
{
    public class MeetController : ApiController
    {
        MeetRepository rep = new MeetRepository();

        [HttpGet]
        public string Get(Guid id)
        {
            var meet = rep.Get(id);

            string json = JsonConvert.SerializeObject(meet, Formatting.None, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });

            return json;
        }

        [HttpGet]
        public string GetByName(string name)
        {
            var meet = rep.GetByName(name);

            string json = JsonConvert.SerializeObject(meet, Formatting.None, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });

            return json;
        }

        [HttpGet]
        public string GetByPlayground(Guid playgroundId)
        {
            var meets = rep.GetByPlayground(playgroundId);

            string json = JsonConvert.SerializeObject(meets, Formatting.None, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });

            return json;
        }

        [HttpGet]
        public string GetByFounder(Guid founderId)
        {
            var meets = rep.GetByFounder(founderId);

            string json = JsonConvert.SerializeObject(meets, Formatting.None, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });

            return json;
        }

        [HttpGet]
        public string GetByDate(DateTime date)
        {
            var meets = rep.GetByDate(date);

            string json = JsonConvert.SerializeObject(meets, Formatting.None, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });

            return json;
        }

        [HttpGet]
        public string GetPartakers(Guid meetId)
        {
            var partakers = rep.GetPartakers(meetId);

            string json = JsonConvert.SerializeObject(partakers, Formatting.None, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });

            return json;
        }

        [HttpPost]
        public void AddPartaker([FromBody]Match match)
        {
            rep.AddPartaker(match);
        }

        [HttpDelete]
        public void DeletePartaker(Guid meetId, Guid userId)
        {
            rep.DeletePartaker(meetId, userId);
        }

        [HttpGet]
        public string GetAll()
        {
            var meets = rep.GetAll();

            string json = JsonConvert.SerializeObject(meets, Formatting.None, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });

            return json;
        }

        [HttpPost]
        public void Add([FromBody]Meet meet)
        {
            rep.Add(meet);
        }

        [HttpDelete]
        public void Delete(Guid id)
        {
            rep.Delete(id);
        }

        [HttpPut]
        public void Edit([FromBody]Meet meet)
        {
            rep.Edit(meet);
        }
    }
}

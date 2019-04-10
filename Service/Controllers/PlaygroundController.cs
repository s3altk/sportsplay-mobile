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
    public class PlaygroundController : ApiController
    {
        PlaygroundRepository rep = new PlaygroundRepository();

        [HttpGet]
        public string Get(Guid id)
        {
            var pg = rep.Get(id);

            string json = JsonConvert.SerializeObject(pg, Formatting.None, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });

            return json;
        }

        [HttpGet]
        public string GetByLocation(double locX, double locY)
        {
            var pg = rep.GetByLocation(locX, locY);

            string json = JsonConvert.SerializeObject(pg, Formatting.None, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });

            return json;
        }

        [HttpGet]
        public string GetByAddress(string address)
        {
            var pg = rep.GetByAddress(address);

            string json = JsonConvert.SerializeObject(pg, Formatting.None, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });

            return json;
        }

        [HttpGet]
        public string GetAll()
        {
            var pgs = rep.GetAll();

            string json = JsonConvert.SerializeObject(pgs, Formatting.None, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });

            return json;
        }

        [HttpPost]
        public void Add([FromBody]Playground playground)
        {
            rep.Add(playground);
        }

        [HttpDelete]
        public void Delete(Guid id)
        {
            rep.Delete(id);
        }

        [HttpPut]
        public void Edit([FromBody]Playground playground)
        {
            rep.Edit(playground);
        }
    }
}

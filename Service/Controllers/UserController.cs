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
    public class UserController : ApiController
    {
        UserRepository rep = new UserRepository();

        [HttpGet]
        public string Get(Guid id)
        {
            var user = rep.Get(id);

            string json = JsonConvert.SerializeObject(user, Formatting.None, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });

            return json;
        }

        [HttpGet]
        public string GetByName(string name)
        {
            var user = rep.GetByName(name);

            string json = JsonConvert.SerializeObject(user, Formatting.None, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });

            return json;
        }

        [HttpGet]
        public string GetCreatedMeets(Guid userId)
        {
            var meets = rep.GetCreatedMeets(userId);

            string json = JsonConvert.SerializeObject(meets, Formatting.None, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });

            return json;
        }

        [HttpGet]
        public string GetTakenMeets(Guid userId)
        {
            var meets = rep.GetTakenMeets(userId);

            string json = JsonConvert.SerializeObject(meets, Formatting.None, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });

            return json;
        }

        [HttpGet]
        public string GetAll()
        {
            var users = rep.GetAll();

            string json = JsonConvert.SerializeObject(users, Formatting.None, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });

            return json;
        }

        [HttpPost]
        public void Add([FromBody]User value)
        {
            rep.Add(value);
        }

        [HttpDelete]
        public void Delete(Guid id)
        {
            rep.Delete(id);
        }

        [HttpPut]
        public void Edit([FromBody]User value)
        {
            rep.Edit(value);
        }
    }
}

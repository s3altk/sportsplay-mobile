using System;
using System.Linq;
using System.Net;
using System.Text;
using System.IO;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace Mobile
{
    public static class HttpClient
    {
        static HttpWebRequest _request;
        static HttpWebResponse _response;

        public static void Post(string json, string uri)
        {
            var data = Encoding.UTF8.GetBytes(json);

            _request = (HttpWebRequest)WebRequest.Create(uri);
            _request.Method = "POST";
            _request.ContentType = "application/json";
            _request.ContentLength = data.Length;

            var stream = _request.GetRequestStream();
            stream.Write(data, 0, data.Length);
            stream.Close();
        }

        public static string Get(string uri)
        {
            _request = (HttpWebRequest)WebRequest.Create(uri);
            _request.Method = "GET";
            _request.ContentType = "application/json";

            _response = (HttpWebResponse)_request.GetResponse();
            var stream = _response.GetResponseStream();

            var reader = new StreamReader(stream);
            string result = reader.ReadToEnd();

            reader.Close();
            stream.Close();

            result = result.Replace("\\", "");
            result = result.Substring(1, result.Length - 2);

            return result;
        }

        public static void Put(string json, string uri)
        {
            var data = Encoding.UTF8.GetBytes(json);

            _request = (HttpWebRequest)WebRequest.Create(uri);
            _request.Method = "PUT";
            _request.ContentType = "application/json";
            _request.ContentLength = data.Length;

            var stream = _request.GetRequestStream();
            stream.Write(data, 0, data.Length);
            stream.Close();
        }

        public static string Delete(string uri)
        {
            _request = (HttpWebRequest)WebRequest.Create(uri);
            _request.Method = "DELETE";
            _request.ContentType = "application/json";

            _response = (HttpWebResponse)_request.GetResponse();

            return _response.StatusDescription;
        }
    }
}
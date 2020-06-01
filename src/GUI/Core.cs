using System;
using System.IO;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.Net.NetworkInformation;

namespace CheckCovid19
{
    class User
    {
        string _url;
        public User(string url)
        {
            _url = "https://" + url;
        }

        public bool upload(string userID, JObject userData)
        {
            string url = _url;
            WebClient client = new WebClient();
            
            client.Headers.Add("Content-Type","application/json");
            string result = "";
            try
            {
                client.UploadStringCompleted += (sender, e) => {
                    result = e.Result;
                };
                client.UploadStringAsync(new Uri(url + "/api"), "PUT", userData.ToString());
                while (string.IsNullOrEmpty(result))
                {
                    System.Threading.Thread.Sleep(10);
                }
                if ((bool)JObject.Parse(result)["success"])
                {
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
        public JObject addUser(string userID, int grade, int @class, int number, string name)
        {
            JObject user = new JObject();
            user.Add("process", "insert");
            user.Add("id", userID);
            user.Add("grade", grade);
            user.Add("class", @class);
            user.Add("number", number);
            user.Add("name", name);

            if (upload(userID, user))
            {
                user.Add("success", true);
                return user;
            }
            else
            {
                return JObject.Parse("{\"success\":false}");
            }
        }        
        public JObject check(string userID, out int err, bool ondo = false)
        {
            string url = "_url";
            WebClient client = new WebClient();
            JObject user = new JObject();

            user.Add("process", "check");
            user.Add("ondo", ondo);
            user.Add("id", userID);

            bool doing = true;
            string result = "";

            client.UploadStringCompleted += (sender, e) => {
                try
                {
                    result = e.Result;
                }
                catch 
                {
                    result = "{\"success\":false}";
                }
                doing = false;
            };

            client.Headers.Add("Content-Type", "application/json");
            err = (int)errorType.success; //0

            try
            {
                client.UploadStringAsync(new Uri(url + "/api"), "PUT", user.ToString());
            }
            catch (Exception e)
            {
                if (e.HResult == -2146233033) //url이 잘못된 에러
                {
                    //err = (int)errorType.urlerror; //2
                    err = (int)errorType.timeout;
                }
                else
                {
                    err = (int)errorType.timeout; //1
                }
                doing = false;
                result = "{\"success\":false}";
            }

            while (doing)
            {
                System.Threading.Thread.Sleep(10);
            }

            return JObject.Parse(result);
        }
        public JObject check(string grade, string @class, string number, bool ondo = false)
        {
            string url = _url;
            WebClient client = new WebClient();
            JObject user = new JObject();

            user.Add("process", "check");
            user.Add("ondo", ondo);
            user.Add("grade", grade);
            user.Add("class", @class);
            user.Add("number", number);
            string result = "";

            client.UploadStringCompleted += (sender, e) => {
                try
                {
                    result = e.Result;
                }
                catch
                {
                    result = "{\"success\":false}";
                }
            };

            client.Headers.Add("Content-Type", "application/json");
            client.UploadStringAsync(new Uri(url + "/api"), "PUT", user.ToString());

            while (string.IsNullOrEmpty(result))
            {
                System.Threading.Thread.Sleep(10);
            }

            return JObject.Parse(result);
        }
        public JObject uncheck(string userID)
        {
            string url = _url;
            WebClient client = new WebClient();
            JObject user = new JObject();

            user.Add("process", "uncheck");
            user.Add("id", userID);
            string result = "";

            client.UploadStringCompleted += (sender, e) => {
                try
                {
                    result = e.Result;
                }
                catch
                {
                    result = "{\"success\":false}";
                }
            };

            client.Headers.Add("Content-Type", "application/json");
            client.UploadStringAsync(new Uri(url + "/api"), "PUT", user.ToString());
            while (string.IsNullOrEmpty(result))
            {
                System.Threading.Thread.Sleep(10);
            }
            return JObject.Parse(result);
        }
        public JObject uncheck(string grade, string @class, string number)
        {
            string url = _url;
            WebClient client = new WebClient();
            JObject user = new JObject();

            user.Add("process", "uncheck");
            user.Add("grade", grade);
            user.Add("class", @class);
            user.Add("number", number);
            string result = "";

            client.UploadStringCompleted += (sender, e) => {
                try
                {
                    result = e.Result;
                }
                catch
                {
                    result = "{\"success\":false}";
                }
            };

            client.Headers.Add("Content-Type", "application/json");
            client.UploadStringAsync(new Uri(url + "/api"), "PUT", user.ToString());

            while (string.IsNullOrEmpty(result))
            {
                System.Threading.Thread.Sleep(10);
            }

            return JObject.Parse(result);
        }
        public JObject delUser(string userID)
        {
            string url = _url;
            WebClient client = new WebClient();
            WebClient del = new WebClient();
            JObject user = new JObject();

            user.Add("process", "delete");
            user.Add("id", userID);
            string result = "";

            client.UploadStringCompleted += (sender, e) => {
                result = e.Result;
            };

            client.Headers.Add("Content-Type", "application/json");
            client.UploadStringAsync(new Uri(url + "/api"), "PUT", user.ToString());

            while (string.IsNullOrEmpty(result))
            {
                System.Threading.Thread.Sleep(10);
            }
            return JObject.Parse(result);
        }
        public JObject delUser(string grade, string @class, string number)
        {
            string url = _url;
            WebClient client = new WebClient();
            WebClient del = new WebClient();
            JObject user = new JObject();

            user.Add("process", "delete");
            user.Add("grade", grade);
            user.Add("class", @class);
            user.Add("number", number);
            string result = "";

            client.UploadStringCompleted += (sender, e) => {
                result = e.Result;
            };

            client.Headers.Add("Content-Type", "application/json");
            client.UploadStringAsync(new Uri(url + "/api"), "PUT", user.ToString());
            
            while (string.IsNullOrEmpty(result))
            {
                System.Threading.Thread.Sleep(10);
            }
            return JObject.Parse(result);
        }
        public int getPing()
        {
            Ping p = new Ping();
            var r = p.Send(File.ReadAllLines("config.txt")[0]);
            Console.WriteLine(r.RoundtripTime);
            return 0;
        }

        enum errorType
        {
            success,
            timeout,
            urlerror
        }
    }
}
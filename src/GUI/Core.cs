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

        public JObject upload(JObject data, out int err)
        {
            string url = _url;
            WebClient client = new WebClient();
            string result = "";
            bool doing = true;

            err = (int)errorType.success;

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
            try
            {
                client.UploadStringAsync(new Uri(url + "/api"), "PUT", data.ToString());
            }
            catch (Exception e)
            {
                if (e.HResult == -2146233033) //Uri 잘못된거
                {
                    err = (int)errorType.urlerror;
                }
                else
                {
                    err = (int)errorType.timeout;
                }
                result = "{\"success\":false}";
                doing = false;
            }

            while (doing) {
            } //작업이 완료될 때 까지 기다리기

            return JObject.Parse(result);
        }
        public JObject addUser(string userID, int grade, int @class, int number, string name, out int err)
        {
            JObject user = new JObject();
            user.Add("process", "insert");
            user.Add("id", userID);
            user.Add("grade", grade);
            user.Add("class", @class);
            user.Add("number", number);
            user.Add("name", name);

            return upload(user, out err);
        }        
        public JObject check(string userID, out int err, bool ondo = false)
        {
            JObject user = new JObject();
            user.Add("process", "check");
            user.Add("ondo", ondo);
            user.Add("id", userID);
            
            return upload(user, out err);
        }
        public JObject check(string grade, string @class, string number, out int err, bool ondo = false)
        {
            JObject user = new JObject();
            user.Add("process", "check");
            user.Add("ondo", ondo);
            user.Add("grade", grade);
            user.Add("class", @class);
            user.Add("number", number);

            return upload(user, out err);
        }
        public JObject uncheck(string userID, out int err)
        {
            JObject user = new JObject();
            user.Add("process", "uncheck");
            user.Add("id", userID);

            return upload(user, out err);
        }
        public JObject uncheck(string grade, string @class, string number, out int err)
        {
            JObject user = new JObject();

            user.Add("process", "uncheck");
            user.Add("grade", grade);
            user.Add("class", @class);
            user.Add("number", number);
            
            return upload(user, out err);
        }
        public JObject delUser(string userID, out int err)
        {
            JObject user = new JObject();

            user.Add("process", "delete");
            user.Add("id", userID);
            
            return upload(user, out err);
        }
        public JObject delUser(string grade, string @class, string number, out int err)
        {
            JObject user = new JObject();
            user.Add("process", "delete");
            user.Add("grade", grade);
            user.Add("class", @class);
            user.Add("number", number);

            return upload(user, out err);
        }
        public long getPing()
        {
            Ping p = new Ping();
            var r = p.Send(File.ReadAllLines("config.txt")[0]);
            return r.RoundtripTime;
        }

        enum errorType
        {
            success,
            timeout,
            urlerror
        }
    }
}
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
        const int versions = 0;
        string[] _url = new string[2];

        public string url
        {
            set 
            {
                _url[0] = "http://" + value;
                _url[1] = "https://" + value;
            }
        }
        public User(string url)
        {
            _url[0] = "http://" + url;
            _url[1] = "https://" + url;
        }

        public JObject upload(JObject data, out int err)
        {
            WebClient client = new WebClient();
            string result = "";
            bool doing = true;

            err = (int)errorType.success;
            int errTemp = 0;
            int tryUpload = 0;

            client.UploadStringCompleted += (sender, e) => {
                tryUpload++;
                try
                {
                    result = e.Result;
                }
                catch (Exception ex)
                {                        
                    if (ex.HResult == -2146232828) //Uri 잘못된거
                    {
                        if (tryUpload < 2)
                        {
                            try
                            {
                                client.Headers.Add("Content-Type", "application/json");
                                client.UploadStringAsync(new Uri(_url[1] + "/api"), "PUT", data.ToString());
                            }
                            catch (Exception ee)
                            {
                                if (ee.HResult == -2146232828)
                                {
                                    errTemp = (int)errorType.urlerror;
                                }
                                else
                                {
                                    errTemp = (int)errorType.timeout;
                                }
                                result = "{\"success\":false}";
                            }
                        }
                        else
                        {
                            result = "{\"success\":false}";
                            errTemp = (int)errorType.urlerror;
                            doing = false;
                        }
                    }
                    else
                    {
                        errTemp = (int)errorType.timeout;
                        result = "{\"success\":false}";
                    }
                }
                
            };

            client.Headers.Add("Content-Type", "application/json");
            try
            {
                client.UploadStringAsync(new Uri(_url[0] + "/api"), "PUT", data.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                if (e.HResult == -2146233033) //Uri 잘못된거
                {
                    try
                    {
                        client.Headers.Add("Content-Type", "application/json");
                        client.UploadStringAsync(new Uri(_url[1] + "/api"), "PUT", data.ToString());
                    }
                    catch (Exception ee)
                    {
                        if (ee.HResult == -2146233033)
                        {
                            errTemp = (int)errorType.urlerror;
                            result = "{\"success\":false}";
                        }
                        else
                        {
                            errTemp = (int)errorType.timeout;   
                            result = "{\"success\":false}";
                        }                    
                    }
                }
                else
                {
                    errTemp = (int)errorType.timeout;
                    result = "{\"success\":false}";
                }
                
                doing = false;
            }

            while (doing) {
            } //작업이 완료될 때 까지 기다리기
            err = errTemp;

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
        public long getPing(string url)
        {
            Ping p = new Ping();
            PingReply pr = null;
            try
            {
               pr = p.Send(url);
               return pr.RoundtripTime;
            }
            catch
            {
                return 0;
            }
        }
        public bool hasNewVersion(int now, out JArray result)
        {
            WebClient client = new WebClient();
            client.Encoding = System.Text.Encoding.UTF8;
            client.Headers.Add("user-agent", "CovidCheckClientCheckUpdate");
            JArray down = JArray.Parse(client.DownloadString("https://api.github.com/repos/SoftWareAndGuider/Covid-Check-Client/releases"));

            result = new JArray();

            if (down.Count == 0) return false;
            if (down.Count > now)
            {
                result = down;
                return true;
            }
            return false;
        }
        enum errorType
        {
            success,
            timeout,
            urlerror
        }
    }
}
using System;
using System.IO;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace CheckCovid19
{
    class User
    {
        public bool upload(string userID, JObject userData)
        {
            string url = File.ReadAllLines("config.txt")[0];
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
        public string addUser(string userID, int grade, int @class, int number, string name)
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
                return $"{grade}학년 {@class}반 {number}번 {name}(ID: {userID}) 추가가 완료되었습니다.";
            }
            else
            {
                return $"추가 실패, 정보를 확인 후 다시 시도해주세요.";
            }
        }        
        public JObject check(string userID)
        {
            string url = File.ReadAllLines("config.txt")[0];
            WebClient client = new WebClient();
            JObject user = new JObject();

            user.Add("process", "check");
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
        public JObject uncheck(string userID)
        {
            string url = File.ReadAllLines("config.txt")[0];
            WebClient client = new WebClient();
            JObject user = new JObject();

            user.Add("process", "uncheck");
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
    }
}
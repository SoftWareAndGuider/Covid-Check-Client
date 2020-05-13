using System;
using System.IO;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace CheckCovid19
{
    class User
    {
        public string download(string userID)
        {
            return "";
        }
        public bool upload(string userID, JObject userData)
        {
            string url = File.ReadAllLines("config.txt")[0];
            WebClient client = new WebClient();
            
            client.Headers.Add("user-agent", "CheckCovid19");
            try
            {
                client.UploadString(url + "/" + userID, "PUT", userData.ToString());
                return true;
            }
            catch
            {
                return false;
            }
        }
        public string addUser(string userID, int grade, int @class, int number, string name)
        {
            JObject user = new JObject();
            user.Add("grade", grade);
            user.Add("class", @class);
            user.Add("number", number);
            if (upload(userID, user))
            {
                return $"{grade}학년 {@class}반 {number}번 {name}(ID: {userID}) 추가가 완료되었습니다.";
            }
            else
            {
                return $"추가 실패, 정보를 확인 후 다시 시도해주세요.";
            }
        }        
    }
}
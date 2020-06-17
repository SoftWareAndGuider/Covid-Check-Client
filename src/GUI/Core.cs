using System;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Net.NetworkInformation;

namespace CheckCovid19
{
    class User
    {
        string _url = ""; //정보를 주고받을 url
        

        public string url
        {
            get
            {
                return _url;
            }
            set
            {
                _url = value;
            }
        }
        public User(string url)
        {
            _url = url;
        }

        public JObject upload(JObject data, out int err, bool retry = false) //업로드
        {
            MyWebCient client = new MyWebCient(); //타임아웃 시간을 조정한 WebClient
            string result = "";
            bool doing = true;

            err = (int)errorType.success; //기본
            int errTemp = 0; //catch문 안에서 사용할 err변수

            client.UploadStringCompleted += (sender, e) => {
                try
                {
                    result = e.Result;
                }
                catch (Exception ex)
                {
                    WebException web = (WebException)ex.InnerException;
                    if (web.Status == WebExceptionStatus.Timeout) //타임아웃
                    {
                        errTemp = (int)errorType.timeout;
                        result = @"{""success"":false}";
                    }
                    else
                    {
                        if (retry) //이미 재시도한거라면 urlerror
                        {
                            errTemp = (int)errorType.urlerror;
                            result = @"{""sucess"":false}";
                        }
                        else
                        {
                            result = upload(data, out errTemp, true).ToString(); //https://가 아니라 http://를 사용할 수 있으니 다시 시도
                        }
                    }
                }
                doing = false;                
            };

            client.Headers.Add("Content-Type", "application/json");
            try
            {
                if (retry) //https 우선
                {
                    client.UploadStringAsync(new Uri("http://" + _url + "/api"), "PUT", data.ToString());
                }
                else
                {
                    client.UploadStringAsync(new Uri("https://" + _url + "/api"), "PUT", data.ToString());
                }
            }
            catch
            {
                result = @"{""success"":false}";
                errTemp = (int)errorType.urlerror;
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
        public long getPing() //핑
        {
            if (_url == "localhost") return 0; //localhost 입구컷
            try
            {
                return new Ping().Send(_url).RoundtripTime;
            }
            catch
            {
                return -1; //-1: 알 수 없음
            }
        }
        public bool hasNewVersion(int now, out JArray result) //새로운 버전이 있는지 확인하는 것
        {
            try
            {
                WebClient client = new WebClient();
                client.Encoding = System.Text.Encoding.UTF8;
                client.Headers.Add("user-agent", "CovidCheckClientCheckUpdate"); //user-agent가 없으면 github api가 연결 거부
                JArray down = JArray.Parse(client.DownloadString("https://api.github.com/repos/SoftWareAndGuider/Covid-Check-Client/releases")); //GitHub Api 사랑해요

                result = new JArray();

                if (down.Count == 0) return false;
                if (down.Count > now)
                {
                    result = down;
                    return true;
                }
                return false;
            }
            catch 
            {
                result = new JArray();
                return false;
            }
        }
        enum errorType
        {
            success,
            timeout,
            urlerror
        }
    }
    class MyWebCient : WebClient
    {
        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest request = base.GetWebRequest(address);
            request.Timeout = 5000; //타임아웃 시간
            return request;
        }
    }
}
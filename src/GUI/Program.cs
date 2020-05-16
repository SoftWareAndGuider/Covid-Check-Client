using System;
using System.Threading;
using System.Threading.Tasks;
using CheckCovid19;
using Newtonsoft.Json.Linq;
using Gtk;

namespace CovidCheckClientGui
{
    partial class Program : Window
    {
        static void Main(string[] args)
        {
            Application.Init();
            new Program();
            Application.Run();
        }
        void ifTeacher(title t)
        {
            if (t == title.check)
            {
                checkInsertGrade.Sensitive = !checkIsTeacher.Active;
                checkInsertClass.Sensitive = !checkIsTeacher.Active;
                checkInsertUser.Sensitive = isFull(title.check);
            }
            else if (t == title.uncheck)
            {
                uncheckInsertGrade.Sensitive = !uncheckIsTeacher.Active;
                uncheckInsertClass.Sensitive = !uncheckIsTeacher.Active;
                uncheckInsertUser.Sensitive = isFull(title.uncheck);
            }
            else
            {
                addInsertGrade.Sensitive = !addIsTeacher.Active;
                addInsertClass.Sensitive = !addIsTeacher.Active;
                insertUser.Sensitive = isFull(title.add);
            }
        }
        enum title
        {
            check,
            uncheck,
            add
        }

        void uncheckIDLengthChangeValue(object sender, EventArgs e)
        {
            if (checkIDLength.Value == uncheckIDLength.Value) return;
            checkIDLength.Value = uncheckIDLength.Value;
            addLog($"바코드 길이가 {uncheckIDLength.Value}(으)로 조정됨");
        }
        void checkIDLengthChangeValue(object sender, EventArgs e)
        {
            if (checkIDLength.Value == uncheckIDLength.Value) return;
            uncheckIDLength.Value = checkIDLength.Value;
            addLog($"바코드 길이가 {uncheckIDLength.Value}(으)로 조정됨");
        }
        
        async void checkInsertIDChangeText(object sender, EventArgs e)
        {            
            if (checkInsertID.Text.Length != checkIDLength.Value) return;
            await Task.Delay(10);
            if (checkInsertID.Text.Length != checkIDLength.Value) return;
            Thread thread = new Thread(new ThreadStart(() => {check(checkInsertID.Text);}));
            thread.Start();
            await Task.Delay(10);
            checkInsertID.Text = "";
        }
        async void uncheckInsertIDChangeText(object sender, EventArgs e)
        {
            if (uncheckInsertID.Text.Length != uncheckIDLength.Value) return;
            await Task.Delay(10);
            if (uncheckInsertID.Text.Length != uncheckIDLength.Value) return;
            Thread thread = new Thread(new ThreadStart(() => {uncheck(uncheckInsertID.Text);}));
            thread.Start();
            await Task.Delay(10);
            uncheckInsertID.Text = "";
        }
        
        async void checkOKClicked(object sender, EventArgs e)
        {
            Thread thread = new Thread(new ThreadStart(() => {check(checkInsertID.Text);}));
            thread.Start();
            await Task.Delay(10);
            checkInsertID.Text = "";
        }
        async void uncheckOKClicked(object sender, EventArgs e)
        {
            Thread thread = new Thread(new ThreadStart(() => {uncheck(uncheckInsertID.Text);}));
            thread.Start();
            await Task.Delay(10);
            uncheckInsertID.Text = "";
        }
        void insertUserClicked(object sender, EventArgs e)
        {
            Thread thread = new Thread(new ThreadStart(() => {addUser(addIsTeacher.Active, addInsertID.Text, addInsertNumber.Text, addInsertName.Text, addInsertGrade.Text, addInsertClass.Text);}));
            addInsertID.Sensitive = false;
            addInsertNumber.Sensitive = false;
            addInsertName.Sensitive = false;
            addInsertGrade.Sensitive = false;
            addInsertClass.Sensitive = false;
            insertUser.Sensitive = false;
            thread.Start();
        }

        void checkInsertUserClicked(object sender, EventArgs e)
        {
            Thread thread = new Thread(new ThreadStart(() => {check(checkInsertGrade.Text, checkInsertClass.Text, checkInsertNumber.Text);}));
            thread.Start();
            checkInsertGrade.Sensitive = false;
            checkInsertClass.Sensitive = false;
            checkInsertNumber.Sensitive = false;
            checkInsertUser.Sensitive = false;
        }

        void check(string id)
        {
            User user = new User();
            JObject result = user.check(id);
            string toLog = "";
            if ((bool)result["success"])
            {
                toLog = $"{result["data"]["grade"]}학년 {result["data"]["class"]}반 {result["data"]["number"]}번 {result["data"]["name"]}(ID: {result["data"]["id"]}) 체크됨";
            }
            else
            {
                toLog = $"체크 실패 (인식된 ID: {id})";
            }
            Application.Invoke (delegate {
                addLog(toLog);
            });
        }
        void check(string grade, string @class, string number)
        {
            User user = new User();
            JObject result = new JObject();
            if (checkIsTeacher.Active)
            {
                grade = "0";
                @class = "0";
            }
            result = user.check(grade, @class, number);
            string toLog = "";
            if ((bool)result["success"])
            {
                toLog = $"{result["data"]["grade"]}학년 {result["data"]["class"]}반 {result["data"]["number"]}번 {result["data"]["name"]}(ID: {result["data"]["id"]}) 체크됨";
            }
            else
            {
                toLog = $"체크 실패 (인식된 정보: {grade}학년 {@class}반 {number}번)";
            }
            Application.Invoke (delegate {
                addLog(toLog);
                checkInsertGrade.Sensitive = !checkIsTeacher.Active;
                checkInsertClass.Sensitive = !checkIsTeacher.Active;
                checkInsertNumber.Sensitive = true;
                checkInsertGrade.Text = "";
                checkInsertClass.Text = "";
                checkInsertNumber.Text = "";
            });
        }
        void uncheck(string id)
        {
            User user = new User();
            JObject result = user.uncheck(id);
            string toLog = "";
            if ((bool)result["success"])
            {
                toLog = $"{result["data"]["grade"]}학년 {result["data"]["class"]}반 {result["data"]["number"]}번 {result["data"]["name"]}(ID: {id}) 체크 해제됨";
            }
            else
            {
                toLog = $"체크 해제 실패 (인식된 ID: {id})";
            }
            Application.Invoke (delegate {
                addLog(toLog);
            });
        }
        
        void addUser(bool isNotStudent, string id, string number, string name, string grade, string @class)
        {
            User user = new User();
            JObject result = new JObject();
            
            if (isNotStudent)
            {
                grade = "0";
                @class = "0";
            }
            result = user.addUser(id, int.Parse(grade), int.Parse(@class), int.Parse(number), name);
            string toLog = "";
            if ((bool)result["success"])
            {
                toLog = $"{grade}학년 {@class}반 {number}번 이름: {name}(ID: {id}) 추가됨";
            }
            else
            {
                toLog = $"사용자 추가에 실패함 ({grade}학년 {@class}반 {number}번 이름: {name}(ID: {id}))";
            }
            Application.Invoke(delegate {
                addInsertID.Sensitive = true;
                addInsertNumber.Sensitive = true;
                addInsertName.Sensitive = true;
                addInsertGrade.Sensitive = !addIsTeacher.Active;
                addInsertClass.Sensitive = !addIsTeacher.Active;
                addInsertID.Text = "";
                addInsertNumber.Text = "";
                addInsertName.Text = "";
                addInsertClass.Text = "";
                addInsertGrade.Text = "";
                addLog(toLog);
            });
        }
 
        bool isFull(title t)
        {
            if (t == title.check)
            {
                if (string.IsNullOrEmpty(checkInsertNumber.Text))
                {
                    return false;
                }

                if (checkIsTeacher.Active)
                {
                    return true;
                }
                else
                {
                    if (string.IsNullOrEmpty(checkInsertGrade.Text) || string.IsNullOrEmpty(checkInsertClass.Text)) return false;
                    return true;
                }
            }
            else if (t == title.uncheck)
            {
                if (string.IsNullOrEmpty(uncheckInsertNumber.Text))
                {
                    return false;
                }

                if (uncheckIsTeacher.Active)
                {
                    return true;
                }
                else
                {
                    if (string.IsNullOrEmpty(uncheckInsertGrade.Text) || string.IsNullOrEmpty(uncheckInsertClass.Text)) return false;
                    return true;
                }
            }
            else
            {
                if (string.IsNullOrEmpty(addInsertID.Text) || string.IsNullOrEmpty(addInsertName.Text) || string.IsNullOrEmpty(addInsertNumber.Text)) return false;

                if (addIsTeacher.Active) return true;
                else
                {
                    if (string.IsNullOrEmpty(addInsertGrade.Text) || string.IsNullOrEmpty(addInsertClass.Text)) return false;
                    return true;
                }
            }
        }


        void checkWithoutIDKeyRelease(object sender, EventArgs e) => checkInsertUser.Sensitive = isFull(title.check);
        void addUserKeyRelease(object sender, EventArgs e) => insertUser.Sensitive = isFull(title.add);
        void uncheckWithoutIDKeyRelease(object sender, EventArgs e) => uncheckInsertUser.Sensitive = isFull(title.uncheck);
    }
}

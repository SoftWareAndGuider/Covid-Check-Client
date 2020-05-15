using System;
using CheckCovid19;
using Newtonsoft.Json.Linq;
using Gtk;
using System.Threading;

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
        void isTeacherClicked(object sender, EventArgs e)
        {
            insertGrade.Sensitive = !isTeacher.Active;
            insertClass.Sensitive = !isTeacher.Active;
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
        string toLog = "";
        async void checkInsertIDChangeText(object sender, EventArgs e)
        {
            if (checkInsertID.Text.Length != checkIDLength.Value) return;
            await System.Threading.Tasks.Task.Delay(10);
            if (checkInsertID.Text.Length != checkIDLength.Value) return;
            Thread thread = new Thread(new ThreadStart(check));
            thread.Start();
        }
        void check()
        {
            User user = new User();
            JObject result = result = user.check(checkInsertID.Text);
            if ((bool)result["success"])
            {
                toLog = $"{result["data"]["grade"]}학년 {result["data"]["class"]}반 {result["data"]["number"]}번 {result["data"]["name"]}(ID: {checkInsertID.Text}) 체크됨";
            }
            else
            {
                toLog = $"체크 실패 (인식된 ID: {checkInsertID.Text})";
            }
            checkInsertID.Text = "";
            Application.Invoke (delegate {
                addLog(toLog);
            });
        }
    }
}

using System;
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
    }
}

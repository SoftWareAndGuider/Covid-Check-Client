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
        
        //학생이 아님이 체크됐을 때 실행하는 것
        void unlessStudent(title t)
        {
            if (t == title.check)
            {
                checkInsertGrade.Sensitive = !checkIsTeacher.Active;
                checkInsertClass.Sensitive = !checkIsTeacher.Active;
                checkInsertUser.Sensitive = isFull(t);
            }
            else if (t == title.uncheck)
            {
                uncheckInsertGrade.Sensitive = !uncheckIsTeacher.Active;
                uncheckInsertClass.Sensitive = !uncheckIsTeacher.Active;
                uncheckInsertUser.Sensitive = isFull(t);
            }
            else if (t == title.add)
            {
                addInsertGrade.Sensitive = !addIsTeacher.Active;
                addInsertClass.Sensitive = !addIsTeacher.Active;
                insertUser.Sensitive = isFull(t);
            }
            else if (t == title.delete)
            {
                delInsertGrade.Sensitive = !delIsTeacher.Active;
                delInsertClass.Sensitive = !delIsTeacher.Active;
                delInsertUserWithoutID.Sensitive = isFull(t);
            }
        }
        
        //체크인지 체크 해제인지 사용자 만들기인지 확인하는 것
        enum title
        {
            check,
            uncheck,
            add,
            delete
        }

        //ID의 길이를 입력하는 Scale이 조정되었을 때 실행되는 이벤트
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
        
        //ID가 입력되는 Entry(사용자 추가 제외)의 텍스트가 바뀌었을 때 실행되는 이벤트
        async void checkInsertIDChangeText(object sender, EventArgs e)
        {
            if (checkInsertID.Text.Length == 0) checkOK.Sensitive = false;
            else checkOK.Sensitive = true;
            if (checkInsertID.Text.Length != checkIDLength.Value) return;
            await Task.Delay(10);
            if (checkInsertID.Text.Length != checkIDLength.Value) return;
            Thread thread = new Thread(new ThreadStart(() => {check(checkInsertID.Text);}));
            thread.Start();
            await Task.Delay(10);
            checkInsertID.Text = "";
            checkOK.Sensitive = false;
        }
        async void uncheckInsertIDChangeText(object sender, EventArgs e)
        {
            if (uncheckInsertID.Text.Length == 0) uncheckOK.Sensitive = false;
            else uncheckOK.Sensitive = true;
            if (uncheckInsertID.Text.Length != uncheckIDLength.Value) return;
            await Task.Delay(10);
            if (uncheckInsertID.Text.Length != uncheckIDLength.Value) return;
            Thread thread = new Thread(new ThreadStart(() => {uncheck(uncheckInsertID.Text);}));
            thread.Start();
            await Task.Delay(10);
            uncheckInsertID.Text = "";
            uncheckOK.Sensitive = false;
        }
        
        //ID 입력하는 버튼 빼고 버튼을 눌렀을 때 실행되는 이벤트
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
        void delInsertUserClicked(object sender, EventArgs e)
        {
            string id = delInsertID.Text;
            Thread thread = new Thread(new ThreadStart(() => {delUser(id);}));
            thread.Start();
            delInsertID.Text = "";
        }
        void delInsertUserWithoutIDClicked(object sender, EventArgs e)
        {
            string[] info = new string[] {
                delInsertGrade.Text,
                delInsertClass.Text,
                delInsertNumber.Text
            };
            Thread thread = new Thread(new ThreadStart(() => {delUser(info[0], info[1], info[2]);}));
            thread.Start();
            delInsertGrade.Text = "";
            delInsertClass.Text = "";
            delInsertNumber.Text = "";
        }
        
        //ID 없이 입력하는 버튼이 눌렸을 때 실행되는 이벤트
        void checkInsertUserClicked(object sender, EventArgs e)
        {
            Thread thread = new Thread(new ThreadStart(() => {check(checkInsertGrade.Text, checkInsertClass.Text, checkInsertNumber.Text);}));
            thread.Start();
            checkInsertGrade.Sensitive = false;
            checkInsertClass.Sensitive = false;
            checkInsertNumber.Sensitive = false;
            checkInsertUser.Sensitive = false;
        }
        void uncheckInsertUserClicked(object sender, EventArgs e)
        {
            Thread thread = new Thread(new ThreadStart(() => {uncheck(uncheckInsertGrade.Text, uncheckInsertClass.Text, uncheckInsertNumber.Text);}));
            thread.Start();
            uncheckInsertGrade.Sensitive = false;
            uncheckInsertClass.Sensitive = false;
            uncheckInsertNumber.Sensitive = false;
            uncheckInsertUser.Sensitive = false;
        }
        
        
        
        //실제로 작업을 하는 곳 (별도의 스레드 사용)
        void check(string id)
        {
            User user = new User();
            JObject result = new JObject();
            try
            {
                result = user.check(id);;  
            } 
            catch
            {
                Application.Invoke(delegate {
                    MessageDialog dialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Error, ButtonsType.Close, false, "./config.txt에 올바른 홈페이지 주소를 입력해 주세요");
                    dialog.Run();
                    dialog.Dispose();
                    Environment.Exit(0);
                });
                return;  
            }
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
            try
            {
                result = user.check(grade, @class, number);
            }
            catch
            {
                Application.Invoke(delegate {
                    MessageDialog dialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Error, ButtonsType.Close, false, "./config.txt에 올바른 홈페이지 주소를 입력해 주세요");
                    dialog.Run();
                    dialog.Dispose();
                    Environment.Exit(0);
                });
                return;
            }
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
            JObject result = new JObject();
            try
            {
                result = user.uncheck(id);
            }
            catch
            {
                Application.Invoke(delegate {
                    MessageDialog dialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Error, ButtonsType.Close, false, "./config.txt에 올바른 홈페이지 주소를 입력해 주세요");
                    dialog.Run();
                    dialog.Dispose();
                    Environment.Exit(0);
                });
                return;
            }
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
        void uncheck(string grade, string @class, string number)
        {
            User user = new User();
            JObject result = new JObject();
            if (uncheckIsTeacher.Active)
            {
                grade = "0";
                @class = "0";
            }
            try
            {
                result = user.uncheck(grade, @class, number);
            }
            catch
            {
                Application.Invoke(delegate {
                    MessageDialog dialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Error, ButtonsType.Close, false, "./config.txt에 올바른 홈페이지 주소를 입력해 주세요");
                    dialog.Run();
                    dialog.Dispose();
                    Environment.Exit(0);
                });
                return;
            }
            string toLog = "";
            if ((bool)result["success"])
            {
                toLog = $"{result["data"]["grade"]}학년 {result["data"]["class"]}반 {result["data"]["number"]}번 {result["data"]["name"]}(ID: {result["data"]["id"]}) 체크 해제됨";
            }
            else
            {
                toLog = $"체크 해제 실패 (인식된 정보: {grade}학년 {@class}반 {number}번)";
            }
            Application.Invoke (delegate {
                addLog(toLog);
                uncheckInsertGrade.Sensitive = !uncheckIsTeacher.Active;
                uncheckInsertClass.Sensitive = !uncheckIsTeacher.Active;
                uncheckInsertNumber.Sensitive = true;
                uncheckInsertGrade.Text = "";
                uncheckInsertClass.Text = "";
                uncheckInsertNumber.Text = "";
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
            try
            {
                result = user.addUser(id, int.Parse(grade), int.Parse(@class), int.Parse(number), name);
            }
            catch
            {
                Application.Invoke(delegate {
                    MessageDialog dialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Error, ButtonsType.Close, false, "./config.txt에 올바른 홈페이지 주소를 입력해 주세요");
                    dialog.Run();
                    dialog.Dispose();
                    Environment.Exit(0);
                });
                return;
            }
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
        void delUser(string id)
        {
            User user = new User();
            JObject result = new JObject();
            try
            {
                result = user.delUser(id);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Application.Invoke(delegate {
                    MessageDialog dialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Error, ButtonsType.Close, false, "./config.txt에 올바른 홈페이지 주소를 입력해 주세요");
                    dialog.Run();
                    dialog.Dispose();
                    Environment.Exit(0);
                });
                return;
            }
            string toLog = "";
            if ((bool)result["success"])
            {
                try
                {
                    toLog = $"{result["data"]["grade"]}학년 {result["data"]["class"]}반 {result["data"]["number"]}번 {result["data"]["name"]}(ID: {id}) 삭제됨";
                }
                catch
                {
                    toLog = $"삭제 실패 (인식된 ID: {id})";
                }
            }
            else
            {
                toLog = $"삭제 실패 (인식된 ID: {id})";
            }
            Application.Invoke (delegate {
                addLog(toLog);
            });
        }
        void delUser(string grade, string @class, string number)
        {
            User user = new User();
            JObject result = new JObject();
            if (delIsTeacher.Active)
            {
                grade = "0";
                @class = "0";
            }
            try
            {
                result = user.delUser(grade, @class, number);
            }
            catch
            {
                Application.Invoke(delegate {
                    MessageDialog dialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Error, ButtonsType.Close, false, "./config.txt에 올바른 홈페이지 주소를 입력해 주세요");
                    dialog.Run();
                    dialog.Dispose();
                    Environment.Exit(0);
                });
                return;
            }
            string toLog = "";
            if ((bool)result["success"])
            {
                try
                {
                    toLog = $"{result["data"]["grade"]}학년 {result["data"]["class"]}반 {result["data"]["number"]}번 {result["data"]["name"]}(ID: {result["data"]["id"]}) 삭제됨";
                }
                catch
                {
                    toLog = $"삭제 실패 (인식된 정보: {grade}학년 {@class}반 {number}번)";
                }
            }
            else
            {
                toLog = $"삭제 실패 (인식된 정보: {grade}학년 {@class}반 {number}번)";
            }
            Application.Invoke (delegate {
                addLog(toLog);
            });
        }

        //사용자의 정보들이 입력외었는지 확인하는 것
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
            else if (t == title.add)
            {
                if (string.IsNullOrEmpty(addInsertID.Text) || string.IsNullOrEmpty(addInsertName.Text) || string.IsNullOrEmpty(addInsertNumber.Text)) return false;

                if (addIsTeacher.Active) return true;
                else
                {
                    if (string.IsNullOrEmpty(addInsertGrade.Text) || string.IsNullOrEmpty(addInsertClass.Text)) return false;
                    return true;
                }
            }
            else
            {
                if (string.IsNullOrEmpty(delInsertNumber.Text)) return false;

                if (delIsTeacher.Active) return true;
                else
                {
                    if (string.IsNullOrEmpty(delInsertGrade.Text) || string.IsNullOrEmpty(delInsertClass.Text)) return false;
                    return true;
                }
            }
        }

        //사용자 정보 입력할 때 실행되는 이벤트
        void checkWithoutIDKeyRelease(object sender, EventArgs e) => checkInsertUser.Sensitive = isFull(title.check);
        void addUserKeyRelease(object sender, EventArgs e) => insertUser.Sensitive = isFull(title.add);
        void uncheckWithoutIDKeyRelease(object sender, EventArgs e) => uncheckInsertUser.Sensitive = isFull(title.uncheck);
        void delUserWithoutIDKeyRelease(object sender, EventArgs e) => delInsertUserWithoutID.Sensitive = isFull(title.delete);
    }
}

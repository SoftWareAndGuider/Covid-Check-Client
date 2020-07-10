using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using CheckCovid19;
using Newtonsoft.Json.Linq;
using Gtk;

namespace CovidCheckClientGui
{
    partial class Program : Window
    {
        bool saveData;
        static User user;
        static bool doneUpdate = false;
        static string[,] csv = new string[3,2];


        static void Main(string[] args)
        {
            for (int i = 0; i < 6; i++)
            {
                csv[i / 2, i % 2] = "학년,반,번호,이름,ID";
            }
            if (args.Length == 0)
            {
                Application.Init();
                new Program();
                Application.Run();
            }
            else if (args[0] == "update")
            {
                UpdateWindow window = new UpdateWindow(args);
                doneUpdate = true;
                new Program();
                Application.Init();
                Application.Run();
            }
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
            else if (t == title.checkDoubt)
            {
                checkDoubtInsertGrade.Sensitive = !checkDoubtIsTeacher.Active;
                checkDoubtInsertClass.Sensitive = !checkDoubtIsTeacher.Active;
                checkDoubtInsertUser.Sensitive = isFull(t);
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
            checkDoubt,
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
            string id = checkInsertID.Text;
            Thread thread = new Thread(new ThreadStart(() => {check(id);}));
            thread.Start();
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
        async void checkDoubtInsertIDChangeText(object sender, EventArgs e)
        {
            if (checkDoubtInsertID.Text.Length == 0) checkDoubtOK.Sensitive = false;
            else checkDoubtOK.Sensitive = true;
            if (checkDoubtInsertID.Text.Length != checkIDLength.Value) return;
            await Task.Delay(10);
            if (checkDoubtInsertID.Text.Length != checkIDLength.Value) return;
            string id = checkDoubtInsertID.Text;
            Thread thread = new Thread(new ThreadStart(() => {checkDoubt(id);}));
            thread.Start();
            checkDoubtInsertID.Text = "";
            checkDoubtOK.Sensitive = false;
        }

        //ID 입력하는 버튼 빼고 버튼을 눌렀을 때 실행되는 이벤트
        void checkOKClicked(object sender, EventArgs e)
        {
            string id = checkInsertID.Text;
            checkOK.Sensitive = false;
            checkInsertID.Text = "";
            Thread thread = new Thread(new ThreadStart(() => {check(id);}));
            thread.Start();
            checkInsertID.Text = "";
        }
        void checkDoubtOKClicked(object sender, EventArgs e)
        {
            string id = checkDoubtInsertID.Text;
            checkDoubtOK.Sensitive = false;
            checkDoubtInsertID.Text = "";
            Thread thread = new Thread(new ThreadStart(() => {checkDoubt(id);}));
            thread.Start();
            checkDoubtInsertID.Text = "";
        }
        void uncheckOKClicked(object sender, EventArgs e)
        {
            string id = uncheckInsertID.Text;
            uncheckOK.Sensitive = false;
            Thread thread = new Thread(new ThreadStart(() => {uncheck(id);}));
            thread.Start();
            uncheckInsertID.Text = "";
        }
        void insertUserClicked(object sender, EventArgs e)
        {
            string[] info = new string[5] {
                addInsertID.Text, addInsertNumber.Text, addInsertName.Text, addInsertGrade.Text, addInsertClass.Text
            };
            bool student = addIsTeacher.Active;
            insertUser.Sensitive = false;
            addInsertNumber.Text = "";
            addInsertClass.Text = "";
            addInsertName.Text = "";
            addInsertGrade.Text = "";
            addInsertID.Text = "";
            Thread thread = new Thread(new ThreadStart(() => {addUser(student, info[0], info[1], info[2], info[3], info[4]);}));
            thread.Start();
        }
        void delInsertUserClicked(object sender, EventArgs e)
        {
            string id = delInsertID.Text;
            delInsertUser.Sensitive = false;
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
            delInsertUserWithoutID.Sensitive = false;
            Thread thread = new Thread(new ThreadStart(() => {delUser(info[0], info[1], info[2]);}));
            thread.Start();
            delInsertGrade.Text = "";
            delInsertClass.Text = "";
            delInsertNumber.Text = "";
        }
        
        //ID 없이 입력하는 버튼이 눌렸을 때 실행되는 이벤트
        void checkInsertUserClicked(object sender, EventArgs e)
        {
            string[] info = new string[3] {
                checkInsertGrade.Text, checkInsertClass.Text, checkInsertNumber.Text
            };
            checkInsertUser.Sensitive = false;
            checkInsertGrade.Text = "";
            checkInsertClass.Text = "";
            checkInsertNumber.Text = "";
            Thread thread = new Thread(new ThreadStart(() => {check(info[0], info[1], info[2]);}));
            thread.Start();
        }
        void checkDoubtInsertUserClicked(object sender, EventArgs e)
        {
            string[] info = new string[] {
                checkDoubtInsertGrade.Text,
                checkDoubtInsertClass.Text,
                checkDoubtInsertNumber.Text
            };
            Thread thread = new Thread(new ThreadStart(() => {checkDoubt(info[0], info[1], info[2]);}));
            thread.Start();

            checkDoubtInsertUser.Sensitive = false;
            checkDoubtInsertGrade.Text = "";
            checkDoubtInsertClass.Text = "";
            checkDoubtInsertNumber.Text = "";
        }
        void uncheckInsertUserClicked(object sender, EventArgs e)
        {
            string[] info = new string[3] {
                uncheckInsertGrade.Text, uncheckInsertClass.Text, uncheckInsertNumber.Text
            };
            Thread thread = new Thread(new ThreadStart(() => {uncheck(info[0], info[1], info[2]);}));
            thread.Start();
            uncheckInsertGrade.Text = "";
            uncheckInsertClass.Text = "";
            uncheckInsertNumber.Text = "";
            uncheckInsertUser.Sensitive = false;
        }
        
        
        
        //실제로 작업을 하는 곳 (별도의 스레드 사용)
        void check(string id, int loop = 0)
        {
            JObject result = new JObject();
            int error = 0;
            result = user.check(id, out error);
            
            if (error == 2)
            {
                urlErrorNotice();
                return;
            }
            else if (error == 1)
            {
                if (loop > (int)helpSet.Value)
                {
                    internetErrorNotice();
                    return;
                }
                Thread.Sleep(1000);
                Application.Invoke(delegate {
                    addTimeoutLog($"타임아웃 재시도.... (인식된 ID: {id}) ({loop + 1}번째 재시도)");
                });
                check(id, loop + 1);
                return;
            }


            string toLog = "";
            if ((bool)result["success"])
            {
                toLog = $"{result["data"]["grade"]}학년 {result["data"]["class"]}반 {result["data"]["number"]}번 {result["data"]["name"]}(ID: {result["data"]["id"]}) 정상 체크됨";
                if (saveData && (bool)settingJson["saves"][0][0])
                {
                    csv[0, 0] += $"\n{result["data"]["grade"]},{result["data"]["class"]},{result["data"]["number"]},{result["data"]["name"]},{result["data"]["id"]}";
                }
            }
            else
            {
                toLog = $"정상 체크 실패 (인식된 ID: {id})";
            }
            
            Application.Invoke (delegate {
                addLog(toLog);
            });
        }
        void check(string grade, string @class, string number, int loop = 0)
        {
            JObject result = new JObject();
            if (checkIsTeacher.Active)
            {
                grade = "0";
                @class = "0";
            }
            int err = 0;

            result = user.check(grade, @class, number, out err);


            if (err == 2)
            {
                urlErrorNotice();
                return;
            }
            else if (err == 1)
            {
                
                if (loop > (int)helpSet.Value)
                {
                    internetErrorNotice();
                    return;
                }
                Thread.Sleep(1000);
                Application.Invoke(delegate {
                    addTimeoutLog($"타임아웃 재시도.... (인식된 정보: {grade}학년 {@class}반 {number}번) ({loop + 1}번째 재시도)");
                });
                check(grade, @class, number, loop + 1);
                return;
            }

            string toLog = "";
            if ((bool)result["success"])
            {
                toLog = $"{result["data"]["grade"]}학년 {result["data"]["class"]}반 {result["data"]["number"]}번 {result["data"]["name"]}(ID: {result["data"]["id"]}) 정상 체크됨";
            }
            else
            {
                toLog = $"체크 실패 (인식된 정보: {grade}학년 {@class}반 {number}번)";
            }
            if (saveData && (bool)settingJson["saves"][0][1])
            {
                csv[0, 1] += $"\n{result["data"]["grade"]},{result["data"]["class"]},{result["data"]["number"]},{result["data"]["name"]},{result["data"]["id"]}";
            }
            Application.Invoke (delegate {
                addLog(toLog);
            });
        }
        void checkDoubt(string id, int loop = 0)
        {
            JObject result = new JObject();
            int error = 0;
            result = user.check(id, out error, true);
            if (error == 2)
            {
                urlErrorNotice();
                return;  
            }
            else if (error == 1)
            {
                if (loop > (int)helpSet.Value)
                {
                    internetErrorNotice();
                    return;
                }
                Thread.Sleep(1000);
                Application.Invoke(delegate {
                    addTimeoutLog($"타임아웃 재시도.... (인식된 ID: {id}) ({loop + 1}번째 재시도)");
                });
                checkDoubt(id, loop + 1);
                return;
            }

            string toLog = "";
            if ((bool)result["success"])
            {
                toLog = $"{result["data"]["grade"]}학년 {result["data"]["class"]}반 {result["data"]["number"]}번 {result["data"]["name"]}(ID: {result["data"]["id"]}) 발열 체크됨";
                if (saveData && (bool)settingJson["saves"][1][0])
                {
                    csv[1, 0] += $"\n{result["data"]["grade"]},{result["data"]["class"]},{result["data"]["number"]},{result["data"]["name"]},{result["data"]["id"]}";
                }
            }
            else
            {
                toLog = $"발열 체크 실패 (인식된 ID: {id})";
            }
            Application.Invoke (delegate {
                addLog(toLog);
            });
        }       
        void checkDoubt(string grade, string @class, string number, int loop = 0)
        {
            JObject result = new JObject();
            if (checkDoubtIsTeacher.Active)
            {
                grade = "0";
                @class = "0";
            }

            int err = 0;
            result = user.check(grade, @class, number, out err, true);

            if (err == 2)
            {
                urlErrorNotice();
                return;
            }
            else if (err == 1)
            {
                if (loop > (int)helpSet.Value)
                {
                    internetErrorNotice();
                    return;
                }
                Thread.Sleep(1000);
                Application.Invoke(delegate {
                    addTimeoutLog($"타임아웃 재시도.... (인식된 정보: {grade}학년 {@class}반 {number}번) ({loop}번째 재시도)");
                });
                checkDoubt(grade, @class, number, loop + 1);
                return;
            }
            string toLog = "";
            if ((bool)result["success"])
            {
                toLog = $"{result["data"]["grade"]}학년 {result["data"]["class"]}반 {result["data"]["number"]}번 {result["data"]["name"]}(ID: {result["data"]["id"]}) 발열 체크됨";
                if (saveData && (bool)settingJson["saves"][1][1])
                {
                    csv[1, 1] += $"\n{result["data"]["grade"]},{result["data"]["class"]},{result["data"]["number"]},{result["data"]["name"]},{result["data"]["id"]}";
                }
            }
            else
            {
                toLog = $"발열 체크 실패 (인식된 정보: {grade}학년 {@class}반 {number}번)";
            }
            Application.Invoke (delegate {
                addLog(toLog);
            });
        }
        void uncheck(string id, int loop = 0)
        {            
            JObject result = new JObject();

            int err = 0;

            result = user.uncheck(id, out err);

            if (err == 2)
            {
                urlErrorNotice();
                return;
            }
            else if (err == 1)
            {
                if (loop > (int)helpSet.Value)
                {
                    internetErrorNotice();
                    return;
                }
                Thread.Sleep(1000);
                Application.Invoke(delegate {
                    addTimeoutLog($"타임아웃 재시도.... (인식된 ID: {id}) ({loop + 1}번째 재시도)");
                });
                uncheck(id, loop + 1);
                return;
            }

            string toLog = "";
            if ((bool)result["success"])
            {
                toLog = $"{result["data"]["grade"]}학년 {result["data"]["class"]}반 {result["data"]["number"]}번 {result["data"]["name"]}(ID: {id}) 체크 해제됨";
                if (saveData && (bool)settingJson["saves"][2][0])
                {
                    csv[2, 0] += $"\n{result["data"]["grade"]},{result["data"]["class"]},{result["data"]["number"]},{result["data"]["name"]},{result["data"]["id"]}";
                }
            }
            else
            {
                toLog = $"체크 해제 실패 (인식된 ID: {id})";
            }
            Application.Invoke (delegate {
                addLog(toLog);
            });
        }
        void uncheck(string grade, string @class, string number, int loop = 0)
        {
            JObject result = new JObject();
            if (uncheckIsTeacher.Active)
            {
                grade = "0";
                @class = "0";
            }

            int err = 0;
            result = user.uncheck(grade, @class, number, out err);

            if (err == 2)
            {
                urlErrorNotice();
                return;
            }
            else if (err == 1)
            {
                if (loop > (int)helpSet.Value)
                {
                    internetErrorNotice();
                    return;
                }
                Thread.Sleep(1000);
                Application.Invoke(delegate {
                    addTimeoutLog($"타임아웃 재시도.... (인식된 정보: {grade}학년 {@class}반 {number}번) ({loop + 1}번째 재시도)");
                });
                uncheck(grade, @class, number, loop + 1);
                return;
            }

            string toLog = "";
            if ((bool)result["success"])
            {
                toLog = $"{result["data"]["grade"]}학년 {result["data"]["class"]}반 {result["data"]["number"]}번 {result["data"]["name"]}(ID: {result["data"]["id"]}) 체크 해제됨";
                if (saveData && (bool)settingJson["saves"][2][1])
                {
                    csv[2, 1] += $"\n{result["data"]["grade"]},{result["data"]["class"]},{result["data"]["number"]},{result["data"]["name"]},{result["data"]["id"]}";
                }
            }
            else
            {
                toLog = $"체크 해제 실패 (인식된 정보: {grade}학년 {@class}반 {number}번)";
            }
            Application.Invoke (delegate {
                addLog(toLog);
            });
        }
        void addUser(bool isNotStudent, string id, string number, string name, string grade, string @class, int loop = 0)
        {

            JObject result = new JObject();
            
            if (isNotStudent)
            {
                grade = "0";
                @class = "0";
            } 

            int err = 0;
            result = user.addUser(id, int.Parse(grade), int.Parse(@class), int.Parse(number), name, out err);

            if (err == 2)
            {
                urlErrorNotice();
                return;
            }
            else if (err == 1)
            {
                if (loop > (int)helpSet.Value)
                {
                    internetErrorNotice();
                    return;
                }
                Thread.Sleep(1000);
                Application.Invoke(delegate {
                    addTimeoutLog($"타임아웃 재시도.... (인식된 정보: {id}학년 {@class}반 {number}번 {name}({id})) ({loop + 1}번째 재시도)");
                });
                addUser(isNotStudent, id, number, name, grade, @class, loop + 1);
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
                addLog(toLog);
            });
        }
        void delUser(string id, int loop = 0)
        {
            JObject result = new JObject();

            int err = 0;
            result = user.delUser(id, out err);

            if (err == 2)
            {
                urlErrorNotice();
                return;
            }
            else if (err == 1)
            {
                if (loop > (int)helpSet.Value)
                {
                    internetErrorNotice();
                    return;
                }

                Thread.Sleep(1000);
                Application.Invoke(delegate {
                    addTimeoutLog($"타임아웃 재시도.... (인식된 ID: {id}) ({loop + 1}번째 재시도)");
                });
                delUser(id, loop + 1);
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
        void delUser(string grade, string @class, string number, int loop = 0)
        {
            JObject result = new JObject();
            if (delIsTeacher.Active)
            {
                grade = "0";
                @class = "0";
            }

            int err = 0;
            result = user.delUser(grade, @class, number, out err);

            if (err == 2)
            {
                urlErrorNotice();
                return;
            }
            else if (err == 1)
            {
                if (loop > (int)helpSet.Value)
                {
                    internetErrorNotice();
                    return;
                }
                Thread.Sleep(1000);
                Application.Invoke(delegate {
                    addTimeoutLog($"타임아웃 재시도.... (인식된 정보: {grade}학년 {@class}반 {number}번) ({loop + 1}번째 재시도)");
                });
                delUser(grade, @class, number, loop + 1);
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
            else if (t == title.checkDoubt)
            {
                if (string.IsNullOrEmpty(checkDoubtInsertNumber.Text))
                {
                    return false;
                }

                if (checkDoubtIsTeacher.Active)
                {
                    return true;
                }
                else
                {
                    if (string.IsNullOrEmpty(checkDoubtInsertGrade.Text) || string.IsNullOrEmpty(checkDoubtInsertClass.Text)) return false;
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
        void checkDoubtWithoutIDKeyRelease(object sender, EventArgs e) => checkDoubtInsertUser.Sensitive = isFull(title.checkDoubt);
        void addUserKeyRelease(object sender, EventArgs e) => insertUser.Sensitive = isFull(title.add);
        void uncheckWithoutIDKeyRelease(object sender, EventArgs e) => uncheckInsertUser.Sensitive = isFull(title.uncheck);
        void delUserWithoutIDKeyRelease(object sender, EventArgs e) => delInsertUserWithoutID.Sensitive = isFull(title.delete);
        void urlErrorNotice() //url이 잘못되었다는 에러를 보여주는 함수
        {
            Application.Invoke(delegate {
                    MessageDialog dialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Error, ButtonsType.Close, false, "설정에 올바른 홈페이지 주소를 입력해 주세요");
                    dialog.Run();
                    dialog.Dispose();
                    selectMode.Page = 3;
                });
        }    
        void internetErrorNotice() //인터넷이 잘못되었을 때 에러를 보여주는거 (타임아웃)
        {
            Application.Invoke(delegate {
                    MessageDialog dialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Error, ButtonsType.Close, false, "인터넷이 원활한 환경에서 사용해 주세요.");
                    dialog.Run();
                    dialog.Dispose();
                });
        }
    }
}

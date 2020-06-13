using System;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Net;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System.IO.Compression;

using Gtk;
using Newtonsoft.Json.Linq;


namespace CovidCheckClientGui
{
    partial class Program : Window
    {
        JObject settingJson = new JObject();

        Grid grid = new Grid();


            //왼쪽의 탭들 (체크, 체크 해제, 추가)
        Notebook selectMode = new Notebook();
        
        ScrolledWindow scroll = new ScrolledWindow();
        ScrolledWindow timeoutLogScroll = new ScrolledWindow();


        // 오른쪽에 뜨는 로그
        ListBox log = new ListBox();
        ListBox timeoutLog = new ListBox();
        bool hasTimeout = false;

        // 사용자 체크
        Entry checkInsertID = new Entry();
        Scale checkIDLength = new Scale(Orientation.Horizontal, new Adjustment(8, 5, 10, 0, 1, 0));

        Entry checkInsertGrade = new Entry();
        Entry checkInsertClass = new Entry();
        Entry checkInsertNumber = new Entry();
        CheckButton checkIsTeacher = new CheckButton("학생이 아님");

        Button checkOK = new Button("정상 체크하기");
        Button checkInsertUser = new Button("정상 체크하기");


        Entry checkDoubtInsertID = new Entry();
        Entry checkDoubtInsertGrade = new Entry();
        Entry checkDoubtInsertClass = new Entry();
        Entry checkDoubtInsertNumber = new Entry();
        CheckButton checkDoubtIsTeacher = new CheckButton("학생이 아님");

        Button checkDoubtOK = new Button("발열 체크하기");
        Button checkDoubtInsertUser = new Button("발열 체크하기");

        // 사용자 체크 해제        
        Entry uncheckInsertID = new Entry();
        Scale uncheckIDLength = new Scale(Orientation.Horizontal, new Adjustment(8, 5, 10, 0, 1, 0));

        Entry uncheckInsertGrade = new Entry();
        Entry uncheckInsertClass = new Entry();
        Entry uncheckInsertNumber = new Entry();
        CheckButton uncheckIsTeacher = new CheckButton("학생이 아님");

        Button uncheckOK = new Button("체크 해제하기");
        Button uncheckInsertUser = new Button("체크 해제하기");
        

        //사용자 추가
        Entry addInsertID = new Entry();
        Entry addInsertGrade = new Entry();
        Entry addInsertClass = new Entry();
        Entry addInsertNumber = new Entry();
        Entry addInsertName = new Entry();
        CheckButton addIsTeacher = new CheckButton("학생이 아님");
        Button insertUser = new Button("사용자 만들기");


        //사용자 삭제
        Entry delInsertID = new Entry();
        Entry delInsertGrade = new Entry();
        Entry delInsertClass = new Entry();
        Entry delInsertNumber = new Entry();
        CheckButton delIsTeacher = new CheckButton("학생이 아님");
        Button delInsertUser = new Button("사용자 삭제");
        Button delInsertUserWithoutID = new Button("사용자 삭제");


        //체크 상황 보기
        bool programProcessing = true;
        Label[] userCount = new Label[4] { //[0, ]: 1학년, [1, ]: 2학년, [2, ]: 3학년, [3, ]: 기타
                new Label(""),
                new Label(""),
                new Label(""),
                new Label("")
        };
        Label allUserCount = new Label("");

        Label time = new Label("");

        CheckButton seeMoreInfo = new CheckButton("상세정보 보기");
        Frame[] statusListFrame = new Frame[2] {new Frame("사용자 수 (3초마다 새로고침)"), new Frame("검사 현황 (3초마다 새로고침)")};

        ProgressBar[,] statusProgressBar = new ProgressBar[4, 3] { //[0, ]: 1학년, [1, ]: 2학년, [2, ]: 3학년, [3, ]: 기타, [, 0]: 미검사, [, 1]: 검사, [, 2]: 발열
            {
                new ProgressBar(),
                new ProgressBar(),
                new ProgressBar()
            },
            {
                new ProgressBar(),
                new ProgressBar(),
                new ProgressBar()
            },
            {
                new ProgressBar(),
                new ProgressBar(),
                new ProgressBar()
            },
            {
                new ProgressBar(),
                new ProgressBar(),
                new ProgressBar()
            }
        };        

        public Program() : base("코로나19 예방용 발열체크 프로그램")
        {
            const string settingPath = "config.json";
            CssProvider cssProvider = new CssProvider(); //기본 CSS설정
            cssProvider.LoadFromData(@"
                #add {
                    font-size: 18px;
                }
                #gray > trough > progress {
                    background-image: none;
                    background-color: gray;
                }
                #green > trough > progress {
                    background-image: none;
                    background-color: #5DE3BD;
                }
                #red > trough > progress {
                    background-image: none;
                    background-color: red;
                }
                .nowlog {
                    background-color: lightpink;
                }
            ");
            StyleContext.AddProviderForScreen(Gdk.Screen.Default, cssProvider, 800);

            JObject settingJson = new JObject();

            if (doneUpdate)
            {
                MessageDialog dialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Error, ButtonsType.Close, false, "새로운 버전으로 업데이트를 완료했습니다!");
                    dialog.Run();
                    dialog.Dispose();
            }
            try
            {
                settingJson = JObject.Parse(File.ReadAllText(settingPath));
            }
            catch
            {
                string defaultSetting = @"{
                    ""url"": ""localhost"",
                    ""barcodeLength"": 8,
                    ""timeoutRetry"": 100,
                    ""checkUpdate"": true,
                    ""autoUpdate"": false,
                    ""usePassword"": false,
                    ""password"": ""password""
                }";
                File.WriteAllText(settingPath, defaultSetting);
                settingJson = JObject.Parse(defaultSetting);
            }

            user = new CheckCovid19.User(settingJson["url"].ToString());

            long ping = new CheckCovid19.User(settingJson["url"].ToString()).getPing(settingJson["url"].ToString());
            base.Title = ($"코로나19 예방용 발열체크 프로그램 (통신 속도: {ping}ms)");

            


            addLog("프로그램이 시작됨");            
            DeleteEvent += delegate {programProcessing = false; Application.Quit();};

            SetDefaultSize(1280, 850);
            
            // 전체를 감싸는 Grid
            grid.Margin = 20;
            grid.ColumnHomogeneous = true;
            grid.ColumnSpacing = 8;


            //사용자 체크 Grid
            Grid check = new Grid();
            Frame checkFrame = new Frame("정상");
            {
                //사용자 체크 속성 설정
                check.ColumnHomogeneous = true; //창의 크기가 달라지면 알아서 위젯 크기 조절해줌
                check.RowSpacing = 10; //Row는 위아래
                check.ColumnSpacing = 10; //Column은 양 옆
                check.Margin = 15;
                check.MarginTop = 5;
                checkInsertID.PlaceholderText = "사용자의 ID를 스캔 혹은 입력해 주세요";
                checkIDLength.Digits = 0;
                checkIDLength.ValuePos = PositionType.Right;
                checkInsertGrade.PlaceholderText = "사용자의 학년을 입력해 주세요";
                checkInsertClass.PlaceholderText = "사용자의 반을 입력해 주세요";
                checkInsertNumber.PlaceholderText = "사용자의 번호를 입력해 주세요";
                checkInsertUser.Sensitive = false;
                checkOK.Sensitive = false;

                //사용자 체크 이벤트 설정
                checkInsertID.KeyReleaseEvent += checkInsertIDChangeText;
                checkIDLength.ValueChanged += checkIDLengthChangeValue;
                checkOK.Clicked += checkOKClicked;
                checkIsTeacher.Clicked += delegate {unlessStudent(title.check);};
                checkInsertUser.Clicked += checkInsertUserClicked;
                checkInsertGrade.KeyReleaseEvent += checkWithoutIDKeyRelease;
                checkInsertClass.KeyReleaseEvent += checkWithoutIDKeyRelease;
                checkInsertNumber.KeyReleaseEvent += checkWithoutIDKeyRelease;

                //사용자 체크 배치(ID)
                check.Attach(new Label("실제 바코드의 길이가 지정한 바코드의 길이와 다를 경우 체크하기 버튼을 눌러 체크해주세요."), 1, 1, 5, 1); // 공지 추가
                check.Attach(checkInsertID, 1, 2, 4, 1); // 텍스트박스 추가
                check.Attach(checkOK, 5, 2, 1, 1); //OK 버튼 추가

                check.Attach(new Separator(Orientation.Horizontal), 1, 4, 5, 1);
                    
                    //사용자 체크 배치(학년, 반, 번호)
                    {
                        check.Attach(new Label("ID 없이 체크하기"), 1, 5, 5, 1);
                        check.Attach(checkIsTeacher, 1, 6, 5, 1);
                        check.Attach(new Label("학년"), 1, 7, 1, 1);
                        check.Attach(checkInsertGrade, 2, 7, 4, 1);
                        check.Attach(new Label("반"), 1, 8, 1, 1);
                        check.Attach(checkInsertClass, 2, 8, 4, 1);
                        check.Attach(new Label("번호"), 1, 9, 1, 1);
                        check.Attach(checkInsertNumber, 2, 9, 4, 1);
                        check.Attach(checkInsertUser, 1, 10, 5, 1);
                    }

                    checkFrame.Margin = 15;
                    checkFrame.MarginBottom = 0;
                    checkFrame.MarginTop = 0;
                    checkFrame.Add(check);
            }
            
            Grid checkDoubt = new Grid();
            Frame checkDoubtFrame = new Frame("발열");
            {
                checkDoubt.ColumnHomogeneous = true;
                checkDoubt.RowSpacing = 10; 
                checkDoubt.ColumnSpacing = 10;
                checkDoubt.Margin = 15;
                checkDoubt.MarginTop = 5;
                checkDoubtInsertID.PlaceholderText = "사용자의 ID를 스캔 혹은 입력해 주세요";
                checkDoubtInsertGrade.PlaceholderText = "사용자의 학년을 입력해 주세요";
                checkDoubtInsertClass.PlaceholderText = "사용자의 반을 입력해 주세요";
                checkDoubtInsertNumber.PlaceholderText = "사용자의 번호를 입력해 주세요";
                checkDoubtInsertUser.Sensitive = false;
                checkDoubtOK.Sensitive = false;

                checkDoubtInsertID.KeyReleaseEvent += checkDoubtInsertIDChangeText;
                checkDoubtOK.Clicked += checkDoubtOKClicked;
                checkDoubtIsTeacher.Clicked += delegate {unlessStudent(title.checkDoubt);};
                checkDoubtInsertUser.Clicked += checkDoubtInsertUserClicked;
                checkDoubtInsertGrade.KeyReleaseEvent += checkDoubtWithoutIDKeyRelease;
                checkDoubtInsertClass.KeyReleaseEvent += checkDoubtWithoutIDKeyRelease;
                checkDoubtInsertNumber.KeyReleaseEvent += checkDoubtWithoutIDKeyRelease;

                //사용자 체크 배치(ID)
                checkDoubt.Attach(new Label("실제 바코드의 길이가 지정한 바코드의 길이와 다를 경우 체크하기 버튼을 눌러 체크해주세요."), 1, 1, 5, 1); // 공지 추가
                checkDoubt.Attach(checkDoubtInsertID, 1, 2, 4, 1); // 텍스트박스 추가
                checkDoubt.Attach(checkDoubtOK, 5, 2, 1, 1); //OK 버튼 추가
                checkDoubt.Attach(new Label("바코드 길이 조절"), 1, 3, 1, 1);

                checkDoubt.Attach(new Separator(Orientation.Horizontal), 1, 4, 5, 1);
                
                //사용자 체크 배치(학년, 반, 번호)
                {
                    checkDoubt.Attach(new Label("ID 없이 체크하기"), 1, 5, 5, 1);
                    checkDoubt.Attach(checkDoubtIsTeacher, 1, 6, 5, 1);
                    checkDoubt.Attach(new Label("학년"), 1, 7, 1, 1);
                    checkDoubt.Attach(checkDoubtInsertGrade, 2, 7, 4, 1);
                    checkDoubt.Attach(new Label("반"), 1, 8, 1, 1);
                    checkDoubt.Attach(checkDoubtInsertClass, 2, 8, 4, 1);
                    checkDoubt.Attach(new Label("번호"), 1, 9, 1, 1);
                    checkDoubt.Attach(checkDoubtInsertNumber, 2, 9, 4, 1);
                    checkDoubt.Attach(checkDoubtInsertUser, 1, 10, 5, 1);
                }

                checkDoubtFrame.Margin = 15;
                checkDoubtFrame.MarginTop = 0;
                checkDoubtFrame.Add(checkDoubt);
            }

            Grid checkAll = new Grid();
            {
                checkAll.ColumnHomogeneous = true;
                checkAll.RowSpacing = 10;
                Label changeBarcodeLength = new Label("바코드 길이 조절");
                changeBarcodeLength.MarginTop = 10;
                checkIDLength.MarginTop = 10;
                checkAll.Attach(changeBarcodeLength, 1, 1, 1, 1);
                checkAll.Attach(checkIDLength, 2, 1, 4, 1);     
                checkAll.Attach(checkFrame, 1, 2, 5, 1);
                checkAll.Attach(checkDoubtFrame, 1, 3, 5, 1);            
            }


            //사용자 체크 해제 Grid
            Grid uncheck = new Grid();
            {
                //사용자 체크 해제 속성 설정
                uncheck.ColumnHomogeneous = true; //창의 크기가 달라지면 알아서 위젯 크기 조절해줌
                uncheck.RowSpacing = 10; //Row는 위아래
                uncheck.ColumnSpacing = 10; //Column은 양 옆
                uncheck.Margin = 15;
                uncheckInsertID.PlaceholderText = "사용자의 ID를 스캔 혹은 입력해 주세요";
                uncheckIDLength.Digits = 0;
                uncheckIDLength.ValuePos = PositionType.Right;
                uncheckInsertGrade.PlaceholderText = "사용자의 학년을 입력해 주세요";
                uncheckInsertClass.PlaceholderText = "사용자의 반을 입력해 주세요";
                uncheckInsertNumber.PlaceholderText = "사용자의 번호를 입력해 주세요";
                uncheckInsertUser.Sensitive = false;
                uncheckOK.Sensitive = false;

                //사용자 체크 해제 이벤트 설정
                uncheckInsertID.KeyReleaseEvent += uncheckInsertIDChangeText;
                uncheckIDLength.ValueChanged += uncheckIDLengthChangeValue;
                uncheckOK.Clicked += uncheckOKClicked;
                uncheckInsertGrade.KeyReleaseEvent += uncheckWithoutIDKeyRelease;
                uncheckInsertClass.KeyReleaseEvent += uncheckWithoutIDKeyRelease;
                uncheckInsertNumber.KeyReleaseEvent += uncheckWithoutIDKeyRelease;
                uncheckIsTeacher.Clicked += delegate {unlessStudent(title.uncheck);};
                uncheckInsertUser.Clicked += uncheckInsertUserClicked;


                //사용자 체크 해제 배치(ID)
                uncheck.Attach(new Label("실제 바코드의 길이가 지정한 바코드의 길이와 다를 경우 체크 해제하기 버튼을 눌러 체크해주세요."), 1, 1, 5, 1); // 공지 추가
                uncheck.Attach(uncheckInsertID, 1, 2, 4, 1); // 텍스트박스 추가
                uncheck.Attach(uncheckOK, 5, 2, 1, 1); //OK 버튼 추가
                uncheck.Attach(new Label("바코드 길이 조절"), 1, 3, 1, 1);
                uncheck.Attach(uncheckIDLength, 2, 3, 4, 1);
                
                uncheck.Attach(new Separator(Orientation.Horizontal), 1, 4, 5, 1);

                //사용자 체크 해제 배치(학년, 반, 번호)
                {
                    uncheck.Attach(new Label("ID 없이 체크 해제하기"), 1, 5, 5, 1);
                    uncheck.Attach(uncheckIsTeacher, 1, 6, 5, 1);
                    uncheck.Attach(new Label("학년"), 1, 7, 1, 1);
                    uncheck.Attach(uncheckInsertGrade, 2, 7, 4, 1);
                    uncheck.Attach(new Label("반"), 1, 8, 1, 1);
                    uncheck.Attach(uncheckInsertClass, 2, 8, 4, 1);
                    uncheck.Attach(new Label("번호"), 1, 9, 1, 1);
                    uncheck.Attach(uncheckInsertNumber, 2, 9, 4, 1);
                    uncheck.Attach(uncheckInsertUser, 1, 10, 5, 1);
                }
            }



            //사용자 추가 Grid
            Frame addUserFrame = new Frame("사용자 추가");
            Grid addUser = new Grid();
            {
                //사용자 추가 속성 설정
                addUser.ColumnHomogeneous = true;
                addUser.Margin = 15;
                addUser.MarginTop = 5;
                addUser.RowSpacing = 10;
                addInsertID.PlaceholderText = "사용자의 ID를 스캔 혹은 입력해 주세요";
                addInsertGrade.PlaceholderText = "사용자의 학년을 입력해 주세요";
                addInsertClass.PlaceholderText = "사용자의 반을 입력해 주세요";
                addInsertNumber.PlaceholderText = "사용자의 번호를 입력해 주세요";
                addInsertName.PlaceholderText = "사용자의 이름을 입력해 주세요";  
                insertUser.Sensitive = false;          
                
                //사용자 추가 이벤트 설정
                insertUser.Clicked += insertUserClicked;
                addIsTeacher.Clicked += delegate { unlessStudent(title.add); };
                addInsertClass.KeyReleaseEvent += addUserKeyRelease;
                addInsertGrade.KeyReleaseEvent += addUserKeyRelease;
                addInsertNumber.KeyReleaseEvent += addUserKeyRelease;
                addInsertName.KeyReleaseEvent += addUserKeyRelease;
                addInsertID.KeyReleaseEvent += addUserKeyRelease;
                
                //사용자 추가 배치
                {
                    addUser.Attach(addIsTeacher, 1, 1, 4, 1);

                    addUser.Attach(new Label("학년"), 1, 2, 1, 1);
                    addUser.Attach(addInsertGrade, 2, 2, 3, 1);

                    addUser.Attach(new Label("반"), 1, 3, 1, 1);
                    addUser.Attach(addInsertClass, 2, 3, 3, 1);

                    addUser.Attach(new Label("번호"), 1, 4, 1, 1);
                    addUser.Attach(addInsertNumber, 2, 4, 3, 1);

                    addUser.Attach(new Label("이름"), 1, 5, 1, 1);
                    addUser.Attach(addInsertName, 2, 5, 3, 1);

                    addUser.Attach(new Label("ID"), 1, 6, 1, 1);
                    addUser.Attach(addInsertID, 2, 6, 3, 1);

                    addUser.Attach(insertUser, 1, 7, 4, 1);
                }


                
                addUserFrame.Margin = 15;
                addUserFrame.MarginBottom = 0;
                addUserFrame.Add(addUser);
            }


            Frame delUserFrame = new Frame("사용자 삭제");
            Grid delUser = new Grid();
            {
                delInsertID.PlaceholderText = "사용자의 ID를 스캔 또는 입력해 주세요";
                delInsertGrade.PlaceholderText = "사용자의 학년을 입력해 주세요";
                delInsertClass.PlaceholderText = "사용자의 반을 입력해 주세요";
                delInsertNumber.PlaceholderText = "사용자의 번호를 입력해 주세요";
                delInsertUserWithoutID.Sensitive = false;
                delInsertUser.Sensitive = false;


                delUser.Margin = 15;
                delUser.ColumnHomogeneous = true;
                delUser.ColumnSpacing = 10;
                delUser.RowSpacing = 10;

                delInsertClass.KeyReleaseEvent += delUserWithoutIDKeyRelease;
                delInsertGrade.KeyReleaseEvent += delUserWithoutIDKeyRelease;
                delInsertNumber.KeyReleaseEvent += delUserWithoutIDKeyRelease;
                delIsTeacher.Clicked += delegate {
                    unlessStudent(title.delete);
                };
                delInsertID.KeyReleaseEvent += delegate {
                    if (string.IsNullOrEmpty(delInsertID.Text)) delInsertUser.Sensitive = false;
                    else delInsertUser.Sensitive = true;
                };
                delInsertUser.Clicked += delInsertUserClicked;
                delInsertUserWithoutID.Clicked += delInsertUserWithoutIDClicked;

                {
                    delUser.Attach(delInsertID, 1, 1, 4, 1);
                    delUser.Attach(delInsertUser, 5, 1, 1, 1);
                    delUser.Attach(new Separator(Orientation.Horizontal), 1, 2, 5, 1);
                    delUser.Attach(new Label("ID없이 사용자 삭제하기"), 1, 3, 5, 1);
                    delUser.Attach(delIsTeacher, 1, 4, 5, 1);
                    delUser.Attach(new Label("학년"), 1, 5, 1, 1);
                    delUser.Attach(delInsertGrade, 2, 5, 3, 1);
                    delUser.Attach(new Label("반"), 1, 6, 1, 1);
                    delUser.Attach(delInsertClass, 2, 6, 3, 1);
                    delUser.Attach(new Label("번호"), 1, 7, 1, 1);
                    delUser.Attach(delInsertNumber, 2, 7, 3, 1);
                    delUser.Attach(delInsertUserWithoutID, 5, 5, 1, 3);
                }
                delUserFrame.Margin = 15;
                delUserFrame.MarginTop = 0;
                delUserFrame.MarginBottom = 0;
                
                delUserFrame.Add(delUser);
            }    
            

            Grid statusList = new Grid();
            {
                statusList.RowSpacing = 10;
                statusList.Margin = 15;
                statusList.ColumnHomogeneous = true;

                {
                    statusList.Attach(userCount[0], 1, 1, 1, 1);
                    statusList.Attach(userCount[1], 2, 1, 1, 1);
                    statusList.Attach(userCount[2], 1, 2, 1, 1);
                    statusList.Attach(userCount[3], 2, 2, 1, 1);
                    statusList.Attach(allUserCount, 3, 1, 1, 2);
                }
            }
            
            Grid statusListMore = new Grid();
            {
                statusListMore.RowSpacing = 10;
                statusListMore.Margin = 15;
                statusListMore.ColumnHomogeneous = true;
            }

            
            
            foreach (var a in statusProgressBar)
            {
                a.ShowText = true;
                a.Text = "로딩...";
            }
            for (int i = 0; i < 4; i++)
            {
                statusProgressBar[i, 0].Name = "gray";
                statusProgressBar[i, 1].Name = "green";
                statusProgressBar[i, 2].Name = "red";
            }
            {
                statusListMore.Attach(new Label("학년"), 1, 1, 1, 1);
                statusListMore.Attach(new Label("미검사"), 2, 1, 2, 1);
                statusListMore.Attach(new Label("정상"), 4, 1, 2, 1);
                statusListMore.Attach(new Label("발열"), 6, 1, 2, 1);

                statusListMore.Attach(new Label("1"), 1, 2, 1, 1);
                statusListMore.Attach(statusProgressBar[0, 0], 2, 2, 2, 1);
                statusListMore.Attach(statusProgressBar[0, 1], 4, 2, 2, 1);
                statusListMore.Attach(statusProgressBar[0, 2], 6, 2, 2, 1);

                statusListMore.Attach(new Label("2"), 1, 3, 1, 1);
                statusListMore.Attach(statusProgressBar[1, 0], 2, 3, 2, 1);
                statusListMore.Attach(statusProgressBar[1, 1], 4, 3, 2, 1);
                statusListMore.Attach(statusProgressBar[1, 2], 6, 3, 2, 1);

                statusListMore.Attach(new Label("3"), 1, 4, 1, 1);
                statusListMore.Attach(statusProgressBar[2, 0], 2, 4, 2, 1);
                statusListMore.Attach(statusProgressBar[2, 1], 4, 4, 2, 1);
                statusListMore.Attach(statusProgressBar[2, 2], 6, 4, 2, 1);

                statusListMore.Attach(new Label("기타"), 1, 5, 1, 1);
                statusListMore.Attach(statusProgressBar[3, 0], 2, 5, 2, 1);
                statusListMore.Attach(statusProgressBar[3, 1], 4, 5, 2, 1);
                statusListMore.Attach(statusProgressBar[3, 2], 6, 5, 2, 1);
            }

            statusListFrame[0].Add(statusList);
            statusListFrame[0].Margin = 15;
            statusListFrame[0].MarginTop = 0;

            ScrolledWindow scroll2 = new ScrolledWindow();
            Grid manageMode = new Grid();
            {
                manageMode.RowSpacing = 10;
                manageMode.ColumnSpacing = 10;
                manageMode.ColumnHomogeneous = true;
                statusListFrame[0].MarginBottom = 0;
                statusListFrame[1].MarginBottom = 0;
                seeMoreInfo.MarginStart = 10;

                seeMoreInfo.Clicked += (sender, e) => {
                    if (seeMoreInfo.Active)
                    {
                        statusListFrame[0].Hide();
                        statusListFrame[1].ShowAll();
                    }
                    else
                    {
                        statusListFrame[1].Hide();
                        statusListFrame[0].ShowAll();
                    }
                };

                manageMode.Attach(addUserFrame, 1, 1, 1, 2);
                manageMode.Attach(delUserFrame, 1, 3, 1, 2);
                manageMode.Attach(statusListFrame[0], 1, 5, 1, 1);
                manageMode.Attach(seeMoreInfo, 1, 6, 1, 1);
            
                scroll2.Add(manageMode);
            }
            
            Grid setting = new Grid();
            {
                Dictionary<string, Grid> grids = new Dictionary<string, Grid>();

                Frame setUrlFrame = new Frame("URL 설정");
                Frame setTimeoutRetryFrame = new Frame("타임아웃 재시도 횟수 설정");
                Frame setUpdateCheckFrame = new Frame("업데이트 설정");
                Frame getSettingFileFrame = new Frame("설정 파일 가져오기");
                Frame setPasswordFrame = new Frame("비밀번호 설정");

                //setting Grid 설정
                {
                    setting.ColumnHomogeneous = true;
                    setting.RowSpacing = 10;
                    setting.Margin = 10;
                }
                //setting Grid 배치
                {
                    setting.Attach(setUrlFrame, 1, 1, 1, 1);
                    setting.Attach(setTimeoutRetryFrame, 1, 2, 1, 1);
                    setting.Attach(setUpdateCheckFrame, 1, 3, 1, 1);
                }                

                grids.Add("setUrl", new Grid());
                {
                    Entry url = new Entry();
                    {
                        url.PlaceholderText = "웹 사이트의 URL을 입력하세요.";
                        url.Text = settingJson["url"].ToString();
                    }
                    {
                        url.KeyReleaseEvent += delegate {
                            settingJson["url"] = url.Text;
                            user.url = url.Text;
                            File.WriteAllText(settingPath, settingJson.ToString());
                        };
                    }
                    {
                        grids["setUrl"].Attach(new Label("http://, https://"), 1, 1, 1, 1);
                        grids["setUrl"].Attach(url, 2, 1, 5, 1);
                    }
                    setUrlFrame.Add(grids["setUrl"]);                    
                }

                grids.Add("setTimeoutRetry", new Grid());
                {
                    Scale time = new Scale(Orientation.Horizontal, new Adjustment((double)settingJson["timeoutRetry"], 0, 500, 0, 1, 0));
                    SpinButton helpSet = new SpinButton(new Adjustment((double)settingJson["timeoutRetry"], 0, 500, 1, 1, 0), 1, 0);

                    {
                        time.RoundDigits = 0;
                        time.Digits = 0;
                        time.DrawValue = false;

                        grids["setTimeoutRetry"].Attach(new Label("타임아웃시 재시도 횟수를 설정해 주세요."), 1, 1, 5, 1);
                        grids["setTimeoutRetry"].Attach(time, 1, 2, 4, 1);
                        grids["setTimeoutRetry"].Attach(helpSet, 5, 2, 1, 1);
                    }
                    {
                        time.ValueChanged += delegate {
                            helpSet.Value = time.Value;
                        };
                        helpSet.ValueChanged += delegate {
                            time.Value = helpSet.Value;
                            settingJson["timeoutRetry"] = time.Value;
                            File.WriteAllText(settingPath, settingJson.ToString());
                        };

                    }
                    setTimeoutRetryFrame.Add(grids["setTimeoutRetry"]);
                }

                grids.Add("setUpdateCheck", new Grid());
                {
                    Gtk.Switch checkUpdate = new Gtk.Switch();
                    Gtk.Switch autoUpdate = new Gtk.Switch();
                    {
                        checkUpdate.StateChanged += delegate {
                            autoUpdate.Sensitive = checkUpdate.State;                            
                            settingJson["checkUpdate"] = checkUpdate.State;
                            File.WriteAllText(settingPath, settingJson.ToString());
                        };

                        autoUpdate.StateChanged += delegate {
                            settingJson["autoUpdate"] = autoUpdate.State;
                            File.WriteAllText(settingPath, settingJson.ToString());
                        };
                    }

                    {
                        checkUpdate.State = (bool)settingJson["checkUpdate"];
                        autoUpdate.State = (bool)settingJson["autoUpdate"];
                    }

                    {
                        grids["setUpdateCheck"].Attach(new Label("프로그램을 킬 때마다 업데이트를 확인하기"), 1, 1, 5, 1);
                        Grid checkgrid = new Grid();
                        checkgrid.Add(checkUpdate);
                        grids["setUpdateCheck"].Attach(checkgrid, 6, 1, 1, 1);

                        Grid autogrid = new Grid();
                        autogrid.Add(autoUpdate);
                        autoUpdate.Sensitive = false;
                        grids["setUpdateCheck"].Attach(new Label("업데이트 확인시 자동으로 업데이트하기"), 1, 2, 5, 1);
                        grids["setUpdateCheck"].Attach(autogrid, 6, 2, 1, 1);
                    }
                    setUpdateCheckFrame.Add(grids["setUpdateCheck"]);

                }

                grids.Add("setPassword", new Grid());
                {

                }
                
                foreach (var a in grids)
                {
                    a.Value.ColumnHomogeneous = true;
                    a.Value.Margin = 10;
                    a.Value.MarginTop = 0;
                }
            }

            //Grid들 Notebook에 추가
            selectMode.AppendPage(checkAll, new Label("체크"));
            selectMode.AppendPage(uncheck, new Label("체크 해제"));
            selectMode.AppendPage(scroll2, new Label("사용자 관리"));
            selectMode.AppendPage(setting, new Label("설정"));
            
            //로그 나타내는 ScrolledWindow에 추가
            scroll.Add(log);


            //시간 표시하는 레이블 놓을 Grid
            Grid setTimer = new Grid();
            setTimer.Attach(time, 1, 1, 1, 1);

            Label licence = new Label("MIT + ɑ License Copyright (c) 2020 SoftWareAndGuider, csnewcs, pmh-only, Noeul-Night / 자세한 저작권 관련 사항과 이 프로그램의 소스코드는 https://github.com/softwareandguider/covid-check-client에서 확인해 주세요.");
            licence.Margin = 10;
            licence.Valign = Align.End;
            EventBox b = new EventBox();
            b.Add(licence);


            //모든 것을 배치
            grid.RowHomogeneous = true;
            grid.Attach(b, 1, 2, 10, 1);
            grid.Attach(setTimer, 5, 1, 2, 2);
            grid.Attach(selectMode, 1, 1, 5, 2);
            grid.Attach(scroll, 6, 1, 5, 2);

            checkInsertID.SetSizeRequest(1, 3);

            //창에 추가
            Add(grid);

            //이제 보여주기
            ShowAll();

            statusListFrame[1].Add(statusListMore);
            statusListFrame[1].Margin = 15;
            statusListFrame[1].MarginTop = 0;
            manageMode.Attach(statusListFrame[1], 1, 5, 1, 1);


            Thread status = new Thread(new ThreadStart(getStatus));
            Thread showTime = new Thread(new ThreadStart(timer));
            status.Start();
            showTime.Start();

            selectMode.Page = 2;
            selectMode.Page = 0; //이런식으로 하지 않으면 종종 발열체크를 선택할 수 없을 때가 있음

            addLog("프로그램 로딩이 완료됨");

            if ((bool)settingJson["checkUpdate"] && !doneUpdate)
            {
                JArray update = new JArray();
                if (user.hasNewVersion(2, out update))
                {
                    if ((bool)settingJson["autoUpdate"])
                    {
                        WebClient client = new WebClient();
                        client.Encoding = System.Text.Encoding.UTF8;
                        client.Headers.Add("user-agent", "CovidCheckClientCheckUpdate");
                        JArray files = update.First()["assets"] as JArray;

                        foreach (var file in files)
                        {
                            if (file["name"].ToString() == "Default-Version.zip")
                            {
                                client.DownloadFile(file["browser_download_url"].ToString(), "update.zip");
                                break;
                            }
                        }
                        ZipFile.ExtractToDirectory("./update.zip", "./", true);
                        ZipFile.ExtractToDirectory("./GUI.zip", "./", true);
                        Directory.CreateDirectory("update");


                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                        {
                            ZipFile.ExtractToDirectory("./linux-x64.zip", "./update", true);
                            ProcessStartInfo info = new ProcessStartInfo("update/CovidCheckClientGui", "update linux");
                            info.WorkingDirectory = "./update";
                            try
                            {
                                Process.Start(info);
                            }
                            catch
                            {
                                info = new ProcessStartInfo("./update/CovidCheckClientGui", "update linux");
                                info.UseShellExecute = true;
                                Process.Start("chmod", "777 update");
                                Thread.Sleep(1000);
                                //info.WorkingDirectory = "./update";
                                Process process = new Process();
                                process.StartInfo = info;
                                process.Start();
                            }

                            Environment.Exit(0);
                        }
                        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        {
                            ZipFile.ExtractToDirectory("./win-x64.zip", "./update", true);
                            ProcessStartInfo info = new ProcessStartInfo("update/CovidCheckClientGui.exe", "update windows");
                            info.WorkingDirectory = "./update";
                            Process.Start(info);
                        }
                    }
                    else
                    {
                        string name = update.First()["name"].ToString();
                        MessageDialog dialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Info, ButtonsType.Close, true, $"프로그램의 새 버전({name})을 찾았습니다. <a href=\"https://github.com/SoftWareAndGuider/Covid-Check-Client/releases\">여기를 눌러</a> 확인해 주세요.");
                        dialog.Run();
                        dialog.Dispose();
                    }
                }                
            }
        }
        
        


        private void getStatus()
        {
            while (programProcessing)
            {
                try
                {
                    JObject result = new JObject();
                    string down = up();

                    result = JObject.Parse(down);

                    StatusParsing sp = new StatusParsing();
                    if (seeMoreInfo.Active)
                    {
                        double[,] parse = sp.moreInfo(result);
                        double[] allUsers = new double[4] {
                            parse[0, 0] + parse[0, 1] + parse[0, 2],
                            parse[1, 0] + parse[1, 1] + parse[1, 2],
                            parse[2, 0] + parse[2, 1] + parse[2, 2],
                            parse[3, 0] + parse[3, 1] + parse[3, 2]
                        };
                        Application.Invoke(delegate {
                            {
                                statusProgressBar[0, 0].Text = $"{parse[0, 0]}/{allUsers[0]}";
                                statusProgressBar[0, 1].Text = $"{parse[0, 1]}/{allUsers[0]}";
                                statusProgressBar[0, 2].Text = $"{parse[0, 2]}/{allUsers[0]}";

                                statusProgressBar[1, 0].Text = $"{parse[1, 0]}/{allUsers[1]}";
                                statusProgressBar[1, 1].Text = $"{parse[1, 1]}/{allUsers[1]}";
                                statusProgressBar[1, 2].Text = $"{parse[1, 2]}/{allUsers[1]}";

                                statusProgressBar[2, 0].Text = $"{parse[2, 0]}/{allUsers[2]}";
                                statusProgressBar[2, 1].Text = $"{parse[2, 1]}/{allUsers[2]}";
                                statusProgressBar[2, 2].Text = $"{parse[2, 2]}/{allUsers[2]}";

                                statusProgressBar[3, 0].Text = $"{parse[3, 0]}/{allUsers[3]}";
                                statusProgressBar[3, 1].Text = $"{parse[3, 1]}/{allUsers[3]}";
                                statusProgressBar[3, 2].Text = $"{parse[3, 2]}/{allUsers[3]}";
                            }
                            for (int i = 0; i < 4; i++)
                            {
                                if (allUsers[i] == 0) allUsers[i] = 1;
                            }
                            {
                                statusProgressBar[0, 0].Fraction = parse[0, 0] / allUsers[0];
                                statusProgressBar[0, 1].Fraction = parse[0, 1] / allUsers[0];
                                statusProgressBar[0, 2].Fraction = parse[0, 2] / allUsers[0];

                                statusProgressBar[1, 0].Fraction = parse[1, 0] / allUsers[1];
                                statusProgressBar[1, 1].Fraction = parse[1, 1] / allUsers[1];
                                statusProgressBar[1, 2].Fraction = parse[1, 2] / allUsers[1];

                                statusProgressBar[2, 0].Fraction = parse[2, 0] / allUsers[2];
                                statusProgressBar[2, 1].Fraction = parse[2, 1] / allUsers[2];
                                statusProgressBar[2, 2].Fraction = parse[2, 2] / allUsers[2];
                            
                                statusProgressBar[3, 0].Fraction = parse[3, 0] / allUsers[3];
                                statusProgressBar[3, 1].Fraction = parse[3, 1] / allUsers[3];
                                statusProgressBar[3, 2].Fraction = parse[3, 2] / allUsers[3];
                            }
                        });
                    }
                    else
                    {
                        int[] parse = sp.lessInfo(result);
                        Application.Invoke(delegate {
                            userCount[0].Text = "1학년: " + parse[0].ToString() + "명";
                            userCount[1].Text = "2학년: " + parse[1].ToString() + "명";
                            userCount[2].Text = "3학년: " + parse[2].ToString() + "명";
                            userCount[3].Text = "기타: " + parse[3].ToString() + "명";
                            allUserCount.Text = "합계: " + (parse[0] + parse[1] + parse[2] + parse[3]).ToString() + "명";
                        });
                    }
                    long ping = user.getPing(settingJson["url"].ToString());
                    Application.Invoke(delegate {
                        base.Title = $"코로나19 예방용 발열체크 프로그램 (통신 속도: {ping}ms)";
                    });
                }
                catch
                {
                }
                GC.Collect();
                Thread.Sleep(3000);
            }
        }
        private string up()
        {
            WebClient client = new WebClient();
            string url = "";
            string uploadString = "{\"process\":\"info\", \"multi\": true}";
            bool doing = true;
            bool success = false;
            string result = "";
            client.UploadStringCompleted += (sender, e) => {
                try
                {
                    result = e.Result;
                    success = true;
                }
                catch
                {
                    Application.Invoke(delegate {
                        addTimeoutLog("사용자 정보를 불러오는데 실패함.");
                    });
                }
                doing = false;
            };
            
            try
            {
                url = "http://" + settingJson["url"] + "/api";
                client.Headers.Add("Content-Type", "application/json");
                client.UploadStringAsync(new Uri(url), "PUT", uploadString);
                while (doing)
                {

                }
            }
            catch
            {
                url = "https://" + settingJson["url"] + "/api";

                client.Headers.Add("Content-Type", "application/json");
                
                client.UploadStringAsync(new Uri(url), "PUT", uploadString);
                while (doing)
                {

                }
            }
            if (success)
            {
                return result;
            }
            return null;
        }
        private void timer()
        {
            while (programProcessing)
            {
                DateTime dt = DateTime.Now;
                string text = $"{dt.Month}월 {dt.Day}일 {dt.Hour}:{dt.Minute}:{dt.Second}";
                Application.Invoke(delegate {
                    time.Text = text;
                });
                Thread.Sleep(100);
            }
        }
        
        string last = "";
        Label logLabel = new Label();
        public void addLog(string text)
        {
            if (last == text) return;
            last = text;
            string storeTime = time.Text;
            if (string.IsNullOrEmpty(storeTime))
            {
                DateTime dt = DateTime.Now;
                storeTime = $"{dt.Month}월 {dt.Day}일 {dt.Hour}:{dt.Minute}:{dt.Second}";
            }
            
            logLabel.StyleContext.RemoveClass("nowlog");            

            logLabel = new Label($"{text} ({storeTime})");

            logLabel.StyleContext.AddClass("nowlog");
            
            logLabel.Name = "add";
            log.Insert(logLabel, 0);
            log.ShowAll();
        }

        Label timeoutLogLabel = new Label();
        private void addTimeoutLog(string text)
        {
            if (!hasTimeout)
            {
                hasTimeout = true;
                log.SetSizeRequest(5, 1);
                grid.Remove(scroll);
                grid.RowSpacing = 10;
                
                Frame defaultFrame = new Frame("일반 로그");
                Frame timeoutFrame = new Frame("재시도 로그");

                timeoutLogScroll.Add(timeoutLog);

                defaultFrame.Add(scroll);
                timeoutFrame.Add(timeoutLogScroll);

                grid.Attach(defaultFrame, 6, 1, 5, 1);
                grid.Attach(timeoutFrame, 6, 2, 5, 1);

                timeoutLog.Show();
                timeoutLogScroll.Show();
                defaultFrame.Show();
                timeoutFrame.Show();
            }
            
            string storeTime = time.Text;
            if (string.IsNullOrEmpty(storeTime))
            {
                DateTime dt = DateTime.Now;
                storeTime = $"{dt.Month}월 {dt.Day}일 {dt.Hour}:{dt.Minute}:{dt.Second}";
            }

            timeoutLogLabel.StyleContext.RemoveClass("nowlog");

            timeoutLogLabel = new Label($"{text} ({storeTime})");
            timeoutLogLabel.StyleContext.AddClass("nowlog");
            timeoutLogLabel.Name = "add";

            timeoutLog.Insert(timeoutLogLabel, 0);
            timeoutLog.ShowAll();
        }
    }
}

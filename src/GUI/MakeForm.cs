using System;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Net;
using Gtk;
using Newtonsoft.Json.Linq;


namespace CovidCheckClientGui
{
    partial class Program : Window
    {
        // 오른쪽에 뜨는 로그
        ListBox log = new ListBox();

        // 사용자 체크
        Entry checkInsertID = new Entry();
        Scale checkIDLength = new Scale(Orientation.Horizontal, new Adjustment(8, 5, 10, 0, 1, 0));

        Entry checkInsertGrade = new Entry();
        Entry checkInsertClass = new Entry();
        Entry checkInsertNumber = new Entry();
        CheckButton checkIsTeacher = new CheckButton("학생이 아님");

        Button checkOK = new Button("체크하기");
        Button checkInsertUser = new Button("체크하기");


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
        Label[,] userCount = new Label[4, 4] { //[0, ]: 1학년, [1, ]: 2학년, [2, ]: 3학년, [3, ]: 기타, [, 0]: 미검사, [, 1]: 검사, [, 2]: 발열, [, 3]: 상세정보 없음
            {
                new Label(""),
                new Label(""),
                new Label(""),
                new Label("")
            },
            {
                new Label(""),
                new Label(""),
                new Label(""),
                new Label("")
            },
            {
                new Label(""),
                new Label(""),
                new Label(""),
                new Label("")
            },
            {
                new Label(""),
                new Label(""),
                new Label(""),
                new Label("")
            }
        };
        Label allUserCount = new Label("");

        Label time = new Label("");

        CheckButton seeMoreInfo = new CheckButton("상세정보 보기");
        Frame[] statusListFrame = new Frame[2] {new Frame("사용자 수 (3초마다 새로고침)"), new Frame("검사 현황 (3초마다 새로고침)")};
        LevelBar[,] statusLevelBar = new LevelBar[4, 3] { //[0, ]: 1학년, [1, ]: 2학년, [2, ]: 3학년, [3, ]: 기타, [, 0]: 미검사, [, 1]: 검사, [, 2]: 발열
            {
                new LevelBar(0, 100),
                new LevelBar(0, 100),
                new LevelBar(0, 100)
            },
            {
                new LevelBar(0, 100),
                new LevelBar(0, 100),
                new LevelBar(0, 100)
            },
            {
                new LevelBar(0, 100),
                new LevelBar(0, 100),
                new LevelBar(0, 100)
            },
            {
                new LevelBar(0, 100),
                new LevelBar(0, 100),
                new LevelBar(0, 100)
            }
        };        

        public Program() : base("코로나19 예방용 발열체크 프로그램")
        {
            try
            {
                System.IO.File.ReadAllText("config.txt");
            }
            catch
            {
                File.WriteAllText("config.txt", "홈페이지 URL을 입력해 주세요... (마지막에 / 빼고)");
                MessageDialog dialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Error, ButtonsType.Close, false, "./config.txt에 홈페이지 주소를 입력해 주세요");
                    dialog.Run();
                    dialog.Dispose();
                Environment.Exit(0);
            }
            addLog("프로그램이 시작됨");            
            DeleteEvent += delegate {programProcessing = false; Application.Quit();};

            SetDefaultSize(1280, 720);
            
            // 전체를 감싸는 Grid
            Grid grid = new Grid();
            grid.Margin = 20;
            grid.ColumnHomogeneous = true;
            grid.ColumnSpacing = 8;
            //왼쪽의 탭들 (체크, 체크 해제, 추가)
            Notebook selectMode = new Notebook();


            //사용자 체크 Grid
            Grid check = new Grid();

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
            {
                check.Attach(new Label("실제 바코드의 길이가 지정한 바코드의 길이와 다를 경우 체크하기 버튼을 눌러 체크해주세요."), 1, 1, 5, 1); // 공지 추가
                check.Attach(checkInsertID, 1, 2, 4, 1); // 텍스트박스 추가
                check.Attach(checkOK, 5, 2, 1, 1); //OK 버튼 추가

                check.Attach(new Separator(Orientation.Horizontal), 1, 4, 5, 1);
            }
            
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

            Frame checkFrame = new Frame("정상");
            checkFrame.Margin = 15;
            checkFrame.MarginBottom = 0;
            checkFrame.MarginTop = 0;
            checkFrame.Add(check);

            Grid checkDoubt = new Grid();
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

            Frame checkDoubtFrame = new Frame("의심 체크");
            checkDoubtFrame.Margin = 15;
            checkDoubtFrame.MarginTop = 0;
            checkDoubtFrame.Add(checkDoubt);


            Grid checkAll = new Grid();
            checkAll.ColumnHomogeneous = true;
            checkAll.RowSpacing = 10;
            Label changeBarcodeLength = new Label("바코드 길이 조절");
            changeBarcodeLength.MarginTop = 10;
            checkIDLength.MarginTop = 10;
            checkAll.Attach(changeBarcodeLength, 1, 1, 1, 1);
            checkAll.Attach(checkIDLength, 2, 1, 4, 1);     
            checkAll.Attach(checkFrame, 1, 2, 5, 1);
            checkAll.Attach(checkDoubtFrame, 1, 3, 5, 1);            


            //사용자 체크 해제 Grid
            Grid uncheck = new Grid();

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



            //사용자 추가 Grid
            Grid addUser = new Grid();
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


            
            Frame addUserFrame = new Frame("사용자 추가");
            addUserFrame.Margin = 15;
            addUserFrame.MarginBottom = 0;
            addUserFrame.Add(addUser);


            Grid delUser = new Grid();
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
            

            Frame delUserFrame = new Frame("사용자 삭제");
            delUserFrame.Margin = 15;
            delUserFrame.MarginTop = 0;
            delUserFrame.MarginBottom = 0;
            
            delUserFrame.Add(delUser);

            Grid statusList = new Grid();
            statusList.RowSpacing = 10;
            statusList.Margin = 15;
            statusList.ColumnHomogeneous = true;

            {
                statusList.Attach(userCount[0, 3], 1, 1, 1, 1);
                statusList.Attach(userCount[1, 3], 2, 1, 1, 1);
                statusList.Attach(userCount[2, 3], 1, 2, 1, 1);
                statusList.Attach(userCount[3, 3], 2, 2, 1, 1);
                statusList.Attach(allUserCount, 3, 1, 1, 2);
            }
            
            Grid statusListMore = new Grid();
            statusListMore.RowSpacing = 10;
            statusListMore.Margin = 15;
            statusListMore.ColumnHomogeneous = true;
            
            {
                statusListMore.Attach(new Label("학년"), 1, 1, 1, 1);
                statusListMore.Attach(new Label("미검사"), 2, 1, 2, 1);
                statusListMore.Attach(new Label("검사"), 4, 1, 2, 1);
                statusListMore.Attach(new Label("의심"), 6, 1, 2, 1);

                statusListMore.Attach(new Label("1"), 1, 2, 1, 1);
                statusListMore.Attach(userCount[0, 0], 3, 2, 1, 1);
                statusListMore.Attach(statusLevelBar[0, 0], 2, 2, 2, 1);
                statusListMore.Attach(userCount[0, 1], 5, 2, 1, 1);
                statusListMore.Attach(statusLevelBar[0, 1], 4, 2, 2, 1);
                statusListMore.Attach(userCount[0, 2], 7, 2, 1, 1);
                statusListMore.Attach(statusLevelBar[0, 2], 6, 2, 2, 1);

                statusListMore.Attach(new Label("2"), 1, 3, 1, 1);
                statusListMore.Attach(userCount[1, 0], 3, 3, 1, 1);
                statusListMore.Attach(statusLevelBar[1, 0], 2, 3, 2, 1);
                statusListMore.Attach(userCount[1, 1], 5, 3, 1, 1);
                statusListMore.Attach(statusLevelBar[1, 1], 4, 3, 2, 1);
                statusListMore.Attach(userCount[1, 2], 7, 3, 1, 1);
                statusListMore.Attach(statusLevelBar[1, 2], 6, 3, 2, 1);

                statusListMore.Attach(new Label("3"), 1, 4, 1, 1);
                statusListMore.Attach(userCount[2, 0], 3, 4, 1, 1);
                statusListMore.Attach(statusLevelBar[2, 0], 2, 4, 2, 1);
                statusListMore.Attach(userCount[2, 1], 5, 4, 1, 1);
                statusListMore.Attach(statusLevelBar[2, 1], 4, 4, 2, 1);
                statusListMore.Attach(userCount[2, 2], 7, 4, 1, 1);
                statusListMore.Attach(statusLevelBar[2, 2], 6, 4, 2, 1);

                statusListMore.Attach(new Label("기타"), 1, 5, 1, 1);
                statusListMore.Attach(userCount[3, 0], 3, 5, 1, 1);
                statusListMore.Attach(statusLevelBar[3, 0], 2, 5, 2, 1);
                statusListMore.Attach(userCount[3, 1], 5, 5, 1, 1);
                statusListMore.Attach(statusLevelBar[3, 1], 4, 5, 2, 1);
                statusListMore.Attach(userCount[3, 2], 7, 5, 1, 1);
                statusListMore.Attach(statusLevelBar[3, 2], 6, 5, 2, 1);
            }

            statusListFrame[0].Add(statusList);
            statusListFrame[0].Margin = 15;
            statusListFrame[0].MarginTop = 0;

            Grid manageMode = new Grid();
            manageMode.RowSpacing = 10;
            manageMode.ColumnSpacing = 10;
            manageMode.ColumnHomogeneous = true;

            manageMode.Attach(addUserFrame, 1, 1, 1, 2);
            manageMode.Attach(delUserFrame, 1, 3, 1, 2);
            manageMode.Attach(statusListFrame[0], 1, 5, 1, 1);
            

            //Grid들 Notebook에 추가
            selectMode.AppendPage(checkAll, new Label("체크"));
            selectMode.AppendPage(uncheck, new Label("체크 해제"));
            selectMode.AppendPage(manageMode, new Label("사용자 관리"));
            
            
            //로그 나타내는 ScrolledWindow에 추가
            ScrolledWindow scroll = new ScrolledWindow();
            scroll.Add(log);

            //시간 표시하는 레이블 놓을 Grid
            Grid setTimer = new Grid();
            setTimer.Attach(time, 1, 1, 1, 1);

            //모든 것을 배치
            grid.RowHomogeneous = true;
            
            grid.Attach(setTimer, 5, 1, 2, 1);
            grid.Attach(selectMode, 1, 1, 5, 1);
            grid.Attach(scroll, 6, 1, 5, 1);
            
            //창에 추가
            Add(grid);
            //이제 보여주기
            ShowAll();

            statusListFrame[1].Add(statusListMore);
            statusListFrame[1].Margin = 15;
            statusListFrame[1].MarginTop = 0;

            Thread status = new Thread(new ThreadStart(getStatus));
            Thread showTime = new Thread(new ThreadStart(timer));
            status.Start();
            showTime.Start();
            addLog("프로그램 로딩이 완료됨");
        }
        
        string last = "";
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
            log.Insert(new Label($"{text} ({storeTime})"), 0);
            log.ShowAll();
        }


        private void getStatus()
        {
            string url = File.ReadAllLines("config.txt")[0] + "/api";
            WebClient client = new WebClient();
            string uploadString = "{\"process\":\"info\", \"multi\": true}";
            while (programProcessing)
            {
                client.Headers.Add("Content-Type", "application/json");
                JObject result = new JObject();
                try
                {
                    string down = "";
                    client.UploadStringCompleted += (sender, e) => {
                        down = e.Result;
                    };
                    client.UploadStringAsync(new Uri(url), "PUT", uploadString);
                    while (down == "")
                    {
                        System.Threading.Thread.Sleep(10);
                    }
                    result = JObject.Parse(down);
                    StatusParsing sp = new StatusParsing();
                    if (seeMoreInfo.Active)
                    {

                    }
                    else
                    {
                        int[] parse = sp.lessInfo(result);
                        Application.Invoke(delegate {
                            userCount[0, 3].Text = "1학년: " + parse[0].ToString() + "명";
                            userCount[1, 3].Text = "2학년: " + parse[1].ToString() + "명";
                            userCount[2, 3].Text = "3학년: " + parse[2].ToString() + "명";
                            userCount[3, 3].Text = "기타: " + parse[3].ToString() + "명";
                            allUserCount.Text = "합계: " + (parse[0] + parse[1] + parse[2] + parse[3]).ToString() + "명";
                        });
                    }
                    
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
                }
                GC.Collect();
                Thread.Sleep(3000);
            }
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
    }
}

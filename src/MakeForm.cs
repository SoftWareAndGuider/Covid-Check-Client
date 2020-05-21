using System;
using System.IO;
using System.Diagnostics;
using Gtk;


namespace CovidCheckClientGui
{
    partial class Program : Window
    {
        // 오른쪽에 뜨는 로그
        ListBox log = new ListBox();

        // 사용자 체크
        Entry checkInsertID = new Entry();

        Entry checkInsertGrade = new Entry();
        Entry checkInsertClass = new Entry();
        Entry checkInsertNumber = new Entry();
        CheckButton checkIsTeacher = new CheckButton("학생이 아님");

        Button checkInsertUser = new Button("검사 확인하기");

        // 사용자 체크 해제        
        Entry uncheckInsertID = new Entry();

        Entry uncheckInsertGrade = new Entry();
        Entry uncheckInsertClass = new Entry();
        Entry uncheckInsertNumber = new Entry();
        CheckButton uncheckIsTeacher = new CheckButton("학생이 아님");

        Button uncheckInsertUser = new Button("검사 취소하기");
        

        //사용자 추가
        Entry addInsertID = new Entry();
        Entry addInsertGrade = new Entry();
        Entry addInsertClass = new Entry();
        Entry addInsertNumber = new Entry();
        Entry addInsertName = new Entry();
        CheckButton addIsTeacher = new CheckButton("학생이 아님");
        Button insertUser = new Button("사용자 만들기");


        Entry delInsertID = new Entry();

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
            DeleteEvent += delegate {Application.Quit();};

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
            checkInsertID.PlaceholderText = "사용자의 번호를 스캔 혹은 입력해 주세요";
            checkInsertGrade.PlaceholderText = "사용자의 학년을 입력해 주세요";
            checkInsertClass.PlaceholderText = "사용자의 반을 입력해 주세요";
            checkInsertNumber.PlaceholderText = "사용자의 번호를 입력해 주세요";
            checkInsertUser.Sensitive = false;

            //사용자 체크 이벤트 설정
            checkInsertID.KeyReleaseEvent += checkInsertIDChangeText;
            checkIsTeacher.Clicked += delegate {unlessStudent(title.check);};
            checkInsertUser.Clicked += checkInsertUserClicked;
            checkInsertGrade.KeyReleaseEvent += checkWithoutIDKeyRelease;
            checkInsertClass.KeyReleaseEvent += checkWithoutIDKeyRelease;
            checkInsertNumber.KeyReleaseEvent += checkWithoutIDKeyRelease;

            //사용자 체크 배치(ID)
            check.Attach(new Label("번호로 체크 확인하기"), 1, 1, 5, 1); // 공지 추가
            check.Attach(checkInsertID, 1, 2, 5, 1); // 텍스트박스 추가

            check.Attach(new Separator(Orientation.Horizontal), 1, 4, 5, 1);
            
            //사용자 체크 배치(학년, 반, 번호)
            check.Attach(new Label("번호 없이 체크하기"), 1, 5, 5, 1);
            check.Attach(checkIsTeacher, 1, 6, 5, 1);
            check.Attach(new Label("학년"), 1, 7, 1, 1);
            check.Attach(checkInsertGrade, 2, 7, 4, 1);
            check.Attach(new Label("반"), 1, 8, 1, 1);
            check.Attach(checkInsertClass, 2, 8, 4, 1);
            check.Attach(new Label("번호"), 1, 9, 1, 1);
            check.Attach(checkInsertNumber, 2, 9, 4, 1);
            check.Attach(checkInsertUser, 1, 10, 5, 1);


            //사용자 체크 해제 Grid
            Grid uncheck = new Grid();

            //사용자 체크 해제 속성 설정
            uncheck.ColumnHomogeneous = true; //창의 크기가 달라지면 알아서 위젯 크기 조절해줌
            uncheck.RowSpacing = 10; //Row는 위아래
            uncheck.ColumnSpacing = 10; //Column은 양 옆
            uncheck.Margin = 15;
            uncheckInsertID.PlaceholderText = "사용자의 번호를 스캔 혹은 입력해 주세요";
            uncheckInsertGrade.PlaceholderText = "사용자의 학년을 입력해 주세요";
            uncheckInsertClass.PlaceholderText = "사용자의 반을 입력해 주세요";
            uncheckInsertNumber.PlaceholderText = "사용자의 번호를 입력해 주세요";
            uncheckInsertUser.Sensitive = false;

            //사용자 체크 해제 이벤트 설정
            uncheckInsertID.KeyReleaseEvent += uncheckInsertIDChangeText;
            uncheckInsertGrade.KeyReleaseEvent += uncheckWithoutIDKeyRelease;
            uncheckInsertClass.KeyReleaseEvent += uncheckWithoutIDKeyRelease;
            uncheckInsertNumber.KeyReleaseEvent += uncheckWithoutIDKeyRelease;
            uncheckIsTeacher.Clicked += delegate {unlessStudent(title.uncheck);};
            uncheckInsertUser.Clicked += uncheckInsertUserClicked;


            //사용자 체크 해제 배치(ID)
            uncheck.Attach(new Label("번호로 검사 확인하기"), 1, 1, 5, 1); // 공지 추가
            uncheck.Attach(uncheckInsertID, 1, 2, 5, 1); // 텍스트박스 추가
            
            uncheck.Attach(new Separator(Orientation.Horizontal), 1, 4, 5, 1);

            //사용자 체크 해제 배치(학년, 반, 번호)
            uncheck.Attach(new Label("번호 없이 체크 해제하기"), 1, 5, 5, 1);
            uncheck.Attach(uncheckIsTeacher, 1, 6, 5, 1);
            uncheck.Attach(new Label("학년"), 1, 7, 1, 1);
            uncheck.Attach(uncheckInsertGrade, 2, 7, 4, 1);
            uncheck.Attach(new Label("반"), 1, 8, 1, 1);
            uncheck.Attach(uncheckInsertClass, 2, 8, 4, 1);
            uncheck.Attach(new Label("번호"), 1, 9, 1, 1);
            uncheck.Attach(uncheckInsertNumber, 2, 9, 4, 1);
            uncheck.Attach(uncheckInsertUser, 1, 10, 5, 1);


            Grid manageMode = new Grid();

            //사용자 추가 Grid
            Grid addUser = new Grid();
            //사용자 추가 속성 설정
            addUser.ColumnHomogeneous = true;
            addUser.Margin = 15;
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

            Frame addUserFrame = new Frame("사용자 추가");
            addUserFrame.Margin = 15;
            addUserFrame.Add(addUser);


            Grid delUser = new Grid();
            Button delInsertUser = new Button("사용자 삭제");

            delUser.ColumnHomogeneous = true;
            delUser.Margin = 15;
            delUser.RowSpacing = 10;
            delInsertID.PlaceholderText = "삭제할 사용자의 ID를 스캔 혹은 입력해 주세요";
            delInsertUser.Sensitive = false;

            delUser.Attach(delInsertID, 1, 1, 1, 1);
            delUser.Attach(delInsertUser, 1, 2, 1, 1);

            Frame delUserFrame = new Frame("사용자 삭제");
            delUserFrame.Margin = 15;
            delUserFrame.Add(delUser);
            
            manageMode.ColumnHomogeneous = true;
            manageMode.Attach(addUserFrame, 1, 1, 1, 1);
            manageMode.Attach(delUserFrame, 1, 2, 1, 1);

            //Grid들 Notebook에 추가
            selectMode.AppendPage(check, new Label("검사 확인"));
            selectMode.AppendPage(uncheck, new Label("검사 취소"));
            selectMode.AppendPage(manageMode, new Label("관리자 모드"));
            
            
            //로그 나타내는 ScrolledWindow에 추가
            ScrolledWindow scroll = new ScrolledWindow();
            scroll.Add(log);

            //모든 것을 배치
            grid.RowHomogeneous = true;
            grid.Attach(selectMode, 1, 1, 1, 1);
            grid.Attach(scroll, 2, 1, 1, 1);
            
            //창에 추가
            Add(grid);
            //이제 보여주기
            ShowAll();
            addLog("프로그램 로딩이 완료됨");
            
        }
        
        string last = "";
        public void addLog(string text)
        {
            DateTime dt = DateTime.Now;
            if (last == text) return;
            last = text;
            string time = $" ({dt.Hour}:{dt.Minute}:{dt.Second})";
            log.Insert(new Label(text + time), 0);
            log.ShowAll();
        }
    }
}

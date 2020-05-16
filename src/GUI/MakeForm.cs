using System;
using Gtk;


namespace CovidCheckClientGui
{
    partial class Program : Window
    {
        ListBox log = new ListBox();

        Entry checkInsertID = new Entry();
        Scale checkIDLength = new Scale(Orientation.Horizontal, new Adjustment(8, 5, 10, 0, 1, 0));
        Entry checkInsertGrade = new Entry();
        Entry checkInsertClass = new Entry();
        Entry checkInsertNumber = new Entry();
        CheckButton checkIsTeacher = new CheckButton("학생이 아님");


        Entry uncheckInsertID = new Entry();
        Scale uncheckIDLength = new Scale(Orientation.Horizontal, new Adjustment(8, 5, 10, 0, 1, 0));

        Entry addInsertID = new Entry();
        Entry addInsertGrade = new Entry();
        Entry addInsertClass = new Entry();
        Entry addInsertNumber = new Entry();
        Entry addInsertName = new Entry();
        CheckButton addIsTeacher = new CheckButton("학생이 아님");
        Button insertUser = new Button("사용자 만들기");
        public Program() : base("코로나19 예방용 발열체크 프로그램")
        {
            addLog("프로그램이 시작됨");            
            DeleteEvent += delegate {Application.Quit();};

            SetDefaultSize(1280, 720);
            
            Grid grid = new Grid();
            grid.Margin = 20;
            grid.ColumnHomogeneous = true;
            grid.ColumnSpacing = 8;
            Notebook selectMode = new Notebook();          


            Grid check = new Grid();
            Button checkOK = new Button("체크하기");
            Button checkInsertUser = new Button("체크하기");

            check.ColumnHomogeneous = true; //창의 크기가 달라지면 알아서 위젯 크기 조절해줌
            check.RowSpacing = 10; //Row는 위아래
            check.ColumnSpacing = 10; //Column은 양 옆
            check.Margin = 15;
            checkInsertID.PlaceholderText = "사용자의 ID를 스캔 혹은 입력해 주세요";
            checkIDLength.Digits = 0;
            checkIDLength.ValuePos = PositionType.Right;
            checkInsertID.PlaceholderText = "사용자의 ID를 스캔 혹은 입력해 주세요";
            checkInsertGrade.PlaceholderText = "사용자의 학년을 입력해 주세요";
            checkInsertClass.PlaceholderText = "사용자의 반을 입력해 주세요";
            checkInsertNumber.PlaceholderText = "사용자의 번호를 입력해 주세요";

            checkInsertID.KeyReleaseEvent += checkInsertIDChangeText;
            checkIDLength.ValueChanged += checkIDLengthChangeValue;
            checkOK.Clicked += checkOKClicked;
            checkIsTeacher.Clicked += delegate {ifTeacher(title.check);};

            check.Attach(new Label("실제 바코드의 길이가 지정한 바코드의 길이와 다를 경우 체크하기 버튼을 눌러 체크해주세요."), 1, 1, 5, 1); // 공지 추가
            check.Attach(checkInsertID, 1, 2, 4, 1); // 텍스트박스 추가
            check.Attach(checkOK, 5, 2, 1, 1); //OK 버튼 추가
            check.Attach(new Label("바코드 길이 조절"), 1, 3, 1, 1);
            check.Attach(checkIDLength, 2, 3, 4, 1);

            check.Attach(new Separator(Orientation.Horizontal), 1, 4, 5, 1);

            check.Attach(new Label("ID 없이 체크하기"), 1, 5, 5, 1);
            check.Attach(checkIsTeacher, 1, 6, 5, 1);
            check.Attach(new Label("학년"), 1, 7, 1, 1);
            check.Attach(checkInsertGrade, 2, 7, 4, 1);
            check.Attach(new Label("반"), 1, 8, 1, 1);
            check.Attach(checkInsertClass, 2, 8, 4, 1);
            check.Attach(new Label("번호"), 1, 9, 1, 1);
            check.Attach(checkInsertNumber, 2, 9, 4, 1);
            check.Attach(checkInsertUser, 1, 10, 5, 1);



            Grid uncheck = new Grid();
            Button uncheckOK = new Button("체크 해제하기");

            uncheck.ColumnHomogeneous = true; //창의 크기가 달라지면 알아서 위젯 크기 조절해줌
            uncheck.RowSpacing = 10; //Row는 위아래
            uncheck.ColumnSpacing = 10; //Column은 양 옆
            uncheck.Margin = 15;
            uncheckInsertID.PlaceholderText = "사용자의 ID를 스캔 혹은 입력해 주세요";
            uncheckIDLength.Digits = 0;
            uncheckIDLength.ValuePos = PositionType.Right;

            uncheckInsertID.KeyReleaseEvent += uncheckInsertIDChangeText;
            uncheckIDLength.ValueChanged += uncheckIDLengthChangeValue;
            uncheckOK.Clicked += uncheckOKClicked;

            uncheck.Attach(new Label("실제 바코드의 길이가 지정한 바코드의 길이와 다를 경우 체크 해제하기 버튼을 눌러 체크해주세요."), 1, 1, 5, 1); // 공지 추가
            uncheck.Attach(uncheckInsertID, 1, 2, 4, 1); // 텍스트박스 추가
            uncheck.Attach(uncheckOK, 5, 2, 1, 1); //OK 버튼 추가
            uncheck.Attach(new Label("바코드 길이 조절"), 1, 3, 1, 1);
            uncheck.Attach(uncheckIDLength, 2, 3, 4, 1);
            

            Grid addUser = new Grid();
            addUser.ColumnHomogeneous = true;
            addUser.Margin = 15;
            addUser.RowSpacing = 10;
            addInsertID.PlaceholderText = "사용자의 ID를 스캔 혹은 입력해 주세요";
            addInsertGrade.PlaceholderText = "사용자의 학년을 입력해 주세요";
            addInsertClass.PlaceholderText = "사용자의 반을 입력해 주세요";
            addInsertNumber.PlaceholderText = "사용자의 번호를 입력해 주세요";
            addInsertName.PlaceholderText = "사용자의 이름을 입력해 주세요";            

            insertUser.Clicked += insertUserClicked;
            addIsTeacher.Clicked += delegate { ifTeacher(title.add); };
            
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
            

            selectMode.AppendPage(check, new Label("체크"));
            selectMode.AppendPage(uncheck, new Label("체크 해제"));
            selectMode.AppendPage(addUser, new Label("사용자 추가"));
            
            
            ScrolledWindow scroll = new ScrolledWindow();
            scroll.Add(log);

            grid.RowHomogeneous = true;
            grid.Attach(selectMode, 1, 1, 1, 1);
            grid.Attach(scroll, 2, 1, 1, 1);
            
            Add(grid);

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

using System;
using Gtk;


namespace CovidCheckClientGui
{
    partial class Program : Window
    {
        ListBox log = new ListBox();
        Entry checkInsertID = new Entry();
        Entry uncheckInsertID = new Entry();
        Button checkOK = new Button("체크하기");
        Button uncheckOK = new Button("체크 해제하기");

        public Program() : base("코로나19 예방용 발열체크 프로그램")
        {
            DeleteEvent += delegate {Application.Quit();};
            
            SetDefaultSize(1280, 720);
            
            Grid grid = new Grid();
            grid.Margin = 20;
            grid.ColumnHomogeneous = true;
            grid.ColumnSpacing = 8;
            Notebook selectMode = new Notebook();
            

            addLog("프로그램이 시작됨");
            addLog("로그 테스트를 해 봅시다");
            


            Label checkNotice = new Label("선생님의 바코드를 스캔 혹은 입력할 때는 입력 후 확인 버튼을 눌러주세요.");
            Grid check = new Grid();            
            check.ColumnHomogeneous = true; //창의 크기가 달라지면 알아서 위젯 크기 조절해줌
            check.RowSpacing = 10; //Row는 위아래
            check.ColumnSpacing = 10; //Column은 양 옆
            check.Margin = 15;

            check.Attach(checkNotice, 1, 1, 5, 1); // 공지 추가
            check.Attach(checkInsertID, 1, 2, 4, 1); // 텍스트박스 추가
            check.Attach(checkOK, 5, 2, 1, 1); //OK 버튼 추가



            Grid uncheck = new Grid();    
            Label uncheckNotice = new Label("선생님의 바코드를 스캔 혹은 입력할 때는 입력 후 확인 버튼을 눌러주세요.");        
            uncheck.ColumnHomogeneous = true; //창의 크기가 달라지면 알아서 위젯 크기 조절해줌
            uncheck.RowSpacing = 10; //Row는 위아래
            uncheck.ColumnSpacing = 10; //Column은 양 옆
            uncheck.Margin = 15;

            uncheck.Attach(uncheckNotice, 1, 1, 5, 1); // 공지 추가
            uncheck.Attach(uncheckInsertID, 1, 2, 4, 1); // 텍스트박스 추가
            uncheck.Attach(uncheckOK, 5, 2, 1, 1); //OK 버튼 추가

            selectMode.AppendPage(check, new Label("체크"));
            selectMode.AppendPage(uncheck, new Label("체크 해제"));

            grid.Attach(selectMode, 1, 1, 1, 1);
            grid.Attach(log, 2, 1, 1, 1);

            Add(grid);
            ShowAll();
        }

        public void addLog(string text)
        {
            DateTime dt = DateTime.Now;
            string time = $" ({dt.Hour}:{dt.Minute}:{dt.Second})";
            log.Insert(new Label(text + time), 0);
        }
    }
}

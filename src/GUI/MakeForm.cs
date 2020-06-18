using System;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Net;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System.IO.Compression;
using System.Threading.Tasks;

using Gtk;
using Newtonsoft.Json.Linq;
using System.Text;

namespace CovidCheckClientGui
{
    partial class Program : Window
    {
        JObject settingJson = new JObject(); //설정 JSON
        const string settingPath = "config.json"; //설정 파일 경로

        Grid grid = new Grid(); //전체(비밀번호 제외)를 감싸는 Grid


        //왼쪽의 탭들 (체크, 체크 해제, 추가 등등)
        Notebook selectMode = new Notebook();
        
        ScrolledWindow scroll = new ScrolledWindow(); //로그를 위한 ScrolledWindow
        ScrolledWindow timeoutLogScroll = new ScrolledWindow(); //재시도 로그를 위한 ScrolledWindow


        // 오른쪽에 뜨는 로그
        ListBox log = new ListBox(); //log (일반)
        ListBox timeoutLog = new ListBox(); //재시도 로그 (재시도를 한 적이 없으면 보이지 않음)
        bool hasTimeout = false;

        // 사용자 체크
        Entry checkInsertID = new Entry(); //(체크) 사용자의 ID를 입력하는 Entry
        Scale checkIDLength; //사용자의 ID의 길이를 조정하는 것 (이 스케일에서와 checkInsertID.Text.Length값이 같으면 자동으로 체크)

        Entry checkInsertGrade = new Entry(); //(체크) 사용자의 학년을 입력하는 Entry
        Entry checkInsertClass = new Entry(); //(체크) 사용자의 반을 입력하는 Entry
        Entry checkInsertNumber = new Entry(); //(체크) 사용자의 번호를 입력하는 Entry
        CheckButton checkIsTeacher = new CheckButton("학생이 아님"); //(체크) 대상이 학생이 아닐 때 체크하는 체크버튼 (0학년 0반 판정)

        Button checkOK = new Button("정상 체크하기"); //(체크) ID 사용시 누르는 버튼 (미리 설정한 값과 같으면 이 버튼 없어도 자동으로 체크)
        Button checkInsertUser = new Button("정상 체크하기"); //(체크) 학년, 반, 번호를 사용시 누르는 버튼



        // ====================== 발열체크 =======================
        Entry checkDoubtInsertID = new Entry();
        Entry checkDoubtInsertGrade = new Entry();
        Entry checkDoubtInsertClass = new Entry();
        Entry checkDoubtInsertNumber = new Entry();
        CheckButton checkDoubtIsTeacher = new CheckButton("학생이 아님");

        Button checkDoubtOK = new Button("발열 체크하기");
        Button checkDoubtInsertUser = new Button("발열 체크하기");

        
        
        // ===================== 사용자 체크 해제 ==========================
        Entry uncheckInsertID = new Entry();
        Scale uncheckIDLength = new Scale(Orientation.Horizontal, new Adjustment(8, 5, 10, 0, 1, 0));

        Entry uncheckInsertGrade = new Entry();
        Entry uncheckInsertClass = new Entry();
        Entry uncheckInsertNumber = new Entry();
        CheckButton uncheckIsTeacher = new CheckButton("학생이 아님");

        Button uncheckOK = new Button("체크 해제하기");
        Button uncheckInsertUser = new Button("체크 해제하기");
        



        //================== 사용자 추가 ===================
        Entry addInsertID = new Entry(); //(추가) ID를 입력하는 Entry
        Entry addInsertGrade = new Entry(); //(추가) 학년을 입력하는 Entry
        Entry addInsertClass = new Entry(); //(추가) 반을 입력하는 Entry
        Entry addInsertNumber = new Entry(); //(추가) 번호를 입력하는 Entry
        Entry addInsertName = new Entry(); //(추거) 이름을 입력하는 Entry
        CheckButton addIsTeacher = new CheckButton("학생이 아님"); //(추가) 사용자가 학생이 아닐 때(예: 선생님) 체크하는 CheckButton (0학년 0반 처리)
        Button insertUser = new Button("사용자 만들기"); //(추가) 모든 정보를 입력하고 작업을 시작하는 버튼


        //=================== 사용자 삭제 ====================
        Entry delInsertID = new Entry(); //(삭제) ID로 삭제할 때 입력하는 Entry
        Entry delInsertGrade = new Entry(); //(삭제) 학년, 반, 번호로 삭제할 때 학년을 입력하는 Entry
        Entry delInsertClass = new Entry(); //(삭제) 학년, 반, 번호로 삭제할 때 반을 입력하는 Entry
        Entry delInsertNumber = new Entry(); //(삭제) 학년, 반, 번호로 삭제할 때 번호를 입력하는 Entry
        CheckButton delIsTeacher = new CheckButton("학생이 아님"); //(삭제) 학년, 반, 번호로 삭제할 때 학생이 아닐 때(예: 선생님) 체크하는 CheckButton (0학년 0반 처리)
        Button delInsertUser = new Button("사용자 삭제"); //(삭제) ID를 입력하고 작업을 시작하는 버튼 (삭제는 자동으로 실행되지 않음)
        Button delInsertUserWithoutID = new Button("사용자 삭제"); //(삭제) 학년, 반, 번호로 삭제할 때 모든 정보를 입력하고 작업을 시작하는 버튼


        //==================== 체크 상황 보기 ===================
        bool programProcessing = true; //이 값이 false면 루프가 끝나면서 체크 상황을 가져오지 않음
        Label[] userCount = new Label[4] { //사용자 수만 볼 때 사용하는 배열 (학년이 3개일 때 기준)
                                           //[0, ]: 1학년, [1, ]: 2학년, [2, ]: 3학년, [3, ]: 기타
                new Label(""),
                new Label(""),
                new Label(""),
                new Label("")
        };
        Label allUserCount = new Label(""); //전체 사용자 수를 보여주는 Label

        Label time = new Label(""); //현재 시각을 보여주는 Label

        CheckButton seeMoreInfo = new CheckButton("상세정보 보기"); //상세정보(체크 현황) 을 볼 건지 아니면 사용자 수만 볼 건지 결정하는 CheckButton
        Frame[] statusListFrame = new Frame[2] { //상세정보를 보지 않을 때, 상세 정보를 볼 때 Frame
            new Frame("사용자 수 (3초마다 새로고침)"), new Frame("검사 현황 (3초마다 새로고침)")
        };
        ProgressBar[,] statusProgressBar = new ProgressBar[4, 3] { //상세정보를 볼 때 사용하는 ProgressBar들 (학년과 검사 현황을 위해 2차원 배열)
            //[0, ]: 1학년, [1, ]: 2학년, [2, ]: 3학년, [3, ]: 기타, [, 0]: 미검사, [, 1]: 검사, [, 2]: 발열
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

        //======================= 설정 =========================
        SpinButton helpSet; //타임아웃시 재시도 횟수를 설정할 때 오른쪽에 있는 것 (세세하게 조절할 때 도와주는 역할)


        
        
        public Program() : base("코로나19 예방용 발열체크 프로그램")
        {
            addLog("프로그램이 시작됨");
            programProcessing = true; //아래에 있는 루프 도는 스레드들 일하도록 true 설정
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
            StyleContext.AddProviderForScreen(Gdk.Screen.Default, cssProvider, 800); //CSS 적용

            if (doneUpdate) //(자동으로) 업데이트를 했다면
            {
                MessageDialog dialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Error, ButtonsType.Close, false, "새로운 버전으로 업데이트를 완료했습니다!");
                    dialog.Run();
                    dialog.Dispose();
            }

            user = new CheckCovid19.User("localhost"); //User 선언
            try
            {
                settingJson = user.loadSetting(settingPath); //설정 JSON파일을 가져와봄
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
                }"; //기본 JSON 세팅
                user.saveSetting(defaultSetting, settingPath); //기본 JSON 저장
                settingJson = JObject.Parse(defaultSetting);
            }

            user.url = settingJson["url"].ToString(); //설정 속 url 입력

            helpSet = new SpinButton(new Adjustment((double)settingJson["timeoutRetry"], 0, 500, 1, 1, 0), 1, 0);
            checkIDLength = new Scale(Orientation.Horizontal, new Adjustment((double)settingJson["barcodeLength"], 5, 10, 0, 1, 0));
            uncheckIDLength = new Scale(Orientation.Horizontal, new Adjustment((double)settingJson["barcodeLength"], 5, 10, 0, 1, 0));


            DeleteEvent += delegate {
                programProcessing = false;
                Application.Quit();
            }; //이 창이 삭제되면 프로그램 종료

            SetDefaultSize(1450, 850); //기본 창 사이즈 (권장)
            
            // 전체를 감싸는 Grid의 속성 설정
            grid.Margin = 20;
            grid.ColumnHomogeneous = true; //위젯들을 가로로 잡아당길거냐?
            grid.ColumnSpacing = 8; //각 셀의 간격


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
                checkIDLength.ValueChanged += delegate {
                    settingJson["barcodeLength"] = checkIDLength.Value;
                    user.saveSetting(settingJson.ToString(), settingPath);
                };
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

            Grid checkAll = new Grid(); //check하는 Frame들 배치
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
                uncheckIDLength.ValueChanged += delegate {
                    settingJson["barcodeLength"] = uncheckIDLength.Value;
                    user.saveSetting(settingJson.ToString(), settingPath);
                };
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

            //사용자 삭제 Grid
            Frame delUserFrame = new Frame("사용자 삭제");
            Grid delUser = new Grid();
            {
                //위젯 속성 설정
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

                //위젯 이벤트 설정
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

                //위젯들 배치
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
            

            Grid statusList = new Grid(); //사용자 수만 알려주는거
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
            
            Grid statusListMore = new Grid(); //검사 현황까지 알려주는거
            {
                statusListMore.RowSpacing = 10;
                statusListMore.Margin = 15;
                statusListMore.ColumnHomogeneous = true;
                foreach (var a in statusProgressBar) //값을 받아오기 전엔 로딩
                {
                    a.ShowText = true;
                    a.Text = "로딩...";
                }
                for (int i = 0; i < 4; i++) //ProgressBar에 색깔 입히기
                {
                    statusProgressBar[i, 0].Name = "gray";
                    statusProgressBar[i, 1].Name = "green";
                    statusProgressBar[i, 2].Name = "red";
                }
                
                //배치
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
            }

            
            

            ScrolledWindow scroll2 = new ScrolledWindow();
            Grid manageMode = new Grid(); //사용자 설정 Grid
            {
                //속성 설정
                manageMode.RowSpacing = 10;
                manageMode.ColumnSpacing = 10;
                manageMode.ColumnHomogeneous = true;
                statusListFrame[0].MarginBottom = 0;
                statusListFrame[1].MarginBottom = 0;
                seeMoreInfo.MarginStart = 10;

                //CheckButton 이벤트
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

                //배치
                manageMode.Attach(addUserFrame, 1, 1, 1, 2);
                manageMode.Attach(delUserFrame, 1, 3, 1, 2);
                manageMode.Attach(statusListFrame[0], 1, 5, 1, 1);
                manageMode.Attach(seeMoreInfo, 1, 6, 1, 1);
            
                scroll2.Add(manageMode);
            }
            
            Grid setting = new Grid(); //설정 Grid
            {
                Dictionary<string, Grid> grids = new Dictionary<string, Grid>(); //Grid가 꽤나 필요해서 Dict로 묶음

                //Frame들은 설정할 거 없으니 그냥 바로
                Frame setUrlFrame = new Frame("URL 설정");
                Frame setTimeoutRetryFrame = new Frame("타임아웃 재시도 횟수 설정");
                Frame setUpdateCheckFrame = new Frame("업데이트 설정");
                Frame setPasswordFrame = new Frame("비밀번호 설정");
                Frame getSettingFrame = new Frame("설정 파일 불러오기");

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
                    setting.Attach(getSettingFrame, 1, 4, 1, 1);
                    setting.Attach(setPasswordFrame, 1, 5, 1, 1);
                }                

                grids.Add("setUrl", new Grid()); //URL 설정하는 곳
                {
                    Label label = new Label("http://, https://"); //http://, https://같은거 입력하지 말라는 거
                    Entry url = new Entry(); //url 입력하는 곳
                    {
                        //속성 설정
                        url.PlaceholderText = "웹 사이트의 URL을 입력하세요.";
                        label.Halign = Align.End;
                        url.Text = settingJson["url"].ToString();
                        grids["setUrl"].ColumnSpacing = 10;
                    }
                    {
                        url.KeyReleaseEvent += async delegate {
                            string now = url.Text;
                            await Task.Delay(500); //계속 저장하진 않고 입력이 완료되었을 것 같을 때만
                            if (now == url.Text)
                            {
                                settingJson["url"] = url.Text;
                                user.url = url.Text;
                                user.saveSetting(settingJson.ToString(), settingPath);
                            }
                        };
                    }
                    {
                        grids["setUrl"].Attach(label, 1, 1, 1, 1);
                        grids["setUrl"].Attach(url, 2, 1, 5, 1);
                    }
                    setUrlFrame.Add(grids["setUrl"]);                    
                }

                grids.Add("setTimeoutRetry", new Grid()); //타임아웃시 재시도 횟수 설정하는 곳
                {
                    Scale time = new Scale(Orientation.Horizontal, new Adjustment((double)settingJson["timeoutRetry"], 0, 500, 0, 1, 0)); //0 ~ 500 사이

                    {
                        //속성 설정
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
                        helpSet.ValueChanged += async delegate {
                            time.Value = helpSet.Value;
                            double now = helpSet.Value;
                            await Task.Delay(100); //얘는 값이 빨리빨리 바뀔 것 같으니 0.1초
                            if (now == helpSet.Value)
                            {
                                settingJson["timeoutRetry"] = time.Value;
                                user.saveSetting(settingJson.ToString(), settingPath);
                            }
                        };

                    }
                    setTimeoutRetryFrame.Add(grids["setTimeoutRetry"]);
                }

                grids.Add("setUpdateCheck", new Grid()); //업데이트 설정
                {
                    Gtk.Switch checkUpdate = new Gtk.Switch(); //업데이트 체크 여부 (Switch가 이미 있으니...)
                    Gtk.Switch autoUpdate = new Gtk.Switch(); //자동 업데이트 여부
                    {
                        checkUpdate.StateChanged += delegate {
                            autoUpdate.Sensitive = checkUpdate.State;
                            settingJson["checkUpdate"] = checkUpdate.State; //여긴 뭐 딜레이 넣을 필요 없겠지
                            user.saveSetting(settingJson.ToString(), settingPath);
                        };

                        autoUpdate.StateChanged += delegate {
                            settingJson["autoUpdate"] = autoUpdate.State;
                            user.saveSetting(settingJson.ToString(), settingPath);
                        };
                    }

                    {
                        checkUpdate.State = (bool)settingJson["checkUpdate"];
                        autoUpdate.State = (bool)settingJson["autoUpdate"];
                        autoUpdate.Sensitive = (bool)settingJson["checkUpdate"];
                    }

                    {
                        grids["setUpdateCheck"].Attach(new Label("프로그램을 킬 때마다 업데이트를 확인하기"), 1, 1, 5, 1);
                        Grid checkgrid = new Grid();
                        checkgrid.Add(checkUpdate);
                        grids["setUpdateCheck"].Attach(checkgrid, 6, 1, 1, 1);

                        Grid autogrid = new Grid();
                        autogrid.Add(autoUpdate);
                        grids["setUpdateCheck"].Attach(new Label("업데이트 확인시 자동으로 업데이트하기"), 1, 2, 5, 1);
                        grids["setUpdateCheck"].Attach(autogrid, 6, 2, 1, 1);
                    }
                    setUpdateCheckFrame.Add(grids["setUpdateCheck"]);

                }

                grids.Add("getSetting", new Grid()); //설정 파일 가져오기
                {
                    Entry filePath = new Entry("파일을 선택하세요.");
                    Button getFile = new Button("파일 불러오기");
                    FileChooserDialog fileChooser = new FileChooserDialog("설정 파일 불러오기", null, FileChooserAction.Open, "불러오기", ResponseType.Accept);
                    FileFilter filter = new FileFilter();
                    filter.AddPattern("*.json"); //json파일만
                    fileChooser.Filter = filter;
                    {
                        getFile.Clicked += delegate {
                            if (fileChooser.Run() == -3)
                            {
                                filePath.Text = fileChooser.Filename;
                                try
                                {
                                    user.loadSetting(fileChooser.Filename); //파일 가져오기
                                }
                                catch
                                {
                                    MessageDialog dialog = new MessageDialog(null, DialogFlags.DestroyWithParent, MessageType.Info, ButtonsType.Ok, false, "잘못된 설정 파일입니다. 설정 값이 변하지 않습니다."); //설정 파일 잘못된거면 빠꾸
                                    dialog.Run();
                                    dialog.Dispose();
                                    return;
                                }
                                
                                JObject newSetting = user.loadSetting(fileChooser.Filename); //제대로 된 놈이면 로드
                                if (newSetting.ContainsKey("url") && newSetting.ContainsKey("barcodeLength") && newSetting.ContainsKey("timeoutRetry") && newSetting.ContainsKey("checkUpdate") && newSetting.ContainsKey("autoUpdate") && newSetting.ContainsKey("usePassword") && newSetting.ContainsKey("password")) //원하는 값이 다 있으면 적용
                                {
                                    File.Copy(fileChooser.Filename, "./config.json", true);
                                    MessageDialog dialog = new MessageDialog(null, DialogFlags.DestroyWithParent, MessageType.Info, ButtonsType.Ok, false, "설정 파일을 불러왔습니다. 프로그램을 다시 시작합니다.");
                                    dialog.Run();
                                    dialog.Dispose();
                                    programProcessing = false;
                                    base.Close();
                                    Program.Main(new string[0]);
                                }
                                else
                                {
                                    MessageDialog dialog = new MessageDialog(null, DialogFlags.DestroyWithParent, MessageType.Info, ButtonsType.Ok, false, "잘못된 설정 파일입니다. 설정 값이 변하지 않습니다.");
                                    dialog.Run();
                                    dialog.Dispose();
                                }
                            }
                            fileChooser.Dispose();
                            fileChooser = new FileChooserDialog("설정 파일 불러오기", null, FileChooserAction.Open, "불러오기", ResponseType.Accept);
                            fileChooser.Filter = filter;
                        };
                    }

                    {
                        filePath.IsEditable = false;
                        fileChooser.SelectMultiple = false;
                    }
                    {
                        grids["getSetting"].Attach(filePath, 1, 1, 4, 1);
                        grids["getSetting"].Attach(getFile, 5, 1, 1, 1);
                    }
                    getSettingFrame.Add(grids["getSetting"]);

                }
                
                grids.Add("setPassword", new Grid()); //비밀번호 설정
                {
                    Entry setEnterPassword = new Entry();
                    Button usePassword = new Button("비밀번호 설정하기");
                    Button showPassword = new Button("보기");
                    {
                        grids["setPassword"].RowSpacing = 5;
                        grids["setPassword"].ColumnSpacing = 5;
                        grids["setPassword"].Margin = 5;
                        setEnterPassword.PlaceholderText = "비밀번호를 입력하세요.";
                        setEnterPassword.Valign = Align.Center;
                        setEnterPassword.Visibility = false;
                    }
                    {
                        grids["setPassword"].Attach(setEnterPassword, 1, 1, 4, 2);
                        grids["setPassword"].Attach(usePassword, 5, 1, 1, 1);
                        grids["setPassword"].Attach(new Label(), 2, 1, 4, 1);
                        grids["setPassword"].Attach(showPassword, 5, 2, 1, 1);
                    }
                    {
                        showPassword.Entered += delegate { //마우스 커서 올려뒀을 때
                            setEnterPassword.Visibility = true;
                            setEnterPassword.IsEditable = false;
                        };
                        showPassword.LeaveNotifyEvent += delegate { //마우스 커서 땔 때
                            setEnterPassword.Visibility = false; //텍스트를 못 보도록 함
                            setEnterPassword.IsEditable = true;
                        };
                        usePassword.Clicked += delegate {
                            MessageDialog done = null;
                            if (setEnterPassword.Text == "") //텍스트가 없으면 비밀번호 사용 안함
                            {
                                settingJson["usePassword"] = false;
                                user.saveSetting(settingJson.ToString(), settingPath);
                                done = new MessageDialog(null, DialogFlags.DestroyWithParent, MessageType.Info, ButtonsType.Ok, false, "비밀번호 설정 (비밀번호 사용 안함)이 완료되었습니다.");
                            }
                            else
                            {
                                settingJson["usePassword"] = true;
                                settingJson["password"] = user.getSha512(setEnterPassword.Text);
                                user.saveSetting(settingJson.ToString(), settingPath);
                                done = new MessageDialog(null, DialogFlags.DestroyWithParent, MessageType.Info, ButtonsType.Ok, false, $"비밀번호 설정 (비밀번호: {setEnterPassword.Text})이 완료되었습니다.");
                            }
                            done.Run();
                            done.Dispose();                            
                        };
                    }
                    setPasswordFrame.Add(grids["setPassword"]);
                }
                
                foreach (var a in grids) //설정 Grid에 공통으로 적용되는 것
                {
                    a.Value.ColumnHomogeneous = true;
                    a.Value.Margin = 10;
                    a.Value.MarginTop = 0;
                }
            }

            Grid toDev = new Grid(); //개발자들에게 건의하거나 할 때 쓰는거
            {
                toDev.ColumnHomogeneous = true;
                toDev.RowSpacing = 10;
                toDev.Margin = 10;

                Grid toClientDev = new Grid();
                //GitHub Repo: https://github.com/SoftWareAndGuider/Covid-Check-Client
                //GitHub Issue: https://github.com/SoftWareAndGuider/Covid-Check-Client/issues/new
                Grid toServerDev = new Grid();
                //(Server) GitHub Repo: https://github.com/SoftWareAndGuider/Covid-Check
                //(Server) GitHub Issue: https://github.com/SoftWareAndGuider/Covid-Check/issues/new
                Frame toClient = new Frame("클라이언트 개발자 (csnewcs)");
                Frame toServer = new Frame("서버 개발자 (pmh-only, Noeul-Night)");

                Button clientRepo = new Button("클라이언트 프로그램의 소스코드 보러 가기");
                Button serverRepo = new Button("서버 프로그램의 소스코드 보러 가기");

                Button clientIssue = new Button("클라이언트 개발자에게 건의하기");
                Button serverIssue = new Button("서버 개발자에게 건의하기");

                Button downloadOld = new Button("클라이언트 프로그램의 릴리즈 보기");

                toClientDev.ColumnSpacing = 10;
                toClientDev.Margin = 10;
                toServerDev.ColumnSpacing = 10;
                toServerDev.Margin = 10;

                toClientDev.Attach(clientRepo, 1, 1, 1, 1);
                toClientDev.Attach(clientIssue, 2, 1, 1, 1);
                toClientDev.Attach(downloadOld, 3, 1, 1, 1);
                toClient.Add(toClientDev);

                toServerDev.Attach(serverRepo, 1, 1, 1, 1);
                toServerDev.Attach(serverIssue, 2, 1, 1, 1);
                toServer.Add(toServerDev);

                toDev.Attach(toClient, 1, 1, 1, 1);
                toDev.Attach(toServer, 1, 2, 1, 1);

                {
                    clientRepo.Clicked += delegate {
                        try 
                        {
                            Process.Start("https://github.com/SoftWareAndGuider/Covid-Check-Client");
                        }
                        catch 
                        {
                            ProcessStartInfo pr = new ProcessStartInfo("https://github.com/SoftWareAndGuider/Covid-Check-Client"); //리눅스
                            pr.UseShellExecute = true;
                            Process.Start(pr);
                        }
                    };
                    serverRepo.Clicked += delegate {
                        try 
                        {
                            Process.Start("https://github.com/SoftWareAndGuider/Covid-Check");
                        }
                        catch 
                        {
                            ProcessStartInfo pr = new ProcessStartInfo("https://github.com/SoftWareAndGuider/Covid-Check");
                            pr.UseShellExecute = true;
                            Process.Start(pr);
                        }
                    };
                    clientIssue.Clicked += delegate {
                        try 
                        {
                            Process.Start("https://github.com/SoftWareAndGuider/Covid-Check-Client/issues/new");
                        }
                        catch 
                        {
                            ProcessStartInfo pr = new ProcessStartInfo("https://github.com/SoftWareAndGuider/Covid-Check-Client/issues/new");
                            pr.UseShellExecute = true;
                            Process.Start(pr);
                        }
                    };
                    clientRepo.Clicked += delegate {
                        try 
                        {
                            Process.Start("https://github.com/SoftWareAndGuider/Covid-Check/issues/new");
                        }
                        catch 
                        {
                            ProcessStartInfo pr = new ProcessStartInfo("https://github.com/SoftWareAndGuider/Covid-Check/issues/new");
                            pr.UseShellExecute = true;
                            Process.Start(pr);
                        }
                    };
                    downloadOld.Clicked += delegate {
                        try 
                        {
                            Process.Start("https://github.com/SoftWareAndGuider/Covid-Check-Client/releases");
                        }
                        catch 
                        {
                            ProcessStartInfo pr = new ProcessStartInfo("https://github.com/SoftWareAndGuider/Covid-Check-Client/releases");
                            pr.UseShellExecute = true;
                            Process.Start(pr);
                        }
                    };
                }
            }
            
            //Grid들 Notebook에 추가
            selectMode.AppendPage(checkAll, new Label("체크"));
            selectMode.AppendPage(uncheck, new Label("체크 해제"));
            selectMode.AppendPage(scroll2, new Label("사용자 관리"));
            selectMode.AppendPage(setting, new Label("설정"));
            selectMode.AppendPage(toDev, new Label("개발자들에게"));
            
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

            
            //이제 보여주기
            if ((bool)settingJson["usePassword"]) //비밀번호를 사용한다면
            {
                Grid usePassword = new Grid(); //모양 잡기
                usePassword.Margin = 15;
                usePassword.ColumnSpacing = 10;
                usePassword.RowSpacing = 10;
                usePassword.RowHomogeneous = true;
                usePassword.Halign = Align.Center;
                
                Label notice = new Label("비밀번호를 입력하고 확인 버튼을 눌러주세요.");
                notice.Valign = Align.End;
                
                Entry enterPassword = new Entry();
                enterPassword.PlaceholderText = "비밀번호를 입력하세요.";
                enterPassword.Visibility = false;
                enterPassword.Valign = Align.Start;

                Button enter = new Button("입력");
                enter.Valign = Align.Start;

                usePassword.Attach(notice, 1, 1, 5, 1);
                usePassword.Attach(enterPassword, 1, 2, 3, 1);
                usePassword.Attach(enter, 4, 2, 2, 1);

                Add(usePassword);
                ShowAll();

                enter.Clicked += delegate {
                    if (user.getSha512(enterPassword.Text) == settingJson["password"].ToString()) //비밀번호가 맞다면
                    {
                        Remove(usePassword); //이거 지우고

                        //창에 추가
                        Add(grid);
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
                    }
                    else //비밀번호가 틀렸다면
                    {
                        MessageDialog dialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Error, ButtonsType.Ok, false, "틀렸습니다. 다시 시도해주세요.");
                        dialog.Run();
                        dialog.Dispose();
                    }
                };
            }
            else //비밁번호를 사용하지 않는다면
            {
                //창에 추가
                Add(grid);
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
            }


            addLog("프로그램 로딩이 완료됨");

            //=========== 업데이트 확인 ===============
            if ((bool)settingJson["checkUpdate"] && !doneUpdate) //업데이트를 체크하고 방금 업데이트를 하지 않았다면
            {
                JArray update = new JArray();
                if (user.hasNewVersion(2, out update)) //신버전 확인
                {
                    if ((bool)settingJson["autoUpdate"]) //자동 업데이트가 켜져있다면
                    {
                        WebClient client = new WebClient();
                        client.Encoding = System.Text.Encoding.UTF8;
                        client.Headers.Add("user-agent", "CovidCheckClientCheckUpdate");
                        JArray files = update.First()["assets"] as JArray;

                        foreach (var file in files)
                        {
                            if (file["name"].ToString() == "Default-Version.zip")
                            {
                                client.DownloadFile(file["browser_download_url"].ToString(), "update.zip"); //god Github api
                                break;
                            }
                        }
                        ZipFile.ExtractToDirectory("./update.zip", "./", true); //압축을 풀고
                        ZipFile.ExtractToDirectory("./GUI.zip", "./", true);
                        Directory.CreateDirectory("update"); //업데이트 파일들 집어넣을 폴더


                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) //리눅스 (프로그램이 켜져 있어도 파일 복사 가능, ProcessStartInfo.UseShellExecute = false면 실행 불가)
                        {
                            ZipFile.ExtractToDirectory("./linux-x64.zip", "./update", true);
                            ProcessStartInfo info = new ProcessStartInfo("update/CovidCheckClientGui", "update linux");
                            info.UseShellExecute = true;
                            Process.Start("chmod", "777 update");
                            Process.Start("chmod", "777 update/CovidCheckClientGui"); //실행 가능하도록 해주고
                            Thread.Sleep(1000);
                            Process process = new Process();
                            process.StartInfo = info;
                            process.Start();

                            Environment.Exit(0);
                        }
                        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) //윈도우 (프로그램이 켜져 있으면 파일 복사 불가능)
                        {
                            ZipFile.ExtractToDirectory("./win-x64.zip", "./update", true);
                            Directory.CreateDirectory("files");

                            DirectoryInfo updateDictInfo = new DirectoryInfo("update");
                            foreach (var file in updateDictInfo.GetFiles()) //파일 복사
                            {
                                file.CopyTo("files/" + file.Name, true);
                            }

                            ProcessStartInfo info = new ProcessStartInfo("files/CovidCheckClientGui.exe", "update windows");
                            info.WorkingDirectory = "./update";
                            Process.Start(info);
                            Environment.Exit(0);
                        }
                    }
                    else //자동 업데이트 기능이 꺼져있다면
                    {
                        string name = update.First()["name"].ToString();
                        MessageDialog dialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Info, ButtonsType.Close, true, $"프로그램의 새 버전({name})을 찾았습니다. <a href=\"https://github.com/SoftWareAndGuider/Covid-Check-Client/releases\">여기를 눌러</a> 확인해 주세요.");
                        dialog.Run();
                        dialog.Dispose();
                    }
                }                
            }
        }


        private void getStatus() //학생들 상황 가져오는 메서드
        {
            while (programProcessing) //프로그램 꺼질 때 까지 루프
            {
                long ping = user.getPing(); //핑
                Application.Invoke(delegate {
                    if (ping == -1)
                    {
                        base.Title = $"코로나19 예방용 발열체크 프로그램 (통신 속도: 알 수 없음)";
                    }
                    else
                    {
                        base.Title = $"코로나19 예방용 발열체크 프로그램 (통신 속도: {ping}ms)";
                    }
                });


                try
                {
                    int err = 0;
                    JObject uploadString = JObject.Parse(@"{""process"":""info"", ""multi"":true}"); //PUT할 string
                    JObject result = user.upload(uploadString, out err);

                    StatusParsing sp = new StatusParsing();
                    if (seeMoreInfo.Active) //검사 현황을 볼 때
                    {
                        double[,] parse = sp.moreInfo(result);
                        double[] allUsers = new double[4] {
                            parse[0, 0] + parse[0, 1] + parse[0, 2],
                            parse[1, 0] + parse[1, 1] + parse[1, 2],
                            parse[2, 0] + parse[2, 1] + parse[2, 2],
                            parse[3, 0] + parse[3, 1] + parse[3, 2]
                        };
                        Application.Invoke(delegate {
                            //text 설정
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
                                if (allUsers[i] == 0) allUsers[i] = 1; //0으로 나눌 수 없음
                            }
                            //실제 값 설정
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
                    else //사람 수만 볼 때
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
                    
                }
                catch
                {
                }
                Thread.Sleep(3000); //3초마다 새로고침
            }
        }
        
        private void timer() //시계
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
        public void addLog(string text) //로그 추가
        {
            if (last == text) return;
            last = text;
            string storeTime = time.Text;
            if (string.IsNullOrEmpty(storeTime))
            {
                DateTime dt = DateTime.Now;
                storeTime = $"{dt.Month}월 {dt.Day}일 {dt.Hour}:{dt.Minute}:{dt.Second}";
            }
            
            logLabel.StyleContext.RemoveClass("nowlog"); //일단 기존 로그에서 nowlog 클래스 지우고 (=배경색 지우고)

            logLabel = new Label($"{text} ({storeTime})");

            logLabel.StyleContext.AddClass("nowlog"); //새 로그에 nowlog 클래스 추가 (=배경색 추가)
            
            logLabel.Name = "add";
            log.Insert(logLabel, 0);
            log.ShowAll();
        }

        Label timeoutLogLabel = new Label();
        private void addTimeoutLog(string text) //재시도 로그 추가
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

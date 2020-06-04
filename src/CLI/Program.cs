using System;
using System.Net;
using System.Text;
using System.Linq;
using CheckCovid19;
using Newtonsoft.Json.Linq;

namespace Covid_Check_Client
{
    class Program
    {
        static User user = new User(System.IO.File.ReadAllLines("config.txt")[0]);
        static void Main(string[] args)
        {
            long p = user.getPing();
            Console.Title = $"코로나19 예방용 발열체크 프로그램 (CLI)(통신 속도: {p}ms)";
            Program program = new Program();
            Console.WriteLine("CovidCheckClient, GPLv2 License\nCopyright (c) 2020 JanggokSWAG, 자세한 저작권 관련 사항과 이 프로그램의 소스코드는 https://github.com/softwareandguider/covid-check-client 에서 확인해주세요.\n");
            Console.WriteLine("서버와의 통신 속도: " + p + "ms");

            string verName = "";
            if (user.hasNewVersion(1, out verName))
            {
                Console.WriteLine("새로운 버전 {0}이(가) 출시되었습니다. https://github.com/SoftWareAndGuider/Covid-Check-Client/releases/ 에서 확인해 주세요.\n", verName);
            }
            string change = "1"; //일단 프로그램을 켰을 땐 체크모드
            while (true)
            {
                switch (change)
                {
                    case "1":
                        change = program.check();
                        break;
                    case "2":
                        change = program.ondoCheck();
                        break;
                    case "3":
                        change = program.uncheck();
                        break;
                    case "4":
                        change = program.add();
                        break;
                    case "5":
                        change = program.remove();
                        break;
                    case "6":
                        change = program.checkWithoutID();
                        break;
                    case "7":
                        change = program.ondoCheckWithoutID();
                        break;
                    case "8":
                        change = program.uncheckWithoutID();
                        break;
                    case "9":
                        change = program.removeWithoutID();
                        break;
                    default:
                        Console.WriteLine("오류 기본 체크모드로 전환");
                        change = program.check();
                        break;
                }
            }
        }
        string check()
        {
            string change = "0";
            while (true)
            {
                if (first("체크", out change, "체크하려면 사용자의 바코드를 스캔하거나 숫자를 입력하세요.")) return change;

                int err = 0;
                JObject result = user.check(change, out err);
                if ((bool)result["success"])
                {
                    Console.WriteLine($"{result["data"]["name"]}(ID: {result["data"]["id"]})의 체크가 완료되었습니다.\n");
                }
                else
                {
                    Console.WriteLine("체크를 실패하였습니다. 확인 후 다시 시도해주세요.\n");
                }
            }
        }
        string uncheck()
        {
            string change = "0";
            while (true)
            {
                Console.WriteLine("현재는 체크 해제모드 입니다. 모드를 변경하려면 change를, 프로그램 종료는 exit를 입력해 주세요\n체크를 해제하려면 사용자의 바코드를 스캔하거나 숫자를 입력하세요.");
                string scan = Console.ReadLine();
                if (scan == "change") //모드 바꾸기
                {
                    change = changeMode();
                    break;
                }
                else if (scan == "exit") Environment.Exit(0);
                
                int err = 0;
                JObject result = user.uncheck(scan, out err);
                if ((bool)result["success"])
                {
                    Console.WriteLine($"{result["data"]["name"]}(ID: {result["data"]["id"]})의 체크 해제가 완료되었습니다.\n");
                }
                else
                {
                    Console.WriteLine("체크 해제를 실패하였습니다. 확인 후 다시 시도해주세요.\n");
                }
                Console.WriteLine();
            }
            return change;
        }
        string add()
        {
            string change = "0";
            while (true)
            {
                if (first("추가", out change, "추가할 사용자의 ID를 입력하세요.")) return change;
                Console.WriteLine("사용자의 학년을 입력해 주세요.");
                int grade = int.Parse(Console.ReadLine());
                Console.WriteLine("사용자의 반을 입력해 주세요.");
                int @class = int.Parse(Console.ReadLine());
                Console.WriteLine("사용자의 번호를 입력해 주세요.");
                int number = int.Parse(Console.ReadLine());
                Console.WriteLine("사용자의 이름을 입력해 주세요.");
                string name = Console.ReadLine();

                int err = 0;
                JObject result = user.addUser(change, grade, @class, number, name, out err)["data"] as JObject;
                if ((bool)result["success"]) Console.WriteLine($"{result["data"]["grade"]}학년 {result["data"]["class"]}반 {result["data"]["number"]}번 {result["data"]["name"]}(ID: {result["data"]["id"]})사용자가 추가되었습니다." + "\n");
                else Console.WriteLine("사용자 추가에 실패하였습니다. 확인 후 다시 시도해 주세요.");
            }
        }
        string remove()
        {
            while (true)
            {
                string scan = "";
                if (first("삭제", out scan, "사용자를 삭제하려면 사용자의 바코드를 스캔하거나 숫자를 입력하세요.")) return scan; //모드 바꾸기

                int err = 0;
                JObject result = user.delUser(scan, out err);
                if ((bool)result["success"])
                {
                    Console.WriteLine($"{result["data"]["grade"]}학년 {result["data"]["class"]}반 {result["data"]["number"]}번 {result["data"]["name"]}(ID: {result["data"]["id"]})의 삭제가 완료되었습니다.\n");
                }
                else
                {
                    Console.WriteLine("삭제를 실패하였습니다. 확인 후 다시 시도해주세요.\n");
                }
                Console.WriteLine();
            }
        }
        string ondoCheck()
        {
            while (true)
            {
                string scan = "";
                if (first("삭제", out scan, "사용자를 발열 체크하려면 사용자의 바코드를 스캔하거나 숫자를 입력하세요.")) return scan; //모드 바꾸기

                int err = 0;
                JObject result = user.check(scan, out err, true);
                if ((bool)result["success"])
                {
                    Console.WriteLine($"{result["data"]["name"]}(ID: {result["data"]["id"]})의 발열 체크가 완료되었습니다.\n");
                }
                else
                {
                    Console.WriteLine("발열 체크를 실패하였습니다. 확인 후 다시 시도해주세요.\n");
                }
                Console.WriteLine();
            }
        }
        string changeMode()
        {
            string change = "";
            while (true)
            {
                Console.WriteLine("1: 사용자 체크\n2: 사용자 발열 체크\n3: 사용자 체크 해제\n4: 사용자 추가\n5: 사용자 삭제\n6: ID없이 사용자 체크\n7: ID없이 사용자 발열 체크\n8: ID없이 사용자 체크 해제\n9: ID없이 사용자 삭제");
                change = Console.ReadLine();
                string[] lists = new string[] {
                    "1", "2", "3", "4", "5", "6", "7", "8", "9"
                };
                if (lists.Contains(change)) break;
                Console.WriteLine("올바른 번호를 선택해 주세요.");
            }
            Console.WriteLine();
            return change;
        }
        
        
        
        string checkWithoutID()
        {
            string change = "0";
            while (true)
            {
                if (first("ID없이 체크", out change, "사용자의 학년을 입력해 주세요."))
                {
                    return change;
                }
                string[] info = getManyInfo();

                int err = 0;
                JObject result = user.check(change, info[0], info[1], out err);
                if ((bool)result["success"])
                {
                    Console.WriteLine($"{result["data"]["name"]}(ID: {result["data"]["id"]})의 체크가 완료되었습니다.\n");
                }
                else
                {
                    Console.WriteLine("체크를 실패하였습니다. 확인 후 다시 시도해주세요.\n");
                }
            }
        }
        string uncheckWithoutID()
        {
            string change = "0";
            while (true)
            {
                if (first("ID없이 체크 해제", out change, "사용자의 학년을 입력해 주세요"))
                {
                    return change;
                }
                string[] info = getManyInfo();

                int err = 0;
                JObject result = user.uncheck(change, info[0], info[1], out err);
                if ((bool)result["success"])
                {
                    Console.WriteLine($"{result["data"]["name"]}(ID: {result["data"]["id"]})의 체크 해제가 완료되었습니다.\n");
                }
                else
                {
                    Console.WriteLine("체크 해제를 실패하였습니다. 확인 후 다시 시도해주세요\n");
                }
            }
        }
        string removeWithoutID()
        {
            string change = "0";
            while (true)
            {
                if (first("ID없이 체크 삭제", out change, "사용자의 학년을 입력해 주세요."))
                {
                    return change;
                }
                string[] info = getManyInfo();

                int err = 0;
                JObject result = user.delUser(change, info[0], info[1], out err);
                if ((bool)result["success"])
                {
                    Console.WriteLine($"{result["data"]["grade"]}학년 {result["data"]["class"]}반 {result["data"]["number"]}번 {result["data"]["name"]}(ID: {result["data"]["id"]})의 삭제가 완료되었습니다.\n");
                }
                else
                {
                    Console.WriteLine("삭제를 실패하였습니다. 확인 후 다시 시도해주세요.\n");
                }
            }
        }
        string ondoCheckWithoutID()
        {
            string change = "0";
            while (true)
            {
                if (first("ID없이 발열 체크", out change, "사용자의 학년을 입력해 주세요."))
                {
                    return change;
                }
                string[] info = getManyInfo();

                int err = 0;
                JObject result = user.check(change, info[0], info[1], out err, true);
                if ((bool)result["success"])
                {
                    Console.WriteLine($"{result["data"]["name"]}(ID: {result["data"]["id"]})의 발열 체크가 완료되었습니다.\n");
                }
                else
                {
                    Console.WriteLine("발열 체크를 실패하였습니다. 확인 후 다시 시도해주세요.\n");
                }
            }
        }
        
        bool first(string title, out string what, string and = "")
        {
            Console.WriteLine($"현재는 사용자 {title}모드 입니다. 모드를 변경하려면 change를, 프로그램 종료는 exit를 입력해 주세요.\n{and}");
            bool turn = false;
            while (true)
            {
                what = Console.ReadLine();
                if (string.IsNullOrEmpty(what))
                {
                    Console.WriteLine("정보를 입력하지 않았습니다. 다시 입력해주세요.");
                    continue;
                }
                break;
            }
            if (what == "change") //모드 바꾸기
            {
                what = changeMode();
                turn = true;
            }
            else if (what == "exit") Environment.Exit(0);
            return turn;
        }
              
        string[] getManyInfo()
        {
            string[] info = new string[3];
            Console.WriteLine("사용자의 반을 입력하세요.");
            while (true)
            {
                info[0] = Console.ReadLine();
                if (string.IsNullOrEmpty(info[0]))
                {
                    Console.WriteLine("정보를 입력하지 않았습니다. 다시 사용자의 반을 입력하세요.");
                    continue;
                }
                break;
            }
            Console.WriteLine("사용자의 번호을 입력하세요.");
            while (true)
            {
                info[1] = Console.ReadLine();
                if (string.IsNullOrEmpty(info[1]))
                {
                    Console.WriteLine("정보를 입력하지 않았습니다. 다시 사용자의 번호을 입력하세요.");
                    continue;
                }
                break;
            }
            return info;
        }
    }
}

using System;
using System.Net;
using System.Text;
using System.IO;
using CheckCovid19;
using Newtonsoft.Json.Linq;

namespace Covid_Check_Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Program program = new Program();
            string change = "1"; //일단 프로그램을 켰을 땐 체크모드
            while (true)
            {
                if (change == "1")
                {
                    change = program.check();
                }
                else if (change == "2")
                {
                    change = program.uncheck();
                }
                else if (change == "3")
                {
                    change = program.add();
                }
            }
        }
        string check()
        {
            User user = new User();
            string change = "0";
            while (true)
            {
                Console.WriteLine("현재는 체크모드 입니다. 모드를 변경하려면 change를, 프로그램 종료는 exit를 입력해 주세요\n체크하려면 사용자의 바코드를 스캔하거나 숫자를 입력하세요.");
                string scan = Console.ReadLine();

                if (scan == "change") //모드 바꾸기
                {
                    change = changeMode();
                    break;
                }
                else if (scan == "exit") Environment.Exit(0);
                JObject result = user.check(scan);
                if ((bool)result["success"])
                {
                    Console.WriteLine(result);
                    Console.WriteLine($"{result["data"]["name"]}(ID: {result["data"]["id"]})의 체크가 완료되었습니다.\n");
                }
                else
                {
                    Console.WriteLine("체크를 실패하였습니다. 확인 후 다시 시도해주세요\n");
                }
            }
            return change;
        }
        string uncheck()
        {
            User user = new User();
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
                
                JObject result = user.uncheck(scan);
                if ((bool)result["success"])
                {
                    Console.WriteLine($"{result["data"]["name"]}(ID: {result["data"]["id"]})의 체크 해제가 완료되었습니다.\n");
                }
                else
                {
                    Console.WriteLine("체크 해제를 실패하였습니다. 확인 후 다시 시도해주세요\n");
                }
                Console.WriteLine();
            }
            return change;
        }
        string add()
        {
            User user = new User();
            string change = "0";
            while (true)
            {
                Console.WriteLine("현재는 사용자 추가모드 입니다. 모드를 변경하려면 change를, 프로그램 종료는 exit를 입력해 주세요\n사용자를 추가하려면 사용자의 바코드를 스캔하거나 숫자를 입력하세요.");
                string scan = Console.ReadLine();
                if (scan == "change") //모드 바꾸기
                {
                    change = changeMode();
                    break;
                }
                else if (scan == "exit") Environment.Exit(0);

                Console.WriteLine("사용자의 학년을 입력해 주세요 (선생님: 0학년)");
                int grade = int.Parse(Console.ReadLine());
                Console.WriteLine("사용자의 반을 입력해 주세요 (선생님: 0반)");
                int @class = int.Parse(Console.ReadLine());
                Console.WriteLine("사용자의 번호를 입력해 주세요 (선생님: 자신의 바코드 아래 숫자에서 2020뒤)");
                int number = int.Parse(Console.ReadLine());
                Console.WriteLine("사용자의 이름을 입력해 주세요");
                string name = Console.ReadLine();
                Console.WriteLine(user.addUser(scan, grade, @class, number, name) + "\n");
            }
            return change;
        }
        string changeMode()
        {
            string change = "";
            while (true)
            {
                Console.WriteLine("1: 사용자 체크\n2: 사용자 체크 해제\n3: 사용자 추가");
                change = Console.ReadLine();
                if (change == "1" || change == "2" || change == "3") break;
                Console.WriteLine("올바른 번호를 선택해 주세요");
            }
            Console.WriteLine();
            return change;
        }
    }
}

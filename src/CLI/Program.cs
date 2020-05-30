﻿using System;
using System.Net;
using System.Text;
using System.Linq;
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
                switch (change)
                {
                    case "1":
                        change = program.check();
                        break;
                    case "2":
                        change = program.uncheck();
                        break;
                    case "3":
                        change = program.add();
                        break;
                    case "4":
                        change = program.remove();
                        break;
                    case "5":
                        change = program.checkWithoutID();
                        break;
                    case "6":
                        change = program.uncheckWithoutID();
                        break;
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
                if (first("추가", out change)) return change;
                Console.WriteLine("사용자를 추가하려면 사용자의 바코드를 스캔하거나 숫자를 입력하세요\n사용자의 학년을 입력해 주세요");
                int grade = int.Parse(Console.ReadLine());
                Console.WriteLine("사용자의 반을 입력해 주세요");
                int @class = int.Parse(Console.ReadLine());
                Console.WriteLine("사용자의 번호를 입력해 주세요");
                int number = int.Parse(Console.ReadLine());
                Console.WriteLine("사용자의 이름을 입력해 주세요");
                string name = Console.ReadLine();
                JObject result = user.addUser(change, grade, @class, number, name)["data"] as JObject;
                Console.WriteLine($"{result["grade"]}학년 {result["class"]}반 {result["number"]}번 {result["name"]}(ID: {result["id"]})사용자가 추가되었습니다." + "\n");
            }
        }
        string remove()
        {
            User user = new User();
            while (true)
            {
                string scan = "";
                if (first("삭제", out scan, "사용자를 삭제하려면 사용자의 바코드를 스캔하거나 숫자를 입력하세요.")) return scan; //모드 바꾸기
                JObject result = user.delUser(scan);
                if ((bool)result["success"])
                {
                    Console.WriteLine($"{result["data"]["grade"]}학년 {result["data"]["class"]}반 {result["data"]["number"]}번 {result["data"]["name"]}(ID: {result["data"]["id"]})의 삭제가 완료되었습니다.\n");
                }
                else
                {
                    Console.WriteLine("삭제를 실패하였습니다. 확인 후 다시 시도해주세요\n");
                }
                Console.WriteLine();
            }
        }
        string changeMode()
        {
            string change = "";
            while (true)
            {
                Console.WriteLine("1: 사용자 체크\n2: 사용자 체크 해제\n3: 사용자 추가\n4: 사용자 삭제\n5: ID없이 사용자 체크\n6: ID없이 사용자 체크 해제\n7: ID없이 사용자 삭제");
                change = Console.ReadLine();
                string[] lists = new string[] {
                    "1", "2", "3", "4", "5", "6", "7" 
                };
                if (lists.Contains(change)) break;
                Console.WriteLine("올바른 번호를 선택해 주세요");
            }
            Console.WriteLine();
            return change;
        }
        
        
        
        string checkWithoutID()
        {
            User user = new User();
            string change = "0";
            while (true)
            {
                if (first("ID없이 체크", out change, "사용자의 학년을 입력해 주세요"))
                {
                    return change;
                }
                string[] info = getManyInfo();
                JObject result = user.check(change, info[0], info[1]);
                if ((bool)result["success"])
                {
                    Console.WriteLine($"{result["data"]["name"]}(ID: {result["data"]["id"]})의 체크가 완료되었습니다.\n");
                }
                else
                {
                    Console.WriteLine("체크를 실패하였습니다. 확인 후 다시 시도해주세요\n");
                }
            }
        }
        string uncheckWithoutID()
        {
            User user = new User();
            string change = "0";
            while (true)
            {
                if (first("ID없이 체크 해제", out change, "사용자의 학년을 입력해 주세요"))
                {
                    return change;
                }
                string[] info = getManyInfo();
                JObject result = user.uncheck(change, info[0], info[1]);
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
        
        
        bool first(string title, out string what, string and = "")
        {
            Console.WriteLine($"현재는 사용자 {title}모드 입니다. 모드를 변경하려면 change를, 프로그램 종료는 exit를 입력해 주세요\n{and}");
            what = Console.ReadLine();
            bool turn = false;
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
            Console.WriteLine("사용자의 반을 입력하세요");
            info[0] = Console.ReadLine();
            Console.WriteLine("사용자의 번호을 입력하세요");
            info[1] = Console.ReadLine();
            return info;
        }
    }
}
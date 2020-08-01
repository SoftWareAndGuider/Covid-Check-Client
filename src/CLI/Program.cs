using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using CheckCovid19;
using Newtonsoft.Json.Linq;

namespace Covid_Check_Client
{
    class Program
    {
        static string[,] csv = new string[3,2];
        static User user = new User("localhost");
        const string settingPath = "config.json";
        static JObject settingJson = new JObject();

        static void Main(string[] args)
        {
            string defaultSetting = @"{
                ""url"": ""localhost"",
                ""barcodeLength"": 8,
                ""timeoutRetry"": 100,
                ""checkUpdate"": true,
                ""autoUpdate"": false,
                ""usePassword"": false,
                ""password"": ""password"",
                ""csvSave"": false,
                ""saves"": [
                    [
                        false, false
                    ],
                    [
                        false, false
                    ],
                    [
                        false, false
                    ]
                ]
            }"; //기본 JSON 세팅
            try
            {
                settingJson = user.loadSetting(settingPath);
                settingJson = user.trimSetting(JObject.Parse(defaultSetting), settingJson);
            }
            catch
            {
                settingJson = JObject.Parse(defaultSetting);
            }
            
            user.url = settingJson["url"].ToString();

            Program program = new Program();
            if ((bool)settingJson["usePassword"])
            {
                Console.WriteLine("비밀번호를 사용하도록 설정되어 있습니다. 비밀번호를 입력해 주세요");
                while (true)
                {
                    string read = program.ReadLine("*");
                    Console.CursorLeft = 0;
                    Console.CursorTop = Console.CursorTop - 1;
                    if (user.getSha512(read) == settingJson["password"].ToString())
                    {
                        break;
                    }
                    else
                    {
                        Console.WriteLine("비밀번호가 틀렸습니다. 다시 시도해 주세요");
                    }
                }
            }
            
            for (int i = 0; i < 6; i++)
            {
                csv[i / 2, i % 2] = "학년,반,번호,이름,ID";
            }
            
            Console.WriteLine("CovidCheckClient, Custom License\nCopyright (c) 2020 SoftWareAndGuider, cnsewcs, pmh-only, Noeul-Night / 자세한 저작권 관련 사항과 이 프로그램의 소스코드는 https://github.com/softwareandguider/covid-check-client 에서 확인해주세요.\n");

            JArray verName = new JArray();
            if ((bool)settingJson["checkUpdate"])
            {
                if (user.hasNewVersion(2, out verName))
                {
                    Console.WriteLine("새로운 버전 {0}이(가) 출시되었습니다. https://github.com/SoftWareAndGuider/Covid-Check-Client/releases/ 에서 확인해 주세요.\n", verName.First()["name"]);
                }
            }
            var getPing = user.getPing();
            string ping = "알 수 없음";
            if (getPing != -1) ping = getPing.ToString() + "ms";

            Console.Title = $"코로나19 예방용 발열체크 프로그램 (CLI)(통신 속도: {ping})";
            Console.WriteLine("서버({0})와의 통신 속도: " + ping + "\n", settingJson["url"]);
            
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
                    case "setting":
                    case "s":
                        change = program.setting();
                        break;
                    case "export":
                    case "e":
                        change = program.export();
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
                    if ((bool)settingJson["saves"][0][0])
                    {
                        csv[0, 0] += $"\n{result["data"]["grade"]},{result["data"]["class"]},{result["data"]["number"]},{result["data"]["name"]},{result["data"]["id"]}";
                    }
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
                string scan = ReadLine();
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
                    if ((bool)settingJson["saves"][2][0])
                    {
                        csv[2, 0] += $"\n{result["data"]["grade"]},{result["data"]["class"]},{result["data"]["number"]},{result["data"]["name"]},{result["data"]["id"]}";
                    }
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
                int grade = int.Parse(ReadLine());
                Console.WriteLine("\n사용자의 반을 입력해 주세요.");
                int @class = int.Parse(ReadLine());
                Console.WriteLine("\n사용자의 번호를 입력해 주세요.");
                int number = int.Parse(ReadLine());
                Console.WriteLine("\n사용자의 이름을 입력해 주세요.");
                string name = ReadLine();

                int err = 0;
                JObject result = user.addUser(change, grade, @class, number, name, out err);
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
                    if ((bool)settingJson["saves"][1][0])
                    {
                        csv[1, 0] += $"\n{result["data"]["grade"]},{result["data"]["class"]},{result["data"]["number"]},{result["data"]["name"]},{result["data"]["id"]}";
                    }
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
                Console.WriteLine("1: 사용자 체크\n2: 사용자 발열 체크\n3: 사용자 체크 해제\n4: 사용자 추가\n5: 사용자 삭제\n6: ID없이 사용자 체크\n7: ID없이 사용자 발열 체크\n8: ID없이 사용자 체크 해제\n9: ID없이 사용자 삭제\nsetting 혹은 s: 설정\nexport 혹은 e: 데이터를 저장하기로 설정한 것들을 csv로 저장합니다.");
                change = ReadLine();
                string[] lists = new string[] {
                    "1", "2", "3", "4", "5", "6", "7", "8", "9", "setting", "s", "export", "e"
                };
                if (lists.Contains(change)) break;
                Console.WriteLine("올바른 문자를 선택해 주세요.");
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
                    if ((bool)settingJson["saves"][0][1])
                    {
                        csv[0, 1] += $"\n{result["data"]["grade"]},{result["data"]["class"]},{result["data"]["number"]},{result["data"]["name"]},{result["data"]["id"]}";
                    }
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
                    if ((bool)settingJson["saves"][2][1])
                    {
                        csv[2, 1] += $"\n{result["data"]["grade"]},{result["data"]["class"]},{result["data"]["number"]},{result["data"]["name"]},{result["data"]["id"]}";
                    }
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
                    if ((bool)settingJson["saves"][1][1])
                    {
                        csv[1, 1] += $"\n{result["data"]["grade"]},{result["data"]["class"]},{result["data"]["number"]},{result["data"]["name"]},{result["data"]["id"]}";
                    }
                }
                else
                {
                    Console.WriteLine("발열 체크를 실패하였습니다. 확인 후 다시 시도해주세요.\n");
                }
            }
        }
        string setting()
        {
            string what = "";
            bool turn = false;
            Console.WriteLine($"설정 모드 입니다. 모드를 변경하려면 change를, 프로그램 종료는 exit를 입력해 주세요. 해당하는 명령어를 입력해 주세요.\nurl [홈페이지 주소]: 홈페이지의 url을 [홈페이지 주소]로 저장\npassword [y/n] [password]: 비밀번호를 [password]로 지정하고 [y/n]에서 y라면 비밀번호를 사용, n이라면 비밀번호 사용 안함\nupdate [y/n]: 프로그램을 시작할 때 업데이트를 [y/n]이 y라면 확인, n이라면 확인하지 않음\nsettingfile [파일 경로]: [파일 경로]에 있는 설정 파일을 가져와서 설정을 복사합니다.\nexport: csv로 저장할지 여부를 결정합니다. 자세한건 export help를 사용해 주세요.");
            while (true)
            {
                while (true)
                {
                    what = ReadLine();
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
                if (turn)
                {
                    return what;
                }

                string[] command = what.Split(' ');
                switch (command[0])
                {
                    case "url":
                        try
                        {
                            settingJson["url"] = command[1];
                            user.saveSetting(settingJson.ToString(), settingPath);
                            user.url = command[1];

                            var getPing = user.getPing();
                            string ping = "알 수 없음";
                            if (getPing != -1) ping = getPing.ToString() + "ms";
                            Console.WriteLine($"url 변경({command[1]})이 완료되었습니다. (새로운 서버와 통신 속도: {ping})");
                        }
                        catch
                        {
                            Console.WriteLine("명령어가 잘못되었습니다. 설정 값이 저장되지 않습니다.");
                        }
                    break;

                    case "password":
                        try
                        {
                            string password = "";                            


                            for (int i = 2; i < command.Length; i++)
                            {
                                try
                                {
                                    password += command[i];
                                    if (i + 1 < command.Length)
                                    {
                                        password += " ";
                                    }
                                }
                                catch
                                {

                                }
                            }

                            if ((password.Length == 0 && command[1] == "y") || (command[1] != "y" && command[1] != "n"))
                            {
                                throw new Exception();
                            }
                            if (command[1] == "y") settingJson["usePassword"] = true;

                            else if (command[1] == "n") 
                            {
                                settingJson["usePassword"] = false;
                            }
                            settingJson["password"] = user.getSha512(password);

                            user.saveSetting(settingJson.ToString(), settingPath);
                            string writeLine = "비밀번호 변경(비밀번호 ";
                            string mask = "";
                            for (int i = 0; i < password.Length; i++)
                            {
                                mask += "*";
                            }
                            Console.SetCursorPosition(11, Console.CursorTop - 1);
                            Console.WriteLine(mask);
                            if (command[1] == "y")
                            {
                                writeLine += $"사용, 비밀번호: {mask})";
                            }
                            else
                            {
                                if (password.Length == 0)
                                {
                                    writeLine += $"사용 안함)";
                                }
                                else
                                {
                                    writeLine += $"사용 안함, 비밀번호: {mask})";
                                }
                            }
                            writeLine += "이 완료되었습니다.";
                            Console.WriteLine(writeLine);
                        }
                        catch
                        {
                            Console.WriteLine("명령어가 잘못되었습니다. 설정 값이 저장되지 않습니다.");
                        }
                    break;
                    case "update":
                        try
                        {
                            if (command[1] == "y")
                            {
                                settingJson["checkUpdate"] = true;
                                Console.WriteLine("업데이트 확인을 하도록 설정했습니다.");
                            }
                            else if (command[1] == "n")
                            {
                                settingJson["checkUpdate"] = false;
                                Console.WriteLine("업데이트 확인을 하지 않도록 설정했습니다.");
                            }
                            else
                            {
                                throw new Exception();
                            }
                        }
                        catch
                        {
                            Console.WriteLine("명령어가 잘못되었습니다. 설정 값이 저장되지 않습니다.");
                        }
                    break;
                    case "settingfile":
                        try
                        {
                            string path = "";
                            for (int i = 1; i < command.Length; i++)
                            {
                                path += command[i];
                                if (i + 1 < command.Length)
                                {
                                    path += " ";
                                }
                            }
                            JObject newSetting = user.loadSetting(path);
                            if (!(newSetting.ContainsKey("url") && newSetting.ContainsKey("timeoutRetry") && newSetting.ContainsKey("checkUpdate") && newSetting.ContainsKey("autoUpdate") && newSetting.ContainsKey("usePassword") && newSetting.ContainsKey("password")))
                            {
                                throw new Exception();
                            }
                            File.Copy(path, settingPath, true);
                            Console.WriteLine("설정이 변경되었습니다. 프로그램이 다시 시작되면 이 설정이 적용됩니다.");
                        }
                        catch
                        {
                            Console.WriteLine("명령어 혹은 파일이 잘못되었습니다. 설정이 변하지 않습니다.");
                        }
                        break;
                    case "export":
                        try
                        {
                            //0~: 모두, 1~: 체크, 2~: 발열 체크, 3~: 체크 해제
                            //~0: 사용 안함, ~1: id로 하는거, ~2: 학년, 반, 번호로 하는거, ~3: id와 학년, 반, 번호로 하는 것
                            switch (command[1])
                            {
                                case "help":
                                    Console.WriteLine("export [수]: [수]에 맞게 csv파일을 저장할지 여부를 결정합니다.\n[수]양식: 0~: 모두, 1~: 체크, 2~: 발열 체크, 3~: 체크 해제 / ~0: 사용 안함, ~1: id로 하는거, ~2: 학년, 반, 번호로 하는거, ~3: id와 학년, 반, 번호로 하는 것\n예시) export 12: 학년 반 번호로 체크하는 것을 기록하고, ID로 체크하는걸 기록하지 않도록 설정합니다.");
                                    break;
                                case "00":
                                    settingJson["saves"][0][0] = false;
                                    settingJson["saves"][0][1] = false;
                                    settingJson["saves"][1][0] = false;
                                    settingJson["saves"][1][1] = false;
                                    settingJson["saves"][2][0] = false;
                                    settingJson["saves"][2][1] = false;
                                    break;
                                case "01":
                                    settingJson["saves"][0][0] = true;
                                    settingJson["saves"][0][1] = false;
                                    settingJson["saves"][1][0] = true;
                                    settingJson["saves"][1][1] = false;
                                    settingJson["saves"][2][0] = true;
                                    settingJson["saves"][2][1] = false;
                                    break;
                                case "02":
                                    settingJson["saves"][0][0] = false;
                                    settingJson["saves"][0][1] = true;
                                    settingJson["saves"][0][0] = false;
                                    settingJson["saves"][1][1] = true;
                                    settingJson["saves"][0][0] = false;
                                    settingJson["saves"][2][1] = true;
                                    break;
                                case "03":
                                    settingJson["saves"][0][0] = true;
                                    settingJson["saves"][0][1] = true;
                                    settingJson["saves"][1][0] = true;
                                    settingJson["saves"][1][1] = true;
                                    settingJson["saves"][2][0] = true;
                                    settingJson["saves"][2][1] = true;
                                    break;

                                case "10":
                                    settingJson["saves"][0][0] = false;
                                    settingJson["saves"][0][1] = false;
                                    break;
                                case "11":
                                    settingJson["saves"][0][0] = true;
                                    settingJson["saves"][0][1] = false;
                                break;
                                case "12":
                                    settingJson["saves"][0][0] = false;
                                    settingJson["saves"][0][1] = true;
                                break;
                                case "13":
                                    settingJson["saves"][0][0] = true;
                                    settingJson["saves"][0][1] = true;
                                break;

                                case "20":
                                    settingJson["saves"][1][0] = false;
                                    settingJson["saves"][1][1] = false;
                                    break;
                                case "21":
                                    settingJson["saves"][1][0] = true;
                                    settingJson["saves"][1][1] = false;
                                break;
                                case "22":
                                    settingJson["saves"][1][0] = false;
                                    settingJson["saves"][1][1] = true;
                                break;
                                case "23":
                                    settingJson["saves"][1][0] = true;
                                    settingJson["saves"][1][1] = true;
                                break;

                                case "30":
                                    settingJson["saves"][2][0] = false;
                                    settingJson["saves"][2][1] = false;
                                    break;
                                case "31":
                                    settingJson["saves"][2][0] = true;
                                    settingJson["saves"][2][1] = false;
                                break;
                                case "32":
                                    settingJson["saves"][2][0] = false;
                                    settingJson["saves"][2][1] = true;
                                break;
                                case "33":
                                    settingJson["saves"][2][0] = true;
                                    settingJson["saves"][2][1] = true;
                                break;

                                default:
                                    throw new Exception();
                            }
                            user.saveSetting(settingJson.ToString(), settingPath);
                            if (command[1] != "help") Console.WriteLine("설정 완료");                            
                        }
                        catch
                        {
                            Console.WriteLine("명령어 혹은 파일이 잘못되었습니다. 설정이 변하지 않습니다.");
                        }
                        break;

                    default:
                        Console.WriteLine($"설정 모드 입니다. 모드를 변경하려면 change를, 프로그램 종료는 exit를 입력해 주세요. 해당하는 명령어를 입력해 주세요.\nurl [홈페이지 주소]: 홈페이지의 url을 [홈페이지 주소]로 저장\npassword [y/n] [password]: 비밀번호를 [password]로 지정하고 [y/n]에서 y라면 비밀번호를 사용, n이라면 비밀번호 사용 안함\nupdate [y/n]: 프로그램을 시작할 때 업데이트를 [y/n]이 y라면 확인, n이라면 확인하지 않음\nsettingfile [파일 경로]: [파일 경로]에 있는 설정 파일을 가져와서 설정을 복사합니다.\nexport: csv로 저장할지 여부를 결정합니다. 자세한건 export help를 사용해 주세요.");
                        break;
                }
                // Console.WriteLine();
            }
        }
        string export()
        {
            while (true)
            {
                string scan = "";
                if (first("삭제", out scan, "어떤 csv를 출력할까요?\n11: ID로 체크, 12: 학년, 반, 번호로 체크, 21: ID로 발열 체크, 22: 학년, 반, 번호로 발열 체크, 31: ID로 체크 해제, 32: 학년, 반, 번호로 체크 해제")) return scan; //모드 바꾸기
                string save = "";
                string path = "";
                bool on = false;
                switch (scan)
                {
                    case "11":
                        if ((bool)settingJson["saves"][0][0])
                        {
                            on = true;
                            save = csv[0, 0];
                            Console.WriteLine("csv 파일을 저장할 경로를 입력하세요.");
                            path = ReadLine();
                        }
                    break;
                    case "12":
                        if ((bool)settingJson["saves"][0][1])
                        {
                            on = true;
                            save = csv[0, 1];
                            Console.WriteLine("csv 파일을 저장할 경로를 입력하세요.");
                            path = ReadLine();
                        }
                    break;

                    case "21":
                        if ((bool)settingJson["saves"][1][0])
                        {
                            on = true;
                            save = csv[1, 0];
                            Console.WriteLine("csv 파일을 저장할 경로를 입력하세요.");
                            path = ReadLine();
                        }
                    break;
                    case "22":
                        if ((bool)settingJson["saves"][1][1])
                        {
                            on = true;
                            save = csv[1, 1];
                            Console.WriteLine("csv 파일을 저장할 경로를 입력하세요.");
                            path = ReadLine();
                        }
                    break;

                    case "31":
                        if ((bool)settingJson["saves"][2][0])
                        {
                            on = true;
                            save = csv[2, 0];
                            Console.WriteLine("csv 파일을 저장할 경로를 입력하세요.");
                            path = ReadLine();
                        }
                    break;
                    case "32":
                        if ((bool)settingJson["saves"][2][1])
                        {
                            on = true;
                            save = csv[2, 1];
                            Console.WriteLine("csv 파일을 저장할 경로를 입력하세요.");
                            path = ReadLine();
                        }
                    break;
                    default:
                        Console.WriteLine("올바른 수를 입력해 주세요.");
                        continue;
                }
                if (on)
                {
                    try
                    {
                        Regex r = new Regex("^*.csv$");
                        if (!r.IsMatch(path))
                        {
                            path += ".csv";
                        }
                        File.WriteAllText(path, save);
                        Console.WriteLine(path + "에 저장 완료");
                    }
                    catch
                    {
                        Console.WriteLine("저장 실패");
                    }
                }
                else
                {
                    Console.WriteLine("해당 데이터를 저장하도록 설정되어있지 않습니다.");
                }
            }
        }
        
        bool first(string title, out string what, string and = "")
        {
            Console.WriteLine($"현재는 사용자 {title}모드 입니다. 모드를 변경하려면 change를, 프로그램 종료는 exit를 입력해 주세요.\n{and}");
            bool turn = false;
            while (true)
            {
                what = ReadLine();
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
            Console.WriteLine("\n사용자의 반을 입력하세요.");
            while (true)
            {
                info[0] = ReadLine();
                if (string.IsNullOrEmpty(info[0]))
                {
                    Console.WriteLine("정보를 입력하지 않았습니다. 다시 사용자의 반을 입력하세요.");
                    continue;
                }
                break;
            }
            Console.WriteLine("\n사용자의 번호을 입력하세요.");
            while (true)
            {
                info[1] = ReadLine();
                if (string.IsNullOrEmpty(info[1]))
                {
                    Console.WriteLine("정보를 입력하지 않았습니다. 다시 사용자의 번호을 입력하세요.");
                    continue;
                }
                break;
            }
            return info;
        }
    
        public string ReadLine(string password = null)
        {
            List<char> read = new List<char>();
            int insert = 0;
            string clear = "";

            for (int i = 0; i < Console.BufferWidth - 1; i++)
            {
                clear += " ";
            }

            while (true)
            {
                ConsoleKeyInfo info = Console.ReadKey();
                if (info.Key == ConsoleKey.Enter)
                {
                    break;
                }
                else
                {
                    // //Console.Write(insert);
                    // int left = Console.CursorLeft;
                    // Console.CursorTop = Console.CursorTop + 1;
                    // Console.CursorLeft = 0;
                    // Console.Write($"삽입 위치: {insert}, 리스트 수: {read.Count}");
                    // Console.CursorLeft = left;
                    // Console.CursorTop = Console.CursorTop - 1;
                    Console.CursorLeft = insert;
                    switch (info.Key)
                    {
                        case ConsoleKey.Home:
                            insert = 0;
                            break;
                        case ConsoleKey.End:
                            insert = read.Count;
                            break;
                        case ConsoleKey.LeftArrow:
                            if (insert != 0)
                            {
                                insert--;
                            }
                            break;
                        case ConsoleKey.RightArrow:
                            if (insert < read.Count)
                            {
                                insert++;
                            }
                            break;
                        case ConsoleKey.Backspace:
                            if (insert != 0)
                            {
                                read.RemoveAt(insert - 1);
                                insert--;
                            }
                            break;
                        case ConsoleKey.Delete:
                            if (insert < read.Count)
                            {
                                read.RemoveAt(insert);
                                //insert--;
                            }
                            break;
                        default:
                            read.Insert(insert, info.KeyChar);
                            insert++;
                            break;
                    }
                    
                    Console.CursorLeft = 0;

                    Console.Write(clear);
                    Console.CursorLeft = 0;
                    if (password == null)
                    {
                        foreach (char a in read)
                        {
                            Console.Write(a);
                        }
                    }
                    else
                    {
                        foreach (char a in read)
                        {
                            Console.Write(password);
                        }
                    }
                    Console.CursorLeft = insert;
                }
            }
            string turn = "";
            foreach (char a in read)
            {
                turn += a;
            }
            Console.WriteLine();
            return turn;
        }
        
    }
}
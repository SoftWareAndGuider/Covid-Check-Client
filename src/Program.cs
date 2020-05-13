using System;
using System.Net;
using System.Text;
using System.IO;
using CheckCovid19;

namespace Covid_Check_Client
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("학생의 바코드를 스캔하거나 숫자를 입력해 주세요");
                string scan = Console.ReadLine();
                if (scan == "owner") //관리자 모드로 넘어가기
                {
                    new Program().admin();
                }
            }

        }
        void admin()
        {
            //////////////////////////////////////////////////////////////////
            //                                                              //
            //                                                              //
            // 관리자 모드: 사용자 생성, 수정, 삭제 프로그램 종료 등 중요한 것을 담당    //
            //                                                              //
            //                                                              //
            //////////////////////////////////////////////////////////////////
            bool keepLoop = true;
            Console.WriteLine("================================================================================\n\n\n주의: 관리자 모드입니다. 관리자의 작은 행동 하나로 사용자들의 정보가 훼손될 수 있습니다.\n\n\n================================================================================\n\n새로운 사용자 등록은 1, 사용자 정보 수정은 2, 사용자 삭제는 3, 프로그램 종료는 4, 관리자 모드 나가기는 0을 눌러주세요>");
            while (keepLoop)
            {
                Console.SetCursorPosition(119, Console.CursorTop - 1);
                string order = Console.ReadLine();
                switch (order)
                {
                    case "1":
                        Console.WriteLine("사용자의 ID(바코드 숫자)를 스캔하거나 입력해 주세요");
                        break;
                    case "2":
                        Console.WriteLine("사용자의 ID(바코드 숫자)를 스캔하거나 입력해 주세요");
                        break;
                    case "3":
                        Console.WriteLine("사용자의 ID(바코드 숫자)를 스캔하거나 입력해 주세요");
                        break;
                    case "4":
                        Environment.Exit(0);
                        break;
                    case "0":
                        keepLoop = false;
                        break;
                }
            }
        }
    }
}

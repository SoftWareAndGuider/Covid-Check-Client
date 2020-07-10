using System;
using Newtonsoft.Json.Linq;

namespace CovidCheckClientGui
{
    public class StatusParsing
    {
        public int[] lessInfo(JObject result) //사람 수만 볼 때
        {
            int firstGrade = 0;
            int secondGrade = 0;
            int thirdGrade = 0;
            int etcGrade = 0;
            foreach (var a in result["data"])
            {
                if (a["grade"].ToString() == "1")
                {
                    firstGrade++;
                }
                else if (a["grade"].ToString() == "2")
                {
                    secondGrade++;
                }
                else if (a["grade"].ToString() == "3")
                {
                    thirdGrade++;
                }
                else
                {
                    etcGrade++;
                }
            }
            return new int[4] {firstGrade, secondGrade, thirdGrade, etcGrade};
        }
        public double[,] moreInfo(JObject result) //검사 현황을 볼 때
        {
            double[,] info = new double[4,3] { {0, 0, 0}, {0, 0, 0}, {0, 0, 0}, {0, 0, 0} };
            foreach (var a in result["data"])
            {
                if (a["grade"].ToString() == "1") //1학년
                {
                    if ((int)a["checked"] == 0) //체크 안함
                    {
                        info[0, 0]++;
                    }
                    else if ((int)a["checked"] == 1) //체크함
                    {
                        info[0, 1]++;
                    }
                    else //발열자
                    {
                        info[0, 2]++;
                    }
                }
                else if (a["grade"].ToString() == "2")
                {
                    if ((int)a["checked"] == 0)
                    {
                        info[1, 0]++;
                    }
                    else if ((int)a["checked"] == 1)
                    {
                        info[1, 1]++;
                    }
                    else
                    {
                        info[1, 2]++;
                    }
                }
                else if (a["grade"].ToString() == "3")
                {
                    if ((int)a["checked"] == 0)
                    {
                        info[2, 0]++;
                    }
                    else if ((int)a["checked"] == 1)
                    {
                        info[2, 1]++;
                    }
                    else
                    {
                        info[2, 2]++;
                    }
                }
                else
                {
                    if ((int)a["checked"] == 0)
                    {
                        info[3, 0]++;
                    }
                    else if ((int)a["checked"] == 1)
                    {
                        info[3, 1]++;
                    }
                    else
                    {
                        info[3, 2]++;
                    }
                }
            }
            return info;
        }
    }
}
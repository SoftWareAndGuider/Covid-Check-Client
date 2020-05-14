using System;
using Gtk;


namespace CovidCheckClientGui
{
    partial class Program : Window
    {
        public Program() : base("코로나19 예방용 발열체크 프로그램")
        {
            SetDefaultSize(500, 500);
            ShowAll();
        }
    }
}

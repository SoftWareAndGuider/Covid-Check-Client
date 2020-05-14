using System;
using Gtk;


namespace CovidCheckClientGui
{
    partial class Program : Window
    {
        static void Main(string[] args)
        {
            Application.Init();
            new Program();
            Application.Run();
        }

    }
}

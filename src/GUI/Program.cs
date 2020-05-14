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

        void isTeacherClicked (object sender, EventArgs e)
        {
            if (isTeacher.Active)
            {
                
            }
            else
            {

            }
        }
    }
}

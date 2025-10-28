using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client1
{
    internal static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            int sensorId = 0; 
            if (args.Length > 0 && int.TryParse(args[0], out int id))
            {
                sensorId = id;
            }

            Application.Run(new Form1(sensorId));
        }
    }
}

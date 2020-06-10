using System;
using System.Management;
using System.Runtime.InteropServices;
using System.Threading;

// VERSION = "1.2.1";
// AUTHOR = "Gotoev Roman Ilich";

namespace GetPrinters
{
    class Program
    {
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int HIDE = 0;
        const int SHOW = 5;
        
        static void Main()
        {
            if (!IsSingleInstance())
                return;
            else
                WorkWithPrinter();
        }
        private static bool IsSingleInstance()
        {
            bool flag;
            new Mutex(true, "GetPrinters", out flag);
            return flag;
        }

        private static void WorkWithPrinter()
        {
            var handle = GetConsoleWindow();
            var printerQuery = new ManagementObjectSearcher("SELECT * from Win32_Printer");
            string statusReadyOrOK = "13";
            string charsInPrinterName404 = "404";
            string charsInPrinterName212 = "212";
            int waitInMinutes = 45;
            int millisecond = 60000;

            foreach (var printer in printerQuery.Get())
            {
                var name = printer.GetPropertyValue("Name");
                var status = printer.GetPropertyValue("PrinterStatus");
                var isDefault = printer.GetPropertyValue("Default");
                var ipAdress = printer.GetPropertyValue("PortName");

                if (name.ToString().Contains(charsInPrinterName404) || name.ToString().Contains(charsInPrinterName212))
                {
                    new Thread(() =>
                    {                        
                        while (true)
                        {                            
                            status = statusReadyOrOK.Contains(status.ToString()) ? "Ready" : status;
                            ipAdress = ipAdress.ToString().Contains("nul") ? "None" : ipAdress;                            
                            Console.WriteLine("{0} \n<Status: {1}, Default: {2}, Port/IP-adress: {3}>", name, status, isDefault, ipAdress);
                            Console.WriteLine("Polling interval {0} minutes...", waitInMinutes);                            
                            ShowWindow(handle, HIDE);
                            Thread.Sleep(waitInMinutes * millisecond);
                            Console.Clear();
                        }
                    }).Start();
                }
            }
        }
    }
}

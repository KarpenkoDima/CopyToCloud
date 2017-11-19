using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using aejw.Network;

namespace CopyFilesToCloudDriver
{
    class Program
    {
        static object workerLocker = new object();
        private static int run = 1;
       
        static void Main(string[] args)
        {
            ThreadPool.QueueUserWorkItem(ConnectToDrive, null);
            Console.WriteLine("Waiting...");
            lock(workerLocker)
                while (run > 0)
                {
                  
                    Monitor.Wait(workerLocker);
                 
                }
        }

        static void ConnectToDrive(object o)
        {
            NetworkDrive oNetDrive = new NetworkDrive();
          
            try
            {
                bool isDowload = false;
                FileInfo file = new FileInfo(@"H:\ATI2016 v19.5620.iso");
               
                if (file.Exists)
                {
                    if (file.Extension.ToUpper() == ".ISO")
                    {
                        if (file.CreationTime.Date.ToShortDateString() == "21.09.2015")
                        {
                            isDowload = true;
                        }
                    }
                }
                if (isDowload)
                {

                    //set propertys
                    oNetDrive.Force = Convert.ToBoolean(ConfigurationManager.AppSettings["Force"]);
                    oNetDrive.Persistent = Convert.ToBoolean(ConfigurationManager.AppSettings["Persistent"]);
                    oNetDrive.LocalDrive = ConfigurationManager.AppSettings["LocalDrive"];
                    oNetDrive.PromptForCredentials = Convert.ToBoolean(ConfigurationManager.AppSettings["PromptForCred"]);
                    oNetDrive.ShareName = ConfigurationManager.AppSettings["ShareName"];
                    oNetDrive.SaveCredentials = Convert.ToBoolean(ConfigurationManager.AppSettings["SaveCredentials"]);
                    string username = ConfigurationManager.AppSettings["UserName"];
                    string password = ConfigurationManager.AppSettings["Password"];

                    string source = ConfigurationManager.AppSettings["Source"];
                    string destination = ConfigurationManager.AppSettings["Destination"];
                    //match call to options provided
                    if (string.IsNullOrEmpty(username) && string.IsNullOrEmpty(password))
                    {
                        oNetDrive.MapDrive();
                    }
                    else if (string.IsNullOrEmpty(username))
                    {
                        oNetDrive.MapDrive(password);
                    }
                    else
                    {
                        oNetDrive.MapDrive(username, password);
                    }

                    using (FileStream fstreamW = new FileStream(destination, FileMode.Create))
                    {
                        // чтение из файла
                        using (FileStream fstreamR = File.OpenRead(source))
                        {
                            // преобразуем строку в байты
                            byte[] buffer = new byte[16 * 1024];
                            int read;
                            // считываем данные
                            lock (workerLocker) 
                            {

                                while ((read = fstreamR.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    fstreamW.Write(buffer, 0, read);
                                }
                                run = 0;
                                Monitor.Pulse(workerLocker);
                            }
                        }
                    }
                    //File.Copy(file.FullName, oNetDrive.LocalDrive + file.Name, true);
                }

            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
            oNetDrive = null;
        }
    }
}

    
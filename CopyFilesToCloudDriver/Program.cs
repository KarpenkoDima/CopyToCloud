using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using aejw.Network;
using CG.Web.MegaApiClient;

namespace CopyFilesToCloudDriver
{
    class Program
    {
        static object workerLocker = new object();
        private static int run = 1;
       
        static void Main(string[] args)
        {
            ICloud cloud = GetCloudType();
            ThreadPool.QueueUserWorkItem(cloud.ConnectToDrive, null);
            Console.WriteLine("Waiting...");
            lock(workerLocker)
                while (run > 0)
                {
                  
                    Monitor.Wait(workerLocker);
                 
                }
        }

        static ICloud GetCloudType()
        {
           string cloudName =  ConfigurationManager.AppSettings["CloudName"];
            ICloud cloud = null;
            switch (cloudName.ToUpper())
            {
                case "MEGA":
                    cloud = new Mega();
                    break;
                case "YANDEX":
                   cloud = new Yandex();
                    break;
            }
            return cloud;
        }

        private class Yandex : ICloud
        {
            public void ConnectToDrive(object o)
            {
                NetworkDrive oNetDrive = new NetworkDrive();

                try
                {
                   // bool isDowload = false;
                    string source = ConfigurationManager.AppSettings["Source"];
                    string typeFiles = ConfigurationManager.AppSettings["TypeFile"];

                    var files = Directory.GetFiles(source, "*." + typeFiles, SearchOption.TopDirectoryOnly);
                    var date = files.Max(n => File.GetCreationTime(n).ToShortDateString());
                    var filterFiles = files.Where((n) => File.GetCreationTime(n).ToShortDateString() == date);

                    //if (element.Exists)
                    //{
                    //    if (file.CreationTime.Date.ToShortDateString() == "21.09.2015")
                    //    {
                    //        isDowload = true;
                    //    }

                    //}

                    //if (isDowload)
                    //{

                    //set propertys
                    oNetDrive.Force = Convert.ToBoolean(ConfigurationManager.AppSettings["Force"]);
                    oNetDrive.Persistent = Convert.ToBoolean(ConfigurationManager.AppSettings["Persistent"]);
                    oNetDrive.LocalDrive = ConfigurationManager.AppSettings["LocalDrive"];
                    oNetDrive.PromptForCredentials = Convert.ToBoolean(ConfigurationManager.AppSettings["PromptForCred"]);
                    oNetDrive.ShareName = ConfigurationManager.AppSettings["ShareName"];
                    oNetDrive.SaveCredentials = Convert.ToBoolean(ConfigurationManager.AppSettings["SaveCredentials"]);
                    string username = ConfigurationManager.AppSettings["UserName"];
                    string password = ConfigurationManager.AppSettings["Password"];

                    //string source = ConfigurationManager.AppSettings["Source"];
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
                    lock (workerLocker)
                    {
                        foreach (var element in filterFiles)
                        {
                            using (FileStream fstreamW = new FileStream(oNetDrive.LocalDrive + "\\" + Path.GetFileName(element),FileMode.Create))
                            {
                                // чтение из файла
                                using (FileStream fstreamR = File.OpenRead(element))
                                {
                                    // преобразуем строку в байты
                                    byte[] buffer = new byte[16*1024];
                                    int read;
                                    // считываем данные


                                    while ((read = fstreamR.Read(buffer, 0, buffer.Length)) > 0)
                                    {
                                        fstreamW.Write(buffer, 0, read);
                                    }

                                }
                            }
                            oNetDrive.UnMapDrive();
                            run = 0;
                            Monitor.Pulse(workerLocker);
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

        class Mega : ICloud
        {

            public void ConnectToDrive(object o)
            {
                lock (workerLocker)
                {
                    MegaApiClient client = new MegaApiClient();

                    client.Login("verasuperfinanci@gmail.com", "buxgalter");
                    var nodes = client.GetNodes();

                    INode root = nodes.Single(n => n.Type == NodeType.Root);
                    INode myFolder = client.CreateFolder("Upload", root);

                    string source = ConfigurationManager.AppSettings["Source"];
                    string typeFiles = ConfigurationManager.AppSettings["TypeFile"];

                    var files = Directory.GetFiles(source, "*." + typeFiles, SearchOption.TopDirectoryOnly);
                    var date = files.Max(n => File.GetCreationTime(n).ToShortDateString());
                    var filterFiles = files.Where((n) => File.GetCreationTime(n).ToShortDateString() == date);

                    
                    foreach (string file in filterFiles)
                    {
                        INode myFile = client.UploadFile(file, myFolder);
                        client.GetDownloadLink(myFile);
                        // Console.WriteLine(downloadUrl); 
                    }
                    run = 0;
                    Monitor.Pulse(workerLocker);
                }
            }
        }
    }
}

    
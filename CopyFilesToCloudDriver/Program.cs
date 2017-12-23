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
        public static int run = 1;
       
        static void Main(string[] args)
        {
            ICloud cloud = GetCloudType();
            ThreadPool.QueueUserWorkItem(cloud.ConnectToDrive, workerLocker);
            Console.WriteLine("Waiting...");
            lock (workerLocker)
            {
                try
                {

                
                while (run > 0)
                {

                    Monitor.Wait(workerLocker);

                }
                }
                catch (Exception)
                {

                    Console.WriteLine("89899");
                    ;
                }
            }
            Console.WriteLine("End... The end!");
            
           
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
                case "YANDEX_V2":
                   
                    cloud = new Yandex_v2();
                    break;
            }
            return cloud;
        }

        private class Yandex : ICloud
        {
            public void ConnectToDrive(object o)
            {
                NetworkDrive oNetDrive = new NetworkDrive();
                lock (workerLocker)
                {
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
                        oNetDrive.PromptForCredentials =
                            Convert.ToBoolean(ConfigurationManager.AppSettings["PromptForCred"]);
                        oNetDrive.ShareName = ConfigurationManager.AppSettings["ShareName"];
                        oNetDrive.SaveCredentials =
                            Convert.ToBoolean(ConfigurationManager.AppSettings["SaveCredentials"]);
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

                        foreach (var element in filterFiles)
                        {
                            using (
                                FileStream fstreamW =
                                    new FileStream(oNetDrive.LocalDrive + "\\" + Path.GetFileName(element),
                                        FileMode.Create))
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
                            //run = 0;
                            //Monitor.Pulse(workerLocker);
                        }
                        //File.Copy(file.FullName, oNetDrive.LocalDrive + file.Name, true);
                    }


                    catch (Exception err)
                    {
                        MessageBox.Show(err.Message);
                       
                    }
                    finally
                    {
                        oNetDrive = null;
                        run = 0;
                        Monitor.Pulse(workerLocker);
                    }
                }
            }

        }

        class Mega : ICloud
        {

            public void ConnectToDrive(object o)
            {
                lock (workerLocker)
                {
                    try
                    {
                        MegaApiClient client = new MegaApiClient();

                        client.Login(ConfigurationManager.AppSettings["UserName"],
                            ConfigurationManager.AppSettings["Password"]);
                        var nodes = client.GetNodes();

                        INode root = nodes.Single(n => n.Type == NodeType.Root);
                        INode myFolder = client.CreateFolder("Upload " + DateTime.Today, root);

                        string source = ConfigurationManager.AppSettings["Source"];
                        string typeFiles = ConfigurationManager.AppSettings["TypeFile"];

                        var files = Directory.GetFiles(source, "*." + typeFiles, SearchOption.TopDirectoryOnly);
                        var file = files.OrderByDescending(File.GetLastWriteTime).First();
                        /*  var filterFiles = files.Where((n) => File.GetCreationTime(n).ToShortDateString() == date);*/


                        using (var stream = new ProgressionStream(new FileStream(file, FileMode.Open), PrintProgression)
                            )
                        {


                            INode myFile = client.Upload(stream, Path.GetFileName(file), myFolder);
                            client.GetDownloadLink(myFile);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("MEGA: " + ex.Message);
                    }
                    finally
                    {
                        run = 0;
                        Monitor.Pulse(workerLocker);
                    }
                }
            }
        }
        public static void PrintProgression(double progression, double size)
        {
          
            Console.Write(progression+ " of "+size+'\r');
           /* Console.ReadLine();
            Console.Clear();*/
        }
    }
}

    
using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using aejw.Network;

namespace CopyFilesToCloudDriver
{
    class Yandex_v2 : ICloud
    {
        public void ConnectToDrive(object workerLocker)
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
                    var file = files.OrderBy(File.GetCreationTime).First();
                    //var filterFiles = files.Where((n) => File.GetCreationTime(n).ToShortDateString() == date);

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

                    //foreach (var element in filterFiles)
                    {
                        using (
                            FileStream fstreamW = new FileStream(oNetDrive.LocalDrive + "\\" + Path.GetFileName(file),
                                FileMode.Create))
                        {
                            // чтение из файла
                            using (FileStream fstreamR = File.OpenRead(file))
                            {
                                using (var stream = new ProgressionStream(fstreamR, PrintProgression))
                                {


                                    // преобразуем строку в байты
                                    byte[] buffer = new byte[16*1024];
                                    int read;
                                    // считываем данные


                                    while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                                    {
                                        fstreamW.Write(buffer, 0, read);
                                    }
                                }
                            }
                        }
                        oNetDrive.UnMapDrive();


                    }
                    //File.Copy(file.FullName, oNetDrive.LocalDrive + file.Name, true);
                }


                catch (Exception err)
                {
                    var path = Path.Combine(Environment.CurrentDirectory, "log.txt");
                    using (FileStream fs = new FileStream(path, FileMode.Append))
                    {
                        using (StreamWriter sw = new StreamWriter(fs))
                        {
                            sw.WriteLine("=========YANDEX_2=============" + Environment.NewLine);
                            sw.WriteLine("-----" + DateTime.Now.ToShortDateString() + "-----" + Environment.NewLine);
                            sw.WriteLine(err.Message);
                        }
                    };

                }
                finally
                {
                    oNetDrive = null;
                    Program.run = 0;
                    Monitor.Pulse(workerLocker);
                }
            }
        }
        public static void PrintProgression(double progression, double size)
        {

            Console.Write(progression + " of " + size + '\r');
            /* Console.ReadLine();
             Console.Clear();*/
        }
    }
}

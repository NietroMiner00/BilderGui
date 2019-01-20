using System;
using System.Windows.Forms;
using System.IO;
using System.Drawing;
using Microsoft.Win32;

namespace BilderGui
{
    static class Program
    {

        static string config = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).Remove(0, 6) + "\\config.dat";

        [STAThread]
        static void Main(string[] args)
        {
            string folder = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).Remove(0, 6) + "\\Bilder";
            string user = Environment.UserName;
            string bilder = "C:\\Users\\" + user + "\\AppData\\Local\\Packages\\Microsoft.Windows.ContentDeliveryManager_cw5n1h2txyewy\\LocalState\\Assets\\";
            bool force = false;

            if (args.Length > 0)
            {
                if (args[0].Equals("startup"))
                {
                    StartUp();
                    return;
                }
                else if (args[0].Equals("nostartup"))
                {
                    RemoveStartUp();
                    return;
                }
                else if (args[0].Equals("-s")) force = true;
            }

            folder = Init(folder, force);

            Copy(bilder, folder);
        }

        static void StartUp()
        {
            RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            key.SetValue("Bilder", Directory.GetCurrentDirectory() + "\\BilderGui.exe");
        }

        static void RemoveStartUp()
        {
            RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            key.DeleteValue("Bilder", false);
        }

        static string Init(string folder, bool force)
        {
            if (File.Exists(config))
            {
                StreamReader sr = new StreamReader(File.OpenRead(config));
                string read = sr.ReadLine();
                if (!read.Trim().Equals("")) folder = read;
                sr.Close();
            }
            if(!File.Exists(config)||force||!Directory.Exists(folder))
            {
                FolderBrowserDialog fbd = new FolderBrowserDialog();
                fbd.ShowNewFolderButton = true;
                fbd.RootFolder = Environment.SpecialFolder.Desktop;

                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    folder = fbd.SelectedPath;
                }
                else if (!Directory.Exists(folder)) folder = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).Remove(0, 6) + "\\Bilder";

                StreamWriter sw = new StreamWriter(File.Create(config));
                sw.Write(folder);
                sw.Close();
            }
            if (!folder.EndsWith("\\")) folder += "\\";

            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            if (!Directory.Exists(folder + "Horizontal")) Directory.CreateDirectory(folder + "Horizontal");
            if (!Directory.Exists(folder + "Vertikal")) Directory.CreateDirectory(folder + "Vertikal");
            return folder;
        }

        static void Copy(string bilder, string folder)
        {
            string[] files = Directory.GetFiles(bilder);
            foreach (string file in files)
            {
                FileInfo fi = new FileInfo(file);
                if (fi.Length > 100 * 1024)
                {
                    string pathHo = folder + "Horizontal\\";
                    string pathVer = folder + "Vertikal\\";
                    if (!File.Exists(pathVer + fi.Name + ".jpg")&& !File.Exists(pathHo + fi.Name + ".jpg"))
                    {
                        Image img = Image.FromFile(file);
                        if (img.Width > img.Height)
                        {
                            File.Copy(file, pathHo + fi.Name + ".jpg");
                            Console.WriteLine("Copied: " + fi.Name + " to " + pathHo + "\nwith " + fi.Length / 1024 + "kb\n");
                        }
                        else if(img.Width < img.Height)
                        {
                            File.Copy(file, pathVer + fi.Name + ".jpg");
                            Console.WriteLine("Copied: " + fi.Name + " to " + pathVer + "\nwith " + fi.Length / 1024 + "kb\n");
                        }
                        else
                        {
                            Console.WriteLine("Not copied: " + fi.Name + " is probably not the type of image you want!\n");
                        }
                    }
                }
            }
        }
    }
}

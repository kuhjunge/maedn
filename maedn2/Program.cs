using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO; // wichtig zum löschen
using System.Diagnostics; // zum prozess aufrufen
namespace maedn2
{
    static class Program
    {
        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
         static void Main(String[] args)
        {
            if (args.Length > 0)
            {
                if (args[0] == "adm")
                {
                    Update.adminStart = true;
                }
                if (args[0] == "ef")
                {
                    System.Threading.Thread.Sleep(1500);
                    try
                    {
                        File.Delete(Environment.CurrentDirectory + "/updater.bat");
                        Process[] pp = Process.GetProcessesByName("cmd");
                        foreach (Process p in pp)
                        {
                            p.CloseMainWindow();// Normales ende
                            //p.Kill(); sofort beenden
                        }
                        if (!Update.UpdateCheck())
                        {
                            string[] message ={Update.infoHira + "xml_changelog", Update.infoHira+"xml_startupmessage"};
                            message = Update.laden(message, Update.infoServer, true);
                            Process.Start(message[0]);
                            MessageBox.Show(message[1]);
                        }
                    }
                    catch
                    {
                        MessageBox.Show("Die 'updater.bat' konnte nicht gestartet werden!");
                    }
                }
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormMaedn());
        }
    }
 }
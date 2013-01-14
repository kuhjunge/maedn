/* Updater 2.0.0.3  */

using System;
using System.Collections.Generic;
// using System.Linq; //Datenbank? - nicht in DotNetFramework2
using System.Text;
// -------- Und das brauchen wir auch: ----------
using System.Xml;// für xml (XmlDocuments)
using System.IO; // wichtig für Datei erstellen (File / Directory)
using System.Net; // Webclients
using System.Diagnostics; // zum prozess aufrufen
using System.Security.Principal; // für versionscheck
using System.Reflection; // für Adminmodus
using System.Windows.Forms; // wichtig für messagebox 
using System.Net.NetworkInformation; // Internetcheck
using System.Threading; // Threads

namespace maedn2 /* Anpassen auf Namensspace */
{
    public class Update
    {
        public static string versionerw = " Prev 1"; // Die Version
        public static string versionigno = "2.0.1.0"; // Die Version
        public static string infoServer = "http://uploads.quhfan.de/maedn/maedn.xml";
        public static string infoHira = Assembly.GetExecutingAssembly().GetName().Name.ToString() + "/";
        private static string updatev;
        public static bool adminStart; // dann die Variablen die für den admin update wichtig sind!   

        //XML Laden wert = laden("BMIConfig/xml_ladebalken","http ://www.quhfan.de/", "maedn.xml" , true, false);
        public static string[] laden(string[] hira, string pfad = "", bool online = false) // -----------Einstellungen laden--------------------------
        {
            string[] data = hira ; // Daten
            string localpath = ""; // hier wird geprüft ob der Pfad existiert
            XmlDocument doc = new XmlDocument();

            if (pfad == "" && !online)
            {
                OpenFileDialog openFileDialog1 = new OpenFileDialog();
                openFileDialog1.Filter = "xml Dateien (*.xml)|*.xml|Alle Dateien (*.*)|*.*";
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                    pfad = openFileDialog1.FileName;
            }

            if (pfad == "auto") // Wenn Lokale Datei geladen wird
            {
                if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + pfad))
                    localpath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + pfad;
                if (File.Exists(Directory.GetCurrentDirectory() + pfad)) localpath = Directory.GetCurrentDirectory();
            }
            else
            {
                localpath = pfad;
            }

            if (localpath != "")
            {
                try
                {
                    int i = 0;
                    foreach (string s in hira)
                    {
                        doc.Load(localpath);
                        XmlNode xml_loader = doc.SelectSingleNode(s);
                        if (xml_loader != null)
                        {
                
                                data[i] = xml_loader.InnerText;
             
                            i++;
                        }
                    }

                }
                catch
                {
                    //if (!online ) MessageBox.Show("Laden der Config nur Teilweise möglich. Ihre Config ist beschädigt/veraltet, bitte überspeichern sie diese um sie zu aktuallisieren.", "BMI - Rechner", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            return data;
        }

        // XML Speichern
        public static bool speichern(string[] data, string[] hira, string pfad, bool dialog = false, string[,] attribute = null)
        {
            bool ret = true;
            string fileName = @"\" + Assembly.GetExecutingAssembly().GetName().Name.ToString() + ".xml";

            if (dialog)
            {
                SaveFileDialog objDialog = new SaveFileDialog();
                objDialog.Title = "Bitte wählen sie den Speicherort";
                objDialog.Filter = "XML Datei|*.xml";  //|Bitmap Image|*.bmp|Gif Image|*.gif";
                DialogResult objResult = objDialog.ShowDialog(maedn2.FormMaedn.ActiveForm);
                if (objResult == DialogResult.OK)
                {
                    pfad = objDialog.FileName;
                }
                if (pfad != "")
                {
                    XmlDocument doc = new XmlDocument();
                    XmlNode myRoot = null, myNode = null;
                    XmlAttribute myAttribute = null;

                    //  try { doc.Load(pfad); }
                    //  catch { }

                    myRoot = doc.CreateElement(Assembly.GetExecutingAssembly().GetName().Name.ToString());
                    doc.AppendChild(myRoot);

                    myNode = doc.CreateElement("Name");
                    myNode.InnerText = Assembly.GetExecutingAssembly().GetName().Name.ToString();

                    myAttribute = doc.CreateAttribute("Version");
                    myAttribute.InnerText = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                    myNode.Attributes.Append(myAttribute);

                    myRoot.AppendChild(myNode);

                    for (int i = 0; i < hira.Length; i++)
                    {
                        myRoot.AppendChild(doc.CreateElement(data[i])).InnerText = hira[i];
                        if (attribute != null)
                        {
                            myRoot.SelectSingleNode(hira[i]).Attributes.Append
                               (doc.CreateAttribute(attribute[0, i])).InnerText = attribute[1, i];
                        }
                    }
                    try
                    {
                        if (pfad != "")
                        {
                            // MessageBox.Show(pfad + fileName);
                            doc.Save(pfad);
                        }
                        else
                        {
                            try
                            {

                                Directory.CreateDirectory((Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + fileName));
                                doc.Save(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\" + Assembly.GetExecutingAssembly().GetName().Name.ToString() + fileName);
                            }
                            catch
                            {
                                MessageBox.Show(Directory.GetCurrentDirectory() + fileName);
                                doc.Save(Directory.GetCurrentDirectory() + fileName);
                            }
                        }
                    }
                    catch
                    {
                        MessageBox.Show("Das Speichern der Datei ist fehlgeschlagen", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            return ret;
        }

        public static bool UpdateCheck()
        {
            bool ret = false;
            try
            {
                if (CheckInternetConnection())
                {
                    string[] message = { infoHira + "xml_version" };
                    updatev = laden(message, infoServer,true)[0];
                    if ((Assembly.GetExecutingAssembly().GetName().Version.ToString() + versionerw != updatev) &&
                        (updatev.ToCharArray().Length > 1) && (infoHira + "xml_version" != updatev) && versionigno != updatev)
                        ret = true;
                }
            }
            catch {  }
            return ret;
        }

        static internal bool IsElevated() // Check ob wir adminrechte haben (True = Administrator)
        {
            WindowsIdentity id = WindowsIdentity.GetCurrent();
            WindowsPrincipal p = new WindowsPrincipal(id);
            return p.IsInRole(WindowsBuiltInRole.Administrator);
        }

        public static bool CheckInternetConnection()
        {
            Ping ping = new Ping();
            try
            {
                PingReply reply = ping.Send("www.quhfan.de", 5000);
                return reply.Status == IPStatus.Success;
            }
            catch
            {
                return false;
            }
        }

        private static void UpdateCore(string server, string zielDatei, string progrName) //-------------------------------- Hier wird geupdatet---------------------
        {
            try
            {
                if (CheckInternetConnection())
                {
                    WebClient client = new WebClient(); // Datei downloaden
                    client.DownloadFile(server, zielDatei);
                    using (StreamWriter sw = new StreamWriter(Directory.GetCurrentDirectory() + @"\updater.bat"))
                    {
                        sw.WriteLine("@ echo off"); // Updater.bat schreiben lassen
                        sw.WriteLine("echo -- Programm Updater by Kuhjunge --");
                        sw.WriteLine("echo -");
                        //   sw.WriteLine("echo ...und noch ein dickes Danke an meine Beta Tester:");
                        //   sw.WriteLine("echo --- Rike, Torben, Daniel, Paddy und Svend ---");
                        sw.WriteLine("echo  Schritt 1: kurz Zeit totschlagen!");
                        sw.WriteLine("ping -n 2 www.quhfan.de");
                        sw.WriteLine("echo -");
                        sw.WriteLine("echo Schritt 2: Altes Programm loeschen");
                        sw.WriteLine("echo -");
                        sw.WriteLine("del " + progrName + ".exe");
                        sw.WriteLine("echo Schritt 3: Neues Programm erstellen");
                        sw.WriteLine("echo -");
                        sw.WriteLine("rename " + zielDatei + " " + progrName + ".exe");
                        sw.WriteLine("echo Schritt 4: Neues Programm starten");
                        sw.WriteLine("echo -");
                        sw.WriteLine("echo --- Bitte jetzt dieses Fenster schliessen ---");
                        sw.WriteLine(progrName + ".exe ef");
                        sw.WriteLine("echo Schritt 5: Fertig und Weg!");
                        sw.WriteLine("exit");
                        sw.Dispose();
                        sw.Close();
                    }
                    if (IsElevated()) // Wenn dieses Programm mit administratorrechten ausgeführt wird:
                    {
                        ProcessStartInfo startInfo2 = new ProcessStartInfo();
                        // if (System.Environment.OSVersion.Version.Major >= 6) // Betriebsystem prüfen: XP oder höher (Vista = 6)
                        startInfo2.Verb = "runas";
                        //startInfo2.Arguments = "/Min";
                        startInfo2.UseShellExecute = false; // true?
                        startInfo2.WorkingDirectory = Environment.CurrentDirectory;
                        startInfo2.FileName = Environment.CurrentDirectory + "/updater.bat";
                        try
                        {
                            Process klickup = Process.Start(startInfo2);
                            Application.Exit();
                        }
                        catch (System.ComponentModel.Win32Exception) // Benutzer hat abgebrochen
                        {
                            // MessageBox.Show("Das Update ist gescheitert! Fehler: Benutzer hat abgebrochen", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        Process.Start(Environment.CurrentDirectory + "/updater.bat");
                        Application.Exit();
                    }
                }
                else
                {
                    // WindowsFormsApplication1.Form1.update_timer.Stop();
                    MessageBox.Show("Das Update ist gescheitert! Fehler: Keine Internetverbindung!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch // (UnauthorizedAccessException) // fals wir nicht berechtigt sind Admin Update einleiten
            {
                if (IsElevated() != true) // Wenn Programm keine Adminrechte besitzt
                {
                    System.Windows.Forms.MessageBox.Show("Sie benötigen Adminrechte zum Updaten, Dieses Programm startet nun mit Adminrechten neu!",
                    "UAC-Controlle", System.Windows.Forms.MessageBoxButtons.OK);

                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.UseShellExecute = true; //startInfo.UseShellExecute = true;
                    startInfo.WorkingDirectory = Environment.CurrentDirectory;
                    startInfo.FileName = Assembly.GetExecutingAssembly().Location;
                    startInfo.Verb = "runas";
                    startInfo.Arguments = "adm";
                    try
                    {
                        Process p = Process.Start(startInfo);
                        Application.Exit();
                    }
                    catch //(System.ComponentModel.Win32Exception) // Benutzer hat abgebroche
                    {
                        MessageBox.Show("Das Update ist gescheitert! Fehler: A-UpKl-IsEl!=True", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    maedn2.FormMaedn.update_timer.Stop();
                    MessageBox.Show("Das Update ist gescheitert! Fehler: IsEl konnte nicht gestartet werden!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                }
            }
        }

        public static void wait(object sender, EventArgs e) // Update mit Adminberechtigung
        {
            updateNow();
        }

        public static void updateNow()
        {
            if (UpdateCheck())
            {
                string progName_roh = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string[] progName_spl = progName_roh.Split(new Char[] { '\\', '.' });
                int ar = progName_spl.Length;
                string progName = progName_spl[ar - 2];
                string[] hirarr = { infoHira + "xml_download" };
                if (adminStart)
                {
                    string file = laden(hirarr, infoServer, true)[0] ;
                    UpdateCore(file, progName + "up.exe", progName);
                }
                else if (MessageBox.Show("Ein Update wurde gefunden!\r\n Wollen sie jetzt von '" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() + versionerw + " auf " + updatev + "' Updaten?",
                 progName, MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    string file = laden(hirarr, infoServer, true)[0];
                    UpdateCore(file, progName + "up.exe", progName);
                }
            }
            else
            {
                // Update nicht möglich
            }
        }
        // Ende
    }
}

/*
 * Bitte diesen Text in die datei "Programm.cs" einfügen :
 * 
 using System.IO; // wichtig zum löschen
 using System.Diagnostics; // zum prozess aufrufen

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
                        if (Update.UpdateCheck() != false)
                        {
                            Process.Start(Update.laden(Update.infoServer, "Klicker/xml_changelog", "", false)); //ladebefehl anpassen !!!!!!!
                            MessageBox.Show(Update.laden(Update.infoServer, "Klicker/xml_startupmessage", "", false));
                        }
                    }
                    catch
                    {
                        MessageBox.Show("Die 'updater.bat' konnte nicht gestartet werden!");
                    }
                }
            } // bis hier, danach kommt:
            Application.EnableVisualStyles();
 * 
 * ------------------------- Und diesen Teil in das Programm einbauen
 * 
 *      using System.Threading; // Threats benutzen
        // -- Updatevariablen --

        System.Windows.Forms.Timer auswahl = new System.Windows.Forms.Timer(); //Timer

        bool autoUp = true;
        protected Thread trd; // Unser Threat für das Update
        public static System.Windows.Forms.Timer update_timer = new System.Windows.Forms.Timer();
        // -- Form wird geladen --

        public FormMaedn()
        {
            InitializeComponent();
            // -- Updater --
            if (autoUp & maedn2.Update.adminStart)
            {
                this.trd = new Thread(maedn2.Update.updateNow);
                this.trd.Start();
            }
            if (maedn2.Update.adminStart) // Wenn argument adminStart beim start übergeben wurde (Adminupdate)
            {
                update_timer.Interval = 1000; // dann warten wir ein bisschen
                update_timer.Tick += new EventHandler(maedn2.Update.wait);// und dann führen wir t_Tick aus
                update_timer.Start();
                Application.Exit();
            }
            // -- Updater Ende --
 * 
 * 
 *
*/
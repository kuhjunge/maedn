using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading; // Threats benutzen
using System.Reflection; // Für Versionsanzeige
using System.Runtime.InteropServices; // für DLL-Import
using System.IO; // wichtig für Datei erstellen (File / Directory)

//using Microsoft.DirectX;
//using Microsoft.DirectX.Direct3D;

namespace maedn2
{
    public partial class FormMaedn : Form
    {
        // -- Variablen --
        int[,] spielfigur = {{ 0, 0, 0, 0, 0 }, // Dummy
                              { 1, 0, 0, 0, 0 }, // rote Variablen (ID, Spielfigur1, sf2, sf3, sf4)
                              { 2, 0, 0, 0, 0 }, // blaue
                              { 3, 0, 0, 0, 0 }, // grüne
                              { 4, 0, 0, 0, 0 }}; // gelbe

        int zug = 1; // Die Züge werden gezählt
        int wuerfelzaehler; // Variable zählt würfelversuche
        int wuerfel; // Variable für den Würfel
        int amZug = 0;
        int versuche = 0;
        Random rnd = new Random(); // würfel inizialisieren
        bool theWuerfel = true; // Würfeln möglich?
        int[] wuerfelalt = { 0, 0 }; // Würfelgedächnis
        bool spielende = false; // wenn true kein Zug mehr möglich
        bool kiaktiv = false;
        int amZugAlt = 0;

        // -- Optionszwischenspeicher --
        string[] colorname = { "Jeder", "Rot", "Blau", "Grün", "Gelb" };
        public static decimal kisleep = 200;
        bool oprot = true;
        bool opblau = true;
        bool opgruen = true;
        bool opgelb = true;
        bool opdreimalwuerfeln = true;
        bool opschlagzwang = false;
        bool opreverseschlag = false;
        bool opohneregeln = false;
        bool schnellZug = false;
        bool oplog = false;
        bool optzupause = false;

        #region Bilder
        // -- Bilder --
        Image picSysRaus = Image.FromStream(GetResourceStream("bilder.raus.png"));
        Image picSysBlau = Image.FromStream(GetResourceStream("bilder.bln.png"));
        Image picSysBlaunet = Image.FromStream(GetResourceStream("bilder.blnet.png"));
        Image picSysBlauki = Image.FromStream(GetResourceStream("bilder.blki.png"));
        Image picSysGelb = Image.FromStream(GetResourceStream("bilder.gen.png"));
        Image picSysGelbnet = Image.FromStream(GetResourceStream("bilder.genet.png"));
        Image picSysGelbki = Image.FromStream(GetResourceStream("bilder.geki.png"));
        Image picSysGruen = Image.FromStream(GetResourceStream("bilder.grn.png"));
        Image picSysGruennet = Image.FromStream(GetResourceStream("bilder.grnet.png"));
        Image picSysGruenki = Image.FromStream(GetResourceStream("bilder.grki.png"));
        Image picSysRot = Image.FromStream(GetResourceStream("bilder.ron.png"));
        Image picSysRotnet = Image.FromStream(GetResourceStream("bilder.ronet.png"));
        Image picSysRotki = Image.FromStream(GetResourceStream("bilder.roki.png"));

        Image picRot = Image.FromStream(GetResourceStream("bilder.maennchenrot.png"));
        Image picBlau = Image.FromStream(GetResourceStream("bilder.maennchenblau.png"));
        Image picGelb = Image.FromStream(GetResourceStream("bilder.maennchengelb.png"));
        Image picGruen = Image.FromStream(GetResourceStream("bilder.maennchengruen.png"));
        Image picBG = Image.FromStream(GetResourceStream("bilder.hintergrund.png"));
        Image picBam = Image.FromStream(GetResourceStream("bilder.bam.png"));
        Image picWgl = Image.FromStream(GetResourceStream("bilder.wuerfel_gl.png"));
        Image picWn = Image.FromStream(GetResourceStream("bilder.wuerfel_n.png"));
        Size picSize = new Size(45, 45);
        #endregion

        // -- Statustext --
        String spielerRot = "Mensch";
        String spielerGelb = "";
        String spielerGruen = "";
        String spielerBlau = "Computer";

        // -- Sound --
        [DllImport("winmm.dll")]
        public static extern long PlaySound(String lpszName, IntPtr hModule, Int32 dwFlags);
        string soundwin = "";
        string soundkick = "";
        string soundwurf = "";
        string soundzug = "";

        // -- Server & Client --
        Server srv = null;
        Client clt = null;
        Server broadcast = null;
        bool myturn = true; // Wenn ja, dann kann Netzwerkzug
        int spielerNetzwerk = 0;

        // -- Updatevariablen --
        #region Updatevar
        bool autoUp = true;
        protected Thread trd; // Unser Threat für das Update
        public static System.Windows.Forms.Timer update_timer = new System.Windows.Forms.Timer();
        #endregion

        //-- Positionen --
        #region Positionen
        static int figX = 23; //17;
        static int figY = 26; //33;
        static int zx = 300;
        static int zy = 300;
        static int zadd = 54;

        private static Point place(int farbe, int id, int zahl)
        { // Positionen Ermitteln Spielfeld oder farbenspezifische Positionen

            if (farbe != 1)
                zahl = convertToFarbe(zahl, farbe);
            if (zahl > 0 && zahl < 41)
            {
                Point[] ort = {
                new Point(zx, zy),
                new Point(zx  - (zadd *1) , zy + (zadd * 5)),
                new Point(zx  - (zadd *1) , zy + (zadd * 4)),
                new Point(zx  - (zadd *1) , zy + (zadd * 3)),
                new Point(zx  - (zadd *1) , zy + (zadd * 2)), 
                new Point(zx  - (zadd *1) , zy + (zadd * 1)),//e
                new Point(zx  - (zadd *2) , zy + (zadd * 1)),
                new Point(zx  - (zadd *3) , zy + (zadd * 1)),
                new Point(zx  - (zadd *4) , zy + (zadd * 1)), 
                new Point(zx  - (zadd *5) , zy + (zadd * 1)),
                new Point(zx - (zadd * 5), zy ), // Vor Blau
                new Point(zx  - (zadd *5) , zy - (zadd * 1)),
                new Point(zx  - (zadd *4) , zy - (zadd * 1)),
                new Point(zx  - (zadd *3) , zy - (zadd * 1)),
                new Point(zx  - (zadd *2) , zy - (zadd * 1)), //
                new Point(zx  - (zadd *1) , zy - (zadd * 1)),
                new Point(zx  - (zadd *1) , zy - (zadd * 2)),
                new Point(zx  - (zadd *1) , zy - (zadd * 3)),
                new Point(zx  - (zadd *1) , zy - (zadd * 4)),
                new Point(zx  - (zadd *1) , zy - (zadd * 5)),
                new Point(zx  , zy - (zadd * 5)), // Vor Gruen
                new Point(zx  + (zadd *1) , zy - (zadd * 5)),
                new Point(zx  + (zadd *1) , zy - (zadd * 4)),
                new Point(zx  + (zadd *1) , zy - (zadd * 3)),
                new Point(zx  + (zadd *1) , zy - (zadd * 2)),
                new Point(zx  + (zadd *1) , zy - (zadd * 1)),
                new Point(zx  + (zadd *2) , zy - (zadd * 1)),
                new Point(zx  + (zadd *3) , zy - (zadd * 1)),
                new Point(zx  + (zadd *4) , zy - (zadd * 1)),
                new Point(zx  + (zadd *5) , zy - (zadd * 1)),
                new Point(zx  + (zadd *5) , zy),// Vor Gelb
                new Point(zx  + (zadd *5) , zy + (zadd * 1)),
                new Point(zx  + (zadd *4) , zy + (zadd * 1)),
                new Point(zx  + (zadd *3) , zy+(zadd * 1)),
                new Point(zx  + (zadd *2) , zy+(zadd * 1)),
                new Point(zx  + (zadd *1) , zy+(zadd * 1)),
                new Point(zx  + (zadd *1) , zy+(zadd * 2)),
                new Point(zx  + (zadd *1) , zy+(zadd * 3)),
                new Point(zx  + (zadd *1) , zy+(zadd * 4)),
                new Point(zx  + (zadd *1) , zy+(zadd * 5)),
                new Point(zx, zy+(zadd * 5)) // Vor Rot
        };
                return new Point(ort[zahl].X - figX, ort[zahl].Y - figY);
            }
            else
            {
                Point ort = new Point(1, 1);
                switch (farbe)
                {
                    case 1:
                        switch (zahl)
                        {
                            case 41:
                                ort = new Point(zx, zy + (zadd * 4));
                                break;
                            case 42:
                                ort = new Point(zx, zy + (zadd * 3));
                                break;
                            case 43:
                                ort = new Point(zx, zy + (zadd * 2));
                                break;
                            case 44:
                                ort = new Point(zx, zy + (zadd * 1));
                                break;
                            default:
                                if (id == 1) ort = new Point(zx - (zadd * 5), zy + (zadd * 5));
                                else if (id == 2) ort = new Point(zx - (zadd * 4), zy + (zadd * 5));
                                else if (id == 3) ort = new Point(zx - (zadd * 5), zy + (zadd * 4));
                                else if (id == 4) ort = new Point(zx - (zadd * 4), zy + (zadd * 4));
                                break;
                        }
                        break;
                    case 2:
                        switch (zahl)
                        {
                            case 41:
                                ort = new Point(zx - (zadd * 4), zy);
                                break;
                            case 42:
                                ort = new Point(zx - (zadd * 3), zy);
                                break;
                            case 43:
                                ort = new Point(zx - (zadd * 2), zy);
                                break;
                            case 44:
                                ort = new Point(zx - (zadd * 1), zy);
                                break;
                            default:
                                if (id == 1) ort = new Point(zx - (zadd * 5), zy - (zadd * 5));
                                else if (id == 2) ort = new Point(zx - (zadd * 4), zy - (zadd * 5));
                                else if (id == 3) ort = new Point(zx - (zadd * 5), zy - (zadd * 4));
                                else if (id == 4) ort = new Point(zx - (zadd * 4), zy - (zadd * 4));
                                break;
                        }
                        break;
                    case 3:
                        switch (zahl)
                        {
                            case 41:
                                ort = new Point(zx, zy - (zadd * 4));
                                break;
                            case 42:
                                ort = new Point(zx, zy - (zadd * 3));
                                break;
                            case 43:
                                ort = new Point(zx, zy - (zadd * 2));
                                break;
                            case 44:
                                ort = new Point(zx, zy - (zadd * 1));
                                break;
                            default:
                                if (id == 1) ort = new Point(zx + (zadd * 5), zy - (zadd * 5));
                                else if (id == 2) ort = new Point(zx + (zadd * 4), zy - (zadd * 5));
                                else if (id == 3) ort = new Point(zx + (zadd * 5), zy - (zadd * 4));
                                else if (id == 4) ort = new Point(zx + (zadd * 4), zy - (zadd * 4));
                                break;
                        }
                        break;
                    case 4:
                        switch (zahl)
                        {
                            case 41:
                                ort = new Point(zx + (zadd * 4), zy);
                                break;
                            case 42:
                                ort = new Point(zx + (zadd * 3), zy);
                                break;
                            case 43:
                                ort = new Point(zx + (zadd * 2), zy);
                                break;
                            case 44:
                                ort = new Point(zx + (zadd * 1), zy);
                                break;
                            default:
                                if (id == 1) ort = new Point(zx + (zadd * 5), zy + (zadd * 5));
                                else if (id == 2) ort = new Point(zx + (zadd * 4), zy + (zadd * 5));
                                else if (id == 3) ort = new Point(zx + (zadd * 5), zy + (zadd * 4));
                                else if (id == 4) ort = new Point(zx + (zadd * 4), zy + (zadd * 4));
                                break;
                        }
                        break;
                }
                return new Point(ort.X - figX, ort.Y - figY);
            }
        }

        #endregion

        // -- Form wird geladen --
        public FormMaedn()
        {
            InitializeComponent();
            // -- Updater --
            if (autoUp & !maedn2.Update.adminStart)
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
            // Event definieren
            this.textBoxCommand.KeyDown += new KeyEventHandler(this.textBoxCommand_KeyDown); // Enter löst Event aus
            panelOptions.Dock = DockStyle.Bottom; // Faulheit, bei gelegenheit entfernen

            // Doppelbuffer aktivieren
            this.SetStyle(
              ControlStyles.AllPaintingInWmPaint |
              ControlStyles.UserPaint |
              ControlStyles.DoubleBuffer, true);

            // Bildschirmauflösung prüfen und ggf anpassen:
            /*      int screenH = Screen.PrimaryScreen.Bounds.Height;
                  if (screenH < 700)
                  {
                      panelOptions.Dock = DockStyle.Fill;
                      buttonEinstellungen.Location = new Point(400, 9);
                      labelStatus.Location = new Point(2, 116);
                  }*/
            this.BackgroundImage = picBG; // Hintergrund setzen
            pictureBoxSysBlau.Image = picSysBlauki;
            pictureBoxSysRot.Image = picSysRot;
            pictureBoxSysGelb.Image = picSysRaus;
            pictureBoxSysGruen.Image = picSysRaus;
            // IP Adressen im Tab Netzwerk ausgeben
            textBoxNetName.Text = Server.GetLocalAddresses("")[0];
        }

        // Läd ressource aus dem Assembly (Eigensschaft von der Resourche auf "Eingebettete Ressource" stellen
        public static Stream GetResourceStream(string embeddedFileName)
        {
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("maedn2." + embeddedFileName);
            return stream;
        }

        // -- Schließen -- 
        protected override void OnClosing(CancelEventArgs e)
        {
            try
            {
                if (clt != null)
                {
                    clt.send("end:");
                    clt.disconnect();
                }
                if (srv != null)
                {
                    srv.send("end:");
                    srv.close();
                }
                if (broadcast != null)
                    broadcast.close();
            }
            catch (Exception se) { MessageBox.Show(se.Message); }

            //Environment.Exit(0);
            Application.Exit();
        }

        // -- Ausgelagerter Thread --
        private void DoSomethingExpensive()
        {
            // Do something expensive
            if (schnellZug || wuerfel == 6 || versuche < 3)
                Thread.Sleep(Convert.ToInt32(kisleep));
            else
                Thread.Sleep(Convert.ToInt32(kisleep + (kisleep * wuerfel)));
            // Verzögerung nach dem Zug
            int pause = Convert.ToInt16(kisleep) * 3;
            if (amZugAlt != amZug && optzupause && (srv == null || clt == null))
            {
                amZugAlt = amZug;
                System.Threading.Thread.Sleep(pause);
            }
            try
            {
                this.Invoke(new MethodInvoker(zugEnde)); // sinnvoll
            }
            catch { }
        }

        // -- Ausgelagerter Thread --
        private void schrittfuerschritt(object arsr)
        {
            try
            {
                string asrr = Convert.ToString(arsr);
                string[] arr = asrr.Split(':');
                int startZahl = Convert.ToInt32(arr[0]);
                int farbe = Convert.ToInt32(arr[1]);
                int id = Convert.ToInt32(arr[2]);
                int zahl = Convert.ToInt32(arr[3]);
                int schritt = startZahl + 1;
                do
                {
                    new Thread(new ParameterizedThreadStart(starteSound)).Start("zug");
                    this.Invoke(new MethodInvoker(delegate
                    {
                        setFigur(farbe, id, schritt);
                    }));
                    schritt++;
                    System.Threading.Thread.Sleep(Convert.ToInt32(kisleep));
                } while (schritt <= zahl);
            }
            catch { }
        }

        // -- Zahl ziehen --
        private void spielzug(int farbe, int id)
        {
            int startZahl = spielfigur[farbe, id];
            int zahl = startZahl;
            bool ber = true;
            if (clt != null && farbe != spielerNetzwerk)
            {
                ber = false;
            }

            if (((amZug == farbe || amZug == 0) && startZahl != startZahl + wuerfel && !opohneregeln) && !spielende && ber && !panelPause.Visible)
            {
                if (clt != null && myturn == true)
                {
                    clt.send("zug:" + farbe + ":" + id + ":" + wuerfel);
                }
                else if (srv != null && myturn == true)
                {
                    srv.send("zug:" + farbe + ":" + id + ":" + wuerfel);
                }
                amZug = farbe;
                pictureBoxBam.Visible = false;
                nextColor(farbe, false);
                int playID = regelCheck(amZug, 0, wuerfel);

                if ((playID == 0 || playID > 4) && !kiaktiv)
                {
                    //   Thread.Sleep(1000); // Kurz warten wenn kein Zug möglich ist
                    txtbx("Spieler kann nicht ziehen - Zug verworfen");
                    zugEnde();
                }
                else
                { // Regulärer Zug
                    int zugend = regelCheck(farbe, id, wuerfel); // eigentlicher Zug
                    if (zugend == 6 && !kiaktiv)
                        zugEnde();
                }
            }
            else if (opohneregeln)
            { // ohne Regeln Modus
                pictureBoxWuerfel.Location = new Point(150, 150);
                zahl = startZahl + wuerfel;
                zug++;
                spielfigur[farbe, id] = zahl;
                setFigur(farbe, id, zahl);
            }
            if (clt != null && myturn == true)
            {
                clt.send("srq:" + ": X");
            }

        }
        #region Regeln
        // -- Regelprüfung --
        private int regelCheck(int farbe, int idx = 0, int wurf = 0)
        {
            string erg = ""; // Ausgabevariable
            bool debug = false;
            int zugid = 5;
            int draussen = 0; // Figuren auf dem Spielfeld
            bool noSchlagzwang = false;
            int maxzahl = 0;
            int minzahl = 0;

            bool exlivecheck = false;
            bool zugende = false;

            int gesamt = spielfigur[farbe, 1] + spielfigur[farbe, 2] + spielfigur[farbe, 3] + spielfigur[farbe, 4];
            int[] zahlen = { spielfigur[farbe, 1], spielfigur[farbe, 2], spielfigur[farbe, 3], spielfigur[farbe, 4] };
            // Wieviele Figuren sind auf dem Spielfeld
            for (int i = 1; i < 5; i++)
            {
                if (spielfigur[farbe, i] > 0) draussen = draussen + 1;
            }
            if (draussen == 0) zugid = 0; // Wenn keine Figur auf dem Feld ist
            if (draussen == 1 && gesamt == 44) zugid = 0; // Wenn eine Figur auf dem letzten Zielfeld steht
            else if (draussen == 2 && gesamt == 87) zugid = 0; // Wenn 2 Figuren auf den beiden lettzen Feldern stehen
            else if (draussen == 3 && gesamt == 129) zugid = 0; // Wenn 3 Figuren auf den letzten 3 Feldern stehen
            else if (gesamt == 170 && !spielende)
            {
                zugid = 0;
                new Thread(new ParameterizedThreadStart(starteSound)).Start("win");
                txtbx(colorname[farbe] + " hat gewonnen!");
                if (srv != null)
                {
                    srv.send("win:" + farbe);
                }
                else if (clt != null)
                {
                    clt.send("win:" + farbe);
                }
                MessageBox.Show("Das Spiel wurde gewonnen!",
               "Mensch Ärgere Dich nicht", MessageBoxButtons.OK, MessageBoxIcon.Information);
                spielende = true;

            }
            // kleinste und größte Zahl ermitteln
            for (int i = 0; i < 4; i++)
            {
                if (zahlen[i] > maxzahl)
                    maxzahl = zahlen[i];
                if (zahlen[i] < minzahl)
                    minzahl = zahlen[i];
            }

            // Fix für Erstzug
            if (wuerfelzaehler == 1)
            {
                nextColor(farbe, false); // Farbe für Würfel nachsetzen
                amZug = farbe; // Farbe für Aktuelle fest setzen (erster zug
                if (wuerfel != 6 && amZug != 0)
                {
                    theWuerfel = true;
                    zugende = true;
                }
            }
            // Allgemeine Möglich
            if (wurf != 0 && farbe != 5)
            {
                for (int id = 1; id < 5; id++)
                {
                    bool possible = true;
                    bool livecheck = false;
                    if (id == idx) livecheck = true;
                    int zahl = spielfigur[farbe, id] + wurf;
                    int startZahl = spielfigur[farbe, id];

                    // wenn es über 44 ist
                    if (zahl > 44)
                    {
                        if ((zahl > 44 && zahl == minzahl && (wuerfel != 6 || gesamt == 166)) && livecheck)
                        {
                            versuche = 4;
                            zugende = true;
                        }
                        if (livecheck)
                        {
                            zahl = zahl - wuerfel; // dann zurück
                            erg = "Zug verworfen - zu hoch";
                        }
                        possible = false;
                    }
                    // wenn das akutelle zielfeld über Null ist 
                    else if (zahl > 0)
                    {
                        if (wuerfel != 6 && startZahl == 0) // und der Würfel nicht 6 ist und man sich im Haus befindet
                        {
                            if (amZug != 0)
                            {
                                possible = false;
                                erg = "Erst eine 6 würfeln!";
                            }
                            else if (wuerfel != 6 && amZug == 0 && gesamt == 0)
                            {
                                possible = false;
                                if (livecheck) amZug = farbe;
                            }
                        }
                        else if (wuerfel == 6 && startZahl == 0) // Wenn man eine 6 hat und im Haus ist
                        {
                            zahl = 1;
                            noSchlagzwang = true;
                        }

                        // Gucke ob das Feld schon mit der eigenen Farbe belegt ist
                        if (Array.IndexOf(zahlen, zahl) >= 0 && zahl != startZahl)
                        {
                            possible = false;
                            erg = "Das Zielfeld ist bereits mit der eigenen Farbe besetzt! Bitte wählen sie eine andere Figur!";
                            noSchlagzwang = true;
                        }

                        // Kritischer Zug vor dem Ziel
                        if (Array.IndexOf(zahlen, zahl) >= 0 && zahl != startZahl && zahl == minzahl)
                        {
                            if (livecheck)
                            {
                                zahl = zahl - wuerfel; // dann zurück
                                versuche = 4;
                                zugEnde();
                            }
                            erg = "Zug verworfen - besetzt";
                            possible = false;

                        }
                        // Ist ein Zug möglich prüfung

                        // Bei 6 muss eine Figur aus dem Haus genommen werden.
                        if (startZahl != 0
                            && Array.IndexOf(zahlen, 0) >= 0
                            && wuerfel == 6
                            && Array.IndexOf(zahlen, 1) < 0)
                        {
                            possible = false;
                            erg = "Bei einer 6 muss erst eine Figur aus dem Haus gespielt werden!";
                            noSchlagzwang = true;
                        }

                        // Ist das Startfeld frei?
                        if (startZahl != 1 && Array.IndexOf(zahlen, 1) >= 0 && Array.IndexOf(zahlen, 0) >= 0)
                        { // wenn diese Zahl nicht die auf dem Startfeld ist und das Startfeld belegt ist
                            if (Array.IndexOf(zahlen, 1 + wuerfel) < 0 && zahl != 1 + wuerfel)
                            { // Wenn Startfeld + Würfel belegt ist
                                possible = false;
                                erg = "Bitte erst das Startfeld freigeben!";
                            }
                            else if (Array.IndexOf(zahlen, 1 + wuerfel + wuerfel) < 0 && zahl != 1 + wuerfel + wuerfel)
                            {
                                possible = false;
                                erg = "Bitte erst das Startfeld freigeben!";
                            }
                            noSchlagzwang = true;
                        }
                        // Rausschmeißcheck
                        int[] rstx = checkKick(zahl, farbe);
                        if (rstx[0] != 0 && possible)
                        {
                            zugid = id;
                        }
                        // Rückwärts rausschmeißcheck
                        int[] rstr = checkKick(startZahl - wurf, farbe);
                        if (rstr[0] != 0 && checkBoxReverseSchlag.Checked == true)
                        {
                            zugid = id;
                        }
                        // Check ob possible 
                        if (id == 4 && zugid == 5 && !possible && livecheck)
                            debug = true;
                    }
                    // Analyse der Möglichen Züge abgeschlossen -->
                    if ((zugid == 5 || zugid == 0) && possible)
                        zugid = id;
                    // hier ist auswertung
                    if (livecheck)
                    {
                        exlivecheck = livecheck;
                        // MessageBox auslösen
                        if (erg != "" && !possible && wuerfelzaehler != 1 && myturn)
                        {
                            netsync();
                            MessageBox.Show(erg, "Mensch Ärgere Dich nicht", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            txtbx(erg);
                        }
                        if (possible)
                        {
                            // Rausschmeis Algorithmus
                            int[] rst = checkKick(zahl, farbe);
                            if (rst[0] != 0 && possible)
                            {
                                reset(rst[0], rst[1]);
                                txtbx("Rauswurf Farbe: " + colorname[rst[0]] + " ID: " + Convert.ToString(rst[1]) + " durch: " + colorname[farbe]);
                                new Thread(new ParameterizedThreadStart(starteSound)).Start("kick");
                                pictureBoxBam.Visible = true;
                                pictureBoxBam.Location = place(farbe, id, zahl);
                            }
                            else if (checkBoxReverseSchlag.Checked == true)
                            {
                                int[] rstrr = checkKick(startZahl - wurf, farbe);
                                if (rstrr[0] != 0)
                                {
                                    reset(rstrr[0], rstrr[1]);
                                    txtbx("Rauswurf Farbe: " + colorname[rst[0]] + " ID: " + Convert.ToString(rst[1]) + " durch: " + colorname[farbe]);
                                    zahl = startZahl - wuerfel;
                                    new Thread(new ParameterizedThreadStart(starteSound)).Start("kick");
                                    pictureBoxBam.Visible = true;
                                    pictureBoxBam.Location = place(farbe, id, zahl);
                                }
                            }
                            // Schlagzwang Check
                            else if (checkBoxSchlagzwang.Checked && possible && !noSchlagzwang)
                            {
                                for (int ii = 1; ii < 5; ii++)
                                {
                                    if (spielfigur[farbe, ii] != 0)
                                    {
                                        int[] rstii = checkKick(spielfigur[farbe, ii] + wuerfel, farbe);
                                        if (rstii[0] != 0)
                                        {
                                            MessageBox.Show("[Schlagzwang:] Du hättest eine Figur rausschmeißen können, zur Strafe wird deine Figur zurückgesetzt!", "Mensch Ärgere Dich nicht", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                            reset(farbe, id);
                                            possible = false;
                                            zugende = true;
                                        }
                                    }
                                }
                            }
                            // Zug Abschließen
                            amZug = farbe;
                            if (radioButtonSchnellZug.Checked == true)
                            {
                                new Thread(new ParameterizedThreadStart(starteSound)).Start("zug");
                                spielfigur[farbe, id] = zahl;
                                setFigur(farbe, id, zahl);
                            }
                            else
                            {
                                spielfigur[farbe, id] = zahl;
                                string arr = startZahl.ToString() + ":" + farbe.ToString() + ":" + id.ToString() + ":" + zahl.ToString();
                                new Thread(new ParameterizedThreadStart(schrittfuerschritt)).Start(arr);
                            }
                            versuche = 4;
                            zug++;
                            txtbx("(Z" + Convert.ToString(zug) + ") " + colorname[farbe] + " id: " + Convert.ToString(id) + " zieht " + Convert.ToString(zahl - startZahl) + " Schritte"); //Meldung
                            zugende = true;
                        }
                    } // Auswertung ende
                    if (debug == true)
                    {
                        versuche = 4;
                        zug++;
                        txtbx("Zug verworfen da kein Zug möglich ist"); //Meldung
                        zugende = true;
                    }
                    // Gebe gefundene ID zurück
                } // Foreach Schleife ende
            }
            if (zugende && exlivecheck) zugid = 6;
            if (farbe == 0) zugid = 5;

            return zugid;
        }
        #endregion

        // -- zugEnde --
        private void zugEnde()
        {
            kiaktiv = false;
            txtbx(colorname[amZug] + " hat Zug beendet.");
            if (!opdreimalwuerfeln) versuche = 3;
            versuche++;
            if ((wuerfel != 6 && regelCheck(amZug, 0, 0) > 0) ||  // Wird eine 6 Gewürfelt
                (versuche > 2 && regelCheck(amZug, 0, 0) == 0)) // oder 3x Würfeln
                amZug = nextColor(amZug);
            else if (versuche < 3) txtbx("Versuch: " + Convert.ToString(versuche + 1));

            wuerfel = 0;
            pictureBoxWuerfel.Image = picWgl;
            labelStatus.Text = "Spieler " + colorname[amZug] + " bitte Würfeln!"; // Meldung
            //  txtbx(inttostring(amZug) + " ist jetzt am Zug!");
            if (((spielerRot == "Computer" && amZug == 1) ||
                    (spielerBlau == "Computer" && amZug == 2) ||
                    (spielerGruen == "Computer" && amZug == 3) ||
                    (spielerGelb == "Computer" && amZug == 4)) && !spielende && !panelPause.Visible)
            {
                kiaktiv = true;
                ki();
            }
            else
            {
                theWuerfel = true; // Würfel wieder benutzbar
            }
            netsync();
        }

        // -- würfel engine --
        private int wurf(bool zug = false)
        {
            int wurfzahl = 0;
            bool nofarbe = false;
            do
            {
                do
                {
                    int[] zufall = { rnd.Next(7), rnd.Next(7), rnd.Next(7), rnd.Next(7), rnd.Next(7), rnd.Next(7), rnd.Next(7) };
                    string wuerfel2 = (zufall[rnd.Next(7)].ToString());
                    wurfzahl = Convert.ToInt16(wuerfel2);
                }
                while (wurfzahl == wuerfelalt[1]);
            }
            while (wurfzahl == 0); // wenn das ergebnis null ist nochmal würfeln
            wuerfelalt[1] = wuerfelalt[0];
            wuerfelalt[0] = wurfzahl;
            if (srv != null && myturn)
            {
                srv.send("wur:" + wurfzahl);
            }
            else if (clt != null && myturn)
            {
                clt.send("wur:" + wurfzahl);
            }
            wuerfelzaehler++; // einen Zug weiter Zählen
            labelWurf.Text = Convert.ToString(wurfzahl); // Anzeige was gewürfelt wurde
            txtbx("W" + Convert.ToString(wuerfelzaehler) + " Eine " + Convert.ToString(wurfzahl) + " wurde gewürfelt!");
            // Algorithmusende
            if (zug)
            {
                wuerfel = wurfzahl;
                labelStatus.Text = colorname[amZug] + " ist jetzt am Zug!";
                if (!opohneregeln)
                {
                    theWuerfel = false;
                    if (regelCheck(amZug, 0, 0) == 0 && wuerfel != 6)
                    {
                        zugEnde();
                        if (versuche == 0)
                            nofarbe = true;
                    }
                }
            }
            if (!nofarbe)
            {
                if (wuerfelzaehler == 1) labelStatus.Text = "!! Klicken Sie die Startfarbe an !!";
                if (amZug == 1) labelWurf.ForeColor = System.Drawing.Color.Red;
                else if (amZug == 2) labelWurf.ForeColor = System.Drawing.Color.Blue;
                else if (amZug == 3) labelWurf.ForeColor = System.Drawing.Color.Green;
                else if (amZug == 4) labelWurf.ForeColor = System.Drawing.Color.Gold;
                else labelWurf.ForeColor = System.Drawing.Color.Black;
            }
            netsync(); // neue Syncfunktion
            return wurfzahl;
        }

        // -- reset --
        private void reset(int farbe, int id)
        {
            for (int i = 1; i < 5; i++)
            {
                if ((farbe == i) || (farbe == 0))
                {
                    for (int ii = 1; ii < 5; ii++)
                    {
                        if (id == 0 || id == ii) spielfigur[i, ii] = 0;
                    }
                }
            }
            if ((farbe == 5) || (farbe == 0))
            {
                wuerfel = 0;
                theWuerfel = true;
                pictureBoxWuerfel.Image = picWgl;
                labelStatus.Text = "";
                labelWurf.Text = ""; // Anzeige was gewürfelt wurde
            }
            if (farbe == 0)
            {
                zug = 1; // Die Züge werden gezählt
                wuerfelzaehler = 0; // Variable zählt würfelversuche
                amZug = 0;
                amZug = 0;
            }
            setFigur(0, 0, 0);
            txtbx("Reset von " + colorname[farbe] + " id: " + Convert.ToString(id));
        }

        // -- Rausschmeißalgorithmus --
        private int[] checkKick(int zahl, int farbe)
        {
            int[] ergo = { 0, 0 };
            zahl = convertToFarbe(zahl, farbe);
            for (int i = 1; i < 5; i++) // i = farbe
            {
                if (farbe != i)
                {
                    for (int ii = 1; ii < 5; ii++) // ii = id
                    {
                        if (zahl == convertToFarbe(spielfigur[i, ii], i) && zahl != 0 && zahl < 41)
                        {
                            ergo[0] = i; // farbe
                            ergo[1] = ii; // zahl
                        }
                    }
                }
            }
            return ergo;
        }

        // -- KI --
        private void ki()
        {
            int color = amZug;
            int playID = 0;
            wuerfel = wurf();
            //  System.Threading.Thread.Sleep(250);
            playID = regelCheck(color, 0, wuerfel);
            txtbx("Möglicher Zug mit: " + Convert.ToString(playID));
            if (playID != 0 && playID != 5) spielzug(color, playID);
            if (srv != null && myturn)
            {
                srv.send("syn:" + color + ": ");
            }
         //   kiaktiv = false;
            new Thread(DoSomethingExpensive).Start();
        }

        // -- Setzt die nächste Farbe --
        private int nextColor(int color, bool set = true)
        {
            int x = zx - (pictureBoxWuerfel.Size.Height / 2); // Mitte minus Würfelgröße
            int y = zy - (pictureBoxWuerfel.Size.Width / 2);
            int add = zadd * 2;

            if (set)
            {
                color = color + 1;
                if (color > 4) color = 1;
                if (!oprot && color == 1) color++;
                if (!opblau && color == 2) color++;
                if (!opgruen && color == 3) color++;
                if (!opgelb && color == 4)
                {
                    if (oprot) color = 1; // Wenn Rot nicht gesetzt ist
                    else color = 2;
                }
                versuche = 0; // versuche zurücksetzen
            }
            else if (wuerfelzaehler < 2)
            {
                if (wuerfelzaehler == 1) labelStatus.Text = "!! Klicken Sie die Startfarbe an !!";

                if (amZug == 1) labelWurf.ForeColor = System.Drawing.Color.Red;
                else if (amZug == 2) labelWurf.ForeColor = System.Drawing.Color.Blue;
                else if (amZug == 3) labelWurf.ForeColor = System.Drawing.Color.Green;
                else if (amZug == 4) labelWurf.ForeColor = System.Drawing.Color.Gold;
                else labelWurf.ForeColor = System.Drawing.Color.Black;
            }
            if (color == 1)
            {
                x = x - add;
                y = y + add;
            }
            else if (color == 2)
            {
                x = x - add;
                y = y - add;
            }
            else if (color == 3)
            {
                x = x + add;
                y = y - add;
            }
            else if (color == 4)
            {
                x = x + add;
                y = y + add;
            }
            // Netzwerk
            if (srv != null && myturn)
            {
                srv.send("nco:" + color + ": ");
            }
            else if (clt != null && myturn)
            {
                clt.send("nco:" + color + ": ");
            }
            // -
            pictureBoxWuerfel.Location = new Point(x, y);
            return color;
        }

        // -- Bewegt die Spielfiguren auf dem Feld --
        private void setFigur(int farbe, int id, int zahl)
        {
            if (farbe == 1 || farbe == 0)
            {
                if (id == 1 || id == 0) pictureBoxRot1.Location = place(1, 1, (zahl > 0) ? zahl : spielfigur[1, 1]);
                if (id == 2 || id == 0) pictureBoxRot2.Location = place(1, 2, (zahl > 0) ? zahl : spielfigur[1, 2]);
                if (id == 3 || id == 0) pictureBoxRot3.Location = place(1, 3, (zahl > 0) ? zahl : spielfigur[1, 3]);
                if (id == 4 || id == 0) pictureBoxRot4.Location = place(1, 4, (zahl > 0) ? zahl : spielfigur[1, 4]);
            }
            if (farbe == 2 || farbe == 0)
            {
                if (id == 1 || id == 0) pictureBoxBlau1.Location = place(2, 1, (zahl > 0) ? zahl : spielfigur[2, 1]);
                if (id == 2 || id == 0) pictureBoxBlau2.Location = place(2, 2, (zahl > 0) ? zahl : spielfigur[2, 2]);
                if (id == 3 || id == 0) pictureBoxBlau3.Location = place(2, 3, (zahl > 0) ? zahl : spielfigur[2, 3]);
                if (id == 4 || id == 0) pictureBoxBlau4.Location = place(2, 4, (zahl > 0) ? zahl : spielfigur[2, 4]);
            }
            if (farbe == 3 || farbe == 0)
            {
                if (id == 1 || id == 0) pictureBoxGruen1.Location = place(3, 1, (zahl > 0) ? zahl : spielfigur[3, 1]);
                if (id == 2 || id == 0) pictureBoxGruen2.Location = place(3, 2, (zahl > 0) ? zahl : spielfigur[3, 2]);
                if (id == 3 || id == 0) pictureBoxGruen3.Location = place(3, 3, (zahl > 0) ? zahl : spielfigur[3, 3]);
                if (id == 4 || id == 0) pictureBoxGruen4.Location = place(3, 4, (zahl > 0) ? zahl : spielfigur[3, 4]);
            }
            if (farbe == 4 || farbe == 0)
            {
                if (id == 1 || id == 0) pictureBoxGelb1.Location = place(4, 1, (zahl > 0) ? zahl : spielfigur[4, 1]);
                if (id == 2 || id == 0) pictureBoxGelb2.Location = place(4, 2, (zahl > 0) ? zahl : spielfigur[4, 2]);
                if (id == 3 || id == 0) pictureBoxGelb3.Location = place(4, 3, (zahl > 0) ? zahl : spielfigur[4, 3]);
                if (id == 4 || id == 0) pictureBoxGelb4.Location = place(4, 4, (zahl > 0) ? zahl : spielfigur[4, 4]);
            }
        }

        // -- Farben aktivieren / deaktivieren --
        private int aktiviereFarben()
        {
            this.BackgroundImage = picBG;
            pictureBoxBam.Image = picBam;
            // pictureBoxBam.Location = new Point(zx - (pictureBoxBam.Size.Height / 2) + (zadd * 5), zy - (pictureBoxBam.Size.Width / 2) + (zadd * 2));
            pictureBoxWuerfel.Location = new Point(zx - (pictureBoxWuerfel.Size.Height / 2), zy - (pictureBoxWuerfel.Size.Width / 2));
            labelVersuche.Location = new Point(zx - 35, zy - 35);
            labelWurf.Location = new Point(zx - (labelWurf.Size.Height / 2), zy - (labelWurf.Size.Height / 2));
            labelStatus.Location = new Point(zx - (zadd * 5) - 30, zy + (zadd * 2) - 30);
            // panelOptions.Dock = DockStyle.None;
            panelOptions.Location = new Point(0, 0);
            buttonEinstellungen.Location = new Point(zx - (zadd * 3) - 20, zy + (zadd * 5));
            labelConsole.Location = new Point(zx + (zadd * 1) + 15, zy + (zadd * 5) + 15);

            int aktiv = 0;
            pictureBoxRot1.Visible = oprot;
            pictureBoxRot2.Visible = oprot;
            pictureBoxRot3.Visible = oprot;
            pictureBoxRot4.Visible = oprot;
            if (oprot)
            {
                aktiv++;
                pictureBoxRot1.Image = picRot;
                pictureBoxRot2.Image = picRot;
                pictureBoxRot3.Image = picRot;
                pictureBoxRot4.Image = picRot;
                pictureBoxRot1.Size = picSize;
                pictureBoxRot2.Size = picSize;
                pictureBoxRot3.Size = picSize;
                pictureBoxRot4.Size = picSize;
            }
            pictureBoxBlau1.Visible = opblau;
            pictureBoxBlau2.Visible = opblau;
            pictureBoxBlau3.Visible = opblau;
            pictureBoxBlau4.Visible = opblau;
            if (opblau)
            {
                aktiv++;

                pictureBoxBlau1.Image = picBlau;
                pictureBoxBlau2.Image = picBlau;
                pictureBoxBlau3.Image = picBlau;
                pictureBoxBlau4.Image = picBlau;
                pictureBoxBlau1.Size = picSize;
                pictureBoxBlau2.Size = picSize;
                pictureBoxBlau3.Size = picSize;
                pictureBoxBlau4.Size = picSize;
            }
            pictureBoxGruen1.Visible = opgruen;
            pictureBoxGruen2.Visible = opgruen;
            pictureBoxGruen3.Visible = opgruen;
            pictureBoxGruen4.Visible = opgruen;
            if (opgruen)
            {
                aktiv++;
                pictureBoxGruen1.Image = picGruen;
                pictureBoxGruen2.Image = picGruen;
                pictureBoxGruen3.Image = picGruen;
                pictureBoxGruen4.Image = picGruen;
                pictureBoxGruen1.Size = picSize;
                pictureBoxGruen2.Size = picSize;
                pictureBoxGruen3.Size = picSize;
                pictureBoxGruen4.Size = picSize;
            }
            pictureBoxGelb1.Visible = opgelb;
            pictureBoxGelb2.Visible = opgelb;
            pictureBoxGelb3.Visible = opgelb;
            pictureBoxGelb4.Visible = opgelb;
            if (opgelb)
            {
                aktiv++;
                pictureBoxGelb1.Image = picGelb;
                pictureBoxGelb2.Image = picGelb;
                pictureBoxGelb3.Image = picGelb;
                pictureBoxGelb4.Image = picGelb;
                pictureBoxGelb1.Size = picSize;
                pictureBoxGelb2.Size = picSize;
                pictureBoxGelb3.Size = picSize;
                pictureBoxGelb4.Size = picSize;
            }

            return aktiv;
        }

        // -- Rote Positionen werden in andere Farben Konvertiert --
        private static int convertToFarbe(int zahl, int farbe)
        {
            if (farbe == 3)
            {
                if (zahl > 0 & zahl < 21)
                    zahl = zahl + 20;
                else if (zahl > 20 & zahl < 41)
                    zahl = zahl - 20;
            }

            else if (farbe == 2)
            {
                if (zahl > 0 & zahl < 31)
                    zahl = zahl + 10;
                else if (zahl > 30 & zahl < 41)
                    zahl = zahl - 30;
            }

            else if (farbe == 4)
            {
                if (zahl > 0 & zahl < 11)
                    zahl = zahl + 30;
                else if (zahl > 10 & zahl < 41)
                    zahl = zahl - 10;
            }
            return zahl;
        }

        // -- Die einzelnen Figuren --
        #region Figuren
        private void pictureBoxRot1_Click(object sender, EventArgs e)
        {
            spielzug(1, 1);
        }

        private void pictureBoxRot2_Click(object sender, EventArgs e)
        {
            spielzug(1, 2);
        }

        private void pictureBoxRot3_Click(object sender, EventArgs e)
        {
            spielzug(1, 3);
        }

        private void pictureBoxRot4_Click(object sender, EventArgs e)
        {
            spielzug(1, 4);
        }

        private void pictureBoxBlau1_Click(object sender, EventArgs e)
        {
            spielzug(2, 1);
        }

        private void pictureBoxBlau2_Click(object sender, EventArgs e)
        {
            spielzug(2, 2);
        }

        private void pictureBoxBlau3_Click(object sender, EventArgs e)
        {
            spielzug(2, 3);
        }

        private void pictureBoxBlau4_Click(object sender, EventArgs e)
        {
            spielzug(2, 4);
        }

        private void pictureBoxGruen1_Click(object sender, EventArgs e)
        {
            spielzug(3, 1);
        }

        private void pictureBoxGruen2_Click(object sender, EventArgs e)
        {
            spielzug(3, 2);
        }

        private void pictureBoxGruen3_Click(object sender, EventArgs e)
        {
            spielzug(3, 3);
        }

        private void pictureBoxGruen4_Click(object sender, EventArgs e)
        {
            spielzug(3, 4);
        }

        private void pictureBoxGelb1_Click(object sender, EventArgs e)
        {
            spielzug(4, 1);
        }

        private void pictureBoxGelb2_Click(object sender, EventArgs e)
        {
            spielzug(4, 2);
        }

        private void pictureBoxGelb3_Click(object sender, EventArgs e)
        {
            spielzug(4, 3);
        }

        private void pictureBoxGelb4_Click(object sender, EventArgs e)
        {
            spielzug(4, 4);
        }
        #endregion

        // -- Textbox aktualisieren --
        public void txtbx(string txt = "")
        {
            try
            {
                if (textBoxCommOut.InvokeRequired)
                {
                    textBoxCommOut.Invoke(new MethodInvoker(delegate
                    {
                        if (textBoxCommOut.Text.Length > 2200)
                            textBoxCommOut.Text = "";
                        textBoxCommOut.Text = textBoxCommOut.Text + txt;
                        netZug(txt);
                    }));
                }
                else
                {
                    if (textBoxCommOut.Text.Length > 2200)
                        textBoxCommOut.Text = "";
                    labelConsole.Text = txt;
                    textBoxCommOut.AppendText(txt + "\r\n");
                    textBoxCommOut.SelectionStart = textBoxCommOut.Text.Length;
                    textBoxCommOut.ScrollToCaret();
                }
            }
            catch { }
            try
            {
                if (oplog)
                {
                    StreamWriter myFile = new StreamWriter("log.txt", true);
                    myFile.Write(txt + "\r\n");
                    myFile.Close();
                }
            }
            catch
            { }
        }

        // -- Komandozeile Enter --
        private void textBoxCommand_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string befehl = textBoxCommand.Text;
                txtbx(befehl);
                if (befehl.StartsWith("/")) { console(befehl); }
                textBoxCommand.Text = "";
            }
        }

        #region einstellungen
        // -- Optionen abbrechen --
        private void buttonWuerfel_Click(object sender, EventArgs e)
        {
                pause(false);
        }

        // -- Optionen --
        private void buttonCommand_Click(object sender, EventArgs e)
        {
            if (buttonCommand.Text == "bereit!")
            {
                clt.send("beg:" + spielerNetzwerk + ": ");
                buttonCommand.Text = "Übernehmen";
                buttonCommand.Enabled = false;
                pictureBoxWuerfel.Visible = false;
                // buttonNeuesSpiel.Enabled = false;
                buttonLoad.Enabled = false;
               // buttonSave.Enabled = false;
                panelOptions.Visible = true;
                labelNetStatus.Text = "Warte auf das Okay des Servers";
                txtbx("Warte auf das Okay des Servers");
            }
            else
            {
                labelRot.Text = spielerRot;
                labelBlau.Text = spielerBlau;
                labelGruen.Text = spielerGruen;
                labelGelb.Text = spielerGelb;

                // Normale einstellungen
                bool first = !buttonNeuesSpiel.Visible;
                // posi Form.ActiveForm
                zx = this.ClientSize.Width / 2;
                zy = this.ClientSize.Height / 2;
                zadd = this.ClientSize.Height / 11;
                // Opt
                kisleep = numericUpDownKISpeed.Value;
                oprot = (spielerRot != "") ? true : false;
                opblau = (spielerBlau != "") ? true : false;
                opgruen = (spielerGruen != "") ? true : false;
                opgelb = (spielerGelb != "") ? true : false;
                opdreimalwuerfeln = checkBox3malWuerfeln.Checked;
                opschlagzwang = checkBoxSchlagzwang.Checked;
                opreverseschlag = checkBoxReverseSchlag.Checked;
                opohneregeln = checkBoxRegeln.Checked;
                schnellZug = radioButtonSchnellZug.Checked;
                oplog = checkBoxLog.Checked;
                optzupause = checkBoxOpZugPause.Checked;

                buttonNeuesSpiel.Visible = true;
                pause(false); // Beendet die pause

                try
                {
                    if (oplog && File.Exists("log.txt"))
                        System.IO.File.Delete("log.txt");
                }
                catch
                {
                }
                // Nur beim ersten mal ausführen
                if (first)
                {
                    #region eigeneBilderSounds
                    // Eigene spielfiguren laden
                    string pfad = "";
                    if (checkBoxEigBilder.Checked || checkBoxSound.Checked)
                    {
                        try
                        {
                            string hira = maedn2.Update.infoHira;
                            string[] loadinfo = {hira + "pfad", hira + "rot", hira + "blau", hira + "gelb", hira + "gruen",
                                        hira + "rotname", hira + "blauname", hira + "gruenname", hira + "gelbname", 
                                        hira + "bg",hira +  "bam",hira +  "wuerfeln", hira + "wuerfelgl",
                                        hira + "figurX",hira +  "figurY", hira + "figurXadd",hira +  "figurYadd",
                                        hira + "soundwin",hira +  "soundkick",hira +  "soundwurf", hira + "soundzug", 
                                        hira + "theme", hira + "author" };
                            string[] load = maedn2.Update.laden(loadinfo, "stylemaedn.xml");
                            pfad = load[0];
                            if (checkBoxEigBilder.Checked)
                            {
                                if (load[21] != hira + "theme") // Überprüfung ob das Theme überhaupt existiert
                                {
                                    if (File.Exists(pfad + load[1])) { picRot = Image.FromFile(pfad + load[1]); }
                                    if (File.Exists(pfad + load[2])) { picBlau = Image.FromFile(pfad + load[2]); }
                                    if (File.Exists(pfad + load[3])) { picGelb = Image.FromFile(pfad + load[3]); }
                                    if (File.Exists(pfad + load[4])) { picGruen = Image.FromFile(pfad + load[4]); }
                                    colorname[1] = load[5]; // Rot
                                    colorname[2] = load[6]; // Blau
                                    colorname[3] = load[7]; // Grün
                                    colorname[4] = load[8]; // Gelb
                                    if (File.Exists(pfad + load[9])) { picBG = Image.FromFile(pfad + load[9]); }
                                    if (File.Exists(pfad + load[10])) { picBam = Image.FromFile(pfad + load[10]); }
                                    if (File.Exists(pfad + load[11])) { picWn = Image.FromFile(pfad + load[11]); }
                                    if (File.Exists(pfad + load[12])) { picWgl = Image.FromFile(pfad + load[12]); }
                                    try
                                    {
                                        picSize.Width = Convert.ToInt32(load[13]);
                                        picSize.Height = Convert.ToInt32(load[14]);
                                        figX = Convert.ToInt32(load[15]);
                                        figY = Convert.ToInt32(load[16]);
                                    }
                                    catch { }
                                    if (checkBoxSound.Checked)
                                    {
                                        if (File.Exists(pfad + load[17])) { soundwin = pfad + load[17]; }
                                        if (File.Exists(pfad + load[18])) { soundkick = pfad + load[18]; }
                                        if (File.Exists(pfad + load[19])) { soundwurf = pfad + load[19]; }
                                        if (File.Exists(pfad + load[20])) { soundzug = pfad + load[20]; }
                                    }
                                    txtbx("Eigenes Theme geladen!");
                                    txtbx("Theme " + load[21] + " by " + load[22]);
                                }
                            }
                        }
                        catch { }
                    }
                    #endregion
                    // Setzt alle Bilder und Farben
                    int i = aktiviereFarben();
                    if (i < 2)
                    {
                        oprot = true;
                        opblau = true;
                        aktiviereFarben();
                    }
                    if (groupBoxSpieler.Enabled || groupBoxRegeln.Enabled)
                    {
                        groupBoxSpieler.Enabled = false;
                        groupBoxRegeln.Enabled = false;
                        aktiviereFarben();
                    }
                    pictureBoxWuerfel.Image = picWgl;
                    setFigur(0, 0, 0); // Figuren neu setzen

                    if (buttonCommand.Text == "Start Lan!")
                    {
                        srv.send("sta:" + spielerRot + ":"
                                + spielerBlau + ":"
                                + spielerGruen + ":"
                                + spielerGelb + ":"
                                + checkBox3malWuerfeln.Checked.ToString() + ":"
                                + checkBoxSchlagzwang.Checked.ToString() + ":"
                                + checkBoxReverseSchlag.Checked.ToString());
                        buttonCommand.Text = "Übernehmen";
                    }
                    if (buttonCommand.Text == "bereit!")
                    {
                        clt.send("beg:" + spielerNetzwerk + ": ");
                        buttonCommand.Text = "Übernehmen";
                        buttonCommand.Enabled = false;
                        pictureBoxWuerfel.Visible = false;
                        buttonNeuesSpiel.Enabled = false;
                        buttonLoad.Enabled = false;
                        //  buttonSave.Enabled = false;
                        panelOptions.Visible = true;
                    }
                }
            }
            // KI wieder starten
            if (((spielerRot == "Computer" && amZug == 1) ||
          (spielerBlau == "Computer" && amZug == 2) ||
         (spielerGruen == "Computer" && amZug == 3) ||
          (spielerGelb == "Computer" && amZug == 4)) && !spielende)
            {
                kiaktiv = true;
                ki();
            }
        }

        // -- Einstellungen --
        private void buttonEinstellungen_Click(object sender, EventArgs e)
        {
            buttonEinstellungen.Visible = false;
            buttonCommand.Text = "Übernehmen";
            buttonWuerfel.Visible = true;
            panelOptions.Visible = true;
            // Optionen
            numericUpDownKISpeed.Value = kisleep;
            checkBox3malWuerfeln.Checked = opdreimalwuerfeln;
            checkBoxSchlagzwang.Checked = opschlagzwang;
            checkBoxReverseSchlag.Checked = opreverseschlag;
            checkBoxRegeln.Checked = opohneregeln;
            if (schnellZug) radioButtonSchnellZug.Checked = true;
            else radioButtonLangsamZug.Checked = true;
            pause(true);
        }

        // -- Neues Spiel starten --
        private void buttonNeuesSpiel_Click(object sender, EventArgs e)
        {
            buttonWuerfel.Visible = false;
            nextColor(0, false);
            reset(0, 0);
            buttonCommand.Text = "Spiel starten";
            buttonNeuesSpiel.Visible = false;
            groupBoxSpieler.Enabled = true;
            groupBoxRegeln.Enabled = true;
            spielende = false;
            labelVersuche.Text = "";
            aktiviereFarben();
            txtbx("Neues Spiel gestartet!");
            updatefiguren("Mensch", "Computer", "", "");
        }
        #endregion

        #region SpeichernLaden
        // -- Speichern --
        private void buttonSave_Click(object sender, EventArgs e)
        {
            // XML Speichern
            string[] data = { "rot1", "rot2", "rot3", "rot4",
                                "blau1", "blau2", "blau3", "blau4", 
                                "gruen1", "gruen2", "gruen3", "gruen4",
                                "gelb1", "gelb2", "gelb3", "gelb4",
                                "zug", "wuerfelzaehler", "amZug", "versuche",
                                "spieltypRot", "spieltypBlau", "spieltypGruen", "spieltypGelb",
                                "checkBoxRot", "checkBoxBlau", "checkBoxGruen", "checkBoxGelb",
                                "dreixwuerfeln", "schlagzwang", "revschlag", "wurf",
                                 "theWuerfel"};
            string[] hira = { Convert.ToString(spielfigur[1,1]), Convert.ToString(spielfigur[1,2]),Convert.ToString(spielfigur[1,3]),Convert.ToString(spielfigur[1,4]),
                                Convert.ToString(spielfigur[2,1]), Convert.ToString(spielfigur[2,2]), Convert.ToString(spielfigur[2,3]), Convert.ToString(spielfigur[2,4]), 
                                Convert.ToString(spielfigur[3,1]), Convert.ToString(spielfigur[3,2]), Convert.ToString(spielfigur[3,3]), Convert.ToString(spielfigur[3,4]),
                                Convert.ToString(spielfigur[4,1]), Convert.ToString(spielfigur[4,2]), Convert.ToString(spielfigur[4,3]), Convert.ToString(spielfigur[4,4]),
                                Convert.ToString(zug), Convert.ToString(wuerfelzaehler), Convert.ToString(amZug), Convert.ToString(versuche),
                                Convert.ToString(spielerRot), Convert.ToString(spielerBlau), Convert.ToString(spielerGruen), Convert.ToString(spielerGelb),
                                Convert.ToString(oprot),Convert.ToString(opblau),Convert.ToString(opgruen),Convert.ToString(opgelb),
                                Convert.ToString(opdreimalwuerfeln), Convert.ToString(opschlagzwang),Convert.ToString(opreverseschlag),Convert.ToString(wuerfel),
                                Convert.ToString(theWuerfel)};

            maedn2.Update.speichern(data, hira, "", true);
        }

        // -- laden --
        private void buttonLoad_Click(object sender, EventArgs e)
        {
            try
            {
                string hira = maedn2.Update.infoHira;
                string[] loadinfo = {hira + "rot1", hira + "rot2", hira + "rot3", hira + "rot4",
                                hira + "blau1", hira + "blau2", hira + "blau3", hira + "blau4", 
                                hira + "gruen1",hira +  "gruen2",hira +  "gruen3", hira + "gruen4",
                                hira + "gelb1", hira + "gelb2", hira + "gelb3", hira + "gelb4",
                                hira + "zug", hira + "wuerfelzaehler", hira + "amZug",hira +  "versuche",
                                hira + "spieltypRot", hira + "spieltypBlau",hira +  "spieltypGruen", hira + "spieltypGelb",
                                hira + "checkBoxRot",hira +  "checkBoxBlau",hira +  "checkBoxGruen", hira + "checkBoxGelb",
                                hira + "dreixwuerfeln", hira + "schlagzwang", hira + "revschlag",hira +  "wurf",
                                hira + "theWuerfel"};
                string[] load = maedn2.Update.laden(loadinfo);


                spielfigur[1, 1] = Convert.ToInt16(load[0]);
                spielfigur[1, 2] = Convert.ToInt16(load[1]);
                spielfigur[1, 3] = Convert.ToInt16(load[2]);
                spielfigur[1, 4] = Convert.ToInt16(load[3]);
                spielfigur[2, 1] = Convert.ToInt16(load[4]);
                spielfigur[2, 2] = Convert.ToInt16(load[5]);
                spielfigur[2, 3] = Convert.ToInt16(load[6]);
                spielfigur[2, 4] = Convert.ToInt16(load[7]);
                spielfigur[3, 1] = Convert.ToInt16(load[8]);
                spielfigur[3, 2] = Convert.ToInt16(load[9]);
                spielfigur[3, 3] = Convert.ToInt16(load[10]);
                spielfigur[3, 4] = Convert.ToInt16(load[11]);
                spielfigur[4, 1] = Convert.ToInt16(load[12]);
                spielfigur[4, 2] = Convert.ToInt16(load[13]);
                spielfigur[4, 3] = Convert.ToInt16(load[14]);
                spielfigur[4, 4] = Convert.ToInt16(load[15]);
                zug = Convert.ToInt16(load[16]);
                wuerfelzaehler = Convert.ToInt16(load[17]);
                amZug = Convert.ToInt16(load[18]);
                versuche = Convert.ToInt16(load[19]);
                spielerRot = load[20];
                spielerBlau = load[21];
                spielerGruen = load[22];
                spielerGelb = load[23];
                oprot = Convert.ToBoolean(load[24]);
                opblau = Convert.ToBoolean(load[25]);
                opgruen = Convert.ToBoolean(load[26]);
                opgelb = Convert.ToBoolean(load[27]);
                opdreimalwuerfeln = Convert.ToBoolean(load[28]);
                opschlagzwang = Convert.ToBoolean(load[29]);
                opreverseschlag = Convert.ToBoolean(load[30]);
                wuerfel = Convert.ToInt16(load[31]);
                theWuerfel = Convert.ToBoolean(load[32]);
                buttonCommand_Click(sender, e);
                setFigur(0, 0, 0);
                txtbx("Spieler " + colorname[amZug] + " bitte Würfeln!");
                nextColor(amZug, false);
            }
            catch
            {
                MessageBox.Show("Das Laden der Datei ist fehlgeschlagen", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        // -- Konsolenbefehle analysieren --
        private void console(string befehl)
        {
            try
            {
                if (befehl.StartsWith("/wurf ")) wuerfel = Convert.ToInt16(befehl.Substring(6, 1));
                else if (befehl.StartsWith("/reset")) reset(0, 0);
                else if (befehl.StartsWith("/set ")) spielfigur[Convert.ToInt16(befehl.Substring(5, 1)), Convert.ToInt16(befehl.Substring(7, 1))] = Convert.ToInt16(befehl.Substring(9, 1));
                else if (befehl.StartsWith("/debug")) theWuerfel = true;
                else if (befehl.StartsWith("/farbe")) amZug = Convert.ToInt16(befehl.Substring(7, 1));
                else if (befehl.StartsWith("/ki")) ki();
                else if (befehl.StartsWith("/ende")) zugEnde();
                else if (befehl.StartsWith("/say"))
                {
                    if (srv != null)
                        srv.send(befehl.Replace("/say", ""));
                    else
                        clt.send(befehl.Replace("/say", ""));
                }
                else if (befehl.StartsWith("/exit") || befehl.StartsWith("/close")) OnClosing(null);
                else if (befehl.StartsWith("/size")) txtbx("X: " + Form.ActiveForm.ClientSize.Width.ToString() + " Y: " + Form.ActiveForm.ClientSize.Height.ToString() + " FigX: " + figX.ToString() + " FigY: " + figY.ToString());
                else if (befehl.StartsWith("/sync")) netsync();
                else if (befehl.StartsWith("/voll"))
                {
                    this.WindowState = FormWindowState.Maximized;
                    this.FormBorderStyle = FormBorderStyle.None;
                    this.TopMost = true;
                    picSize = new Size(zadd - 2, zadd - 2);

                }
                else txtbx("Unbekannter Befehl");
            }
            catch
            {
                txtbx("Befehl fehlerhaft");
            }
        }

        #region Netzwerk
        // -- Ereignis vom Server auslösen --
        void broadcastincomming(object sender, Server.TextEventArgs e)
        {
            if (listBoxNetServer.InvokeRequired)
            {
                listBoxNetServer.Invoke(new MethodInvoker(delegate
                {
                    string HostName = System.Net.Dns.GetHostName();


                    string[] strarr = e.Text.Split(':');
                    if (strarr[0] != Server.GetLocalAddresses("")[3])
                    {
                        listBoxNetServer.Items.Add(strarr[0]);
                        txtbx(strarr[0] + " " + Server.GetLocalAddresses("")[3]);
                    }
                }));
            }
        }

        void UpdateLabelText(object sender, Server.TextEventArgs e)
        {
            if (e.Text.StartsWith("?"))
            {
                srv.send("Server: " + textBoxNetName.Text);
            }
            // TExt ausgabe
            else if (e.Text.Length > 2)
                txtbx("Net: " + e.Text + " \r\n");
            else txtbx(e.Text);
        }

        // -- Ereignis vom client auslösen --
        void UpdateLabelTextc(object sender, Client.TextEventArgs e)
        {
            // TExt ausgabe
            if (e.Text.Length > 2)
                txtbx("Net: " + e.Text + " \r\n");
            else txtbx(e.Text);
        }

        // befehle über Netzwerk entgegennehmen
        private void netZug(string str)
        {
            try
            {
                myturn = false;
                if (str.StartsWith("Net: zug")) // Zug
                {
                    string[] strarr = str.Split(':');
                    amZug = Convert.ToInt16(strarr[2]);
                    wuerfel = Convert.ToInt16(strarr[4]);
                    spielzug(Convert.ToInt16(strarr[2]), Convert.ToInt16(strarr[3]));
                    labelStatus.Text = "Spieler " + colorname[amZug] + " bitte Würfeln!"; // Meldung
                }
                else if (str.StartsWith("Net: nco")) // Nächste Farbe
                {
                    string[] strarr = str.Split(':');
                    if (strarr[2] == "0") strarr[2] = "1";
                    nextColor(Convert.ToInt16(strarr[2]), false);
                    amZug = Convert.ToInt16(strarr[2]);
                    // Farbkorrektur
                    if (amZug == 1) labelWurf.ForeColor = System.Drawing.Color.Red;
                    else if (amZug == 2) labelWurf.ForeColor = System.Drawing.Color.Blue;
                    else if (amZug == 3) labelWurf.ForeColor = System.Drawing.Color.Green;
                    else if (amZug == 4) labelWurf.ForeColor = System.Drawing.Color.Gold;
                    else labelWurf.ForeColor = System.Drawing.Color.Black;
                    //      netsync();
                    txtbx("Farbe über Netzwerk korrigiert!");
                    if (((spielerRot == "Computer" && amZug == 1) ||
                        (spielerBlau == "Computer" && amZug == 2) ||
                        (spielerGruen == "Computer" && amZug == 3) ||
                        (spielerGelb == "Computer" && amZug == 4)) && !spielende && srv != null && !kiaktiv)
                    {
                        kiaktiv = true;
                        ki();
                    }
                }
                else if (str.StartsWith("Net: wur")) // Würfel
                {
                    string[] strarr = str.Split(':');
                    wuerfelzaehler++; // einen Zug weiter Zählen
                    labelWurf.Text = strarr[2]; // Anzeige was gewürfelt wurde
                    txtbx("W" + Convert.ToString(wuerfelzaehler) + " Eine " + strarr[2] + " wurde gewürfelt!");
                }

                else if (str.StartsWith("Net: sta")) // Server Startet
                {
                    string[] strarr = str.Split(':');

                    string bl = "Netzwerk";
                    string gr = "Netzwerk";
                    string ge = "Netzwerk";
                    string ro = "Netzwerk";
                    if (strarr[2] == "") ro = "";
                    if (strarr[3] == "") bl = "";
                    if (strarr[4] == "") gr = "";
                    if (strarr[5] == "") ge = "";
                    if (spielerNetzwerk == 1)
                        updatefiguren("Mensch", bl, gr, ge);
                    if (spielerNetzwerk == 2)
                        updatefiguren(ro, "Mensch", gr, ge);
                    if (spielerNetzwerk == 3)
                        updatefiguren(ro, bl, "Mensch", ge); ;
                    if (spielerNetzwerk == 4)
                        updatefiguren(ro, bl, gr, "Mensch");

                    buttonNeuesSpiel.Visible = false;
                    checkBox3malWuerfeln.Checked = Convert.ToBoolean(strarr[6]);
                    checkBoxSchlagzwang.Checked = Convert.ToBoolean(strarr[7]);
                    checkBoxReverseSchlag.Checked = Convert.ToBoolean(strarr[8]);
                    buttonCommand.Enabled = true;
                    buttonCommand_Click(this, null);
                    buttonCommand.Enabled = false;
                    pictureBoxWuerfel.Visible = true;
                    buttonNeuesSpiel.Enabled = false;
                    labelNetStatus.Text = "Netzwerkspiel gestartet";
                }
                else if (str.StartsWith("Net: beg")) // Client gibt Spiel frei
                {
                    string[] strarr = str.Split(':');
                    string farbe = strarr[2];
                    buttonCommand.Enabled = true;
                    buttonCommand.Text = "Start Lan!";
                    srv.online(false);
                    switch (farbe)
                    {
                        case "1":
                            spielerRot = "Netzwerk";
                            pictureBoxSysRot.Image = picSysRotnet;
                            labelRot.Text = spielerRot;
                            break;
                        case "2":
                            spielerBlau = "Netzwerk";
                            pictureBoxSysBlau.Image = picSysBlaunet;
                            labelBlau.Text = spielerBlau;
                            break;
                        case "3":
                            spielerGruen = "Netzwerk";
                            pictureBoxSysGruen.Image = picSysGruennet;
                            labelGruen.Text = spielerGruen;
                            break;
                        case "4":
                            spielerGelb = "Netzwerk";
                            pictureBoxSysGelb.Image = picSysGelbnet;
                            labelGelb.Text = spielerGelb;
                            break;
                    }
                    labelNetStatus.Text = "Spiel freigegeben";
                }
                else if (str.StartsWith("Net: srq")) // SYncro Abfrage -> führt syncro aus
                {
                    netsync();
                }
                else if (str.StartsWith("Net: syn")) // Syncronisationsvorgang
                {
                    string[] strarr = str.Split(':');
                    spielfigur[1, 1] = Convert.ToInt16(strarr[2]);
                    spielfigur[1, 2] = Convert.ToInt16(strarr[3]);
                    spielfigur[1, 3] = Convert.ToInt16(strarr[4]);
                    spielfigur[1, 4] = Convert.ToInt16(strarr[5]);
                    spielfigur[2, 1] = Convert.ToInt16(strarr[6]);
                    spielfigur[2, 2] = Convert.ToInt16(strarr[7]);
                    spielfigur[2, 3] = Convert.ToInt16(strarr[8]);
                    spielfigur[2, 4] = Convert.ToInt16(strarr[9]);
                    spielfigur[3, 1] = Convert.ToInt16(strarr[10]);
                    spielfigur[3, 2] = Convert.ToInt16(strarr[11]);
                    spielfigur[3, 3] = Convert.ToInt16(strarr[12]);
                    spielfigur[3, 4] = Convert.ToInt16(strarr[13]);
                    spielfigur[4, 1] = Convert.ToInt16(strarr[14]);
                    spielfigur[4, 2] = Convert.ToInt16(strarr[15]);
                    spielfigur[4, 3] = Convert.ToInt16(strarr[16]);
                    spielfigur[4, 4] = Convert.ToInt16(strarr[17]);
                    zug = Convert.ToInt16(strarr[18]);
                    wuerfelzaehler = Convert.ToInt16(strarr[19]);
                    //if (Convert.ToInt16(strarr[20]) != amZug) versuche = Convert.ToInt16(strarr[21]); // Versuche nur Updaten bei neuem amZug Wert
                    amZug = Convert.ToInt16(strarr[20]);
                    labelConsole.Text = strarr[22];
                    setFigur(0, 0, 0);
                    // Farbkorrektur
                    if (amZug == 1) labelWurf.ForeColor = System.Drawing.Color.Red;
                    else if (amZug == 2) labelWurf.ForeColor = System.Drawing.Color.Blue;
                    else if (amZug == 3) labelWurf.ForeColor = System.Drawing.Color.Green;
                    else if (amZug == 4) labelWurf.ForeColor = System.Drawing.Color.Gold;
                    else labelWurf.ForeColor = System.Drawing.Color.Black;
                    //  0    1  2 3 4 5  6 7 8 9 101112131415161718192021222
                    // Net: syn:0:0:0:10:0:0:0:0:4:0:0:0:0:0:0:0:6:13:1:0:Pinkie Pie hat  
                }
                else if (str.StartsWith("Net: win")) // Gewonnen Meldung
                {
                    string[] strarr = str.Split(':');
                    // zugid = 0;
                    new Thread(new ParameterizedThreadStart(starteSound)).Start("win");
                    txtbx(colorname[Convert.ToInt16(strarr[2])] + " hat gewonnen!");
                    MessageBox.Show("Das Spiel wurde gewonnen!",
                   "Mensch Ärgere Dich nicht", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    spielende = true;
                }
                else if (str.StartsWith("Net: cup")) // Client Update
                {
                    string[] strarr = str.Split(':');
                    amZug = Convert.ToInt16(strarr[2]);
                    // Farbkorrektur
                    if (amZug == 1) labelWurf.ForeColor = System.Drawing.Color.Red;
                    else if (amZug == 2) labelWurf.ForeColor = System.Drawing.Color.Blue;
                    else if (amZug == 3) labelWurf.ForeColor = System.Drawing.Color.Green;
                    else if (amZug == 4) labelWurf.ForeColor = System.Drawing.Color.Gold;
                    else labelWurf.ForeColor = System.Drawing.Color.Black;
                }
                else if (str.StartsWith("Net: end"))
                {
                    txtbx("Verbindung verloren");
                    MessageBox.Show("Die Netzwerkverbindung wurde getrennt", "Netzwerk Fehler", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else if (str.StartsWith("Net: pau"))
                {
                    pause(true);
                }
                else if (str.StartsWith("Net: pas"))
                {
                    pause(false);
                }
                myturn = true; // und wieder auf true setzen damit Lanbefehle wieder rausgehen
            }
            catch (Exception e)
            {
                txtbx(e.ToString());
            }

        }

        // Syncronisiert die Netzwerkclients
        private void netsync()
        {
            if (srv != null)
            {
                srv.send("syn:" + spielfigur[1, 1] + ":"
                    + spielfigur[1, 2] + ":"
                    + spielfigur[1, 3] + ":"
                    + spielfigur[1, 4] + ":"
                    + spielfigur[2, 1] + ":"
                    + spielfigur[2, 2] + ":"
                    + spielfigur[2, 3] + ":"
                    + spielfigur[2, 4] + ":"
                    + spielfigur[3, 1] + ":"
                    + spielfigur[3, 2] + ":"
                    + spielfigur[3, 3] + ":"
                    + spielfigur[3, 4] + ":"
                    + spielfigur[4, 1] + ":"
                    + spielfigur[4, 2] + ":"
                    + spielfigur[4, 3] + ":"
                    + spielfigur[4, 4] + ":"
                    + zug + ":"
                    + wuerfelzaehler + ":"
                    + amZug + ":"
                    + versuche + ":"
                    + labelConsole.Text);
            }
        }

        // netzwerk scannen (broadcast)
        private void buttonNetScan_Click(object sender, EventArgs e)
        {
            byte[] byteBuffer = System.Text.Encoding.ASCII.GetBytes("hello Server");
            if (broadcast == null)
            {
                broadcast = new Server();
                if (checkBoxIPv6.Checked) broadcast.connect("udpv6", "7");
                else broadcast.connect("udp", "7");
                broadcast.UpdateText += broadcastincomming; // Ereignis abonnieren
            }
            listBoxNetServer.Items.Clear();
            broadcast.BroadCastSend(byteBuffer, 7);
        }

        // -- Netzwerk starten --
        private void buttonNetServer_Click(object sender, EventArgs e)
        {
            srv = new Server();
            srv.UpdateText += UpdateLabelText; // Ereignis abonnieren
            if (radioButtonNetTCP.Checked)
            {
                if (checkBoxIPv6.Checked) srv.connect("v6", numericUpDownNetPort.Value.ToString());
                else srv.connect("v4", numericUpDownNetPort.Value.ToString());
                numericUpDownKISpeed.Value = 500;
                numericUpDownKISpeed.Minimum = 500;
            }
            else
            {
                if (checkBoxIPv6.Checked) srv.connect("udpv6", numericUpDownNetPort.Value.ToString());
                else srv.connect("udp", numericUpDownNetPort.Value.ToString());
            }
            txtbx("Server Online");
            labelNetStatus.Text = "Sie sind verbunden!";
            MessageBox.Show("Der Server wurde erfolgreich geöffnet!",
                            "Über Mensch Ärgere Dich nicht", MessageBoxButtons.OK, MessageBoxIcon.Information);
            buttonCommand.Enabled = false;
            groupBoxNetwork.Enabled = false;
            srv.online();
            buttonDebug.Visible = true;
            buttonDebug.Text = "Server beenden";
            groupBoxNetwork.Size = new Size(206, 115);
            textBoxNetIP.Text = Server.GetLocalAddresses("")[0];
        }

        private void buttonNetzwerk_Click(object sender, EventArgs e)
        {
            string cltinfo;
            clt = new Client();
            clt.UpdateText += UpdateLabelTextc; // Ereignis abonnieren
            if (radioButtonNetTCP.Checked)
            {
                if (checkBoxIPv6.Checked) cltinfo = clt.connect(textBoxNetIP.Text, numericUpDownNetPort.Value.ToString(), "tcp", false);
                else cltinfo = clt.connect(textBoxNetIP.Text, numericUpDownNetPort.Value.ToString(), "tcp");
                numericUpDownKISpeed.Value = 500; ;
                numericUpDownKISpeed.Minimum = 500;
            }
            else
            {
                if (checkBoxIPv6.Checked) cltinfo = clt.connect(textBoxNetIP.Text, numericUpDownNetPort.Value.ToString(), "udp", false);
                else cltinfo = clt.connect(textBoxNetIP.Text, numericUpDownNetPort.Value.ToString());
            }
            txtbx(cltinfo);
            labelNetStatus.Text = cltinfo;
            clt.send("?");
            clt.send("Spieler: " + textBoxNetName.Text);
            MessageBox.Show(cltinfo, "Über Mensch Ärgere Dich nicht", MessageBoxButtons.OK, MessageBoxIcon.Information);
            // -
            // comboBoxNetFarbe.Enabled = true;
            //  comboBoxNetFarbe.Text = "Rot";
            buttonCommand.Text = "bereit!";
            pictureBoxSysBlau_Click(this, null);
            groupBoxNetwork.Enabled = false; // Hier sollte jtzt nicht mehr gespielt werden
            groupBoxRegeln.Enabled = false; // brauchen wa nicht
            buttonDebug.Visible = true;
            buttonDebug.Text = "Verbindung trennen";
            groupBoxNetwork.Size = new Size(206, 115);
        }
        #endregion

        private void starteSound(object otyp)
        {
            try
            {
                string typ = Convert.ToString(otyp);
                switch (typ)
                {
                    case "zug":
                        if (soundzug != "") PlaySound(soundzug, (IntPtr)0, 0);
                        break;
                    case "win":
                        if (soundwin != "") PlaySound(soundwin, (IntPtr)0, 0);
                        break;
                    case "kick":
                        if (soundkick != "") PlaySound(soundkick, (IntPtr)0, 0);
                        break;
                    case "wurf":
                        if (soundwurf != "") PlaySound(soundwurf, (IntPtr)0, 0);
                        break;
                }
            }
            catch { }
        }

        // -- Link www.quhfan.de --
        private void linkLabelKuhjunge_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.quhfan.de");
        }

        // -- About Schaltfläche --
        private void buttonAbout_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Mensch Ärgere dich nicht\r\n\r\nVersion: " + Assembly.GetExecutingAssembly().GetName().Version.ToString() + maedn2.Update.versionerw +
               "\r\nDas Benutzen dieses Programmes geschieht ausschließlich\r\nauf eigenem Risiko\r\n\r\nEntwicklung:\r\n© 2012 Chris Deter alias Kuhjunge",
               "Über Mensch Ärgere Dich nicht", MessageBoxButtons.OK, MessageBoxIcon.Information);
            /*+ "\r\n\r\nBesonderen Dank an:\r\n\r\nRike (Beta-Testerin und Idee)\r\nLea (Beta-Testerin)\r\nDas Gruselmädchen (Seelische Unterstüzung und Beta-Testerin)\r\n" +
    "Daniel (Beta-Tester)\r\nTorben Hansen (Hilfe bei Konzeption)\r\nPaddy (Beta-Tester und Kritiker)" */
        }

        private void pictureBoxWuerfel_Click(object sender, EventArgs e)
        {
            if (clt != null && myturn)
            {
                clt.send("cup:" + amZug + ":");
                clt.send("srq:" + ": X");
            }
            if ((theWuerfel && !spielende) && netzwerkzug(0))
            {
                new Thread(new ParameterizedThreadStart(starteSound)).Start("wurf");
                pictureBoxWuerfel.Image = picWn;
                wurf(true);
                if (versuche != 0 && versuche < 4) // Passt label für Versuche an
                    labelVersuche.Text = "Versuch Nr.: " + (versuche + 1).ToString();
                else labelVersuche.Text = "";
            }
            else if (!theWuerfel && !spielende && wuerfel == 0 && netzwerkzug(0))
            {
                new Thread(new ParameterizedThreadStart(starteSound)).Start("wurf");
                pictureBoxWuerfel.Image = picWn;
                wurf(true);
                if (versuche != 0 && versuche < 4) // Passt label für Versuche an
                    labelVersuche.Text = "Versuch Nr.: " + (versuche + 1).ToString();
                else labelVersuche.Text = "";
            }
        }

        bool netzwerkzug(int checkzug)
        {
            if (checkzug == 0) checkzug = amZug;
            bool zug = true;
            switch (checkzug)
            {
                case 1:
                    if (spielerRot == "Netzwerk") zug = false;
                    break;
                case 2:
                    if (spielerBlau == "Netzwerk") zug = false;
                    break;
                case 3:
                    if (spielerGruen == "Netzwerk") zug = false;
                    break;
                case 4:
                    if (spielerGelb == "Netzwerk") zug = false;
                    break;
            }
            return zug;
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            OnClosing(null);
        }

        private void listBoxNetServer_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                textBoxNetIP.Text = listBoxNetServer.SelectedItem.ToString();
            }
            catch { textBoxNetIP.Text = "172.0.0.1"; }
        }

        private void pictureBoxSysBlau_Click(object sender, EventArgs e)
        {
            if (spielerBlau != "Netzwerk" && clt == null)
            {
                if (pictureBoxSysBlau.Image == picSysBlau)
                {
                    pictureBoxSysBlau.Image = picSysBlauki;
                    spielerBlau = "Computer";
                }
                else if (pictureBoxSysBlau.Image == picSysBlauki)
                {
                    pictureBoxSysBlau.Image = picSysRaus;
                    spielerBlau = "";
                }
                else if (pictureBoxSysBlau.Image == picSysRaus)
                {
                    pictureBoxSysBlau.Image = picSysBlau;
                    spielerBlau = "Mensch";
                }
                labelBlau.Text = spielerBlau;
            }
            else if (clt != null)
            {
                updatefiguren("Netzwerk", "Mensch", "Netzwerk", "Netzwerk");
                spielerNetzwerk = 2;
            }
        }

        private void pictureBoxSysRot_Click(object sender, EventArgs e)
        {
            if (spielerRot != "Netzwerk" && clt == null)
            {
                if (pictureBoxSysRot.Image == picSysRot)
                {
                    pictureBoxSysRot.Image = picSysRotki;
                    spielerRot = "Computer";
                }
                else if (pictureBoxSysRot.Image == picSysRotki)
                {
                    pictureBoxSysRot.Image = picSysRaus;
                    spielerRot = "";
                }

                else if (pictureBoxSysRot.Image == picSysRaus)
                {
                    pictureBoxSysRot.Image = picSysRot;
                    spielerRot = "Mensch";
                }
                labelRot.Text = spielerRot;
            }
            else if (clt != null)
            {
                updatefiguren("Mensch", "Netzwerk", "Netzwerk", "Netzwerk");
                spielerNetzwerk = 1;
            }
        }

        private void pictureBoxSysGelb_Click(object sender, EventArgs e)
        {
            if (spielerGelb != "Netzwerk" && clt == null)
            {
                if (pictureBoxSysGelb.Image == picSysGelb)
                {
                    pictureBoxSysGelb.Image = picSysGelbki;
                    spielerGelb = "Computer";
                }
                else if (pictureBoxSysGelb.Image == picSysGelbki)
                {
                    pictureBoxSysGelb.Image = picSysRaus;
                    spielerGelb = "";
                }
                else if (pictureBoxSysGelb.Image == picSysRaus)
                {
                    pictureBoxSysGelb.Image = picSysGelb;
                    spielerGelb = "Mensch";
                }
                labelGelb.Text = spielerGelb;
            }
            else if (clt != null)
            {
                updatefiguren("Netzwerk", "Netzwerk", "Netzwerk", "Mensch");
                spielerNetzwerk = 4;
            }
        }

        private void pictureBoxSysGruen_Click(object sender, EventArgs e)
        {
            if (spielerGruen != "Netzwerk" && clt == null)
            {
                if (pictureBoxSysGruen.Image == picSysGruen)
                {
                    pictureBoxSysGruen.Image = picSysGruenki;
                    spielerGruen = "Computer";
                }
                else if (pictureBoxSysGruen.Image == picSysGruenki)
                {
                    pictureBoxSysGruen.Image = picSysRaus;
                    spielerGruen = "";
                }
                else if (pictureBoxSysGruen.Image == picSysRaus)
                {
                    pictureBoxSysGruen.Image = picSysGruen;
                    spielerGruen = "Mensch";
                }
                labelGruen.Text = spielerGruen;
            }
            else if (clt != null)
            {
                updatefiguren("Netzwerk", "Netzwerk", "Mensch", "Netzwerk");
                spielerNetzwerk = 3;
            }
        }

        private void updatefiguren(string Rot, string Blau, string Gruen, string Gelb)
        {
            spielerBlau = Blau;
            spielerRot = Rot;
            spielerGelb = Gelb;
            spielerGruen = Gruen;
            if (Gruen == "Netzwerk") { pictureBoxSysGruen.Image = picSysGruennet; }
            else if (Gruen == "Computer") { pictureBoxSysGruen.Image = picSysGruenki; }
            else if (Gruen == "Mensch") { pictureBoxSysGruen.Image = picSysGruen; }
            else if (Gruen == "") { pictureBoxSysGruen.Image = picSysRaus; }

            if (Rot == "Netzwerk") { pictureBoxSysRot.Image = picSysRotnet; }
            else if (Rot == "Computer") { pictureBoxSysRot.Image = picSysRotki; }
            else if (Rot == "Mensch") { pictureBoxSysRot.Image = picSysRot; }
            else if (Rot == "") { pictureBoxSysRot.Image = picSysRaus; }

            if (Blau == "Netzwerk") { pictureBoxSysBlau.Image = picSysBlaunet; }
            else if (Blau == "Computer") { pictureBoxSysBlau.Image = picSysBlauki; }
            else if (Blau == "Mensch") { pictureBoxSysBlau.Image = picSysBlau; }
            else if (Blau == "") { pictureBoxSysBlau.Image = picSysRaus; }

            if (Gelb == "Netzwerk") { pictureBoxSysGelb.Image = picSysGelbnet; }
            else if (Gelb == "Computer") { pictureBoxSysGelb.Image = picSysGelbki; }
            else if (Gelb == "Mensch") { pictureBoxSysGelb.Image = picSysGelb; }
            else if (Gelb == "") { pictureBoxSysGelb.Image = picSysRaus; }
            labelGruen.Text = Gruen;
            labelBlau.Text = Blau;
            labelGelb.Text = Gelb;
            labelRot.Text = Rot;
        }

        private void buttonDebug_Click(object sender, EventArgs e)
        {
            try
            {
                if (clt != null)
                {
                    clt.send("end:");
                    clt.disconnect();
                    buttonLoad.Enabled = true;
                    clt = null;
                }
                if (srv != null)
                {
                    srv.send("end:");
                    srv.close();
                    srv = null;
                }
                if (broadcast != null) broadcast.close();     
            }
            catch { } //(Exception se) { MessageBox.Show(se.Message); }
            txtbx("Netzwerk getrennt");
            labelNetStatus.Text = "Netzwerk getrennt!";
            buttonCommand.Enabled = true;
            groupBoxNetwork.Enabled = true;
            updatefiguren("Mensch", "Computer", "", "");
            buttonCommand.Text = "Spiel starten";
            buttonDebug.Visible = false;
            groupBoxNetwork.Size = new Size(206, 188);
            buttonNeuesSpiel.Enabled = true;
        }

        void pause(bool an)
        {
            if (an)
            {
                
                if (clt != null && myturn)
                {
                    clt.send("pau:");
                }
                if (srv != null && myturn)
                {
                    srv.send("pau:");
                }
                panelPause.Visible = true;
            }
            else 
            {
                if (clt != null && myturn)
                {
                    clt.send("pas:");
                }
                if (srv != null && myturn)
                {
                    srv.send("pas:");
                }
                panelOptions.Visible = false;
                panelPause.Visible = false;
                buttonEinstellungen.Visible = true;
                if (((spielerRot == "Computer" && amZug == 1) ||
                    (spielerBlau == "Computer" && amZug == 2) ||
                    (spielerGruen == "Computer" && amZug == 3) ||
                    (spielerGelb == "Computer" && amZug == 4)) && !spielende)
                {
                    kiaktiv = true;
                    ki();
                }
            }
        }
        //Ende
    }
}
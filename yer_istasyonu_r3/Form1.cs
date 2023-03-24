using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Device.Location;
using System.IO.Ports;
using System.Windows.Forms.DataVisualization.Charting;
using System.Threading;
using Microsoft.VisualBasic;
using GMap.NET;
using System.IO;
using GMap.NET.WindowsForms;

namespace yer_istasyonu_r3
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        // Roket aviyonik telemetri verileri
        struct RoketTelem
        {
            public byte paketSayaci;
            public float irtifa;
            public float gps_irtifa;
            public float gps_enlem;
            public float gps_boylam;
            public float jiroskop_x;
            public float jiroskop_y;
            public float jiroskop_z;
            public float ivme_x;
            public float ivme_y;
            public float ivme_z;
            public float aci;
            public float kademe_irtifa;
            public float kademe_enlem;
            public float kademe_boylam;
            public byte durum;
            public byte crc;

            public float basinc;
            public float sicaklik;
            public float pil_gerilimi;

        }

        // Görev Yükü aviyonik telemetri verileri
        struct GorevYukuTelem
        {
            public byte paketSayaci;
            public float gps_irtifa;
            public float gps_enlem;
            public float gps_boylam;
            public float irtifa;
            public float basinc;
            public float sicaklik;
            public float pil_gerilimi;
            public float nem;
            public byte durum;
            public byte crc;
        }

        // Hakem Yer İstasyonu telemetri verileri
        struct HakemYerIstasyonu
        {
            public byte teamID;
            public byte paketSayaci;
            public float roket_irtifa;
            public float roket_gps_irtifa;
            public float roket_gps_enlem;
            public float roket_gps_boylam;
            public float gy_gps_irtifa;
            public float gy_gps_enlem;
            public float gy_gps_boylam;
            public float kademe_gps_irtifa;
            public float kademe_gps_enlem;
            public float kademe_gps_boylam;
            public float jiroskop_x;
            public float jiroskop_y;
            public float jiroskop_z;
            public float ivme_x;
            public float ivme_y;
            public float ivme_z;
            public float aci;
            public byte durum;
            public byte crc; //checksome
        }


        // Hakem Yer İstasyonuna göndermek için yer istasyonunda float olan değerleri byte'a çevirip
        // oluşuturulan değişkenlere atıyoruz.
        struct bytesToHakemYerIstasyonu
        {
            public byte teamID;
            public byte paketSayaci;
            public byte[] roket_irtifa;
            public byte[] roket_gps_irtifa;
            public byte[] roket_gps_enlem;
            public byte[] roket_gps_boylam;
            public byte[] gy_gps_irtifa;
            public byte[] gy_gps_enlem;
            public byte[] gy_gps_boylam;
            public byte[] kademe_gps_irtifa;
            public byte[] kademe_gps_enlem;
            public byte[] kademe_gps_boylam;
            public byte[] jiroskop_x;
            public byte[] jiroskop_y;
            public byte[] jiroskop_z;
            public byte[] ivme_x;
            public byte[] ivme_y;
            public byte[] ivme_z;
            public byte[] aci;
            public byte durum;
            public byte crc; //checksome
        }


        string[] portsHYI = SerialPort.GetPortNames();
        string[] portsFY = SerialPort.GetPortNames();
        string[] portsROK = SerialPort.GetPortNames();

        bool HakemYerSistemDegis = true;




        private void Form1_Load(object sender, EventArgs e)
        {
            zamanRoket.Text = DateTime.Now.ToString("HH:mm:ss");

            //Gmap without Internet
            GMap.NET.GMaps.Instance.Mode = GMap.NET.AccessMode.ServerAndCache;
            map.CacheLocation = @"cache";
            map.MapProvider = GMap.NET.MapProviders.OpenStreetMapProvider.Instance;
            map.ShowCenter = true;
            map.DragButton = MouseButtons.Left;
            map.MinZoom = 0;
            map.MaxZoom = 24;
            map.Zoom = 14;

            map.Position = new GMap.NET.PointLatLng(38.399727, 33.711163);


            CheckForIllegalCrossThreadCalls = false;

            FYyukseklikChart.Titles.Add("FY-İrtifa: ").BackColor = Color.LightBlue;

            ASivmeZChart.Titles.Add("AS-İvme Z: ").BackColor = Color.DarkSeaGreen;
            ASyukseklikChart.Titles.Add("AS-İrtifa: ").BackColor = Color.DarkSeaGreen;

            //Arayüz başladığında 'Durdur' butonu deaktif olarak ayarlanıyor.
            lora1durButton.Enabled = false; //Roket
            lora2durButton.Enabled = false; // Faydalı Yük
            hyiVeriGonder_Button.Enabled = false; //HYİ'ye veri gönderme butonu

            //LORA1
            //Serial Port
            foreach (string port in portsFY)
            {
                Lora1PortsCombo.Items.Add(port);
            }
            // Seri port bağlı mı değil mi kontrol ediyor.
            if (Lora1PortsCombo.Items.Count == 2)
            {
                Lora1PortsCombo.SelectedIndex = 1;
            }
            else if (Lora1PortsCombo.Items.Count == 3)
            {
                Lora1PortsCombo.SelectedIndex = 2;
            }
            else if (Lora1PortsCombo.Items.Count == 4)
            {
                Lora1PortsCombo.SelectedIndex = 3;
            }
            else if (Lora1PortsCombo.Items.Count == 5)
            {
                Lora1PortsCombo.SelectedIndex = 4;
            }
            //Lora1 - seri port bağlı değilse


            //Baudrates
            Lora1HizCombo.Items.Add("4800");
            Lora1HizCombo.Items.Add("9600");
            Lora1HizCombo.Items.Add("19200");
            Lora1HizCombo.Items.Add("115200");
            Lora1HizCombo.SelectedIndex = 3;



            //Hakem Yer İstasyonu
            //Serial Port
            foreach (string port in portsHYI)
            {
                HYIPortsCombo.Items.Add(port);
            }
            // Seri port bağlı mı değil mi kontrol ediyor.
            if (HYIPortsCombo.Items.Count == 2)
            {
                HYIPortsCombo.SelectedIndex = 1;
            }
            else if (HYIPortsCombo.Items.Count == 3)
            {
                HYIPortsCombo.SelectedIndex = 2;
            }
            else if (HYIPortsCombo.Items.Count == 4)
            {
                HYIPortsCombo.SelectedIndex = 3;
            }
            else if (HYIPortsCombo.Items.Count == 5)
            {
                HYIPortsCombo.SelectedIndex = 4;
            }
            //Lora1 - seri port bağlı değilse


            //Baudrates
            HYIHizCombo.Items.Add("4800");
            HYIHizCombo.Items.Add("9600");
            HYIHizCombo.Items.Add("19200");
            HYIHizCombo.Items.Add("115200");
            HYIHizCombo.SelectedIndex = 2;




            //LORA2
            //Serial Port
            foreach (string port in portsROK)
            {
                Lora2PortsCombo.Items.Add(port);
            }
            // Seri port bağlı mı değil mi kontrol ediyor.
            if (Lora2PortsCombo.Items.Count == 2)
                Lora2PortsCombo.SelectedIndex = 1;
            else if (Lora1PortsCombo.Items.Count == 3)
            {
                Lora2PortsCombo.SelectedIndex = 2;
            }
            else if (Lora2PortsCombo.Items.Count == 4)
            {
                Lora2PortsCombo.SelectedIndex = 3;
            }
            else if (Lora2PortsCombo.Items.Count == 5)
            {
                Lora2PortsCombo.SelectedIndex = 4;
            }
            //Lora2 - seri port bağlı değilse


            //Baudrates
            Lora2HizCombo.Items.Add("4800");
            Lora2HizCombo.Items.Add("9600");
            Lora2HizCombo.Items.Add("19200");
            Lora2HizCombo.Items.Add("115200");
            Lora2HizCombo.SelectedIndex = 3;


        }


        private void btnAppRestart_Click(object sender, EventArgs e)
        {
            //Application.Restart();
            System.Diagnostics.Process.Start(Application.ExecutablePath); // to start new instance of application
            this.Close(); // System.Diagnostics.Process.GetCurrentProcess().Kill();

        }


        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // if koy aciksa kapat 
            if (timerSaveROKTxt.Enabled == true)
                anaRoketfile.Close();

            if (timerSaveGYTxt.Enabled == true)
                gyFile.Close();

            if (timerHataSaveTxt.Enabled == true)
                hatalarFile.Close();

            Environment.Exit(0);
        }

        // Faydalı Yük Konum Butonu
        private void FYLoc_Click(object sender, EventArgs e)
        {
            if (timerFY.Enabled == true)
            {
                try
                {
                    if (FYLoc.Text == "START FY")
                    {
                        FYLoc.Text = "STOP FY";
                        FYLoc.BackColor = Color.Red;

                        lblDurum.Text = "FY Konumu Gosteriliyor..";
                        lblDurum.BackColor = Color.Green;

                        timerMapFY.Start();
                    }
                    else if (FYLoc.Text == "STOP FY")
                    {
                        FYLoc.Text = "START FY";
                        FYLoc.BackColor = Color.Green;

                        lblDurum.Text = "FY Konumu Durdu";
                        lblDurum.BackColor = Color.Red;

                        timerMapFY.Stop();
                    }

                    if (FYgpsLatLblTxt.Text == "000" && FYgpsLngLblTxt.Text == "000")
                    {
                        FYLoc.Text = "START FY";
                        FYLoc.BackColor = Color.Green;

                        lblDurum.Text = "FY Konumu Başlatılamadı..";
                        lblDurum.BackColor = Color.Red;

                        timerMapFY.Stop();
                    }
                }
                catch (Exception errFYloc)
                {
                    error_richtextbox.Text = "FY Location Error: " + errFYloc + "\n";
                }

                map.DragButton = MouseButtons.Left; // harita üzerinde sürükleme yapılabilir
                map.MapProvider = GMap.NET.MapProviders.GoogleMapProvider.Instance;
            }


        }

        // Roket Konum Butonu
        private void roketLoc_Click(object sender, EventArgs e)
        {
            if (timerROK.Enabled == true)
            {
                try
                {
                    if (roketLoc.Text == "START Roket")
                    {
                        Console.WriteLine("girdi");
                        roketLoc.Text = "STOP Roket";
                        roketLoc.BackColor = Color.Red;

                        lblDurum.Text = "Roket Konumu Gosteriliyor..";
                        lblDurum.BackColor = Color.Green;

                        timerMapROK.Start();
                    }
                    else if (roketLoc.Text == "STOP Roket")
                    {
                        roketLoc.Text = "START Roket";
                        roketLoc.BackColor = Color.Green;

                        lblDurum.Text = "Roket Konumu Durdu";
                        lblDurum.BackColor = Color.Red;

                        timerMapROK.Stop();
                    }

                    if (ROK1gpsLatLblTxt.Text == "000" && ROK1gpsLngLblTxt.Text == "000")
                    {
                        roketLoc.Text = "START Roket";
                        roketLoc.BackColor = Color.Green;

                        lblDurum.Text = "Roket Konumu Başlatılamadı..";
                        lblDurum.BackColor = Color.Red;

                        timerMapROK.Stop();
                    }
                }
                catch (Exception errROKloc)
                {
                    error_richtextbox.Text = "Roket Location Error: " + errROKloc + "\n";
                }

                map.DragButton = MouseButtons.Left; // harita üzerinde sürükleme yapılabilir
                map.MapProvider = GMap.NET.MapProviders.GoogleMapProvider.Instance;
            }

        }

        byte[] gidenpaket = new byte[78];

        byte sabit1 = 0xFF;
        byte hb_sabit2 = 0x54;
        byte hb_sabit3 = 0x52;
        byte hb_sonSabit1 = 0x0D;
        byte hb_sonSabit2 = 0x0A;

        byte sayac = 0, roket_sayac = 0, gy_sayac = 0;
        byte check_sum_hesapla()
        {
            int check_sum = 0;
            for (int i = 4; i < 75; i++)
            {
                check_sum += gidenpaket[i];
            }
            return (byte)(check_sum % 256);
        }


        private void hyiBaglanButton_Click(object sender, EventArgs e)
        {
            if (hyiSerial.IsOpen == false)
            {
                if (HYIPortsCombo.Text != "")
                {
                    hyiSerial.PortName = HYIPortsCombo.SelectedItem.ToString();
                    hyiSerial.BaudRate = Convert.ToInt32(HYIHizCombo.Text);
                }
                else
                {
                    errors_label.Text = "HYI portu bulunmuyor..";
                }

                try
                {
                    hyiSerial.Open();

                    hyiBaglanButton.Text = "Disconnect";
                    hyiBaglanButton.BackColor = Color.LightPink;
                    hyiBaglanButton.ForeColor = Color.Red;

                    hyiVeriGonder_Button.Enabled = true;

                }
                catch (Exception hata)
                {
                    error_richtextbox.Text = "HYI yazdirma Hata: " + hata.Message + "\n";
                }
            }

            else if (hyiSerial.IsOpen == true)
            {

                hyiSerial.Close();
                hyiBaglanButton.Text = "Connect";
                hyiBaglanButton.BackColor = Color.White;
                hyiBaglanButton.ForeColor = Color.Green;
                hyiVeriGonder_Button.Enabled = false;
                // Gönder butonu deaktif oluyor..
                timerHakemYer.Stop();
                hyiVeriGonder_Button.Text = "Gönder";
                hyiVeriGonder_Button.BackColor = Color.LightGreen;

            }


        }

        private void timerKonumSil_Tick(object sender, EventArgs e)
        {
            markers.Markers.Clear();
            map.Overlays.Clear();
        }

        GMap.NET.WindowsForms.GMapOverlay markers = new GMap.NET.WindowsForms.GMapOverlay("markers");

        private void hyiVeriGonder_Button_Click(object sender, EventArgs e)
        {

            // -------------- TAKIM ID KONTROL --------------
            int num;
            if (teamIDtextBox.Text == "")
            {
                error_richtextbox.Text = "LUTFEN TAKIM ID GIRIN!!!";
                teamIDtextBox.BackColor = Color.LightPink;

            }

            else if (!Int32.TryParse(teamIDtextBox.Text, out num))
            {
                error_richtextbox.Text = "LUTFEN TAKIM ID yi NUMARA OLARAK GIRIN!!!";
                teamIDtextBox.BackColor = Color.LightPink;

            }

            else if (teamIDtextBox.Text.Length > 3)
            {
                error_richtextbox.Text = "TAKIM ID 3 HANEDEN FAZLA OLAMAZ!";
            }


            else
            {
                if (hyiVeriGonder_Button.Text == "Gönder")
                {
                    timerHakemYer.Start(); // HYİ'ye veri göndermek için timerı başlatıyoruz.
                    teamIDtextBox.BackColor = Color.LightGreen;
                    error_richtextbox.Text = "";

                    hyiVeriGonder_Button.Text = "Durdur";
                    hyiVeriGonder_Button.BackColor = Color.White;
                    hyiVeriGonder_Button.ForeColor = Color.Red;

                }

                else if (hyiVeriGonder_Button.Text == "Durdur")
                {
                    timerHakemYer.Stop();
                    hyiVeriGonder_Button.Text = "Gönder";
                    hyiVeriGonder_Button.BackColor = Color.White;
                    hyiVeriGonder_Button.ForeColor = Color.Green;
                }
            }


        }

        double fark_lat, fark_lng, ort_mesafe, ort_lat, ort_lng;


        double FYlat, FYlng;
        private void konum_ortala_Click(object sender, EventArgs e)
        {
            if (FYLoc.Text == "STOP FY" && roketLoc.Text == "STOP Roket")
            {
                if (timerMapOrtala.Enabled == false)
                {
                    timerKonumSil.Start();
                    timerMapOrtala.Start();
                    konum_ortala.BackColor = Color.Green;
                }

                else
                {
                    timerKonumSil.Stop();
                    timerMapOrtala.Stop();
                    konum_ortala.BackColor = Color.Red;
                }
            }

        }

        GMap.NET.WindowsForms.GMapMarker fylatmarker;

        private void timerGyMarkerSil_Tick(object sender, EventArgs e)
        {
            markers.Markers.Clear();
            map.Overlays.Clear();
        }

        double rlat, rlng, ralt;

        private void timerRokMarkerSil_Tick_1(object sender, EventArgs e)
        {
            markers.Markers.Clear();
            map.Overlays.Clear();
        }


        RoketTelem roketAnaHaberlesme = new RoketTelem();
        RoketTelem roketAnaLast = new RoketTelem();

        GorevYukuTelem gorevYuku = new GorevYukuTelem();
        HakemYerIstasyonu hakemIstasyonu = new HakemYerIstasyonu();
        bytesToHakemYerIstasyonu bytesHakemIstasyonu = new bytesToHakemYerIstasyonu();

        List<byte> incomebytes = new List<byte>();
        byte[] bytes, fy_bytes;
        byte fy_inbyte;

        void thread_FYincomingData()
        {

            while (true)
            {
                try
                {

                    if (serialPort1.BytesToRead != 0)
                    {

                        fy_inbyte = (byte)serialPort1.ReadByte();

                        if (incomebytes.Count != 0 && incomebytes.Last() == 0xA0 && fy_inbyte == 0x65)
                        {
                            Console.WriteLine("girdi ");
                            int i;
                            for (i = 0; i < incomebytes.Count - 1; i++)
                            {
                                //Console.WriteLine(incomebytes[i]);
                                if (incomebytes[i] == 0xA0 && incomebytes[i + 1] == 0x65)
                                    break;
                            }

                            incomebytes.RemoveRange(0, incomebytes.IndexOf(0xFF));

                            fy_bytes = incomebytes.ToArray();
                            Console.WriteLine("boyut" + fy_bytes.Length);


                            if (fy_bytes.Length == 26)
                            {
                                gy_sayac++;
                                gorevYuku.paketSayaci = gy_sayac; //hakemIstasyonu.paketSayaci = 
                                gorevYuku.gps_irtifa = System.BitConverter.ToSingle(fy_bytes, 4); //hakemIstasyonu.gy_gps_irtifa = 
                                gorevYuku.gps_enlem = System.BitConverter.ToSingle(fy_bytes, 8); //hakemIstasyonu.gy_gps_enlem = 
                                gorevYuku.gps_boylam = System.BitConverter.ToSingle(fy_bytes, 12); //hakemIstasyonu.gy_gps_boylam = 
                                //gorevYuku.irtifa = System.BitConverter.ToSingle(fy_bytes, 17);
                                gorevYuku.irtifa = System.BitConverter.ToSingle(fy_bytes, 16);
                                gorevYuku.nem = System.BitConverter.ToSingle(fy_bytes, 20);
                                gorevYuku.crc = fy_bytes[24];
                                Console.WriteLine("fy crc: " + gorevYuku.crc);
                                FY_check_sum_hesapla();

                                gorevYukuGelenVeri = gy_sayac.ToString() + ", " + Convert.ToString(gorevYuku.gps_irtifa) + ", " +
                                    Convert.ToString(gorevYuku.gps_enlem) + Convert.ToString(gorevYuku.gps_boylam) + ", " +
                                    Convert.ToString(gorevYuku.irtifa) + ", " + Convert.ToString(gorevYuku.nem) + ", " + "gy";

                            }

                            incomebytes.Clear();

                            Console.WriteLine("Paket sonu");
                        }
                        incomebytes.Add(fy_inbyte);

                    }


                }
                catch (Exception ex_err)
                {
                    error_richtextbox.Text = "FY veri alma: " + ex_err.Message + "\n";

                    /*try
                    {
                        //portYenile();
                        Console.WriteLine("STOP: ");
                        if (loraFYname != Lora1PortsCombo.Text)
                        {
                            timerFY.Stop();
                            lora1.Abort();
                        }
                    }
                    catch (Exception)
                    {
                        throw;
                    }*/
                }

            }

        }

        byte FY_check_sum_hesapla()
        {
            int fy_check_sum = 0;
            for (int i = 4; i < 24; i++)
            {
                if (fy_bytes != null)
                {
                    fy_check_sum += fy_bytes[i];
                    //error_richtextbox.Text = "";
                }
                else
                {
                    error_richtextbox.Text = "fy bytes: null";
                }
            }

            Console.WriteLine(fy_check_sum % 256);
            return (byte)(fy_check_sum % 256);
        }


        void thread_RoketIncomingData()
        {
            while (true)
            {
                try
                {
                    if (serialPort2.BytesToRead != 0)
                    {

                        byte inbyte = (byte)serialPort2.ReadByte();

                        if (incomebytes.Count != 0 && incomebytes.Last() == 0xA1 && inbyte == 0x61)
                        {
                            Console.WriteLine("girdi ");
                            int t = 0;
                            for (t = 0; t < incomebytes.Count - 1; t++)
                            {
                                //Console.WriteLine(incomebytes[t]);

                                if (incomebytes[t] == 0xA1 && incomebytes[t + 1] == 0x61)
                                    break;

                            }

                            incomebytes.RemoveRange(0, incomebytes.IndexOf(0xFF));

                            bytes = incomebytes.ToArray();
                            Console.WriteLine("boyut" + bytes.Length);

                            //hakemIstasyonu.teamID = byte.Parse(textBox1.Text);

                            if ((bytes.Length == 51 || bytes.Length == 52))
                            {
                                roket_sayac++;
                                roketAnaHaberlesme.paketSayaci = roket_sayac; //hakemIstasyonu.paketSayaci =
                                roketAnaHaberlesme.gps_irtifa = System.BitConverter.ToSingle(bytes, 4); // hakemIstasyonu.roket_gps_irtifa = 
                                roketAnaHaberlesme.gps_enlem = System.BitConverter.ToSingle(bytes, 8); // hakemIstasyonu.roket_gps_enlem =
                                roketAnaHaberlesme.gps_boylam = System.BitConverter.ToSingle(bytes, 12); //hakemIstasyonu.roket_gps_boylam =
                                roketAnaHaberlesme.kademe_irtifa = 0x00; //YOK  / hakemIstasyonu.kademe_gps_irtifa =
                                roketAnaHaberlesme.kademe_enlem = 0x00; //YOK // hakemIstasyonu.kademe_gps_enlem =
                                roketAnaHaberlesme.kademe_boylam = 0x00; //YOK // hakemIstasyonu.kademe_gps_boylam =
                                roketAnaHaberlesme.jiroskop_x = System.BitConverter.ToSingle(bytes, 16); // hakemIstasyonu.jiroskop_x =
                                roketAnaHaberlesme.jiroskop_y = System.BitConverter.ToSingle(bytes, 20); // hakemIstasyonu.jiroskop_y =
                                roketAnaHaberlesme.jiroskop_z = System.BitConverter.ToSingle(bytes, 24); // hakemIstasyonu.jiroskop_z =
                                roketAnaHaberlesme.ivme_x = System.BitConverter.ToSingle(bytes, 28); // hakemIstasyonu.ivme_x =
                                roketAnaHaberlesme.ivme_y = System.BitConverter.ToSingle(bytes, 32); // hakemIstasyonu.ivme_y =
                                roketAnaHaberlesme.ivme_z = System.BitConverter.ToSingle(bytes, 36); // hakemIstasyonu.ivme_z =
                                roketAnaHaberlesme.aci = System.BitConverter.ToSingle(bytes, 40); // hakemIstasyonu.aci =
                                roketAnaHaberlesme.irtifa = System.BitConverter.ToSingle(bytes, 44); //** // hakemIstasyonu.roket_irtifa =
                                roketAnaHaberlesme.durum = bytes[48]; // hakemIstasyonu.durum =
                                roketAnaHaberlesme.crc = bytes[49]; // hakemIstasyonu.crc =
                                Console.WriteLine("crc1: " + roketAnaHaberlesme.crc);
                                //AS_check_sum_hesapla();

                                anaRoketGelenVeri = roket_sayac.ToString() + ", " + Convert.ToString(roketAnaHaberlesme.irtifa) + ", " +
                                    Convert.ToString(roketAnaHaberlesme.gps_irtifa) + ", " + Convert.ToString(roketAnaHaberlesme.gps_enlem) +
                                    ", " + Convert.ToString(roketAnaHaberlesme.gps_boylam) + ", " + Convert.ToString(roketAnaHaberlesme.jiroskop_x)
                                    + ", " + Convert.ToString(roketAnaHaberlesme.jiroskop_y) + ", " + Convert.ToString(roketAnaHaberlesme.jiroskop_z)
                                    + ", " + Convert.ToString(roketAnaHaberlesme.ivme_x) + ", " + Convert.ToString(roketAnaHaberlesme.ivme_y) + ", "
                                    + Convert.ToString(roketAnaHaberlesme.ivme_z) + ", " + Convert.ToString(roketAnaHaberlesme.aci) + ", " +
                                    Convert.ToString(roketAnaHaberlesme.durum) + ", " + "ana";

                            }
                            incomebytes.Clear();
                        }

                        incomebytes.Add(inbyte);
                    }
                }
                catch (OverflowException ex)
                {
                    error_richtextbox.Text = "ROKET veri alma(overFlow): " + ex.Message + "\n";
                }
                catch (Exception ex_err)
                {
                    error_richtextbox.Text = "ROKET veri alma: " + ex_err.Message + "\n";

                    /*try
                    {
                        //portYenile();

                        if (loraROKname != Lora2PortsCombo.Text)
                        {
                            timerROK.Stop();
                            roketIncoming.Abort();
                            
                        }
                    }
                    catch (Exception)
                    {
                        throw;
                    }*/
                }

            }
        }

        byte AS_check_sum_hesapla()
        {
            int as_check_sum = 0;
            for (int i = 4; i < 49; i++)
            {
                if (bytes != null)
                {
                    as_check_sum += bytes[i];
                    //error_richtextbox.Text = "";
                }
                else
                {
                    error_richtextbox.Text = "bytes: null";
                }

            }
            //Console.WriteLine(as_check_sum % 256);
            return (byte)(as_check_sum % 256);

        }


        /*void thread_yedekRoketIncomingData()
        {
            while (true)
            {
                try
                {

                    if (serialPort3.BytesToRead != 0) //serialPort1
                    {

                        byte yedek_inbyte = (byte)serialPort3.ReadByte(); //serialPort1

                        // YEDEK
                        if (incomebytes.Count != 0 && incomebytes.Last() == 0xA1 && yedek_inbyte == 0x62)
                        {
                            Console.WriteLine("yedek girdi2 ");
                            int t = 0;
                            for (t = 0; t < incomebytes.Count - 1; t++)
                            {
                                //Console.WriteLine(incomebytes[t]);

                                if (incomebytes[t] == 0xA1 && incomebytes[t + 1] == 0x62)
                                    break;

                            }

                            incomebytes.RemoveRange(0, incomebytes.IndexOf(0xFF));

                            yedek_bytes = incomebytes.ToArray();
                            Console.WriteLine("boyut" + yedek_bytes.Length);

                            //hakemIstasyonu.teamID = byte.Parse(textBox1.Text);

                            if (yedek_bytes.Length == 51)
                            {
                                roketYedek_sayac++;
                                roketYedekHaberlesme.paketSayaci = roketYedek_sayac; //hakemIstasyonu.paketSayaci =
                                roketYedekHaberlesme.gps_irtifa = System.BitConverter.ToSingle(yedek_bytes, 4);
                                roketYedekHaberlesme.gps_enlem = System.BitConverter.ToSingle(yedek_bytes, 8);
                                roketYedekHaberlesme.gps_boylam = System.BitConverter.ToSingle(yedek_bytes, 12);
                                roketYedekHaberlesme.kademe_irtifa = 0x00; //YOK
                                roketYedekHaberlesme.kademe_enlem = 0x00; //YOK
                                roketYedekHaberlesme.kademe_boylam = 0x00; //YOK
                                roketYedekHaberlesme.jiroskop_x = System.BitConverter.ToSingle(yedek_bytes, 16);
                                roketYedekHaberlesme.jiroskop_y = System.BitConverter.ToSingle(yedek_bytes, 20);
                                roketYedekHaberlesme.jiroskop_z = System.BitConverter.ToSingle(yedek_bytes, 24);
                                roketYedekHaberlesme.ivme_x = System.BitConverter.ToSingle(yedek_bytes, 28);
                                roketYedekHaberlesme.ivme_y = System.BitConverter.ToSingle(yedek_bytes, 32);
                                roketYedekHaberlesme.ivme_z = System.BitConverter.ToSingle(yedek_bytes, 36);
                                roketYedekHaberlesme.aci = System.BitConverter.ToSingle(yedek_bytes, 40);
                                roketYedekHaberlesme.irtifa = System.BitConverter.ToSingle(yedek_bytes, 44); //**
                                roketYedekHaberlesme.durum = yedek_bytes[48];
                                roketYedekHaberlesme.crc = yedek_bytes[49];
                                Console.WriteLine("crc1: " + roketYedekHaberlesme.crc);

                                yedekRoketGelenVeri = roketYedek_sayac.ToString() + ", " + Convert.ToString(roketYedekHaberlesme.irtifa) + ", " +
                                    Convert.ToString(roketYedekHaberlesme.gps_irtifa) + ", " + Convert.ToString(roketYedekHaberlesme.gps_enlem) +
                                    ", " + Convert.ToString(roketYedekHaberlesme.gps_boylam) + ", " + Convert.ToString(roketYedekHaberlesme.jiroskop_x)
                                    + ", " + Convert.ToString(roketYedekHaberlesme.jiroskop_y) + ", " + Convert.ToString(roketYedekHaberlesme.jiroskop_z)
                                    + ", " + Convert.ToString(roketYedekHaberlesme.ivme_x) + ", " + Convert.ToString(roketYedekHaberlesme.ivme_y) + ", "
                                    + Convert.ToString(roketYedekHaberlesme.ivme_z) + ", " + Convert.ToString(roketYedekHaberlesme.aci) + ", " +
                                    Convert.ToString(roketYedekHaberlesme.durum) + ", " + "yedek";
                            }

                            incomebytes.Clear();

                            Console.WriteLine("Paket sonu");
                        }
                        incomebytes.Add(yedek_inbyte);

                    }
                }
                catch (OverflowException ex)
                {
                    error_richtextbox.Text = "yedek ROKET veri alma(overFlow): " + ex.Message + "\n";
                }
                catch (Exception ex_err)
                {
                    error_richtextbox.Text = "yedek ROKET veri alma: " + ex_err.Message + "\n";

                    /* try
                     {
                         portYenile();

                         if (loraYedekROKname != Lora3PortsCombo.Text)
                         {
                             timerYedekROK.Stop();
                             yedekRoketIncoming.Abort();
                         }
                     }
                     catch (Exception)
                     {
                         throw;
                     }
                }

            }
        }*/

        Thread lora1;
        private void lora1yazButton_Click(object sender, EventArgs e)
        {
            timerFY.Start();
            if (serialPort1.IsOpen == false)
            {
                if (Lora1PortsCombo.Text != "")
                {
                    serialPort1.PortName = Lora1PortsCombo.Text;
                    serialPort1.BaudRate = Convert.ToInt32(Lora1HizCombo.Text);
                }
                else
                {
                    errors_label.Text = "GY portu bulunmuyor..";
                }

                try
                {
                    serialPort1.Open();
                    //loraFYname = Lora1PortsCombo.Text;

                    if (timerFY.Enabled == true)
                    {
                        fileGorevYuku = Path.Combine(Environment.CurrentDirectory, zamanRoket.Text.Replace(":", "_").Remove(zamanRoket.Text.Length - 3) + "_gorevYuku.txt");
                        gyFile = new StreamWriter(fileGorevYuku, true);
                        Console.WriteLine("file GY ana ROKET: " + fileGorevYuku);
                    }


                    timerSaveGYTxt.Start();

                    lora1yazButton.Enabled = false;
                    lora1durButton.Enabled = true;

                    lora1 = new Thread(thread_FYincomingData);
                    lora1.Start();
                }
                catch (Exception hata)
                {
                    error_richtextbox.Text = "Lora1 yazdirma Hata: " + hata.Message + "\n";
                }
            }

            else
            {
                lora1durButton.Enabled = false;
            }

        }


        private void lora1durButton_Click(object sender, EventArgs e)
        {
            try
            {
                gyFile.Close();

                timerFY.Stop();
                timerKonumSil.Stop();
                timerMapOrtala.Stop();
                timerMapFY.Stop();

                serialPort1.DiscardInBuffer(); // iki değer okuma, deneme

                if (serialPort1.IsOpen == true)
                {
                    try
                    {
                        serialPort1.Close();
                        timerFY.Stop();
                        lora1yazButton.Enabled = true;
                        lora1durButton.Enabled = false;

                        if (serialPort2.IsOpen == false)
                        {
                            lora2yazButton.Enabled = true;
                            lora2durButton.Enabled = false;
                        }

                    }
                    catch (Exception hata)
                    {
                        error_richtextbox.Text = "Lora1 Durdurma Hata: " + hata.Message + "\n";
                    }
                }

                lora1.Abort();
            }
            catch (Exception err)
            {

                error_richtextbox.Text = "FY HATA: " + err.Message + "\n";
            }

        }

        Thread FYprinted;
        private void timerFY_Tick(object sender, EventArgs e)
        {
            FYprinted = new Thread(thread_FYprintedData);
            FYprinted.Start();

        }

        Thread roketIncoming;
        private void lora2yazButton_Click(object sender, EventArgs e)
        {
            timerROK.Start();
            if (serialPort2.IsOpen == false)
            {
                if (Lora2PortsCombo.Text != "")
                {
                    serialPort2.PortName = Lora2PortsCombo.Text;
                    serialPort2.BaudRate = Convert.ToInt32(Lora2HizCombo.Text);
                }
                else
                {
                    errors_label.Text = "Lora2 (Ana Roket) portu bulunmuyor..";
                }

                try
                {
                    serialPort2.Open();

                    if (timerROK.Enabled == true)
                    {
                        fileAnaRoket = Path.Combine(Environment.CurrentDirectory, zamanRoket.Text.Replace(":", "_").Remove(zamanRoket.Text.Length - 3) + "_anaRok.txt");
                        anaRoketfile = new StreamWriter(fileAnaRoket, true);
                        Console.WriteLine("file ana ROKET: " + fileAnaRoket);
                    }
                    timerSaveROKTxt.Start();

                    lora2yazButton.Enabled = false;
                    lora2durButton.Enabled = true;


                    roketIncoming = new Thread(thread_RoketIncomingData);
                    roketIncoming.Start();
                }
                catch (Exception hata)
                {
                    error_richtextbox.Text = "Lora2 yazdirma Hata: " + hata.Message + "\n";
                }
            }

            else
            {
                lora2durButton.Enabled = false;
            }
        }

        private void lora2durButton_Click(object sender, EventArgs e)
        {
            try
            {
                anaRoketfile.Close();

                timerROK.Stop();

                serialPort2.DiscardInBuffer(); // iki değer okuma, deneme

                if (serialPort2.IsOpen == true)
                {
                    try
                    {
                        serialPort2.Close();
                        timerROK.Stop();
                        lora2yazButton.Enabled = true;
                        lora2durButton.Enabled = false;

                    }
                    catch (Exception hata)
                    {
                        error_richtextbox.Text = "Lora2 durdurma Hata: " + hata.Message + "\n";
                    }
                }
                roketIncoming.Abort();
            }
            catch (Exception err)
            {
                error_richtextbox.Text = "ROKET SERIAL HATA: " + err.Message + "\n";
            }
        }


        List<string> anaRoketList = new List<string>();
        List<string> yedekRoketList = new List<string>();
        List<string> gorevYukuList = new List<string>();


        // Yer istasyonuna yazdırılan Roket Verisi
        private void timerROK_Tick(object sender, EventArgs e)
        {
            zamanRoket.Text = DateTime.Now.ToString("HH:mm:ss");
            if (anaRoketGelenVeri != null)
            {
                Thread t2 = new Thread(thread_ROKprintedData);
                t2.Start();
            }

        }


        private void timerMapFY_Tick(object sender, EventArgs e)
        {
            timerMapFY.Enabled = true;

            try
            {
                FYlat = Convert.ToDouble(FYgpsLatLblTxt.Text); //ornek konum
                FYlng = Convert.ToDouble(FYgpsLngLblTxt.Text); ; //ornek konum
            }
            catch (Exception mapErr)
            {
                errors_label.Text += "Konum alma(timerMap) Hata: " + mapErr.Message;
            }


            if (timerMapOrtala.Enabled != true)
            {
                map.Position = new GMap.NET.PointLatLng(FYlat, FYlng);

                map.MinZoom = 0;
                map.MaxZoom = 24;
                map.Zoom = 13;
            }

            FYlocLatLblTxt.Text = Convert.ToString(FYlat);
            FYlocLngLblTxt.Text = Convert.ToString(FYlng);

            fylatmarker =
                   new GMap.NET.WindowsForms.Markers.GMarkerGoogle(
                       new GMap.NET.PointLatLng(FYlat, FYlng),
                       GMap.NET.WindowsForms.Markers.GMarkerGoogleType.blue_small);
            markers.Markers.Add(fylatmarker);
            map.Overlays.Add(markers);

            timerGyMarkerSil.Start();
        }

        private void timerMapROK_Tick_1(object sender, EventArgs e)
        {
            timerMapROK.Enabled = true;

            rlat = Convert.ToDouble(ROK1gpsLatLblTxt.Text);
            rlng = Convert.ToDouble(ROK1gpsLngLblTxt.Text);
            ralt = Convert.ToDouble(ROK1gpsAltLblTxt.Text);

            lblDurum.Text = "Ana Sistem Gösteriliyor..";

            //else if (YsBtn.Enabled == false)
            //{
            //    rlat = Convert.ToDouble(ROK2gpsLatLblTxt.Text);
            //    rlng = Convert.ToDouble(ROK2gpsLngLblTxt.Text);
            //    ralt = Convert.ToDouble(ROK2gpsAltLblTxt.Text);

            //    lblDurum.Text = "Yedek Sistem Gösteriliyor..";
            //}


            if (timerMapOrtala.Enabled != true)
            {
                map.Position = new GMap.NET.PointLatLng(rlat, rlng);

                map.MinZoom = 0;
                map.MaxZoom = 24;
                map.Zoom = 14;
            }

            ROKlocLatLblTxt.Text = Convert.ToString(rlat);
            ROKlocLngLblTxt.Text = Convert.ToString(rlng);
            ROKlocAltLblTxt.Text = Convert.ToString(ralt);

            GMap.NET.WindowsForms.GMapMarker marker2 =
                    new GMap.NET.WindowsForms.Markers.GMarkerGoogle(
                        new GMap.NET.PointLatLng(rlat, rlng),
                        GMap.NET.WindowsForms.Markers.GMarkerGoogleType.green_small);
            markers.Markers.Add(marker2);
            map.Overlays.Add(markers);


            timerRokMarkerSil.Start();
        }

        private void timerSaveROKTxt_Tick(object sender, EventArgs e)
        {
            if (serialPort2.IsOpen && anaRoketfile.BaseStream != null)
            {
                Thread threadAnaTxt = new Thread(new ThreadStart(anaRoketTxtKayit));
                threadAnaTxt.Start();
            }
        }

        private void timerSaveGYTxt_Tick(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen && gyFile.BaseStream != null)
            {
                Thread threadGorevYukuTxt = new Thread(new ThreadStart(gorevYukuTxtKayit));
                threadGorevYukuTxt.Start();
            }
        }

        private void timerMapOrtala_Tick(object sender, EventArgs e)
        {
            fark_lat = Math.Pow((Convert.ToDouble(FYgpsLatLblTxt.Text) - rlat), 2);
            fark_lng = Math.Pow((Convert.ToDouble(FYgpsLngLblTxt.Text) - rlng), 2);
            ort_mesafe = Math.Sqrt(fark_lat + fark_lng);
            Console.WriteLine(ort_mesafe);

            ort_lat = (Convert.ToDouble(FYgpsLatLblTxt.Text) + rlat) / 2;
            ort_lng = (Convert.ToDouble(FYgpsLngLblTxt.Text) + rlng) / 2;

            if ((FYgpsLatLblTxt.Text != "000" && FYgpsLngLblTxt.Text != "000" && ((ROK1gpsLatLblTxt.Text != "000" && ROK1gpsLngLblTxt.Text != "000"))) && (timerMapFY.Enabled == true && (timerMapROK.Enabled == true)))
            {

                if (timerMapFY.Enabled == true && timerMapROK.Enabled == true)
                {
                    map.Position = new GMap.NET.PointLatLng(ort_lat, ort_lng);
                    ortMesafe.Text = ort_mesafe.ToString() + " \n " + "Zoom: " + map.Zoom;
                }
                else if (timerMapFY.Enabled == false && timerMapROK.Enabled == true)
                {
                    markers.Clear();
                    lblDurum.Text = "Faydali Yük Konumu Alınamıyor..";
                    //map.Refresh();
                }
                else if (timerMapFY.Enabled == true && timerMapROK.Enabled == false)
                {
                    markers.Clear();
                    lblDurum.Text = "Roket Konumu Alınamıyor..";
                    //map.Refresh();
                }
                else
                {
                    markers.Clear();
                    lblDurum.Text = "Konumlar Alınamıyor..";
                    //map.Refresh();
                }

                if (ort_mesafe <= 0.09)
                {
                    map.Zoom = 12;
                }

                else if (0.09 < ort_mesafe && ort_mesafe <= 0.51)
                {
                    map.Zoom = 10;
                }

                else if (0.51 < ort_mesafe && ort_mesafe <= 0.95)
                {
                    map.Zoom = 9;
                }

                else if (0.95 < ort_mesafe && ort_mesafe <= 1.7)
                {
                    map.Zoom = 8;
                }

                else if (1.7 < ort_mesafe)
                {
                    map.Zoom = 7;
                }


            }
            else
            {
                markers.Clear();
                ortMesafe.Text = "Konum YOK..";
                lblDurum.Text = "Konumlar Alınamıyor..";
            }

            if (timerMapFY.Enabled == false || timerMapROK.Enabled == false)
            {
                timerMapOrtala.Stop();
            }
        }

        private void timerHakemYer_Tick(object sender, EventArgs e)
        {
            sayac++;
            timerHakemYer.Enabled = true;

            if (HakemYerSistemDegis == true)
            {

                hakemIstasyonu.teamID = Convert.ToByte(teamIDtextBox.Text);
                hakemIstasyonu.paketSayaci = roketAnaHaberlesme.paketSayaci;
                hakemIstasyonu.roket_irtifa = roketAnaHaberlesme.irtifa;
                hakemIstasyonu.gy_gps_irtifa = gorevYuku.gps_irtifa;
                hakemIstasyonu.gy_gps_enlem = gorevYuku.gps_enlem;
                hakemIstasyonu.gy_gps_boylam = gorevYuku.gps_boylam;

                hakemIstasyonu.jiroskop_x = roketAnaHaberlesme.jiroskop_x;
                hakemIstasyonu.jiroskop_y = roketAnaHaberlesme.jiroskop_y;
                hakemIstasyonu.jiroskop_z = roketAnaHaberlesme.jiroskop_z;
                hakemIstasyonu.ivme_x = roketAnaHaberlesme.ivme_x;
                hakemIstasyonu.ivme_y = roketAnaHaberlesme.ivme_y;
                hakemIstasyonu.ivme_z = roketAnaHaberlesme.ivme_z;
                hakemIstasyonu.aci = roketAnaHaberlesme.aci;
                hakemIstasyonu.durum = roketAnaHaberlesme.durum;

                hakemIstasyonu.roket_gps_boylam = roketAnaHaberlesme.gps_boylam;
                hakemIstasyonu.roket_gps_enlem = roketAnaHaberlesme.gps_enlem;
                hakemIstasyonu.roket_gps_irtifa = roketAnaHaberlesme.gps_irtifa;

                hakemIstasyonu.durum = roketAnaHaberlesme.durum;
                hakemIstasyonu.crc = roketAnaHaberlesme.crc;


            }
            //else
            //{

            //    hakemIstasyonu.teamID = Convert.ToByte(teamIDtextBox.Text);
            //    hakemIstasyonu.paketSayaci = roketYedekHaberlesme.paketSayaci;
            //    hakemIstasyonu.roket_irtifa = roketYedekHaberlesme.irtifa;
            //    hakemIstasyonu.gy_gps_irtifa = gorevYuku.gps_irtifa;
            //    hakemIstasyonu.gy_gps_enlem = gorevYuku.gps_enlem;
            //    hakemIstasyonu.gy_gps_boylam = gorevYuku.gps_boylam;

            //    hakemIstasyonu.jiroskop_x = roketYedekHaberlesme.jiroskop_x;
            //    hakemIstasyonu.jiroskop_y = roketYedekHaberlesme.jiroskop_y;
            //    hakemIstasyonu.jiroskop_z = roketYedekHaberlesme.jiroskop_z;
            //    hakemIstasyonu.ivme_x = roketYedekHaberlesme.ivme_x;
            //    hakemIstasyonu.ivme_y = roketYedekHaberlesme.ivme_y;
            //    hakemIstasyonu.ivme_z = roketYedekHaberlesme.ivme_z;
            //    hakemIstasyonu.aci = roketYedekHaberlesme.aci;
            //    hakemIstasyonu.durum = roketYedekHaberlesme.durum;

            //    hakemIstasyonu.roket_gps_boylam = roketYedekHaberlesme.gps_boylam;
            //    hakemIstasyonu.roket_gps_enlem = roketYedekHaberlesme.gps_enlem;
            //    hakemIstasyonu.roket_gps_irtifa = roketYedekHaberlesme.gps_irtifa;

            //    hakemIstasyonu.durum = roketYedekHaberlesme.durum;
            //    hakemIstasyonu.crc = roketYedekHaberlesme.crc;
            //}



            //Haberleşme Bilgisayarı
            if (hyiSerial.IsOpen)
            {
                //sayac++;
                gidenpaket[0] = sabit1;
                gidenpaket[1] = sabit1;
                gidenpaket[2] = hb_sabit2;
                gidenpaket[3] = hb_sabit3;
                gidenpaket[4] = hakemIstasyonu.teamID; // belirlenecek // TEAM_ID
                bytesHakemIstasyonu.paketSayaci = sayac;
                gidenpaket[5] = bytesHakemIstasyonu.paketSayaci;
                bytesHakemIstasyonu.roket_irtifa = BitConverter.GetBytes((float)hakemIstasyonu.roket_irtifa);
                gidenpaket[6] = bytesHakemIstasyonu.roket_irtifa[0];
                gidenpaket[7] = bytesHakemIstasyonu.roket_irtifa[1];
                gidenpaket[8] = bytesHakemIstasyonu.roket_irtifa[2];
                gidenpaket[9] = bytesHakemIstasyonu.roket_irtifa[3];
                bytesHakemIstasyonu.roket_gps_irtifa = BitConverter.GetBytes((float)hakemIstasyonu.roket_gps_irtifa);
                gidenpaket[10] = bytesHakemIstasyonu.roket_gps_irtifa[0];
                gidenpaket[11] = bytesHakemIstasyonu.roket_gps_irtifa[1];
                gidenpaket[12] = bytesHakemIstasyonu.roket_gps_irtifa[2];
                gidenpaket[13] = bytesHakemIstasyonu.roket_gps_irtifa[3];
                bytesHakemIstasyonu.roket_gps_enlem = BitConverter.GetBytes((float)hakemIstasyonu.roket_gps_enlem);
                gidenpaket[14] = bytesHakemIstasyonu.roket_gps_enlem[0];
                gidenpaket[15] = bytesHakemIstasyonu.roket_gps_enlem[1];
                gidenpaket[16] = bytesHakemIstasyonu.roket_gps_enlem[2];
                gidenpaket[17] = bytesHakemIstasyonu.roket_gps_enlem[3];
                bytesHakemIstasyonu.roket_gps_boylam = BitConverter.GetBytes((float)hakemIstasyonu.roket_gps_boylam);
                gidenpaket[18] = bytesHakemIstasyonu.roket_gps_boylam[0];
                gidenpaket[19] = bytesHakemIstasyonu.roket_gps_boylam[1];
                gidenpaket[20] = bytesHakemIstasyonu.roket_gps_boylam[2];
                gidenpaket[21] = bytesHakemIstasyonu.roket_gps_boylam[3];
                bytesHakemIstasyonu.gy_gps_irtifa = BitConverter.GetBytes((float)hakemIstasyonu.gy_gps_irtifa);
                gidenpaket[22] = bytesHakemIstasyonu.gy_gps_irtifa[0];
                gidenpaket[23] = bytesHakemIstasyonu.gy_gps_irtifa[1];
                gidenpaket[24] = bytesHakemIstasyonu.gy_gps_irtifa[2];
                gidenpaket[25] = bytesHakemIstasyonu.gy_gps_irtifa[3];
                bytesHakemIstasyonu.gy_gps_enlem = BitConverter.GetBytes((float)hakemIstasyonu.gy_gps_enlem);
                gidenpaket[26] = bytesHakemIstasyonu.gy_gps_enlem[0];
                gidenpaket[27] = bytesHakemIstasyonu.gy_gps_enlem[1];
                gidenpaket[28] = bytesHakemIstasyonu.gy_gps_enlem[2];
                gidenpaket[29] = bytesHakemIstasyonu.gy_gps_enlem[3];
                bytesHakemIstasyonu.gy_gps_boylam = BitConverter.GetBytes((float)hakemIstasyonu.gy_gps_boylam);
                gidenpaket[30] = bytesHakemIstasyonu.gy_gps_boylam[0];
                gidenpaket[31] = bytesHakemIstasyonu.gy_gps_boylam[1];
                gidenpaket[32] = bytesHakemIstasyonu.gy_gps_boylam[2];
                gidenpaket[33] = bytesHakemIstasyonu.gy_gps_boylam[3];
                bytesHakemIstasyonu.kademe_gps_irtifa = BitConverter.GetBytes((float)hakemIstasyonu.kademe_gps_irtifa);
                gidenpaket[34] = bytesHakemIstasyonu.kademe_gps_irtifa[0];
                gidenpaket[35] = bytesHakemIstasyonu.kademe_gps_irtifa[1];
                gidenpaket[36] = bytesHakemIstasyonu.kademe_gps_irtifa[2];
                gidenpaket[37] = bytesHakemIstasyonu.kademe_gps_irtifa[3];
                bytesHakemIstasyonu.kademe_gps_enlem = BitConverter.GetBytes((float)hakemIstasyonu.kademe_gps_enlem);
                gidenpaket[38] = bytesHakemIstasyonu.kademe_gps_enlem[0];
                gidenpaket[39] = bytesHakemIstasyonu.kademe_gps_enlem[1];
                gidenpaket[40] = bytesHakemIstasyonu.kademe_gps_enlem[2];
                gidenpaket[41] = bytesHakemIstasyonu.kademe_gps_enlem[3];
                bytesHakemIstasyonu.kademe_gps_boylam = BitConverter.GetBytes((float)hakemIstasyonu.kademe_gps_boylam);
                gidenpaket[42] = bytesHakemIstasyonu.kademe_gps_boylam[0];
                gidenpaket[43] = bytesHakemIstasyonu.kademe_gps_boylam[1];
                gidenpaket[44] = bytesHakemIstasyonu.kademe_gps_boylam[2];
                gidenpaket[45] = bytesHakemIstasyonu.kademe_gps_boylam[3];
                bytesHakemIstasyonu.jiroskop_x = BitConverter.GetBytes((float)hakemIstasyonu.jiroskop_x);
                gidenpaket[46] = bytesHakemIstasyonu.jiroskop_x[0];
                gidenpaket[47] = bytesHakemIstasyonu.jiroskop_x[1];
                gidenpaket[48] = bytesHakemIstasyonu.jiroskop_x[2];
                gidenpaket[49] = bytesHakemIstasyonu.jiroskop_x[3];
                bytesHakemIstasyonu.jiroskop_y = BitConverter.GetBytes((float)hakemIstasyonu.jiroskop_y);
                gidenpaket[50] = bytesHakemIstasyonu.jiroskop_y[0];
                gidenpaket[51] = bytesHakemIstasyonu.jiroskop_y[1];
                gidenpaket[52] = bytesHakemIstasyonu.jiroskop_y[2];
                gidenpaket[53] = bytesHakemIstasyonu.jiroskop_y[3];
                bytesHakemIstasyonu.jiroskop_z = BitConverter.GetBytes((float)hakemIstasyonu.jiroskop_z);
                gidenpaket[54] = bytesHakemIstasyonu.jiroskop_z[0];
                gidenpaket[55] = bytesHakemIstasyonu.jiroskop_z[1];
                gidenpaket[56] = bytesHakemIstasyonu.jiroskop_z[2];
                gidenpaket[57] = bytesHakemIstasyonu.jiroskop_z[3];
                bytesHakemIstasyonu.ivme_x = BitConverter.GetBytes((float)hakemIstasyonu.ivme_x);
                gidenpaket[58] = bytesHakemIstasyonu.ivme_x[0];
                gidenpaket[59] = bytesHakemIstasyonu.ivme_x[1];
                gidenpaket[60] = bytesHakemIstasyonu.ivme_x[2];
                gidenpaket[61] = bytesHakemIstasyonu.ivme_x[3];
                bytesHakemIstasyonu.ivme_y = BitConverter.GetBytes((float)hakemIstasyonu.ivme_y);
                gidenpaket[62] = bytesHakemIstasyonu.ivme_y[0];
                gidenpaket[63] = bytesHakemIstasyonu.ivme_y[1];
                gidenpaket[64] = bytesHakemIstasyonu.ivme_y[2];
                gidenpaket[65] = bytesHakemIstasyonu.ivme_y[3];
                bytesHakemIstasyonu.ivme_z = BitConverter.GetBytes((float)hakemIstasyonu.ivme_z);
                gidenpaket[66] = bytesHakemIstasyonu.ivme_z[0];
                gidenpaket[67] = bytesHakemIstasyonu.ivme_z[1];
                gidenpaket[68] = bytesHakemIstasyonu.ivme_z[2];
                gidenpaket[69] = bytesHakemIstasyonu.ivme_z[3];
                bytesHakemIstasyonu.aci = BitConverter.GetBytes((float)hakemIstasyonu.aci);
                gidenpaket[70] = bytesHakemIstasyonu.aci[0];
                gidenpaket[71] = bytesHakemIstasyonu.aci[1];
                gidenpaket[72] = bytesHakemIstasyonu.aci[2];
                gidenpaket[73] = bytesHakemIstasyonu.aci[3];
                bytesHakemIstasyonu.durum = (byte)hakemIstasyonu.durum;
                bytesHakemIstasyonu.crc = (byte)hakemIstasyonu.crc;
                if (bytesHakemIstasyonu.durum == (byte)0 || bytesHakemIstasyonu.durum == (byte)1)
                {
                    gidenpaket[74] = (byte)1;
                }
                else if (bytesHakemIstasyonu.durum == (byte)2 || bytesHakemIstasyonu.durum == (byte)3)
                {
                    gidenpaket[74] = (byte)2;
                }
                else if (bytesHakemIstasyonu.durum == (byte)4 || bytesHakemIstasyonu.durum == (byte)5 || bytesHakemIstasyonu.durum == (byte)6)
                {
                    gidenpaket[74] = (byte)4;
                }
                gidenpaket[75] = check_sum_hesapla();
                gidenpaket[76] = hb_sonSabit1;
                gidenpaket[77] = hb_sonSabit2;

                if (hyiVeriGonder_Button.Text == "Durdur")
                {
                    hyiSerial.Write(gidenpaket, 0, gidenpaket.Length);
                }

            }
        }

        string gorevYukuGelenVeri, anaRoketGelenVeri, yedekRoketGelenVeri;

        // Yer İstasyonuna Yazdırılan Roket Verisi
        void thread_ROKprintedData()
        {
            try
            {
                // Ana Bilgisayar (ROK1)
                this.ASyukseklikChart.Series[0].Points.AddXY(zamanRoket.Text, float.Parse(ROK1irtifaLblTxt.Text));//!!
                this.ASivmeZChart.Series[0].Points.AddXY(zamanRoket.Text, float.Parse(ROK1ivmeZLblTxt.Text));//!!

                ASgrafLblYuksek.Text = ROK1irtifaLblTxt.Text + " (m)";
                ASgrafLblivmeZ.Text = ROK1ivmeZLblTxt.Text + " (g)"; //ivme z

                try
                {
                    if ((AS_check_sum_hesapla() == roketAnaHaberlesme.crc))
                    {
                        ROK1paketNoLblTxt.Text = Convert.ToString(roket_sayac);
                        ROK1irtifaLblTxt.Text = String.Format("{0:0.000}", roketAnaHaberlesme.irtifa);
                        ROK1gpsAltLblTxt.Text = Convert.ToString(roketAnaHaberlesme.gps_irtifa);
                        ROK1gpsLatLblTxt.Text = Convert.ToString(roketAnaHaberlesme.gps_enlem);
                        ROK1gpsLngLblTxt.Text = Convert.ToString(roketAnaHaberlesme.gps_boylam);
                        ROK1jiroXLblTxt.Text = String.Format("{0:0.000}", roketAnaHaberlesme.jiroskop_x);
                        ROK1jiroYLblTxt.Text = String.Format("{0:0.000}", roketAnaHaberlesme.jiroskop_y);
                        ROK1jiroZLblTxt.Text = String.Format("{0:0.000}", roketAnaHaberlesme.jiroskop_z);
                        ROK1ivmeXLblTxt.Text = String.Format("{0:0.000}", roketAnaHaberlesme.ivme_x);
                        ROK1ivmeYLblTxt.Text = String.Format("{0:0.000}", roketAnaHaberlesme.ivme_y);
                        ROK1ivmeZLblTxt.Text = String.Format("{0:0.000}", roketAnaHaberlesme.ivme_z);
                        ROK1aciLblTxt.Text = String.Format("{0:0.000}", roketAnaHaberlesme.aci);
                        ROK1durumLblTxt.Text = Convert.ToString(roketAnaHaberlesme.durum);

                        if (ROK1durumLblTxt.Text == "0")
                        {
                            durumAnaInfoLbl.Text = "Bekleme ";
                        }
                        else if (ROK1durumLblTxt.Text == "1")
                        {
                            durumAnaInfoLbl.Text = "Yükselme";
                        }
                        else if (ROK1durumLblTxt.Text == "2")
                        {
                            durumAnaInfoLbl.Text = "Apogee";
                        }
                        else if (ROK1durumLblTxt.Text == "3")
                        {
                            durumAnaInfoLbl.Text = "İniş 1";
                        }
                        else if (ROK1durumLblTxt.Text == "4")
                        {
                            durumAnaInfoLbl.Text = "Ayrılma";
                        }
                        else if (ROK1durumLblTxt.Text == "5")
                        {
                            durumAnaInfoLbl.Text = "İniş 2";
                        }
                        else if (ROK1durumLblTxt.Text == "6")
                        {
                            durumAnaInfoLbl.Text = "Kurtarma";
                        }
                        else
                        {
                            durumAnaInfoLbl.Text = "Durum YOK";
                        }

                    }

                }
                catch (Exception ex2)
                {
                    error_richtextbox.Text = "ROKET veri yazdirma" + ex2.Message + "\n";
                }
            }

            catch (Exception ex3)
            {
                error_richtextbox.Text = "ROKET chart yazdirma" + ex3.Message + "\n";
            }
        }

        
        // Yer İstasyonuna Yazdırılan Görev Yükü Verisi
        void thread_FYprintedData()
        {
            //gy_sayac++;
            zamanRoket.Text = DateTime.Now.ToString("HH:mm:ss");
            try
            {
                // Görev Yükü
                this.FYyukseklikChart.Series[0].Points.AddXY(zamanRoket.Text, float.Parse(FYirtifaLblTxt.Text));//!!

                FYgrafLblYuksek.Text = FYirtifaLblTxt.Text + " (m)";

                try
                {
                    if (FY_check_sum_hesapla() == gorevYuku.crc)
                    {
                        Console.WriteLine("Icindeim..");
                        //
                        FYpaketNoLblTxt.Text = Convert.ToString(gy_sayac);
                        FYgpsAltLblTxt.Text = Convert.ToString(gorevYuku.gps_irtifa);
                        FYgpsLatLblTxt.Text = Convert.ToString(gorevYuku.gps_enlem);
                        FYgpsLngLblTxt.Text = Convert.ToString(gorevYuku.gps_boylam);
                        FYirtifaLblTxt.Text = String.Format("{0:0.000}", gorevYuku.irtifa);
                        FYnemLblTxt.Text = String.Format("{0:0.000}", gorevYuku.nem);
                    }

                }
                catch (Exception ex2)
                {
                    error_richtextbox.Text = "FY veri yazdirma: " + ex2.Message + "\n";
                }
            }

            catch (Exception ex3)
            {
                error_richtextbox.Text = "FY chart yazdirma: " + ex3.Message + "\n";
            }

        }


        //private void timerLastDatas_Tick(object sender, EventArgs e)
        //{
        //    roketAnaLast.gps_irtifa = System.BitConverter.ToSingle(yedek_bytes, 4);
        //    roketAnaLast.gps_enlem = System.BitConverter.ToSingle(yedek_bytes, 8);
        //    roketAnaLast.gps_boylam = System.BitConverter.ToSingle(yedek_bytes, 12);
        //    roketAnaLast.jiroskop_x = System.BitConverter.ToSingle(yedek_bytes, 16);
        //    roketAnaLast.jiroskop_y = System.BitConverter.ToSingle(yedek_bytes, 20);
        //    roketAnaLast.jiroskop_z = System.BitConverter.ToSingle(yedek_bytes, 24);
        //    roketAnaLast.ivme_x = System.BitConverter.ToSingle(yedek_bytes, 28);
        //    roketAnaLast.ivme_y = System.BitConverter.ToSingle(yedek_bytes, 32);
        //    roketAnaLast.ivme_z = System.BitConverter.ToSingle(yedek_bytes, 36);
        //    roketAnaLast.aci = System.BitConverter.ToSingle(yedek_bytes, 40);
        //    roketAnaLast.irtifa = System.BitConverter.ToSingle(yedek_bytes, 44);


        //}

        private void anaSistemDegisBtn_Click(object sender, EventArgs e)
        {
            HakemYerSistemDegis = !HakemYerSistemDegis;
            if (HakemYerSistemDegis)
                anaSistemDegisBtn.Text = "Ana Sistem Gonderiliyor";
            else
            {
                anaSistemDegisBtn.Text = "Yedek Sistem Gonderiliyor";
            }
        }


        private void btnFYGrafikTemizle_Click(object sender, EventArgs e)
        {
            foreach (var series in FYyukseklikChart.Series) series.Points.Clear();
        }

        private void btnROKGrafikTemizle_Click(object sender, EventArgs e)
        {
            foreach (var series in ASyukseklikChart.Series) series.Points.Clear();
            foreach (var series in ASivmeZChart.Series) series.Points.Clear();
        }

        string fileAnaRoket, fileGorevYuku, fileHatalar;
        StreamWriter anaRoketfile, gyFile, hatalarFile;

        private void anaRoketTxtKayit()
        {
            try
            {
                if (serialPort2.IsOpen)
                {
                    try
                    {

                        if (anaRoketGelenVeri != null)
                        {
                            Console.WriteLine("ana roket GELEN VERI: " + anaRoketGelenVeri);
                            if (anaRoketGelenVeri.Contains("ana"))
                            {
                                Console.WriteLine("GİRDİ..");
                                anaRoketfile.WriteLine(anaRoketGelenVeri);
                            }
                        }
                        else
                        {
                            savedTxtDurumLbl.Text = "Ana Sistem (null) Text Yazdiramaz.. ";
                            savedTxtDurumLbl.ForeColor = Color.DarkRed;
                        }

                    }
                    catch (Exception anaTxtErr)
                    {
                        savedTxtDurumLbl.Text = "Ana Sistem Text Yazdirma: " + anaTxtErr.Message;
                    }

                }
            }
            catch (Exception Ex)
            {
                error_richtextbox.Text = "Ana Roket Dosya: " + Ex.ToString() + "\n";
            }

        }

       
        private void gorevYukuTxtKayit()
        {
            if (serialPort1.IsOpen)
            {
                try
                {
                    if (gorevYukuGelenVeri != null)
                    {
                        if (gorevYukuGelenVeri.Contains("gy"))
                        {
                            gyFile.WriteLine(gorevYukuGelenVeri);
                        }
                    }
                    else
                    {
                        savedTxtDurumLbl.Text = "Görev Yükü (null) Text Yazdiramaz.. ";
                        savedTxtDurumLbl.ForeColor = Color.DarkRed;
                    }

                }
                catch (Exception gyTxtErr)
                {
                    savedTxtDurumLbl.Text = "Görev Yükü Text Yazdirma: " + gyTxtErr.Message;
                }
            }
        }


    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using UnityEngine.Networking;
using System;
using System.IO;

public class Blue_Man : MonoBehaviour
{
    private enum Bluetooth_States
    {
        None,
        Scan,
        Connect,
        Subscribe,
        Unsubscribe,
        Disconnect,
        Communication,
    }

    private static class Sinirlar
    {
        public static double Sol_Diz_Gercek_Min = 5.0;      // Veya 10 derece
        public static double Sol_Diz_Gercek_Max = 120.0;    // Veya 150 derece
        public static double Sol_Diz_Olcum_Min  = 0.0;
        public static double Sol_Diz_Olcum_Max  = 0.0;

        public static double Sag_Diz_Gercek_Min = 5.0;
        public static double Sag_Diz_Gercek_Max = 120.0;
        public static double Sag_Diz_Olcum_Min  = 0.0;
        public static double Sag_Diz_Olcum_Max  = 0.0;


        public static bool Ilk_Olcum_Alindi     = false;
    }

    private static class Eklem_Degerleri
    {
        public static double Sol_Diz = 0.0;
        public static double Sag_Diz = 0.0;
    }

    private static class Bluetooth_Bilgileri
    {
        public static Bluetooth_States  State           = Bluetooth_States.None;
        public static float             State_Timeout   = 0f;
        public static string            Device_Address  = null;
        public static Text              Status_Label    = null;
        public static bool              Connected       = false;
        public static int               Receive_Count   = 0;

    }

    public int      Secili_Kullanici_ID         = 0;
    public string   Veri_Merkezi_Adresi         = "127.0.0.1";
    public int      Veri_Merkezi_Port           = 8888;
    public string   Bluetooth_DeviceName        = "Bluno";
    public string   Bluetooth_ServiceUUID       = "dfb0";
    public string   Bluetooth_Characteristic    = "dfb1";
    public char     Paket_Baslangici            = '!';
    public char     Paket_Sonu                  = '#';
    public int      Kalibrasyon_Mesaj_Sayisi    = 10;

    private Animator Animator;
    private string Kalan_Veri;
    private string Veri_Tamponu;

    private string Kayit_Dosyasi_Adi = Directory.GetCurrentDirectory() + "\\Assets\\Others\\Blue_Man_" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss") + ".motion";
    private StreamWriter Kayit_Dosyasi;

    // Start is called before the first frame update
    void Start()
    {
        Animator = GetComponent<Animator>();


        Bluetooth_Hazirla();

        Kayit_Dosyasi = File.CreateText(Kayit_Dosyasi_Adi);
        Kayit_Dosyasi.WriteLine("Tarih,Sag_Aci,Sol_Aci");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Bluetooth_Baglan();
    }

    void OnAnimatorIK()
    {
        Animator.SetBoneLocalRotation(HumanBodyBones.LeftUpperLeg, Quaternion.Euler(new Vector3(0, 0, 0)));
        Animator.SetBoneLocalRotation(HumanBodyBones.RightUpperLeg, Quaternion.Euler(new Vector3(0, 0, 0)));


        Animator.SetBoneLocalRotation(HumanBodyBones.LeftLowerLeg, Quaternion.Euler(new Vector3((float)Eklem_Degerleri.Sol_Diz, 0, 0)));
        Animator.SetBoneLocalRotation(HumanBodyBones.RightLowerLeg, Quaternion.Euler(new Vector3((float)Eklem_Degerleri.Sag_Diz, 0, 0)));
    }

    void LateUpdate()
    {
        transform.GetChild(3).gameObject.GetComponent<TextMesh>().text = "";
        transform.GetChild(3).gameObject.GetComponent<TextMesh>().text += "R: " + Animator.GetBoneTransform(HumanBodyBones.RightLowerLeg).localEulerAngles[0].ToString("000");
        transform.GetChild(3).gameObject.GetComponent<TextMesh>().text += "  ";
        transform.GetChild(3).gameObject.GetComponent<TextMesh>().text += "L: " + Animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg).localEulerAngles[0].ToString("000");

        Kayit_Dosyasi.WriteLine("{0};{1};{2}", DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_ffffff"), Animator.GetBoneTransform(HumanBodyBones.RightLowerLeg).localEulerAngles[0].ToString("000"), Animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg).localEulerAngles[0].ToString("000"));

    }

    void OnApplicationQuit()
    {
        Kayit_Dosyasi.Close();
    }


    void Bluetooth_Hazirla()
    {
        Status_Update("Bluetooth Başlatılıyor...");

        BluetoothLEHardwareInterface.Initialize(true, false, () => {

            SetState(Bluetooth_States.Scan, 0.1f);
            Status_Update("Bluetooth başlatıldı.");

        }, (error) => {
            BluetoothLEHardwareInterface.Log("Error: " + error);
        });
    }

    void Bluetooth_Baglan()
    {
        if (Bluetooth_Bilgileri.State_Timeout > 0f)
        {
            Bluetooth_Bilgileri.State_Timeout -= Time.deltaTime;
            if (Bluetooth_Bilgileri.State_Timeout <= 0f)
            {
                Bluetooth_Bilgileri.State_Timeout = 0f;

                switch (Bluetooth_Bilgileri.State)
                {
                    case Bluetooth_States.None:
                        break;

                    case Bluetooth_States.Scan:

                        Status_Update(Bluetooth_DeviceName + " aranıyor...");

                        BluetoothLEHardwareInterface.ScanForPeripheralsWithServices(null, (address, name) => {
                            // we only want to look at devices that have the name we are looking for
                            // this is the best way to filter out devices
                            if (name.Contains(Bluetooth_DeviceName))
                            {
                                // it is always a good idea to stop scanning while you connect to a device
                                // and get things set up
                                BluetoothLEHardwareInterface.StopScan();
                                Status_Update(Bluetooth_DeviceName + " bulundu...");

                                // add it to the list and set to connect to it
                                Bluetooth_Bilgileri.Device_Address = address;

                                SetState(Bluetooth_States.Connect, 0.5f);

                            }

                        }, null, false, false);
                        break;

                    case Bluetooth_States.Connect:
                        // set these flags

                        Status_Update(Bluetooth_DeviceName + " cihazına bağlanılıyor...");

                        // note that the first parameter is the address, not the name. I have not fixed this because
                        // of backwards compatiblity.
                        // also note that I am note using the first 2 callbacks. If you are not looking for specific characteristics you can use one of
                        // the first 2, but keep in mind that the device will enumerate everything and so you will want to have a timeout
                        // large enough that it will be finished enumerating before you try to subscribe or do any other operations.
                        BluetoothLEHardwareInterface.ConnectToPeripheral(Bluetooth_Bilgileri.Device_Address, null, null, (address, serviceUUID, characteristicUUID) => {

                            if (UUIDIsEqual(serviceUUID, Bluetooth_ServiceUUID))
                            {
                                // if we have found the characteristic that we are waiting for
                                // set the state. make sure there is enough timeout that if the
                                // device is still enumerating other characteristics it finishes
                                // before we try to subscribe
                                if (UUIDIsEqual(characteristicUUID, Bluetooth_Characteristic))
                                {
                                    Bluetooth_Bilgileri.Connected = true;
                                    SetState(Bluetooth_States.Subscribe, 2f);

                                    Status_Update(Bluetooth_DeviceName + " cihazına bağlanıldı.");
                                }
                            }
                        }, (disconnectedAddress) => {
                            BluetoothLEHardwareInterface.Log("Device disconnected: " + disconnectedAddress);
                            Status_Update(Bluetooth_DeviceName + " cihazı ile bağlantı kesildi!");
                        });
                        break;

                    case Bluetooth_States.Subscribe:
                        Status_Update("Veri alımına kaydolunuyor...");

                        BluetoothLEHardwareInterface.SubscribeCharacteristicWithDeviceAddress(Bluetooth_Bilgileri.Device_Address, Bluetooth_ServiceUUID, Bluetooth_Characteristic, null, (address, characteristicUUID, bytes) => {
                            Bluetooth_Bilgileri.Receive_Count++;
                            BluetoothLEHardwareInterface.Log("Alinan: " + Encoding.UTF8.GetString(bytes));
                            Status_Update("Alınan veri: " + Encoding.UTF8.GetString(bytes));

                            Bluetooth_Verilerini_Isle(bytes);
                        });

                        // set to the none state and the user can start sending and receiving data
                        SetState(Bluetooth_States.None, 0);
                        Status_Update("Veri bekleniyor...");

                        break;

                    case Bluetooth_States.Unsubscribe:
                        BluetoothLEHardwareInterface.UnSubscribeCharacteristic(Bluetooth_Bilgileri.Device_Address, Bluetooth_ServiceUUID, Bluetooth_Characteristic, null);
                        SetState(Bluetooth_States.Disconnect, 4f);
                        break;

                    case Bluetooth_States.Disconnect:
                        if (Bluetooth_Bilgileri.Connected)
                        {
                            BluetoothLEHardwareInterface.DisconnectPeripheral(Bluetooth_Bilgileri.Device_Address, (address) => {
                                BluetoothLEHardwareInterface.DeInitialize(() => {

                                    Bluetooth_Bilgileri.Connected = false;
                                    SetState(Bluetooth_States.None, 0);
                                });
                            });
                        }
                        else
                        {
                            BluetoothLEHardwareInterface.DeInitialize(() => {

                                SetState(Bluetooth_States.None, 0);
                            });
                        }
                        break;
                }
            }
        }
    }

    void Bluetooth_Verilerini_Isle(byte[] Alinan_Veri)
    {
        string Ham_Veri;
        double Sol_Diz_Mesaj_Degeri;
        double Sag_Diz_Mesaj_Degeri;
        string Sol_Diz;
        string Sag_Diz;

        Ham_Veri        = Encoding.UTF8.GetString(Alinan_Veri);
        Veri_Tamponu   += Ham_Veri;

        Kalan_Veri                  = Veri_Tamponu.Split(Paket_Sonu).Last();
        Regex Veri_Arayicisi        = new Regex("(?<=!)(.*?)(?=#)");
        MatchCollection Bulunanlar  = Veri_Arayicisi.Matches(Veri_Tamponu);
        Match Son_Bulunan           = Bulunanlar.OfType<Match>().LastOrDefault();
        Veri_Tamponu                = Kalan_Veri;

        if (!Son_Bulunan.Success)
        {
            BluetoothLEHardwareInterface.Log("Bulamadim");
            return;
        }
        BluetoothLEHardwareInterface.Log("Bulunan: " + Son_Bulunan);

        Sol_Diz = Son_Bulunan.Value.Split(';')[0];
        BluetoothLEHardwareInterface.Log("Sol: " + Sol_Diz);
        Sag_Diz = Son_Bulunan.Value.Split(';')[1];
        BluetoothLEHardwareInterface.Log("Sag: " + Sag_Diz);

        Sol_Diz_Mesaj_Degeri = double.Parse(Sol_Diz);
        Sag_Diz_Mesaj_Degeri = double.Parse(Sag_Diz);

        if (Sinirlar.Ilk_Olcum_Alindi == false)
        {
            Sinirlar.Sol_Diz_Olcum_Min = Sol_Diz_Mesaj_Degeri;
            Sinirlar.Sol_Diz_Olcum_Max = Sol_Diz_Mesaj_Degeri;

            Sinirlar.Sag_Diz_Olcum_Min = Sag_Diz_Mesaj_Degeri;
            Sinirlar.Sag_Diz_Olcum_Max = Sag_Diz_Mesaj_Degeri;
        }

        if (Sol_Diz_Mesaj_Degeri < Sinirlar.Sol_Diz_Olcum_Min)
        {
            Sinirlar.Sol_Diz_Olcum_Min = Sol_Diz_Mesaj_Degeri;
        }
        else if (Sol_Diz_Mesaj_Degeri > Sinirlar.Sol_Diz_Olcum_Max)
        {
            Sinirlar.Sol_Diz_Olcum_Max = Sol_Diz_Mesaj_Degeri;
        }

        if (Sag_Diz_Mesaj_Degeri < Sinirlar.Sag_Diz_Olcum_Min)
        {
            Sinirlar.Sag_Diz_Olcum_Min = Sag_Diz_Mesaj_Degeri;
        }
        else if (Sag_Diz_Mesaj_Degeri > Sinirlar.Sag_Diz_Olcum_Max)
        {
            Sinirlar.Sag_Diz_Olcum_Max = Sag_Diz_Mesaj_Degeri;
        }

        if (Bluetooth_Bilgileri.Receive_Count <= Kalibrasyon_Mesaj_Sayisi)
        {
            Status_Update("Kalibre ediliyor...");
            return;
        }

        Eklem_Degerleri.Sol_Diz = Gercek_Degere_Don(Sol_Diz_Mesaj_Degeri, Sinirlar.Sol_Diz_Olcum_Min, Sinirlar.Sol_Diz_Olcum_Max, Sinirlar.Sol_Diz_Gercek_Min, Sinirlar.Sol_Diz_Gercek_Max);
        Eklem_Degerleri.Sag_Diz = Gercek_Degere_Don(Sag_Diz_Mesaj_Degeri, Sinirlar.Sag_Diz_Olcum_Min, Sinirlar.Sag_Diz_Olcum_Max, Sinirlar.Sag_Diz_Gercek_Min, Sinirlar.Sag_Diz_Gercek_Max);

        StartCoroutine(Karakter_Verilerini_Webe_Kaydet());

        Status_Update("SolDiz A: " + Sol_Diz_Mesaj_Degeri.ToString(".00") + " G: " + Eklem_Degerleri.Sol_Diz.ToString(".00") + " | SagDiz A: " + Sag_Diz_Mesaj_Degeri.ToString(".00") + " G: " + Eklem_Degerleri.Sag_Diz.ToString(".00"));
    }

    IEnumerator Karakter_Verilerini_Webe_Kaydet()
    {
        string Kullanici_Veri_URL = "http://" + Veri_Merkezi_Adresi + ":" + Veri_Merkezi_Port + "/Karakter_Bilgisi_Kaydet?Kullanici_ID=" + Secili_Kullanici_ID.ToString() + "&Sol_Diz=" + Eklem_Degerleri.Sol_Diz.ToString() + "&Sag_Diz=" + Eklem_Degerleri.Sag_Diz.ToString();

        UnityWebRequest Web_Talebi = UnityWebRequest.Get(Kullanici_Veri_URL);
        yield return Web_Talebi.SendWebRequest();


        if (Web_Talebi.isNetworkError)
        {
            Debug.Log("Sunucu baglanti  hatasi: " + Web_Talebi.error);
        }
        else
        {
        }
    }


    private double Limitle(double Deger, double Min, double Max)
    {
        if (Deger < Min)
        {
            Deger = Min;
        }
        else if(Deger > Max)
        {
            Deger = Max;
        }

        return Deger;
    }
    private double Gercek_Degere_Don(double Mesaj_Degeri, double Mesaj_Alt_Degeri, double Mesaj_Ust_Degeri, double Gercek_Alt_Deger, double Gercek_Ust_Deger)
    {
        return (Gercek_Alt_Deger + (((Mesaj_Degeri - Mesaj_Alt_Degeri) / (Mesaj_Ust_Degeri - Mesaj_Alt_Degeri)) * (Gercek_Ust_Deger - Gercek_Alt_Deger)));
    }

    void Status_Update(string message)
    {
        if (Bluetooth_Bilgileri.Status_Label != null)
        {
            Bluetooth_Bilgileri.Status_Label.text = message;
        }
    }

    void SetState(Bluetooth_States newState, float timeout)
    {
        Bluetooth_Bilgileri.State         = newState;
        Bluetooth_Bilgileri.State_Timeout = timeout;
    }
    
    bool UUIDIsEqual(string uuid1, string uuid2)
    {
        if (uuid1.Length == 4)
            uuid1 = "0000" + uuid1 + "-0000-1000-8000-00805F9B34FB"; // Convert Full UUID
        if (uuid2.Length == 4)
            uuid2 = "0000" + uuid2 + "-0000-1000-8000-00805F9B34FB"; // Convert Full UUID

        return (uuid1.ToUpper().Equals(uuid2.ToUpper()));
    }
}

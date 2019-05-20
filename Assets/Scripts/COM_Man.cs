using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.IO.Ports;
using System.Text.RegularExpressions;
using System.Linq;
using System.IO;

public class COM_Man : MonoBehaviour
{
    public static class Kalibrasyon
    {
        public static double Sol_Diz_Gercek_Min     = 5.0; 
        public static double Sol_Diz_Gercek_Max     = 90;  
        public static double Sol_Diz_Olcum_Min      = 0.0;
        public static double Sol_Diz_Olcum_Max      = 0.0;

        public static double Sag_Diz_Gercek_Min     = 5.0;
        public static double Sag_Diz_Gercek_Max     = 90.0;
        public static double Sag_Diz_Olcum_Min      = 0.0;
        public static double Sag_Diz_Olcum_Max      = 0.0;

        public static double Sol_Dirsek_Gercek_Min  = 5.0;
        public static double Sol_Dirsek_Gercek_Max  = 90;
        public static double Sol_Dirsek_Olcum_Min   = 0.0;
        public static double Sol_Dirsek_Olcum_Max   = 0.0;

        public static double Sag_Dirsek_Gercek_Min  = 5.0;
        public static double Sag_Dirsek_Gercek_Max  = 90.0;
        public static double Sag_Dirsek_Olcum_Min   = 0.0;
        public static double Sag_Dirsek_Olcum_Max   = 0.0;

        public static int Kalibrasyon_Adimi = 0;
        public static bool Kalibre_Edildi   = false;
    }

    public static class Eklem_Degerleri
    {
        public static Vector3 Sol_Bacak     = new Vector3(-90, 0, 0);
        public static Vector3 Sag_Bacak     = new Vector3(-90, 0, 0);
        public static Vector3 Sol_Diz       = new Vector3(90, 0, 0);
        public static Vector3 Sag_Diz       = new Vector3(90, 0, 0);

        public static Vector3 Sol_Omuz      = new Vector3(0, 0, 0);
        public static Vector3 Sag_Omuz      = new Vector3(0, 0, 0);
        public static Vector3 Sol_Dirsek    = new Vector3(45, 90, -90);
        public static Vector3 Sag_Dirsek    = new Vector3(45, -90, 90);

        public static Vector3 Govde         = new Vector3(0, 150, 0);
    }

    public GameObject       Deger_Gosterici;

    public string           Seri_Port_Adi               = "COM1";
    public int              Seri_Port_Hizi              = 115200;
    public char             Paket_Baslangici            = '!';
    public char             Paket_Sonu                  = '#';
    public int              Kalibrasyon_Mesaj_Sayisi    = 10;

    private SerialPort      Seri_Port;
    private Animator        Animator;

    private string          Kalan_Veri;
    private string          Veri_Tamponu;

    private int             Alinan_Paket_Sayisi         = 0;


    public static List<double> Kalibrasyon_Listesi = new List<double>();
    

    void Start()
    {
        Animator = GetComponent<Animator>();

        Seri_Port = new SerialPort(Seri_Port_Adi, Seri_Port_Hizi);

        Seri_Port.Open();

        Kalibrasyon_Listesi = new List<double>();
    }

    void Update()
    {
        string Okunan_Degerler = Seri_Port.ReadExisting();
        Seri_Port.BaseStream.Flush();


        Seri_Port_Verilerini_Isle(Okunan_Degerler);
    }

    private void OnAnimatorIK(int layerIndex)
    {
        Animator.SetBoneLocalRotation(HumanBodyBones.Hips, Quaternion.Euler(Eklem_Degerleri.Govde));

        Animator.SetBoneLocalRotation(HumanBodyBones.LeftUpperLeg, Quaternion.Euler(Eklem_Degerleri.Sol_Bacak));
        Animator.SetBoneLocalRotation(HumanBodyBones.RightUpperLeg, Quaternion.Euler(Eklem_Degerleri.Sag_Bacak));
        Animator.SetBoneLocalRotation(HumanBodyBones.LeftLowerLeg, Quaternion.Euler(Eklem_Degerleri.Sol_Diz));
        Animator.SetBoneLocalRotation(HumanBodyBones.RightLowerLeg, Quaternion.Euler(Eklem_Degerleri.Sag_Diz));

        Animator.SetBoneLocalRotation(HumanBodyBones.LeftUpperArm, Quaternion.Euler(Eklem_Degerleri.Sol_Omuz));
        Animator.SetBoneLocalRotation(HumanBodyBones.RightUpperArm, Quaternion.Euler(Eklem_Degerleri.Sag_Omuz));
        Animator.SetBoneLocalRotation(HumanBodyBones.LeftLowerArm, Quaternion.Euler(Eklem_Degerleri.Sol_Dirsek));
        Animator.SetBoneLocalRotation(HumanBodyBones.RightLowerArm, Quaternion.Euler(Eklem_Degerleri.Sag_Dirsek));
    }



    void Seri_Port_Verilerini_Isle(string Alinan_Veri)
    {
        int Sol_Diz_Mesaj_Degeri;
        int Sag_Diz_Mesaj_Degeri;
        int Sol_Dirsek_Mesaj_Degeri;
        int Sag_Dirsek_Mesaj_Degeri;
        string Sol_Diz;
        string Sag_Diz;
        string Sol_Dirsek;
        string Sag_Dirsek;

        Veri_Tamponu += Alinan_Veri;

        Kalan_Veri = Veri_Tamponu.Split(Paket_Sonu).Last();
        Regex Veri_Arayicisi = new Regex("(?<=!)(.*?)(?=#)");
        MatchCollection Bulunanlar = Veri_Arayicisi.Matches(Veri_Tamponu);

        Match Son_Bulunan = Bulunanlar.OfType<Match>().LastOrDefault();
        Veri_Tamponu = Kalan_Veri;

        if (Son_Bulunan == null || !Son_Bulunan.Success)
        {
            return;
        }

        Sag_Diz = Son_Bulunan.Value.Split(';')[0];
        Sol_Diz = Son_Bulunan.Value.Split(';')[1];
        Sag_Dirsek = Son_Bulunan.Value.Split(';')[2];
        Sol_Dirsek = Son_Bulunan.Value.Split(';')[3];

        Sol_Diz_Mesaj_Degeri    = int.Parse(Sol_Diz);
        Sag_Diz_Mesaj_Degeri    = int.Parse(Sag_Diz);
        Sol_Dirsek_Mesaj_Degeri = int.Parse(Sag_Dirsek);
        Sag_Dirsek_Mesaj_Degeri = int.Parse(Sag_Dirsek);


        if (Kalibrasyon.Kalibre_Edildi == false)
        {
            switch (Kalibrasyon.Kalibrasyon_Adimi)
            {
                case 1: // Kapali
                    Kalibrasyon_Listesi.Add(Sol_Diz_Mesaj_Degeri);
                    break;
                case 3: // Acik
                    Kalibrasyon_Listesi.Add(Sol_Diz_Mesaj_Degeri);
                    break;
                case 5:
                    Kalibrasyon_Listesi.Add(Sag_Diz_Mesaj_Degeri);
                    break;
                case 7:
                    Kalibrasyon_Listesi.Add(Sag_Diz_Mesaj_Degeri);
                    break;
                case 9:
                    Kalibrasyon_Listesi.Add(Sol_Dirsek_Mesaj_Degeri);
                    break;
                case 11:
                    Kalibrasyon_Listesi.Add(Sol_Dirsek_Mesaj_Degeri);
                    break;
                case 13:
                    Kalibrasyon_Listesi.Add(Sag_Dirsek_Mesaj_Degeri);
                    break;
                case 15:
                    Kalibrasyon_Listesi.Add(Sag_Dirsek_Mesaj_Degeri);
                    break;
                default:
                    break;
            }

            Debug.Log("Adim: " + Kalibrasyon.Kalibrasyon_Adimi + " | Toplanan: " + Kalibrasyon_Listesi.Count);

            return;
        }

        Eklem_Degerleri.Sol_Diz.x = Gercek_Degere_Don(Sol_Diz_Mesaj_Degeri, Kalibrasyon.Sol_Diz_Olcum_Min, Kalibrasyon.Sol_Diz_Olcum_Max, Kalibrasyon.Sol_Diz_Gercek_Min, Kalibrasyon.Sol_Diz_Gercek_Max);
        Eklem_Degerleri.Sag_Diz.x = Gercek_Degere_Don(Sag_Diz_Mesaj_Degeri, Kalibrasyon.Sag_Diz_Olcum_Min, Kalibrasyon.Sag_Diz_Olcum_Max, Kalibrasyon.Sag_Diz_Gercek_Min, Kalibrasyon.Sag_Diz_Gercek_Max);
        Eklem_Degerleri.Sol_Dirsek.x = Gercek_Degere_Don(Sol_Dirsek_Mesaj_Degeri, Kalibrasyon.Sol_Dirsek_Olcum_Min, Kalibrasyon.Sol_Dirsek_Olcum_Max, Kalibrasyon.Sol_Dirsek_Gercek_Min, Kalibrasyon.Sol_Dirsek_Gercek_Max);
        Eklem_Degerleri.Sag_Dirsek.x = Gercek_Degere_Don(Sag_Dirsek_Mesaj_Degeri, Kalibrasyon.Sag_Dirsek_Olcum_Min, Kalibrasyon.Sag_Dirsek_Olcum_Max, Kalibrasyon.Sag_Dirsek_Gercek_Min, Kalibrasyon.Sag_Dirsek_Gercek_Max);


        //Eklem_Degerleri.Sol_Diz = Gercek_Degere_Don(Sol_Diz_Mesaj_Degeri, Sinirlar.Sol_Diz_Olcum_Min, Sinirlar.Sol_Diz_Olcum_Max, Sinirlar.Sol_Diz_Gercek_Min, Sinirlar.Sol_Diz_Gercek_Max);
        Eklem_Degerleri.Sag_Diz.x = 0;

    }

    private float Gercek_Degere_Don(double Mesaj_Degeri, double Mesaj_Alt_Degeri, double Mesaj_Ust_Degeri, double Gercek_Alt_Deger, double Gercek_Ust_Deger)
    {
        double Gercek_Deger = (Gercek_Alt_Deger + (((Mesaj_Degeri - Mesaj_Alt_Degeri) / (Mesaj_Ust_Degeri - Mesaj_Alt_Degeri)) * (Gercek_Ust_Deger - Gercek_Alt_Deger)));

        if (Gercek_Deger > Gercek_Ust_Deger)
        {
            Gercek_Deger = Gercek_Ust_Deger;
        }
        else if (Gercek_Deger < Gercek_Alt_Deger)
        {
            Gercek_Deger = Gercek_Alt_Deger;
        }


        return (float)Gercek_Deger;
    }

    double Deger_Duzenle(double Aci)
    {
        if (Aci > 180)
        {
            Aci = -1 * (360 - Aci);
        }

        return Aci;
    }

    public void Kalibrasyon_Listesi_Temizle()
    {
        Kalibrasyon_Listesi.Clear();
    }

    //public void Ust_Sinir_Sec()
    //{
    //    Sinirlar.Sag_Diz_Olcum_Max = Kalibrasyon_Listesi.Sum() / Kalibrasyon_Listesi.Count;
    //    Kalibrasyon_Listesi.Clear();

    //    Debug.Log("Üst sınır: " + Sinirlar.Sag_Diz_Olcum_Max);
    //}

    //public void Alt_Sinir_Sec()
    //{
    //    Sinirlar.Sag_Diz_Olcum_Min = Kalibrasyon_Listesi.Sum() / Kalibrasyon_Listesi.Count;
    //    Kalibrasyon_Listesi.Clear();

    //    Debug.Log("Alt sınır: " + Sinirlar.Sag_Diz_Olcum_Min);
    //}

    //public void Kalibrasyon_Tamamla()
    //{
    //    Sinirlar.Kalibre_Edildi = true;
    //}
}

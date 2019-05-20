using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class Loglayici : MonoBehaviour
{
    public bool Kalibrasyon_Tamamlandi = false;

    private string Kayit_Dizini = "D:\\Logs\\" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss");

    private StreamWriter Referans_Loger;
    private StreamWriter Textile_Loger;
    private StreamWriter Kinect_Loger;
    private StreamWriter RGB_Loger;
    private StreamWriter PN_Loger;


    // Start is called before the first frame update
    void Start()
    {


        if (!Directory.Exists(Kayit_Dizini))
        {
            Directory.CreateDirectory(Kayit_Dizini);
        }

        Referans_Loger  = File.CreateText(Kayit_Dizini + "\\Referans_Man.csv");
        Textile_Loger   = File.CreateText(Kayit_Dizini + "\\Textile_Man.csv");
        Kinect_Loger    = File.CreateText(Kayit_Dizini + "\\Kinect_Man.csv");
        RGB_Loger       = File.CreateText(Kayit_Dizini + "\\RGB_Man.csv");
        PN_Loger        = File.CreateText(Kayit_Dizini + "\\PN_Man.csv");
        
        Referans_Loger.WriteLine("Tarih;Sag_Diz_Aci;Sol_Diz_Aci;Sag_Dirsek_Aci;Sol_Dirsek_Aci");
        Textile_Loger.WriteLine("Tarih;Sag_Diz_Aci;Sol_Diz_Aci;Sag_Dirsek_Aci;Sol_Dirsek_Aci");
        Kinect_Loger.WriteLine("Tarih;Sag_Diz_Aci;Sol_Diz_Aci;Sag_Dirsek_Aci;Sol_Dirsek_Aci");
        RGB_Loger.WriteLine("Tarih;Sag_Diz_Aci;Sol_Diz_Aci;Sag_Dirsek_Aci;Sol_Dirsek_Aci");
        PN_Loger.WriteLine("Tarih;Sag_Diz_Aci;Sol_Diz_Aci;Sag_Dirsek_Aci;Sol_Dirsek_Aci");
        
    }

    void LateUpdate()
    {
        if (!Kalibrasyon_Tamamlandi)
        {
            return;
        }

        string Zaman = DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss.ffffff");

        Referans_Loger.WriteLine("{0};{1};{2};{3};{4}",
                                 Zaman,
                                 Deger_Duzenle(Gercek_Man.Eklem_Degerleri.Sag_Diz.x).ToString("000"),
                                 Deger_Duzenle(Gercek_Man.Eklem_Degerleri.Sol_Diz.x).ToString("000"),
                                 Deger_Duzenle(Gercek_Man.Eklem_Degerleri.Sag_Dirsek.x).ToString("000"),
                                 Deger_Duzenle(Gercek_Man.Eklem_Degerleri.Sol_Dirsek.x).ToString("000")
                                 );

        Textile_Loger.WriteLine("{0};{1};{2};{3};{4}",
                                Zaman,
                                Deger_Duzenle(COM_Man.Eklem_Degerleri.Sag_Diz.x).ToString("000"),
                                Deger_Duzenle(COM_Man.Eklem_Degerleri.Sol_Diz.x).ToString("000"),
                                Deger_Duzenle(COM_Man.Eklem_Degerleri.Sag_Dirsek.x).ToString("000"),
                                Deger_Duzenle(COM_Man.Eklem_Degerleri.Sol_Dirsek.x).ToString("000")
                                );

        Kinect_Loger.WriteLine("{0};{1};{2};{3};{4}",
                               Zaman,
                               Deger_Duzenle(AvatarController.Eklem_Degerleri.Sag_Diz).ToString("000"),
                               Deger_Duzenle(AvatarController.Eklem_Degerleri.Sol_Diz).ToString("000"),
                               Deger_Duzenle(AvatarController.Eklem_Degerleri.Sag_Dirsek).ToString("000"),
                               Deger_Duzenle(AvatarController.Eklem_Degerleri.Sol_Dirsek).ToString("000")
                               );

        RGB_Loger.WriteLine("{0};{1};{2};{3};{4}",
                            Zaman,
                            Deger_Duzenle(UDP_Man.Eklem_Degerleri.Sag_Diz).ToString("000"),
                            Deger_Duzenle(UDP_Man.Eklem_Degerleri.Sol_Diz).ToString("000"),
                            Deger_Duzenle(UDP_Man.Eklem_Degerleri.Sag_Dirsek).ToString("000"),
                            Deger_Duzenle(UDP_Man.Eklem_Degerleri.Sol_Dirsek).ToString("000")
                            );

        PN_Loger.WriteLine("{0};{1};{2};{3};{4}",
                           Zaman,
                           Deger_Duzenle(NeuronAnimatorInstance.Eklem_Degerleri.Sag_Diz).ToString("000"),
                           Deger_Duzenle(NeuronAnimatorInstance.Eklem_Degerleri.Sol_Diz).ToString("000"),
                           Deger_Duzenle(NeuronAnimatorInstance.Eklem_Degerleri.Sag_Dirsek).ToString("000"),
                           Deger_Duzenle(NeuronAnimatorInstance.Eklem_Degerleri.Sol_Dirsek).ToString("000")
                           );
        
    }

    double Deger_Duzenle(double Aci)
    {
        if (Aci > 180)
        {
            Aci = -1 * (360 - Aci);
        }

        return Aci;
    }
}

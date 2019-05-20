using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class Kalibrasyon : MonoBehaviour
{
    double Kalan_Zaman;
    GameObject Kalibrasyon_Adimi_Resmi;
    GameObject Kalibrasyon_Mesaji;
    GameObject Kalibrasyon_Zaman;

    // Start is called before the first frame update
    void Start()
    {
        COM_Man.Kalibrasyon.Kalibre_Edildi = false;
        COM_Man.Kalibrasyon.Kalibrasyon_Adimi = 0;


        Kalibrasyon_Adimi_Resmi = GameObject.Find("Kalibrasyon_Adimi_Resmi");
        Kalibrasyon_Mesaji = GameObject.Find("Kalibrasyon_Mesaji");
        Kalibrasyon_Zaman = GameObject.Find("Kalibrasyon_Zaman");
        Kalan_Zaman = 10;
    }

    // Update is called once per frame
    void Update()
    {
        Kalan_Zaman -= Time.deltaTime;
        Kalibrasyon_Zaman.GetComponent<Text>().text =  System.Math.Round(Kalan_Zaman).ToString();
        
        if (Kalan_Zaman <= 0.0)
        {
            switch (COM_Man.Kalibrasyon.Kalibrasyon_Adimi)
            {

                case 0:
                    Kalibrasyon_Mesaji.GetComponent<Text>().text = "Please, hold left knee. Collecting...";

                    COM_Man.Kalibrasyon_Listesi.Clear();

                    COM_Man.Kalibrasyon.Kalibrasyon_Adimi++;
                    Kalan_Zaman = 10.0;
                    break;

                case 1: // Kapali             
                    COM_Man.Kalibrasyon.Sol_Diz_Olcum_Max = COM_Man.Kalibrasyon_Listesi.Sum();

                    Kalibrasyon_Adimi_Resmi.GetComponent<RawImage>().texture = Resources.Load<Texture2D>("Images/sol_ayak_acik");
                    Kalibrasyon_Mesaji.GetComponent<Text>().text = "Please, open left knee.";

                    COM_Man.Kalibrasyon.Kalibrasyon_Adimi++;
                    Kalan_Zaman = 10.0;
                    break;

                case 2: // Acik
                    Kalibrasyon_Mesaji.GetComponent<Text>().text = "Please, hold left knee. Collecting...";

                    COM_Man.Kalibrasyon_Listesi.Clear();

                    COM_Man.Kalibrasyon.Kalibrasyon_Adimi++;
                    Kalan_Zaman = 10.0;
                    break;

                case 3:
                    COM_Man.Kalibrasyon.Sol_Diz_Olcum_Min = COM_Man.Kalibrasyon_Listesi.Sum();

                    Kalibrasyon_Adimi_Resmi.GetComponent<RawImage>().texture = Resources.Load<Texture2D>("Images/sag_ayak_kapali");
                    Kalibrasyon_Mesaji.GetComponent<Text>().text = "Please, close right knee.";

                    COM_Man.Kalibrasyon.Kalibrasyon_Adimi++;
                    Kalan_Zaman = 10.0;
                    break;

                case 4:
                    Kalibrasyon_Mesaji.GetComponent<Text>().text = "Please, hold right knee. Collecting...";

                    COM_Man.Kalibrasyon_Listesi.Clear();

                    COM_Man.Kalibrasyon.Kalibrasyon_Adimi++;
                    Kalan_Zaman = 10.0;
                    break;

                case 5:
                    COM_Man.Kalibrasyon.Sag_Diz_Olcum_Max = COM_Man.Kalibrasyon_Listesi.Sum();

                    Kalibrasyon_Adimi_Resmi.GetComponent<RawImage>().texture = Resources.Load<Texture2D>("Images/sag_ayak_acik");
                    Kalibrasyon_Mesaji.GetComponent<Text>().text = "Please, open right knee.";

                    COM_Man.Kalibrasyon.Kalibrasyon_Adimi++;
                    Kalan_Zaman = 10.0;
                    break;

                case 6:
                    Kalibrasyon_Mesaji.GetComponent<Text>().text = "Please, hold right knee. Collecting...";

                    COM_Man.Kalibrasyon_Listesi.Clear();

                    COM_Man.Kalibrasyon.Kalibrasyon_Adimi++;
                    Kalan_Zaman = 10.0;
                    break;

                case 7:
                    COM_Man.Kalibrasyon.Sag_Diz_Olcum_Min = COM_Man.Kalibrasyon_Listesi.Sum();

                    Kalibrasyon_Adimi_Resmi.GetComponent<RawImage>().texture = Resources.Load<Texture2D>("Images/sol_kol_kapali");
                    Kalibrasyon_Mesaji.GetComponent<Text>().text = "Please, close left arm.";

                    COM_Man.Kalibrasyon.Kalibrasyon_Adimi++;
                    Kalan_Zaman = 10.0;
                    break;

                case 8:
                    Kalibrasyon_Mesaji.GetComponent<Text>().text = "Please, hold left arm. Collecting...";

                    COM_Man.Kalibrasyon_Listesi.Clear();

                    COM_Man.Kalibrasyon.Kalibrasyon_Adimi++;
                    Kalan_Zaman = 10.0;
                    break;

                case 9:
                    COM_Man.Kalibrasyon.Sol_Dirsek_Olcum_Max = COM_Man.Kalibrasyon_Listesi.Sum();

                    Kalibrasyon_Adimi_Resmi.GetComponent<RawImage>().texture = Resources.Load<Texture2D>("Images/sol_kol_acik");
                    Kalibrasyon_Mesaji.GetComponent<Text>().text = "Please, open left arm.";

                    COM_Man.Kalibrasyon.Kalibrasyon_Adimi++;
                    Kalan_Zaman = 10.0;
                    break;

                case 10:
                    Kalibrasyon_Mesaji.GetComponent<Text>().text = "Please, hold left arm. Collecting...";

                    COM_Man.Kalibrasyon_Listesi.Clear();

                    COM_Man.Kalibrasyon.Kalibrasyon_Adimi++;
                    Kalan_Zaman = 10.0;
                    break;

                case 11:
                    COM_Man.Kalibrasyon.Sol_Dirsek_Olcum_Min = COM_Man.Kalibrasyon_Listesi.Sum();

                    Kalibrasyon_Adimi_Resmi.GetComponent<RawImage>().texture = Resources.Load<Texture2D>("Images/sag_kol_kapali");
                    Kalibrasyon_Mesaji.GetComponent<Text>().text = "Please, close right arm.";

                    COM_Man.Kalibrasyon.Kalibrasyon_Adimi++;
                    Kalan_Zaman = 10.0;
                    break;

                case 12:
                    Kalibrasyon_Mesaji.GetComponent<Text>().text = "Please, hold right arm. Collecting...";

                    COM_Man.Kalibrasyon_Listesi.Clear();

                    COM_Man.Kalibrasyon.Kalibrasyon_Adimi++;
                    Kalan_Zaman = 10.0;
                    break;

                case 13:
                    COM_Man.Kalibrasyon.Sag_Dirsek_Olcum_Max = COM_Man.Kalibrasyon_Listesi.Sum();

                    Kalibrasyon_Adimi_Resmi.GetComponent<RawImage>().texture = Resources.Load<Texture2D>("Images/sag_kol_acik");
                    Kalibrasyon_Mesaji.GetComponent<Text>().text = "Please, open right arm.";

                    COM_Man.Kalibrasyon.Kalibrasyon_Adimi++;
                    Kalan_Zaman = 10.0;
                    break;

                case 14:
                    Kalibrasyon_Mesaji.GetComponent<Text>().text = "Please, hold right arm. Collecting...";

                    COM_Man.Kalibrasyon_Listesi.Clear();

                    COM_Man.Kalibrasyon.Kalibrasyon_Adimi++;
                    Kalan_Zaman = 10.0;
                    break;

                case 15:
                    COM_Man.Kalibrasyon.Sag_Dirsek_Olcum_Min = COM_Man.Kalibrasyon_Listesi.Sum();

                    
                    Kalibrasyon_Mesaji.GetComponent<Text>().text = "Calibration done.";


                    COM_Man.Kalibrasyon.Kalibre_Edildi = true;
                    GameObject.Find("Kalibrasyon_Yardimcisi").SetActive(false);


                    break;

                default:
                    break;
            }
        }

    }
}

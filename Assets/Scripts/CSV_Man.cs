using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class CSV_Man : MonoBehaviour
{
    private class Kayit
    {
        public DateTime Kayit_Zamani;
        public double Aci_Verisi;
    }

    private static class Eklem_Degerleri
    {
        public static double Sol_Diz = 0.0;
        public static double Sag_Diz = 0.0;
    }

    public GameObject Deger_Gosterici;

    public UnityEngine.Object CSV_Dosyasi;

    private Animator Animator;
    private DateTime Simulasyon_Zamani;

    private string Kayit_Dosyasi_Adi = Directory.GetCurrentDirectory() + "\\Assets\\Others\\CSV_Man_" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss") + ".motion";
    private StreamWriter Kayit_Dosyasi;

    private List<Kayit> Kayitlar = new List<Kayit>();

    // Start is called before the first frame update
    void Start()
    {
        Animator = GetComponent<Animator>();

        if (CSV_Dosyasi != null)
        {
            string[] CSV_Satirlar = CSV_Dosyasi.ToString().Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            foreach (string Satir in CSV_Satirlar)
            {
                Kayit Satir_Kaydi = new Kayit();

                Satir_Kaydi.Kayit_Zamani    = DateTime.Parse(Satir.Split(new char[] {','})[0]);
                Satir_Kaydi.Aci_Verisi      = double.Parse(Satir.Split(new char[] { ',' })[1].Replace('.', ',')); 

                Kayitlar.Add(Satir_Kaydi);
            }

            Simulasyon_Zamani = Kayitlar[0].Kayit_Zamani;
        }

        Kayit_Dosyasi = File.CreateText(Kayit_Dosyasi_Adi);
        Kayit_Dosyasi.WriteLine("Tarih,Sag_Aci,Sol_Aci");

    }

    void Update()
    {
        Veri_Isle();
    }

    void OnAnimatorIK()
    {
        Animator.SetBoneLocalRotation(HumanBodyBones.Hips, Quaternion.Euler(new Vector3(0, 0, 0)));

        Animator.SetBoneLocalRotation(HumanBodyBones.LeftUpperLeg, Quaternion.Euler(new Vector3(-90, 0, 0)));
        Animator.SetBoneLocalRotation(HumanBodyBones.RightUpperLeg, Quaternion.Euler(new Vector3(-90, 0, 0)));


        Animator.SetBoneLocalRotation(HumanBodyBones.LeftLowerLeg, Quaternion.Euler(new Vector3((float)Eklem_Degerleri.Sol_Diz, 0, 0)));
        Animator.SetBoneLocalRotation(HumanBodyBones.RightLowerLeg, Quaternion.Euler(new Vector3((float)Eklem_Degerleri.Sag_Diz, 0, 0)));
    }

    void LateUpdate()
    {
        if (Deger_Gosterici != null)
        {
            Deger_Gosterici.GetComponent<TextMesh>().text = "";
            Deger_Gosterici.GetComponent<TextMesh>().text += "R: " + Animator.GetBoneTransform(HumanBodyBones.RightLowerLeg).localEulerAngles[0].ToString("000");
            Deger_Gosterici.GetComponent<TextMesh>().text += "  ";
            Deger_Gosterici.GetComponent<TextMesh>().text += "L: " + Animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg).localEulerAngles[0].ToString("000");
        }

        Kayit_Dosyasi.WriteLine("{0};{1};{2}", DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_ffffff"), Animator.GetBoneTransform(HumanBodyBones.RightLowerLeg).localEulerAngles[0].ToString("000"), Animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg).localEulerAngles[0].ToString("000"));
    }

    void OnApplicationQuit()
    {
        Kayit_Dosyasi.Close();
    }

    void Veri_Isle()
    {
        Simulasyon_Zamani = Simulasyon_Zamani.AddSeconds(Time.deltaTime);
        //Debug.Log("Zaman: " + Simulasyon_Zamani.ToString());

        while (Kayitlar.Count != 0 && Kayitlar[0].Kayit_Zamani < Simulasyon_Zamani)
        {
            Eklem_Degerleri.Sag_Diz = 180 - Kayitlar[0].Aci_Verisi;
            Eklem_Degerleri.Sol_Diz = 0;

            //Debug.Log("Sag: " + Eklem_Degerleri.Sag_Diz.ToString());

            Kayitlar.RemoveAt(0);
        }

    }
}

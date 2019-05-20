using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.IO;

public class UDP_Man : MonoBehaviour
{

    public static class Eklem_Degerleri
    {
        public static double Sol_Bacak = -90;
        public static double Sag_Bacak = -90;
        public static double Sol_Diz = 0.0;
        public static double Sag_Diz = 0.0;

        public static double Sol_Omuz = 0.0;
        public static double Sag_Omuz = 0.0;
        public static double Sol_Dirsek = 0.0;
        public static double Sag_Dirsek = 0.0;

        public static double Durus = 110.0;
    }
    public GameObject   Deger_Gosterici;

    public int          Port = 5005;

    private Thread      Paket_Yakalayici_Thread;
    private UdpClient   UDP_Client;
    private Animator    Animator;
    private string[]    Degerler;

    public void Start()
    {
        Animator = GetComponent<Animator>();

        Thread_Baslat();
    }


    private void Thread_Baslat()
    {
        Paket_Yakalayici_Thread = new Thread(new ThreadStart(Paket_Yakalayici));
        Paket_Yakalayici_Thread.IsBackground = true;
        Paket_Yakalayici_Thread.Start();
    }

    private void Paket_Yakalayici()
    {

        UDP_Client = new UdpClient(Port);
        while (true)
        {
            try
            {
                IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, Port);
                byte[] Okunan_Veri = UDP_Client.Receive(ref anyIP);
                string Okunan_Mesaj = Encoding.UTF8.GetString(Okunan_Veri);


                Degerler = Okunan_Mesaj.Split(',');


                //Eklem_Degerleri.Sol_Diz = 180 - double.Parse(Degerler[0].Replace('.', ','));
                Eklem_Degerleri.Sag_Diz = 180 - double.Parse(Degerler[1].Replace('.', ','));

            }
            catch (Exception err)
            {
                print(err.ToString());
            }
        }
    }

    private void OnAnimatorIK(int layerIndex)
    {
        Animator.SetBoneLocalRotation(HumanBodyBones.Hips, Quaternion.Euler(new Vector3(0, 110, 0)));

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
            Deger_Gosterici.GetComponent<TextMesh>().text += "Diz: " + Deger_Duzenle(Eklem_Degerleri.Sag_Diz).ToString("000");
        }

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

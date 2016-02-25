using Android.App;
using Android.Widget;
using Android.OS;
using Android.Net.Wifi;
using Android.Net;
using System.Net;
using System.IO;
using System.Threading;
using Newtonsoft.Json;

namespace SportBoxApp
{
    [Activity(Label = "SportBoxApp", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        //const string networkSSID = "\"" + "Arduino Yun-90A2DAF6020A"+"\"";
        const string networkSSID = "\"" + "Arduino Yun-90A2DAF524F6" + "\"";
        string urlCo = "http://192.168.240.1/arduino/digital/10/1";
        string urlDeco = "http://192.168.240.1/arduino/digital/10/0";
        string urlStat = "http://192.168.240.1/arduino/hello/1";

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Main);

            Button buttonConnexion = FindViewById<Button>(Resource.Id.btnCo);
            Button buttonCheck = FindViewById<Button>(Resource.Id.checkLed);
            Button buttonDeco = FindViewById<Button>(Resource.Id.deco);
            Button buttonGetStat = FindViewById<Button>(Resource.Id.getStat);

            TextView etat = FindViewById<TextView>(Resource.Id.etatCo);
            TextView altitude = FindViewById<TextView>(Resource.Id.altitude);
            TextView pression = FindViewById<TextView>(Resource.Id.pression);
            TextView VitMax = FindViewById<TextView>(Resource.Id.VitMax);
            TextView VitMoy = FindViewById<TextView>(Resource.Id.VitMoy);
            TextView dist = FindViewById<TextView>(Resource.Id.dist);
            TextView deni = FindViewById<TextView>(Resource.Id.deniMax);
            TextView title = FindViewById<TextView>(Resource.Id.title);

            buttonConnexion.Click += delegate
            {
                //Connexion to Arduino
                var conf = new WifiConfiguration();
                conf.Ssid = networkSSID;
                conf.AllowedKeyManagement.Set((int)KeyManagementType.None);
                var connectivityManager = (ConnectivityManager)GetSystemService(ConnectivityService);
                var mobileState = connectivityManager.GetNetworkInfo(ConnectivityType.Wifi).GetState();
                if (mobileState != NetworkInfo.State.Connected)
                {
                    etat.Visibility = Android.Views.ViewStates.Visible;
                    etat.Text = "Activation du Wi-Fi en cours ... ";
                    var mawifi = (WifiManager)GetSystemService(WifiService);
                    mawifi.SetWifiEnabled(true);
                    Thread.Sleep(5000);
                }
                var wifiManager = (WifiManager)GetSystemService(WifiService);
                wifiManager.AddNetwork(conf);
                var list = wifiManager.ConfiguredNetworks;
                foreach (var i in list)
                {
                    if (i.Ssid != null && i.Ssid.Equals(networkSSID))
                    {
                        wifiManager.Disconnect();
                        wifiManager.EnableNetwork(i.NetworkId, true);
                        wifiManager.Reconnect();
                        etat.Visibility = Android.Views.ViewStates.Visible;
                        etat.Text = "Connexion à " + networkSSID + " en cours ! Une fois connecté, tester la connexion en cliquant sur : Check Connexion.";
                        break;
                    }
                }
                buttonCheck.Visibility = Android.Views.ViewStates.Visible;
                buttonConnexion.Visibility = Android.Views.ViewStates.Gone;
            };

            buttonCheck.Click += delegate
            {
                var request = HttpWebRequest.Create(urlCo);
                request.ContentType = "application/json";
                request.Method = "GET";

                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                        etat.Text = "Error fetching data. Server returned status code: " + response.StatusCode;
                    else
                    {
                        etat.Text = "Connexion réussie !";
                        buttonDeco.Visibility = Android.Views.ViewStates.Visible;
                        buttonGetStat.Visibility = Android.Views.ViewStates.Visible;
                    }
                        
                }
            };

            buttonDeco.Click += delegate
            {
                var request_2 = HttpWebRequest.Create(urlDeco);
                request_2.ContentType = "application/json";
                request_2.Method = "GET";

                using (HttpWebResponse response_2 = request_2.GetResponse() as HttpWebResponse)
                {
                    if (response_2.StatusCode != HttpStatusCode.OK)
                        etat.Text = " ===================== Error fetching data. Server returned status code: " + response_2.StatusCode;
                    else
                    {
                        etat.Text = "Déconnexion réussie !";
                        buttonGetStat.Visibility = Android.Views.ViewStates.Invisible;
                    }
                        
                }
            };

            buttonGetStat.Click += delegate
            {
                var request_3 = HttpWebRequest.Create(urlStat);
                request_3.ContentType = "application/json";
                request_3.Method = "GET";

                using (HttpWebResponse response_3 = request_3.GetResponse() as HttpWebResponse)
                {
                    if (response_3.StatusCode != HttpStatusCode.OK)
                        etat.Text = "Error fetching data. Server returned status code:" + response_3.StatusCode;
                    using (StreamReader reader = new StreamReader(response_3.GetResponseStream()))
                    {
                        var content = reader.ReadToEnd();
                        title.Visibility = Android.Views.ViewStates.Visible;
                        altitude.Visibility = Android.Views.ViewStates.Visible;

                        if (string.IsNullOrWhiteSpace(content))
                        {
                            altitude.Text = "Erreur lors de la lecture du capteur";
                        }
                        else
                        {
                            Statistique stat = JsonConvert.DeserializeObject<Statistique>(content);
                            string altiStat = stat.altitude;
                            string pressionStat = stat.pression;
                            /*string vitMaxStat = stat.vitesseMax;
                            string vitMoyStat = stat.vitesseMoy;
                            string distStat = stat.distance;*/

                            pression.Visibility = Android.Views.ViewStates.Visible;
                            VitMax.Visibility = Android.Views.ViewStates.Visible;
                            VitMoy.Visibility = Android.Views.ViewStates.Visible;
                            dist.Visibility = Android.Views.ViewStates.Visible;
                            deni.Visibility = Android.Views.ViewStates.Visible;
                            altitude.Text = "Altitude : " + altiStat + " mètres";
                            pression.Text = "Pression : " + pressionStat + " bar";
                            /*
                            VitMax.Text = "Vitesse max : " + vitMaxStat + " km/h";
                            VitMoy.Text = "Vitesse moyenne : " + vitMoyStat + " km/h";
                            dist.Text = "Distance parcourue : " + distStat + " métres";
                            */
                        }
                    }
                }
            };
        }
    }
}


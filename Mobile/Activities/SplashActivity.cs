using System.Linq;
using System;
using System.Threading;
using System.IO;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.App;
using Android.Net;
using Android.Widget;
using Android.Views;

using DataModel;
using Newtonsoft.Json;
using AlertDialog = Android.App.AlertDialog;

namespace Mobile
{
    [Activity(Theme = "@style/no_action_bar_light_theme", MainLauncher = true)]
    public class SplashActivity : AppCompatActivity
    {
        private ProgressBar _loadPb;

        private string _username;
        private string _password;
        private string _serviceUri;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_splash);
            RequestedOrientation = ScreenOrientation.Portrait;

            _loadPb = FindViewById<ProgressBar>(Resource.Id.pbLoad0);

            var pref = Application.Context.GetSharedPreferences("UserInfo", FileCreationMode.Private);

            _username = pref.GetString("Username", string.Empty);
            _password = pref.GetString("Password", string.Empty);
            _serviceUri = ConfigReader.GetWebServiceUri(Assets.Open("config.xml"));

            _loadPb.Visibility = ViewStates.Visible;

            if (_username == string.Empty || _password == string.Empty)
            {
                var intent = new Intent(this, typeof(LoginActivity));
                StartActivity(intent);
                Finish();
            }
            else
            {
                var thread = new Thread(CheckUserData);

                thread.IsBackground = true;
                thread.Start();
            }
        }

        private void CheckUserData()
        {
            var connectionManager = (ConnectivityManager)GetSystemService(Context.ConnectivityService);
            var connectionInfo = connectionManager.ActiveNetworkInfo;

            if (connectionInfo != null && connectionInfo.IsConnected == true)
            {
                string userString = string.Format("{0}/User/GetByName?name={1}", _serviceUri, _username);
                var user = JsonConvert.DeserializeObject<User>(HttpClient.Get(userString));

                var pref = Application.Context.GetSharedPreferences("UserInfo", FileCreationMode.Private);
                var edit = pref.Edit();

                if (user != null && user.Password == _password)
                {
                    edit.PutString("Username", _username);
                    edit.PutBoolean("Remember", true);
                    edit.Apply();

                    var intent = new Intent(this, typeof(MainActivity));
                    StartActivity(intent);
                    Finish();
                }
                else
                {
                    edit.Clear();
                    edit.Apply();

                    var intent = new Intent(this, typeof(LoginActivity));
                    StartActivity(intent);
                    Finish();
                }
            }
            else
            {
                RunOnUiThread(() =>
                {
                    var dialog = new AlertDialog.Builder(this);

                    dialog.SetTitle("Ошибка сети");
                    dialog.SetMessage("Отсутствует подключение к сети. Повторите попытку заново.");
                    dialog.SetPositiveButton("ОК", delegate { dialog.Dispose(); Finish(); });
                    dialog.Show();
                });            
            }
        }
    }
}
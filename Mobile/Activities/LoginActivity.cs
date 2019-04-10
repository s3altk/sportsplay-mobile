using System.Threading;
using System;
using System.Linq;
using System.Net;
using System.IO;

using Android.App;
using Android.OS;
using Android.Widget;
using Android.Content;
using Android.Support.V7.App;
using Android.Views;
using Android.Views.InputMethods;
using Android.Content.PM;
using Android.Net;

using DataModel;
using Newtonsoft.Json;
using AlertDialog = Android.App.AlertDialog;

namespace Mobile
{
    [Activity(Theme = "@style/no_action_bar_light_theme")]
    public class LoginActivity : AppCompatActivity
    {
        private EditText _loginText;
        private EditText _passwordText;
        private Button _registerBtn;
        private Button _loginBtn;
        private Button _guestBtn;
        private ProgressBar _loadPb;
        private CheckBox _rememberChb;
        private EventHandler<OnLoginEventArgs> _loginComplete;

        private string _serviceUri;

        public override void OnBackPressed()
        {
            FinishAffinity();
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_login);
            RequestedOrientation = ScreenOrientation.Portrait;

            _loginText = FindViewById<EditText>(Resource.Id.txtLogin1);
            _passwordText = FindViewById<EditText>(Resource.Id.txtPassword1);
            _rememberChb = FindViewById<CheckBox>(Resource.Id.chbRemember1);
            _loginBtn = FindViewById<Button>(Resource.Id.btnLogin1);
            _guestBtn = FindViewById<Button>(Resource.Id.btnGuest1);
            _registerBtn = FindViewById<Button>(Resource.Id.btnRegister1);
            _loadPb = FindViewById<ProgressBar>(Resource.Id.pbLoad1);

            _rememberChb.Checked = true;
            _loadPb.Visibility = ViewStates.Invisible;

            _serviceUri = ConfigReader.GetWebServiceUri(Assets.Open("config.xml"));

            #region События

            _rememberChb.CheckedChange += (sender, args) =>
            {
                if (!_rememberChb.Checked)
                {
                    var dialog = new AlertDialog.Builder(this);
                    dialog.SetTitle("Внимание");
                    dialog.SetMessage("Не выбрано запоминание пользователя. " +
                                      "В этом случае при выходе из приложения работа будет потеряна.");
                    dialog.SetPositiveButton("ОК", delegate
                    {
                        dialog.Dispose();
                    });
                    dialog.Show();
                }
            };

            _guestBtn.Click += (sender, args) =>
            {
                var connectionManager = (ConnectivityManager)GetSystemService(Context.ConnectivityService);
                var connectionInfo = connectionManager.ActiveNetworkInfo;

                if (connectionInfo != null && connectionInfo.IsConnected == true)
                {
                    var pref = Application.Context.GetSharedPreferences("UserInfo", FileCreationMode.Private);

                    var edit = pref.Edit();
                    edit.PutString("Username", "GUEST");
                    edit.PutBoolean("Remember", false);
                    edit.Apply();

                    var intent = new Intent(this, typeof(MainActivity));
                    StartActivity(intent);
                    Finish();
                }
                else
                {
                    Toast.MakeText(this,
                            "Отсутствует подключение к сети. Повторите попытку заново.",
                            ToastLength.Long).Show();
                }
            };

            _registerBtn.Click += (sender, args) =>
            {
                var intent = new Intent(this, typeof(RegisterActivity));
                StartActivity(intent);
            };

            _loginBtn.Click += (sender, args) =>
            {
                _loginComplete.Invoke(this, new OnLoginEventArgs(_loginText.Text, _passwordText.Text));
            };

            _loginComplete += (sender, args) =>
            {
                _loadPb.Visibility = ViewStates.Visible;

                var thread = new Thread(CheckUserData);

                thread.IsBackground = true;
                thread.Start();
            };

            #endregion
        }

        private void CheckUserData()
        {
            var connectionManager = (ConnectivityManager)GetSystemService(Context.ConnectivityService);
            var connectionInfo = connectionManager.ActiveNetworkInfo;

            if (connectionInfo != null && connectionInfo.IsConnected == true)
            {
                string userString = string.Format("{0}/User/GetByName?name={1}", _serviceUri, _loginText.Text.Trim());
                var user = JsonConvert.DeserializeObject<User>(HttpClient.Get(userString));

                if (user != null && user.Password == _passwordText.Text.Trim())
                {
                    RunOnUiThread(() =>
                    {
                        _loadPb.Visibility = ViewStates.Invisible;

                        var pref = Application.Context.GetSharedPreferences("UserInfo", FileCreationMode.Private);

                        var edit = pref.Edit();
                        edit.PutString("Username", user.Name);
                        edit.PutBoolean("Remember", _rememberChb.Checked);

                        if (_rememberChb.Checked)
                        {
                            edit.PutString("Password", user.Password);
                        }

                        edit.Apply();

                        var intent = new Intent(this, typeof(MainActivity));
                        StartActivity(intent);
                        Finish();
                    });
                }
                else
                {
                    RunOnUiThread(() =>
                    {
                        _loadPb.Visibility = ViewStates.Invisible;

                        var dialog = new AlertDialog.Builder(this);

                        dialog.SetTitle("Ошибка авторизации");
                        dialog.SetMessage("Неправильный логин или пароль.");
                        dialog.SetPositiveButton("ОК", delegate { dialog.Dispose(); });
                        dialog.Show();
                    });
                }
            }
            else
            {
                RunOnUiThread(() =>
                {
                    _loadPb.Visibility = ViewStates.Invisible;

                    Toast.MakeText(this,
                            "Отсутствует подключение к сети. Повторите попытку заново.",
                            ToastLength.Long).Show();
                });
            }
        }
    }
}
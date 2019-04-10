using System.Threading;
using System;
using System.Linq;
using System.Net;
using System.Text;
using System.IO;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Android.Net;
using Android.Provider;

using Newtonsoft.Json;
using DataModel;
using AlertDialog = Android.App.AlertDialog;

namespace Mobile
{
    [Activity(Theme = "@style/no_action_bar_light_theme")]
    public class RegisterActivity : AppCompatActivity
    {
        private EditText _loginText;
        private EditText _passwordText;
        private Button _registerBtn;
        private ImageButton _backBtn;
        private ProgressBar _loadPb;
        private EventHandler<OnRegisterEventArgs> _registerComplete;

        private string _serviceUri;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_register);
            RequestedOrientation = ScreenOrientation.Portrait;

            _loginText = FindViewById<EditText>(Resource.Id.txtLogin2);
            _passwordText = FindViewById<EditText>(Resource.Id.txtPassword2);
            _registerBtn = FindViewById<Button>(Resource.Id.btnRegister2);
            _backBtn = FindViewById<ImageButton>(Resource.Id.btnBack2);
            _loadPb = FindViewById<ProgressBar>(Resource.Id.pbLoad2);

            _loadPb.Visibility = ViewStates.Invisible;

            _serviceUri = ConfigReader.GetWebServiceUri(Assets.Open("config.xml"));

            #region События

            _backBtn.Click += (sender, args) =>
            {
                Finish();
            };

            _registerBtn.Click += (sender, args) =>
            {
                _registerComplete.Invoke(this, new OnRegisterEventArgs(_loginText.Text, _passwordText.Text));
            };

            _registerComplete += (sender, args) =>
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
                var login = _loginText.Text.Trim();
                var password = _passwordText.Text.Trim();

                if (login != string.Empty && password != string.Empty)
                {
                    if (PasswordChecker.IsPasswordCorrect(password))
                    {
                        string userString = string.Format("{0}/User/GetByName?name={1}", _serviceUri, login);
                        var user = JsonConvert.DeserializeObject<User>(HttpClient.Get(userString));

                        if (user == null)
                        {
                            var newUser = new User
                            {
                                Id = Guid.NewGuid(),
                                Name = login,
                                Password = password
                            };

                            string postString = string.Format("{0}/User/Add", _serviceUri);
                            string json = JsonConvert.SerializeObject(newUser);
                            HttpClient.Post(json, postString);

                            RunOnUiThread(() =>
                            {
                                _loadPb.Visibility = ViewStates.Invisible;

                                var pref = Application.Context.GetSharedPreferences("UserInfo", FileCreationMode.Private);

                                var edit = pref.Edit();
                                edit.PutString("Username", newUser.Name);
                                edit.PutBoolean("Remember", false);
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

                                Toast.MakeText(this, "Пользователь с таким именем уже существует.", ToastLength.Short).Show();
                            });
                        }
                    }
                    else
                    {
                        RunOnUiThread(() =>
                        {
                            _loadPb.Visibility = ViewStates.Invisible;

                            var dialog = new AlertDialog.Builder(this);

                            dialog.SetTitle("Ошибка регистрации");
                            dialog.SetMessage("Пароль должен быть составлен из латинских букв. Длина пароля должна быть "
                                            + "больше 5 букв. Пароль должен иметь одну букву из верхнего регистра, "
                                            + "нижнего регистра, одну цифру и один символ.");
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

                        Toast.MakeText(this, "Все поля должны быть заполнены.", ToastLength.Short).Show();
                    });
                }
            }
            else
            {
                RunOnUiThread(() =>
                {
                    _loadPb.Visibility = ViewStates.Invisible;

                    Toast.MakeText(this, "Отсутствует подключение к сети. Повторите попытку заново.",
                                         ToastLength.Long).Show();
                });
            }
        }
    }
}
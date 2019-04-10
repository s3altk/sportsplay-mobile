using System;
using System.Threading;
using System.IO;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Support.V7.App;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.Net;

using DataModel;
using Newtonsoft.Json;
using AlertDialog = Android.App.AlertDialog;

namespace Mobile
{
    [Activity(Theme = "@style/no_action_bar_light_theme")]
    public class SettingsActivity : AppCompatActivity
    {
        private TextView _userNameText;
        private TextView _passwordText;
        private EditText _passwordEdit;
        private Button _saveBtn;
        private Button _logoutBtn;
        private ImageButton _backBtn;
        private ProgressBar _loadPb;
        private TableLayout _tbLayout;

        private User _user;

        private bool _remember;
        private string _username;
        private string _serviceUri;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_settings);
            RequestedOrientation = ScreenOrientation.Portrait;

            _userNameText = FindViewById<TextView>(Resource.Id.txtCurUser3);
            _passwordText = FindViewById<TextView>(Resource.Id.txtPassword3);
            _passwordEdit = FindViewById<EditText>(Resource.Id.editPassword3);
            _saveBtn = FindViewById<Button>(Resource.Id.btnSave3);
            _logoutBtn = FindViewById<Button>(Resource.Id.btnLogout3);
            _backBtn = FindViewById<ImageButton>(Resource.Id.btnBack3);
            _loadPb = FindViewById<ProgressBar>(Resource.Id.pbLoad3);
            _tbLayout = FindViewById<TableLayout>(Resource.Id.tbLayout3);

            _backBtn.Click += (sender, args) =>
            {
                Finish();
            };

            _username = Intent.GetStringExtra("Username");
            _remember = Intent.GetBooleanExtra("Remember", false);
            _serviceUri = ConfigReader.GetWebServiceUri(Assets.Open("config.xml"));

            _loadPb.Visibility = ViewStates.Visible;
            _tbLayout.Visibility = ViewStates.Invisible;

            var thread = new Thread(LoadData);
            thread.IsBackground = true;
            thread.Start();
        }

        private void LoadData()
        {
            var connectionManager = (ConnectivityManager)GetSystemService(Context.ConnectivityService);
            var connectionInfo = connectionManager.ActiveNetworkInfo;

            if (connectionInfo != null && connectionInfo.IsConnected == true)
            {
                string userString = string.Format("{0}/User/GetByName?name={1}", _serviceUri, _username);
                _user = JsonConvert.DeserializeObject<User>(HttpClient.Get(userString));

                RunOnUiThread(() =>
                {
                    _loadPb.Visibility = ViewStates.Invisible;
                    _tbLayout.Visibility = ViewStates.Visible;

                    if (_user == null)
                    {
                        _userNameText.Text = "Гость";

                        _passwordText.SetTextSize(ComplexUnitType.Sp, 15f);
                        _passwordText.SetTypeface(Typeface.Default, TypefaceStyle.Normal);

                        _passwordText.Visibility = ViewStates.Invisible;
                        _passwordEdit.Visibility = ViewStates.Invisible;

                        _saveBtn.Visibility = ViewStates.Invisible;
                        _logoutBtn.Text = "Авторизоваться";
                    }
                    else
                    {
                        _userNameText.Text = _user.Name;
                    }

                    #region События

                    _saveBtn.Click += (sender, args) =>
                    {
                        connectionManager = (ConnectivityManager)GetSystemService(Context.ConnectivityService);
                        connectionInfo = connectionManager.ActiveNetworkInfo;

                        if (connectionInfo != null && connectionInfo.IsConnected == true)
                        {
                            if (_passwordEdit.Text != string.Empty && _passwordEdit.Text != _user.Password)
                            {
                                string password = _passwordEdit.Text.Trim();

                                if (PasswordChecker.IsPasswordCorrect(password))
                                {
                                    _user.Password = password;

                                    string putString = string.Format("{0}/User/Edit", _serviceUri);
                                    string json = JsonConvert.SerializeObject(_user);
                                    HttpClient.Put(json, putString);

                                    var pref = Application.Context.GetSharedPreferences("UserInfo", FileCreationMode.Private);

                                    var edit = pref.Edit();
                                    edit.Clear();

                                    if (_remember)
                                    {
                                        edit.PutString("Username", _user.Name);
                                        edit.PutString("Password", _user.Password);
                                    }

                                    edit.Apply();

                                    Toast.MakeText(this, "Пароль успешно изменен.", ToastLength.Short).Show();
                                }
                                else
                                {
                                    var dialog = new AlertDialog.Builder(this);

                                    dialog.SetTitle("Некорректный пароль");
                                    dialog.SetMessage("Пароль должен быть составлен из латинских букв. Длина пароля должна быть "
                                                    + "больше 5 букв. Пароль должен иметь одну букву из верхнего регистра, "
                                                    + "нижнего регистра, одну цифру и один символ.");
                                    dialog.SetPositiveButton("ОК", delegate { dialog.Dispose(); });
                                    dialog.Show();
                                }
                            }
                            else if (_passwordEdit.Text == _user.Password)
                            {
                                Toast.MakeText(this, "Новый пароль совпадает с текущим. Введите пароль заново.",
                                                     ToastLength.Short).Show();
                            }
                            else
                            {
                                Toast.MakeText(this, "Новый пароль не может быть пустым. Введите пароль заново.",
                                                     ToastLength.Short).Show();
                            }
                        }
                        else
                        {
                            Toast.MakeText(this,
                                    "Отсутствует подключение к сети. Повторите попытку заново.",
                                    ToastLength.Long).Show();
                        }
                    };

                    _logoutBtn.Click += (sender, args) =>
                    {
                        var dialog = new AlertDialog.Builder(this);

                        if (_logoutBtn.Text == "Авторизоваться")
                        {
                            var intent = new Intent(this, typeof(LoginActivity));
                            StartActivity(intent);
                            Finish();
                        }
                        else
                        {
                            dialog.SetTitle("Смена пользователя");
                            dialog.SetMessage("Вы действительно хотите сменить пользователя?");
                            dialog.SetNegativeButton("Да", delegate
                            {
                                var pref = Application.Context.GetSharedPreferences("UserInfo", FileCreationMode.Private);

                                var edit = pref.Edit();
                                edit.Clear();
                                edit.Apply();

                                var intent = new Intent(this, typeof(LoginActivity));
                                StartActivity(intent);
                                Finish();
                            });
                            dialog.SetPositiveButton("Нет", delegate { dialog.Dispose(); });
                            dialog.Show();
                        }
                    };

                    #endregion
                });
            }
            else
            {
                RunOnUiThread(() =>
                {
                    _loadPb.Visibility = ViewStates.Invisible;

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
using System;
using System.Collections.Generic;
using System.Threading;
using System.IO;

using Android.OS;
using Android.Views;
using Android.Widget;
using Android.App;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Net;

using Newtonsoft.Json;
using DataModel;
using SupportFragment = Android.Support.V4.App.Fragment;
using Activity = Android.App.Activity;

namespace Mobile
{
    public class AboutMeetFragment : SupportFragment
    {
        private TextView _addressText;
        private TextView _curMeetText;
        private TextView _nameText;
        private TextView _dateText;
        private TextView _meetsText;
        private Button _moreBtn;
        private Button _addBtn;
        private ProgressBar _loadPb;
        private LinearLayout _tbLayout;
        private RecyclerView _recyclerMeets;
        private MeetsAdapter _adapterMeets;
        private UniversalList<Meet> _meetsUI;

        private Meet _curMeet;
        private List<Meet> _meets;
        private Playground _playground;
        private User _user;
        private View _view;

        private string _username;
        private string _playgroundId;
        private string _serviceUri;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            var pref = Application.Context.GetSharedPreferences("UserInfo", FileCreationMode.Private);

            _username = pref.GetString("Username", string.Empty);
            _playgroundId = pref.GetString("PlaygroundId", string.Empty);
            _serviceUri = ConfigReader.GetWebServiceUri(Activity.Assets.Open("config.xml"));
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            _view = inflater.Inflate(Resource.Layout.fragment_about_meet, container, false);

            _nameText = _view.FindViewById<TextView>(Resource.Id.txtName5);
            _moreBtn = _view.FindViewById<Button>(Resource.Id.btnMore5);
            _addBtn = _view.FindViewById<Button>(Resource.Id.btnAdd5);
            _loadPb = _view.FindViewById<ProgressBar>(Resource.Id.pbLoad5);
            _tbLayout = _view.FindViewById<LinearLayout>(Resource.Id.tbtLayout5);
            _dateText = _view.FindViewById<TextView>(Resource.Id.txtDate5);
            _meetsText = _view.FindViewById<TextView>(Resource.Id.txtMeets5);
            _recyclerMeets = _view.FindViewById<RecyclerView>(Resource.Id.rvMeets5);
            _addressText = _view.FindViewById<TextView>(Resource.Id.txtAddress5);
            _curMeetText = _view.FindViewById<TextView>(Resource.Id.txtCurMeet5);

            _moreBtn.Click += _moreBtn_Click;
            _addBtn.Click += _addBtn_Click;

            _loadPb.Visibility = ViewStates.Visible;
            _tbLayout.Visibility = ViewStates.Invisible;
            _addBtn.Visibility = ViewStates.Invisible;

            var thread = new Thread(LoadData);
            thread.IsBackground = true;
            thread.Start();

            return _view;
        }

        private void UpdateUI()
        {
            _addressText.Text = _playground.Address;

            if (_meets.Count != 0)
            {
                _curMeetText.Text = "Ближайшая встреча";

                _meets.Sort((a, b) => a.Date.CompareTo(b.Date));

                _curMeet = _meets[0];

                var datetime = string.Format("{0} {1}",
                                _curMeet.Date.ToShortDateString(),
                                _curMeet.Date.ToShortTimeString());

                _nameText.Text = _curMeet.Name;
                _dateText.Text = datetime;

                _meetsUI = new UniversalList<Meet>();

                foreach (var item in _meets)
                {
                    if (item != null && item.Id != _curMeet.Id)
                    {
                        _meetsUI.Add(item, _meetsUI.Count);
                    }
                }

                _meetsText.Text = _meetsUI.Count == 0 ? "Нет больше встреч" : "Остальные встречи";

                _adapterMeets = new MeetsAdapter(_meetsUI, MeetsType.Viewed, _username, Activity, _view);
                _meetsUI.Adapter = _adapterMeets;

                _recyclerMeets.SetLayoutManager(new LinearLayoutManager(Activity));
                _recyclerMeets.SetAdapter(_adapterMeets);

                _tbLayout.Visibility = ViewStates.Visible;
            }
            else
            {
                _curMeetText.Text = "Нет встреч на площадке";
            }
        }

        private void LoadData()
        {
            var connectionManager = (ConnectivityManager)Activity.GetSystemService(Context.ConnectivityService);
            var connectionInfo = connectionManager.ActiveNetworkInfo;

            if (connectionInfo != null && connectionInfo.IsConnected == true)
            {
                string meetString = string.Format("{0}/Meet/GetByPlayground?playgroundId={1}", _serviceUri, _playgroundId);
                string pgString = string.Format("{0}/Playground/Get/{1}", _serviceUri, _playgroundId);
                string userString = string.Format("{0}/User/GetByName?name={1}", _serviceUri, _username);

                _meets = JsonConvert.DeserializeObject<List<Meet>>(HttpClient.Get(meetString));
                _playground = JsonConvert.DeserializeObject<Playground>(HttpClient.Get(pgString));
                _user = JsonConvert.DeserializeObject<User>(HttpClient.Get(userString));

                Activity.RunOnUiThread(() =>
                {
                    _loadPb.Visibility = ViewStates.Invisible;

                    if (_user == null)
                    {
                        _addBtn.Visibility = ViewStates.Invisible;
                    }
                    else
                    {
                        _addBtn.Visibility = ViewStates.Visible;
                    }

                    UpdateUI();   
                });
            }
            else
            {
                Activity.RunOnUiThread(() =>
                {
                    Toast.MakeText(Activity, "Отсутствует подключение к сети.", ToastLength.Short).Show();
                });
            }
        }

        void _addBtn_Click(object sender, EventArgs e)
        {
            var view = Activity.LayoutInflater.Inflate(Resource.Layout.fragment_add_meet, null);

            var name = view.FindViewById<EditText>(Resource.Id.edName4);
            var date = view.FindViewById<EditText>(Resource.Id.edDate4);
            var time = view.FindViewById<EditText>(Resource.Id.edTime4);

            var builder = new AlertDialog.Builder(Activity);

            builder.SetView(view);
            builder.SetCancelable(false);
            builder.SetNegativeButton("Готово", delegate { });
            builder.SetPositiveButton("Отмена", delegate { });

            var dialog = builder.Create();
            dialog.Show();

            var btnReady = dialog.GetButton((int)DialogButtonType.Negative);
            var btnCancel = dialog.GetButton((int)DialogButtonType.Positive);

            #region События

            btnCancel.Click += (o, args) =>
            {
                dialog.Dismiss();
            };

            btnReady.Click += (o, args) =>
            {
                var connectionManager = (ConnectivityManager)Activity.GetSystemService(Context.ConnectivityService);
                var connectionInfo = connectionManager.ActiveNetworkInfo;

                if (connectionInfo != null && connectionInfo.IsConnected == true)
                {
                    if (name.Text != string.Empty && date.Text != string.Empty && time.Text != string.Empty)
                    {
                        try
                        {
                            var datetime = DateTime.Parse(date.Text + " " + time.Text);

                            if (datetime > DateTime.Now)
                            {
                                var meet = new Meet()
                                {
                                    Id = Guid.NewGuid(),
                                    Name = name.Text,
                                    Date = datetime,
                                    FounderId = _user.Id,
                                    PlaygroundId = _playground.Id
                                };

                                string nmJSON = JsonConvert.SerializeObject(meet);
                                string nmString = string.Format("{0}/Meet/Add", _serviceUri);
                                HttpClient.Post(nmJSON, nmString);

                                _meets.Add(meet);

                                UpdateUI();

                                dialog.Dismiss();

                                Toast.MakeText(Activity, "Встреча успешно создана.", ToastLength.Short).Show();
                            }
                            else
                            {
                                Toast.MakeText(Activity, "Дата должна быть не раньше сегодняшней.", ToastLength.Short).Show();
                            }
                        }
                        catch
                        {
                            Toast.MakeText(Activity, "Введены некорректные данные.", ToastLength.Short).Show();
                        }                     
                    }
                    else
                    {
                        Toast.MakeText(Activity, "Не все поля заполнены.", ToastLength.Short).Show();
                    }
                }
                else
                {
                    Toast.MakeText(Activity, "Отсутствует подключение к сети.", ToastLength.Short).Show();
                }
            };

            #endregion
        }

        private void _moreBtn_Click(object sender, EventArgs e)
        {
            var intent = new Intent(Activity, typeof(AboutMeetActivity));

            intent.PutExtra("MeetId", _curMeet.Id.ToString());
            intent.PutExtra("Username", _username);

            StartActivity(intent);
        }
    }
}
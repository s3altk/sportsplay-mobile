using System;
using System.Collections.Generic;
using System.Threading;
using System.IO;

using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Widget;
using Android.Content;
using Android.Views;
using Android.Net;
using Android.Graphics;
using Android.Support.V4.App;

using DataModel;
using Newtonsoft.Json;
using AlertDialog = Android.Support.V7.App.AlertDialog;

namespace Mobile
{
    [Activity(Theme = "@style/no_action_bar_light_theme")]
    public class AboutMeetActivity : AppCompatActivity
    {
        private TextView _nameText;
        private TextView _addressText;
        private TextView _dateText;
        private TextView _partakersText;
        private ImageButton _backBtn;
        private Button _choiceBtn;
        private ProgressBar _loadPb;
        private TableLayout _tbLayout;
        private RecyclerView _partakersView;
        private RecyclerView.Adapter _partakersAdapter;
        private UniversalList<User> _partakersUI;

        private Meet _meet;
        private User _user;
        private Playground _playground;
        private List<User> _partakers;

        private string _meetId;
        private string _username;
        private string _serviceUri;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_about_meet);
            RequestedOrientation = ScreenOrientation.Portrait;

            _nameText = FindViewById<TextView>(Resource.Id.txtName4);
            _addressText = FindViewById<TextView>(Resource.Id.txtAddress4);
            _dateText = FindViewById<TextView>(Resource.Id.txtDate4);
            _partakersText = FindViewById<TextView>(Resource.Id.txtPartakers4);
            _choiceBtn = FindViewById<Button>(Resource.Id.btnChoice4);
            _tbLayout = FindViewById<TableLayout>(Resource.Id.tbLayout4);
            _loadPb = FindViewById<ProgressBar>(Resource.Id.pbLoad4);
            _backBtn = FindViewById<ImageButton>(Resource.Id.btnBack4);
            _partakersView = FindViewById<RecyclerView>(Resource.Id.rvPartakers4);

            _choiceBtn.Click += _choiceBtn_Click;
            _backBtn.Click += (sender, args) =>
            {
                Finish();
            };
            
            _meetId = Intent.GetStringExtra("MeetId");
            _username = Intent.GetStringExtra("Username");
            _serviceUri = ConfigReader.GetWebServiceUri(Assets.Open("config.xml"));

            _tbLayout.Visibility = ViewStates.Invisible;
            _loadPb.Visibility = ViewStates.Visible;

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
                string meetString = string.Format("{0}/Meet/Get/{1}", _serviceUri, _meetId);
                string userString = string.Format("{0}/User/GetByName?name={1}", _serviceUri, _username);

                _meet = JsonConvert.DeserializeObject<Meet>(HttpClient.Get(meetString));
                _user = JsonConvert.DeserializeObject<User>(HttpClient.Get(userString));

                string pgString = string.Format("{0}/Playground/Get/{1}", _serviceUri, _meet.PlaygroundId);
                _playground = JsonConvert.DeserializeObject<Playground>(HttpClient.Get(pgString));

                _partakers = new List<User>();
                _partakers.AddRange(_meet.Partakers);

                _partakersUI = new UniversalList<User>();

                foreach (var partaker in _partakers)
                {
                    if (partaker != null)
                    {
                        _partakersUI.Add(partaker, _partakersUI.Count);
                    }
                }

                RunOnUiThread(() =>
                {
                    _tbLayout.Visibility = ViewStates.Visible;
                    _loadPb.Visibility = ViewStates.Invisible;

                    _nameText.Text = _meet.Name;
                    _addressText.Text = _meet.Playground.Address;
                    _dateText.Text = string.Format("{0} {1}", _meet.Date.ToShortDateString(), _meet.Date.ToShortTimeString());

                    _partakersText.Text = _partakersUI.Count == 0 ? "Нет участников" : "Список участников";

                    if (_user != null)
                    {
                        if (_partakers.Exists(p => p.Id == _user.Id))
                        {
                            _choiceBtn.Text = "Не участвовать";
                            _choiceBtn.SetTextColor(Color.Red);
                        }
                        else
                        {
                            _choiceBtn.Text = "Участвовать";
                            _choiceBtn.SetTextColor(Color.Teal);
                        }
                    }
                    else
                    {
                        _choiceBtn.Visibility = ViewStates.Invisible;
                    }

                    _partakersAdapter = new UsersAdapter(_partakersUI, this);
                    _partakersUI.Adapter = _partakersAdapter;
                    _partakersView.SetLayoutManager(new LinearLayoutManager(this));
                    _partakersView.SetAdapter(_partakersAdapter);
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

        private void _choiceBtn_Click(object sender, EventArgs e)
        {
            var connectionManager = (ConnectivityManager)GetSystemService(Context.ConnectivityService);
            var connectionInfo = connectionManager.ActiveNetworkInfo;

            if (connectionInfo != null && connectionInfo.IsConnected == true)
            {
                if (_choiceBtn.Text == "Не участвовать")
                {
                    var index = _partakers.FindIndex(p => p.Id == _user.Id);

                    _partakers.RemoveAt(index);
                    _partakersUI.RemoveAt(index);

                    if (_partakersUI.Count == 0)
                    {
                        _partakersText.Text = "Нет участников";
                    }

                    string deleteString = string.Format("{0}/Meet/DeletePartaker?meetId={1}&userId={2}", _serviceUri, _meetId, _user.Id);
                    HttpClient.Delete(deleteString);

                    _choiceBtn.Text = "Участвовать";
                    _choiceBtn.SetTextColor(Color.Teal);

                    Toast.MakeText(this, "Вы отказались от участия во встрече.", ToastLength.Short).Show();
                }
                else
                {
                    _partakers.Add(_user);
                    _partakersUI.Add(_user, _partakersUI.Count);

                    if (_partakersUI.Count > 0)
                    {
                        _partakersText.Text = "Список участников";
                    }

                    var match = new Match()
                    {
                        MeetId = Guid.Parse(_meetId),
                        UserId = _user.Id
                    };

                    string matchString = string.Format("{0}/Meet/AddPartaker", _serviceUri);
                    string matchJSON = JsonConvert.SerializeObject(match);

                    HttpClient.Post(matchJSON, matchString);

                    _choiceBtn.Text = "Не участвовать";
                    _choiceBtn.SetTextColor(Color.Red);

                    Toast.MakeText(this, "Вы приняли участие во встрече.", ToastLength.Short).Show();
                }
            }
            else
            {
                Toast.MakeText(this,
                        "Отсутствует подключение к сети. Повторите попытку заново.",
                        ToastLength.Long).Show();
            }
        }
    }

    public class UsersAdapter : RecyclerView.Adapter
    {
        private UniversalList<User> _users;
        private FragmentActivity _activity;

        private class UserHolder : RecyclerView.ViewHolder
        {
            private View _thisView;

            public TextView UsernameText { get; set; }

            public UserHolder(View view)
                : base(view)
            {
                _thisView = view;
            }
        }

        public UsersAdapter(UniversalList<User> users, FragmentActivity activity)
        {
            _users = users;
            _activity = activity;
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.fragment_user, parent, false);

            var username = view.FindViewById<TextView>(Resource.Id.txtUsername1);

            var holder = new UserHolder(view)
            {
                UsernameText = username,
            };

            return holder;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            var holder = viewHolder as UserHolder;

            if (holder != null)
            {
                holder.UsernameText.Text = _users[position].Name;
            }
        }

        public override int ItemCount
        {
            get { return _users.Count; }
        }
    }
}
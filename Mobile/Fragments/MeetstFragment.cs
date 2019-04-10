using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.IO;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Android.Net;
using Android.Graphics;
using Android.Support.Design.Widget;

using Newtonsoft.Json;
using DataModel;
using SupportFragment = Android.Support.V4.App.Fragment;

namespace Mobile
{
    public interface IUpdateableFragment
    {
        void OnUpdate();
    }

    public class MeetstFragment : SupportFragment, IUpdateableFragment
    {
        private TextView _userMeetsText;
        private TextView _otherMeetsText;
        private RecyclerView _recyclerUserMeets;
        private RecyclerView _recyclerOtherMeets;
        private RecyclerView.Adapter _adapterUserMeets;
        private RecyclerView.Adapter _adapterOtherMeets;
        private UniversalList<Meet> _userMeetsList;
        private UniversalList<Meet> _otherMeetsList;
        private View _view;
        private Snackbar _snackbar;

        private string _username;
        private string _serviceUri;
        private bool _canUpdate = false;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            var pref = Application.Context.GetSharedPreferences("UserInfo", FileCreationMode.Private);

            _username = pref.GetString("Username", string.Empty);
            _serviceUri = ConfigReader.GetWebServiceUri(Activity.Assets.Open("config.xml"));

            _userMeetsList = new UniversalList<Meet>();
            _otherMeetsList = new UniversalList<Meet>();
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            _view = inflater.Inflate(Resource.Layout.fragment_meets, container, false);

            _userMeetsText = _view.FindViewById<TextView>(Resource.Id.txtUserMeets1);
            _otherMeetsText = _view.FindViewById<TextView>(Resource.Id.txtOtherMeets1);
            _recyclerUserMeets = _view.FindViewById<RecyclerView>(Resource.Id.recyclerView1);
            _recyclerOtherMeets = _view.FindViewById<RecyclerView>(Resource.Id.recyclerView2);

            var thread = new Thread(GroupMeetsByType);
            thread.IsBackground = true;
            thread.Start();

            return _view;
        }

        private void GroupMeetsByType()
        {
            var connectionManager = (ConnectivityManager)Activity.GetSystemService(Context.ConnectivityService);
            var connectionInfo = connectionManager.ActiveNetworkInfo;

            if (connectionInfo != null && connectionInfo.IsConnected == true)
            {
                string userString = string.Format("{0}/User/GetByName?name={1}", _serviceUri, _username);
                string meetString = string.Format("{0}/Meet/GetAll", _serviceUri);

                var user = JsonConvert.DeserializeObject<User>(HttpClient.Get(userString));
                var meets = JsonConvert.DeserializeObject<List<Meet>>(HttpClient.Get(meetString));

                Activity.RunOnUiThread(() =>
                {
                    _userMeetsList.Clear();
                    _otherMeetsList.Clear();

                    if (user != null)
                    {
                        _userMeetsList.AddRange(meets.Where(p => p.Founder.Id == user.Id));
                        _otherMeetsList.AddRange(meets.Where(p => p.Founder.Id != user.Id));
                    }
                    else
                    {
                        _otherMeetsList.AddRange(meets);
                    }

                    if (_userMeetsList.Count == 0 && _otherMeetsList.Count != 0)
                    {
                        _userMeetsText.Text = "Нет ваших встреч";
                    }
                    else if (_userMeetsList.Count != 0 && _otherMeetsList.Count == 0)
                    {
                        _otherMeetsText.Text = string.Empty;
                    }
                    else if (_userMeetsList.Count == 0 && _otherMeetsList.Count == 0)
                    {
                        _userMeetsText.Text = "Нет встреч на текущий момент";
                        _otherMeetsText.Text = string.Empty;
                    }

                    if (_userMeetsList.Count != 0)
                    {
                        _userMeetsText.Text = "Ваши встречи";
                    }

                    if (_otherMeetsList.Count != 0)
                    {
                        _otherMeetsText.Text = "Остальные встречи";
                    }

                    _adapterUserMeets = new MeetsAdapter(_userMeetsList, MeetsType.UserCreated, _username, Activity, _view);
                    _userMeetsList.Adapter = _adapterUserMeets;
                    _recyclerUserMeets.SetLayoutManager(new LinearLayoutManager(Activity));
                    _recyclerUserMeets.SetAdapter(_adapterUserMeets);

                    _adapterOtherMeets = new MeetsAdapter(_otherMeetsList, MeetsType.Viewed, _username, Activity, _view);
                    _otherMeetsList.Adapter = _adapterOtherMeets;
                    _recyclerOtherMeets.SetLayoutManager(new LinearLayoutManager(Activity));
                    _recyclerOtherMeets.SetAdapter(_adapterOtherMeets);

                    if (_canUpdate)
                    {
                        _snackbar.Dismiss();
                        _canUpdate = false;
                    }
                }); 
            }
            else
            {
                Activity.RunOnUiThread(() =>
                {
                    _snackbar.SetText("Отсутствует подключение к сети.");
                });
            }
        }

        public void OnUpdate()
        {
            _canUpdate = true;

            _snackbar = Snackbar.Make(_view, "Обновление...", Snackbar.LengthLong);
            _snackbar.Show();

            var thread = new Thread(GroupMeetsByType);
            thread.IsBackground = true;
            thread.Start();
        }
    }

    public enum MeetsType
    {
        UserCreated,
        Viewed
    }

    public class MeetsAdapter : RecyclerView.Adapter
    {
        private TextView _userMeetsTitle;
        private TextView _otherMeetsTitle;
        private FragmentActivity _activity;
        private UniversalList<Meet> _meets;

        private MeetsType _meetsType;
        private string _username;
        private string _serviceUri;

        private class MeetHolder : RecyclerView.ViewHolder
        {
            public View View;
            public TextView NameText { get; set; }
            public TextView DateText { get; set; }
            public ImageButton EditButton { get; set; }
            public ImageButton DelButton { get; set; }

            public MeetHolder(View view) : base(view)
            {
                View = view;
            }
        }

        public MeetsAdapter(UniversalList<Meet> meets, MeetsType type, string username, FragmentActivity activity, View parent)
        {
            _meets = meets;
            _meetsType = type;
            _username = username;
            _activity = activity;

            _serviceUri = ConfigReader.GetWebServiceUri(activity.Assets.Open("config.xml"));

            _userMeetsTitle = parent.FindViewById<TextView>(Resource.Id.txtUserMeets1);
            _otherMeetsTitle = parent.FindViewById<TextView>(Resource.Id.txtOtherMeets1);
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.fragment_meet, parent, false);

            var nameText = view.FindViewById<TextView>(Resource.Id.txtName1);
            var dateText = view.FindViewById<TextView>(Resource.Id.txtDate1);

            var editBtn = view.FindViewById<ImageButton>(Resource.Id.btnEdit1);
            var delBtn = view.FindViewById<ImageButton>(Resource.Id.btnDelete1);

            if (_meetsType == MeetsType.UserCreated)
            {
                editBtn.Visibility = ViewStates.Visible;
                delBtn.Visibility = ViewStates.Visible;
            }
            else
            {
                editBtn.Visibility = ViewStates.Invisible;
                delBtn.Visibility = ViewStates.Invisible;
            }

            var holder = new MeetHolder(view)
            {
                NameText = nameText,
                DateText = dateText,
                EditButton = editBtn,
                DelButton = delBtn
            };

            #region События

            view.Click += delegate
            {
                var intent = new Intent(_activity, typeof(AboutMeetActivity));

                intent.PutExtra("MeetId", _meets[holder.AdapterPosition].Id.ToString());
                intent.PutExtra("Username", _username);

                _activity.StartActivity(intent);
            };

            delBtn.Click += delegate
            {
                var connectionManager = (ConnectivityManager)_activity.GetSystemService(Context.ConnectivityService);
                var connectionInfo = connectionManager.ActiveNetworkInfo;

                if (connectionInfo != null && connectionInfo.IsConnected == true)
                {
                    var position = holder.AdapterPosition;

                    if (position >= 0 && position < _meets.Count)
                    {
                        var curMeet = _meets[position];
                        _meets.RemoveAt(position);

                        if (_meets.Count == 0)
                        {
                            _userMeetsTitle.Text = "Нет ваших встреч";
                        }

                        var delString = string.Format("{0}/Meet/Delete/{1}", _serviceUri, curMeet.Id);
                        HttpClient.Delete(delString);

                        Toast.MakeText(_activity, "Встреча успешно удалена.", ToastLength.Short).Show();
                    }
                }
                else
                {
                    Toast.MakeText(_activity, "Отсутствует подключение к сети.", ToastLength.Short).Show();
                }
            };

            editBtn.Click += delegate
            {
                var position = holder.AdapterPosition;

                if (position >= 0 && position < _meets.Count)
                {
                    var form = _activity.LayoutInflater.Inflate(Resource.Layout.fragment_add_meet, null);

                    var name = form.FindViewById<EditText>(Resource.Id.edName4);
                    var date = form.FindViewById<EditText>(Resource.Id.edDate4);
                    var time = form.FindViewById<EditText>(Resource.Id.edTime4);

                    name.Text = _meets[position].Name;
                    date.Text = _meets[position].Date.ToShortDateString();
                    time.Text = _meets[position].Date.ToShortTimeString();

                    var builder = new AlertDialog.Builder(_activity);

                    builder.SetView(form);
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
                        var connectionManager = (ConnectivityManager)_activity.GetSystemService(Context.ConnectivityService);
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
                                        _meets[position].Name = name.Text;
                                        _meets[position].Date = datetime;
                                        _meets.Edit(_meets[position], position);

                                        string nmJSON = JsonConvert.SerializeObject(_meets[position]);
                                        string nmString = string.Format("{0}/Meet/Edit", _serviceUri);
                                        HttpClient.Put(nmJSON, nmString);

                                        dialog.Dismiss();

                                        Toast.MakeText(_activity, "Встреча успешно отредактирована.", ToastLength.Short).Show();
                                    }
                                    else
                                    {
                                        Toast.MakeText(_activity, "Дата должна быть не раньше сегодняшней.", ToastLength.Short).Show();
                                    }
                                }
                                catch
                                {
                                    Toast.MakeText(_activity, "Введены некорректные данные.", ToastLength.Short).Show();
                                }
                            }
                            else
                            {
                                Toast.MakeText(_activity, "Не все поля заполнены.", ToastLength.Short).Show();
                            }
                        }
                        else
                        {
                            Toast.MakeText(_activity, "Отсутствует подключение к сети.", ToastLength.Short).Show();
                        }
                    };

                    #endregion
                }
            };

            #endregion

            return holder;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            var holder = viewHolder as MeetHolder;

            if (holder != null)
            {
                string datetime = string.Format("{0} {1}",
                    _meets[position].Date.ToShortDateString(),
                    _meets[position].Date.ToShortTimeString());

                holder.NameText.Text = _meets[position].Name;
                holder.DateText.Text = datetime;
            }
        }

        public override int ItemCount
        {
            get { return _meets.Count; }
        }
    }
}
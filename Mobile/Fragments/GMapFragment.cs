using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.IO;

using Android.App;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Locations;
using Android.OS;
using Android.Content;
using Android.Provider;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Views;
using Android.Widget;
using Android.Net;

using Newtonsoft.Json;
using DataModel;
using SupportFragment = Android.Support.V4.App.Fragment;

namespace Mobile
{
    public class GMapFragment : SupportFragment, 
                                IOnMapReadyCallback, 
                                ILocationListener, 
                                View.IOnTouchListener, 
                                GoogleMap.IOnMapClickListener,
                                IUpdateableFragment
    {
        private MapView _mapView;
        private GoogleMap _map;
        private LocationManager _locationManager;
        private Location _currentLocation;
        private FrameLayout _aboutMeetLayout;
        private FloatingActionButton _findUserLocBtn;
        private FloatingActionButton _addMeetBtn;
        private MarkerOptions _mrkUserLocation;
        private LatLng _addMeetLatLng;
        private LatLng _mapPosition;
        private Marker _curDrugMarker;
        private View _view;
        private Snackbar _snackbar;

        private List<Playground> _playgrounds;
        private string _serviceUri;
        private string _username;
        private float _lastPosY;
        private string _provider;
        private bool _addMeetRepeat = true;
        private bool _canAddMeet = true;
        private bool _canUpdate = false;

        private float StartTranslationY;

        #region Методы фрагмента

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            var pref = Application.Context.GetSharedPreferences("UserInfo", FileCreationMode.Private);

            _username = pref.GetString("Username", string.Empty);
            _serviceUri = ConfigReader.GetWebServiceUri(Activity.Assets.Open("config.xml"));
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            _view = inflater.Inflate(Resource.Layout.fragment_gmap, container, false);

            _mapView = _view.FindViewById<MapView>(Resource.Id.mapView1);
            _aboutMeetLayout = _view.FindViewById<FrameLayout>(Resource.Id.aboutMeetLayout1);
            _findUserLocBtn = _view.FindViewById<FloatingActionButton>(Resource.Id.btnCurLoc1);
            _addMeetBtn = _view.FindViewById<FloatingActionButton>(Resource.Id.btnAddMeet1);

            _addMeetLatLng = new LatLng(0, 0);

            var height = Activity.ApplicationContext.Resources.DisplayMetrics.HeightPixels;
            var density = Activity.ApplicationContext.Resources.DisplayMetrics.Density;

            StartTranslationY = (float)height / density * 3;

            _aboutMeetLayout.TranslationY = StartTranslationY;

            if (_username == "GUEST")
            {
                _addMeetBtn.Visibility = ViewStates.Invisible;
            }

            InitializeMap(savedInstanceState);
            InitializeLocationManager();

            #region События

            _addMeetBtn.Click += (sender, args) =>
            {
                if (_canAddMeet)
                {
                    _canAddMeet = false;

                    var marker = new MarkerOptions();

                    marker.SetIcon(BitmapDescriptorFactory.DefaultMarker(BitmapDescriptorFactory.HueRose));
                    marker.SetTitle("Новая встреча");
                    marker.Draggable(true);

                    if (_currentLocation == null)
                    {
                        marker.SetPosition(new LatLng(56.486092, 84.970783));
                    }
                    else
                    {
                        marker.SetPosition(new LatLng(_currentLocation.Latitude, _currentLocation.Longitude));
                    }

                    if (_addMeetRepeat)
                    {
                        _addMeetRepeat = false;

                        var dialog = new AlertDialog.Builder(Activity);

                        dialog.SetTitle("Внимание");
                        dialog.SetMessage("Нажмите и удерживайте маркер для его перемещения. " +
                                          "После этого выберите место и отпустите маркер.");
                        dialog.SetPositiveButton("ОК", delegate
                        {
                            _map.AddMarker(marker);
                            _map.AnimateCamera(CameraUpdateFactory.NewLatLngZoom(marker.Position, 10.25f));
                        });
                        dialog.Show();
                    }
                    else
                    {
                        _map.AddMarker(marker);
                        _map.AnimateCamera(CameraUpdateFactory.NewLatLngZoom(marker.Position, 14.25f));
                    }
                }
                else
                {
                    ShowWarningsByAddMeet();
                }
            };

            _findUserLocBtn.Click += (sender, args) =>
            {
                if (_canAddMeet)
                {
                    var gpsEnabled = _locationManager.IsProviderEnabled(LocationManager.GpsProvider);

                    if (!gpsEnabled)
                    {
                        ShowGpsWarnings();
                    }
                    else
                    {
                        GetCurrentLocation();

                        if (_currentLocation == null)
                        {
                            Toast.MakeText(Activity,
                                    "Не удалось определить ваше местоположение. Повторите попытку позже.",
                                    ToastLength.Short).Show();
                        }
                        else
                        {
                            OnUpdate();
                        }
                    }
                }
                else
                {
                    ShowWarningsByAddMeet();
                }
            };

            #endregion

            return _view;
        }

        public override void OnResume()
        {
            base.OnResume();

            if (_provider != null && _locationManager.IsProviderEnabled(_provider))
            {
                _locationManager.RequestLocationUpdates(_provider, 2000, 1, this);
            }
        }

        public override void OnPause()
        {
            base.OnPause();
            _locationManager.RemoveUpdates(this);
        }

        public bool OnTouch(View v, MotionEvent e)
        {
            switch (e.Action)
            {
                case MotionEventActions.Down:

                    _lastPosY = e.GetY();
                    return true;

                case MotionEventActions.Move:

                    var currentPos = e.GetY();
                    var deltaY = _lastPosY - currentPos;

                    var transY = v.TranslationY;
                    transY -= deltaY;

                    if (transY < 0)
                    {
                        transY = 0;
                    }

                    v.TranslationY = transY;

                    return true;

                default:
                    return v.OnTouchEvent(e);
            }
        }

        public void OnUpdate()
        {
            if (_canAddMeet)
            {
                _canUpdate = true;

                _snackbar = Snackbar.Make(_view, "Обновление...", Snackbar.LengthLong);
                _snackbar.Show();

                var thread = new Thread(PlaceMarkersOnMap);
                thread.IsBackground = true;
                thread.Start();
            }
        }

        #endregion

        #region Методы карты

        public void OnLocationChanged(Location location)
        {
            _currentLocation = location;
        }

        public void OnProviderDisabled(string provider)
        {
        }

        public void OnProviderEnabled(string provider)
        {
        }

        public void OnStatusChanged(string provider, [GeneratedEnum] Availability status, Bundle extras)
        {
        }

        public void OnMapClick(LatLng point)
        {
            var interpolator = new Android.Views.Animations.AccelerateInterpolator();

            var delta = StartTranslationY - _aboutMeetLayout.TranslationY;

            _aboutMeetLayout.Animate()
                            .SetInterpolator(interpolator)
                            .TranslationYBy(delta)
                            .SetDuration(550);
        }

        public void OnMapReady(GoogleMap googleMap)
        {
            _map = googleMap;
            _map.MapType = GoogleMap.MapTypeNormal;
            _map.UiSettings.MyLocationButtonEnabled = false;
            _map.UiSettings.ZoomControlsEnabled = false;

            var cityPosition = new LatLng(56.486092, 84.970783);
            var cameraUpdate = CameraUpdateFactory.NewLatLngZoom(cityPosition, 10.25f);

            _map.MarkerDrag += MapOnMarkerDrag;
            _map.MarkerDragEnd += MapOnMarkerDragEnd;
            _map.MarkerClick += MapOnMarkerClick;

            _aboutMeetLayout.SetOnTouchListener(this);

            _map.SetOnMapClickListener(this);
            _map.MoveCamera(cameraUpdate);

            _snackbar = Snackbar.Make(_view, "Обновление...", Snackbar.LengthIndefinite);
            _snackbar.Show();

            var thread = new Thread(PlaceMarkersOnMap);
            thread.IsBackground = true;
            thread.Start();
        }

        private void MapOnMarkerClick(object sender, GoogleMap.MarkerClickEventArgs args)
        {
            if (args.Marker.Title == "Вы здесь" || args.Marker.Title == "Новая встреча")
            {
                args.Marker.ShowInfoWindow();
            }
            else
            {
                var connectionManager = (ConnectivityManager)Activity.GetSystemService(Context.ConnectivityService);
                var connectionInfo = connectionManager.ActiveNetworkInfo;

                if (connectionInfo != null && connectionInfo.IsConnected == true)
                {
                    if (_canAddMeet)
                    {
                        var playgroundId = args.Marker.Title;

                        var pref = Application.Context.GetSharedPreferences("UserInfo", FileCreationMode.Private);

                        var edit = pref.Edit();
                        edit.PutString("Username", _username);
                        edit.PutString("PlaygroundId", playgroundId);
                        edit.Apply();

                        var transaction = FragmentManager.BeginTransaction();
                        transaction.Add(_aboutMeetLayout.Id, new AboutMeetFragment());
                        transaction.Commit();

                        _mapPosition = args.Marker.Position;
                        _map.AnimateCamera(CameraUpdateFactory.NewLatLngZoom(_mapPosition, 14.25f));

                        if (_aboutMeetLayout.TranslationY + 2 >= _aboutMeetLayout.Height)
                        {
                            var interpolator = new Android.Views.Animations.AccelerateInterpolator();

                            _aboutMeetLayout.TranslationY = StartTranslationY;

                            _aboutMeetLayout.Animate()
                                            .SetInterpolator(interpolator)
                                            .TranslationYBy(-550)
                                            .SetDuration(500);
                        }
                    }
                    else
                    {
                        ShowWarningsByAddMeet();
                    }
                }
                else
                {
                    _snackbar.SetText("Отсутствует подключение к сети.");
                    _snackbar.SetDuration(Snackbar.LengthIndefinite);
                    _snackbar.SetAction("ОК", delegate
                    {
                        _snackbar.Dismiss();
                    });
                    _snackbar.Show();
                }
            }
        }

        private void MapOnMarkerDragEnd(object sender, GoogleMap.MarkerDragEndEventArgs eventArgs)
        {
            _curDrugMarker = eventArgs.Marker;

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
                var bld = new AlertDialog.Builder(Activity);

                bld.SetTitle("Внимание");
                bld.SetMessage("Продолжить выбор местоположения для маркера новой площадки?");
                bld.SetPositiveButton("Нет", delegate
                {
                    _canAddMeet = true;
                    bld.Dispose();
                    dialog.Dismiss();

                    if (_playgrounds != null)
                    {
                        _map.Clear();

                        foreach (var playground in _playgrounds)
                        {
                            var marker = new MarkerOptions();

                            marker.SetIcon(BitmapDescriptorFactory.DefaultMarker(BitmapDescriptorFactory.HueYellow));
                            marker.SetPosition(new LatLng(playground.LocationX, playground.LocationY));
                            marker.SetTitle(playground.Id.ToString());

                            _map.AddMarker(marker);
                        }
                    }
                    
                    if (_currentLocation != null)
                    {
                        var lat = _currentLocation.Latitude;
                        var lng = _currentLocation.Longitude;

                        _mrkUserLocation = new MarkerOptions();
                        _mrkUserLocation.SetIcon(BitmapDescriptorFactory.DefaultMarker(BitmapDescriptorFactory.HueBlue));
                        _mrkUserLocation.SetPosition(new LatLng(lat, lng));
                        _mrkUserLocation.SetTitle("Вы здесь");

                        _map.AddMarker(_mrkUserLocation);
                    }
                });
                bld.SetNegativeButton("Да", delegate
                {
                    _canAddMeet = false;
                    dialog.Dismiss();
                });
                bld.Show();
            };

            btnReady.Click += async (o, args) =>
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
                                _curDrugMarker.Draggable = false;

                                var addressTask = await GetAddressTaskByLocation(_curDrugMarker.Position.Latitude, _curDrugMarker.Position.Longitude);
                                var address = GetFormatedAddress(addressTask);

                                string userString = string.Format("{0}/User/GetByName?name={1}", _serviceUri, _username);
                                var user = JsonConvert.DeserializeObject<User>(HttpClient.Get(userString));

                                var playground = new Playground()
                                {
                                    Id = Guid.NewGuid(),
                                    Address = address,
                                    LocationX = _curDrugMarker.Position.Latitude,
                                    LocationY = _curDrugMarker.Position.Longitude
                                };

                                string pgJSON = JsonConvert.SerializeObject(playground);

                                var meet = new Meet()
                                {
                                    Id = Guid.NewGuid(),
                                    Name = name.Text,
                                    Date = datetime,
                                    FounderId = user.Id,
                                    PlaygroundId = playground.Id
                                };

                                string meetJSON = JsonConvert.SerializeObject(meet);

                                var match = new Match()
                                {
                                    MeetId = meet.Id,
                                    UserId = user.Id
                                };

                                string matchJSON = JsonConvert.SerializeObject(match);

                                string pgString = string.Format("{0}/Playground/Add", _serviceUri);
                                string meetString = string.Format("{0}/Meet/Add", _serviceUri);
                                string matchString = string.Format("{0}/Meet/AddPartaker", _serviceUri);

                                HttpClient.Post(pgJSON, pgString);
                                HttpClient.Post(meetJSON, meetString);
                                HttpClient.Post(matchJSON, matchString);

                                _curDrugMarker.Title = playground.Id.ToString();
                                _canAddMeet = true;

                                OnUpdate();

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

        private void MapOnMarkerDrag(object sender, GoogleMap.MarkerDragEventArgs eventArgs)
        {
            if (eventArgs.Marker.Title != "Новая встреча") return;

            _addMeetLatLng.Latitude = eventArgs.Marker.Position.Latitude;
            _addMeetLatLng.Longitude = eventArgs.Marker.Position.Longitude;
        }

        private async Task<Address> GetAddressTaskByCurrentLocation()
        {
            var geocoder = new Geocoder(Activity);

            var addressList = await geocoder.GetFromLocationAsync(_currentLocation.Latitude, _currentLocation.Longitude, 10);
            var address = addressList.FirstOrDefault();

            return address;
        }

        private async Task<Address> GetAddressTaskByLocation(double locationX, double locationY)
        {
            var geocoder = new Geocoder(Activity);

            var addressList = await geocoder.GetFromLocationAsync(locationX, locationY, 10);
            var address = addressList.FirstOrDefault();

            return address;
        }

        private void PlaceMarkersOnMap()
        {
            if (_map != null)
            {
                var connectionManager = (ConnectivityManager)Activity.GetSystemService(Context.ConnectivityService);
                var connectionInfo = connectionManager.ActiveNetworkInfo;

                if (connectionInfo != null && connectionInfo.IsConnected == true)
                {
                    string pgString = string.Format("{0}/Playground/GetAll", _serviceUri);
                    _playgrounds = JsonConvert.DeserializeObject<List<Playground>>(HttpClient.Get(pgString));

                    Activity.RunOnUiThread(async () =>
                    {
                        _map.Clear();

                        foreach (var playground in _playgrounds)
                        {
                            var marker = new MarkerOptions();

                            marker.SetIcon(BitmapDescriptorFactory.DefaultMarker(BitmapDescriptorFactory.HueYellow));
                            marker.SetPosition(new LatLng(playground.LocationX, playground.LocationY));
                            marker.SetTitle(playground.Id.ToString());

                            _map.AddMarker(marker);
                        }

                        if (_currentLocation != null)
                        {
                            var lat = _currentLocation.Latitude;
                            var lng = _currentLocation.Longitude;

                            var addressTask = await GetAddressTaskByLocation(lat, lng);
                            var address = GetFormatedAddress(addressTask);

                            _mrkUserLocation = new MarkerOptions();
                            _mrkUserLocation.SetIcon(BitmapDescriptorFactory.DefaultMarker(BitmapDescriptorFactory.HueBlue));
                            _mrkUserLocation.SetPosition(new LatLng(lat, lng));
                            _mrkUserLocation.SetTitle("Вы здесь");
                            _mrkUserLocation.SetSnippet(address);

                            _map.AddMarker(_mrkUserLocation);
                        }

                        if (_canUpdate)
                        {
                            _snackbar.Dismiss();
                            _canUpdate = false;
                        }
                        else
                        {
                            _snackbar.SetText("Обновление завершено.");
                            _snackbar.SetAction("ОК", delegate
                            {
                                _snackbar.Dismiss();
                            });
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
        }

        private void GetCurrentLocation()
        {
            var criteria = new Criteria()
            {
                Accuracy = Accuracy.Fine
            };

            _provider = _locationManager.GetBestProvider(criteria, true);

            if (_provider != null && _locationManager.IsProviderEnabled(_provider))
            {
                _locationManager.RequestLocationUpdates(_provider, 2000, 1, this);

                _currentLocation = _locationManager.GetLastKnownLocation(_provider);
            }
        }

        private string GetFormatedAddress(Address address)
        {
            if (address != null)
            {
                return string.Format("{0}, {1}", address.Thoroughfare, address.SubThoroughfare);
            }

            return string.Empty;
        }

        #endregion

        private void InitializeMap(Bundle savedInstanceState)
        {
            MapsInitializer.Initialize(Activity);

            _mapView.OnCreate(savedInstanceState);
            _mapView.OnResume();
            _mapView.GetMapAsync(this);
        }

        private void InitializeLocationManager()
        {
            _locationManager = (LocationManager)Activity.GetSystemService(Context.LocationService);
        }

        private void ShowGpsWarnings()
        {
            var dialog = new AlertDialog.Builder(Activity);

            dialog.SetTitle("Внимание");
            dialog.SetMessage("Определение местоположения не включено. " +
                              "Для этого перейдите в настройки.");
            dialog.SetPositiveButton("ОК", delegate { dialog.Dispose(); });
            dialog.SetNegativeButton("Настройки", delegate
            {
                var intent = new Intent(Settings.ActionLocationSourceSettings);
                StartActivity(intent);
            });
            dialog.Show();
        }

        private void ShowWarningsByAddMeet()
        {
            var dialog = new AlertDialog.Builder(Activity);

            dialog.SetTitle("Внимание");
            dialog.SetMessage("На карте находится маркер новой площадки. " +
                              "Переместите маркер на новое место и завершите создание новой площадки.");
            dialog.SetPositiveButton("ОК", delegate { dialog.Dispose(); });
            dialog.Show();
        }

        private void ShowAddressToast(Address address)
        {
            if (address != null)
            {
                var formatAddress = new System.Text.StringBuilder();

                for (var i = 0; i < address.MaxAddressLineIndex; i++)
                {
                    formatAddress.AppendLine(address.GetAddressLine(i));
                }

                Toast.MakeText(Activity, formatAddress.ToString(), ToastLength.Long).Show();
            }
            else
            {
                Toast.MakeText(Activity, "Не удалось определить ваш адрес. Повторите попытку позже.", ToastLength.Short).Show();
            }
        }
    }
}
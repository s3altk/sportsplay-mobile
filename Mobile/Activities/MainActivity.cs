using System.Collections.Generic;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Support.V4.View;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;

using Java.Lang;
using SupportFragment = Android.Support.V4.App.Fragment;
using SupportFragmentManager = Android.Support.V4.App.FragmentManager;
using AlertDialog = Android.App.AlertDialog;

namespace Mobile
{
    [Activity(Theme = "@style/action_bar_light_theme")]
    public class MainActivity : AppCompatActivity, TabLayout.IOnTabSelectedListener
    {
        private TabLayout _tabLayout;
        private ViewPager _viewPager;
        private TabAdapter _tabAdapter;

        private string _username;
        private bool _remember;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);
            RequestedOrientation = ScreenOrientation.Portrait;

            _tabLayout = FindViewById<TabLayout>(Resource.Id.tabLayout1);
            _viewPager = FindViewById<ViewPager>(Resource.Id.viewPager1);

            var pref = Application.Context.GetSharedPreferences("UserInfo", FileCreationMode.Private);

            _username = pref.GetString("Username", string.Empty);
            _remember = pref.GetBoolean("Remember", false);

            _tabAdapter = new TabAdapter(SupportFragmentManager);
            _tabAdapter.AddFragment(new GMapFragment(), "Карта");
            _tabAdapter.AddFragment(new MeetstFragment(), "Встречи");

            _viewPager.Adapter = _tabAdapter;

            _tabLayout.SetupWithViewPager(_viewPager);
            _tabLayout.SetOnTabSelectedListener(this);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            menu.Add(0, 101, Menu.None, "Настройки");
            menu.Add(0, 102, Menu.None, "Выход");

            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case 101:

                    var intent = new Intent(this, typeof(SettingsActivity));

                    intent.PutExtra("Username", _username);
                    intent.PutExtra("Remember", _remember);
                    StartActivity(intent);  

                    return true;

                case 102:

                    var dialog = new AlertDialog.Builder(this);

                    dialog.SetTitle("Выход");
                    dialog.SetMessage("Вы действительно хотите закрыть приложение?");
                    dialog.SetNegativeButton("Да", delegate { FinishAffinity(); });
                    dialog.SetPositiveButton("Нет", delegate { dialog.Dispose(); });
                    dialog.Show();

                    return true;

                default:

                    return base.OnOptionsItemSelected(item);
            }
        }

        public void OnTabReselected(TabLayout.Tab tab)
        {
        }

        public void OnTabSelected(TabLayout.Tab tab)
        {
            if (tab.Position == 0)
            {
                var f = _tabAdapter.GetItem(tab.Position) as GMapFragment;
                f.OnUpdate();

                _viewPager.SetCurrentItem(tab.Position, true);
            }
            else
            {
                var f = _tabAdapter.GetItem(tab.Position) as MeetstFragment;
                f.OnUpdate();

                _viewPager.SetCurrentItem(tab.Position, true);
            }
        }

        public void OnTabUnselected(TabLayout.Tab tab)
        {          
        }
    }

    public class TabAdapter : FragmentPagerAdapter
    {
        private readonly List<SupportFragment> _fragments;
        private readonly List<string> _fragmentNames;

        public TabAdapter(SupportFragmentManager manager) : base(manager)
        {
            _fragments = new List<SupportFragment>();
            _fragmentNames = new List<string>();
        }

        public void AddFragment(SupportFragment fragment, string name)
        {
            _fragments.Add(fragment);
            _fragmentNames.Add(name);
        }

        public override int Count
        {
            get { return _fragments.Count; }
        }

        public override SupportFragment GetItem(int position)
        {
            return _fragments[position];
        }

        public override ICharSequence GetPageTitleFormatted(int position)
        {
            return new String(_fragmentNames[position]);
        }
    }
}




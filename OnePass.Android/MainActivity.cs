using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Google.Android.Material.FloatingActionButton;
using System;

namespace OnePass.Android
{
    [Activity(Theme = "@style/AppTheme")]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            // var username = Intent.GetStringExtra("Username")
            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetActionBar(toolbar);

            // Add floating action button
            var addFab = FindViewById<FloatingActionButton>(Resource.Id.add_fab);
            addFab.Click += AddFab_Click;

            // Recycler View
            var productAdapter = new ProductAdapter();
            productAdapter.ItemClick += ProductAdapter_ItemClick;

            var recyclerView = FindViewById<RecyclerView>(Resource.Id.recycler_view);
            recyclerView.SetLayoutManager(new LinearLayoutManager(this));
            recyclerView.SetAdapter(productAdapter);
            recyclerView.AddItemDecoration(new DividerItemDecoration(this, DividerItemDecoration.Vertical));
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.top_menus, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            Toast.MakeText(this, "Action selected: " + item.TitleFormatted, ToastLength.Short).Show();
            return base.OnOptionsItemSelected(item);
        }

        private void ProductAdapter_ItemClick(object sender, int position)
        {
            //var dialog = new AndroidX.AppCompat.App.AlertDialog.Builder(this);
            //dialog.SetTitle("Alert");
            //dialog.SetMessage($"Product {position}");
            //dialog.SetPositiveButton("OK", (sender, args) => { });

            //var alert = dialog.Create();
            //alert.Show();

            var intent = new Intent(this, typeof(AccountEditActivity));
            StartActivity(intent);
        }

        private void AddFab_Click(object sender, EventArgs e)
        {
            var intent = new Intent(this, typeof(AccountCreateActivity));
            StartActivity(intent);
        }
    }
}

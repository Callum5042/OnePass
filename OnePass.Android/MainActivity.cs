using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Google.Android.Material.FloatingActionButton;
using OnePass.Models;
using OnePass.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace OnePass.Droid
{
    [Activity(Theme = "@style/AppTheme")]
    public class MainActivity : Activity
    {
        private ProductAdapter ProductAdapter { get; set; }

        protected override async void OnCreate(Bundle savedInstanceState)
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
            var list = await Accounts();

            ProductAdapter = new ProductAdapter(list);
            ProductAdapter.ItemClick += ProductAdapter_ItemClick;

            var recyclerView = FindViewById<RecyclerView>(Resource.Id.recycler_view);
            recyclerView.SetLayoutManager(new LinearLayoutManager(this));
            recyclerView.SetAdapter(ProductAdapter);
            recyclerView.AddItemDecoration(new DividerItemDecoration(this, DividerItemDecoration.Vertical));
        }

        private async Task<IList<Account>> Accounts()
        {
            var name = "Callum";
            var password = "SUPER";

            var encryptor = new FileEncryptor();

            var documentsPath = GetExternalFilesDir(Android.OS.Environment.DirectoryDocuments).AbsolutePath;
            var filename = $"{name}.bin";
            var path = Path.Combine(documentsPath, filename);

            using var input = File.OpenRead(path);
            using var output = new MemoryStream();
            await encryptor.DecryptAsync(input, output, password);

            output.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(output);
            var jsonOutput = await reader.ReadToEndAsync();

            var accounts = JsonSerializer.Deserialize<IList<Account>>(jsonOutput);
            return accounts;
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

            int id = ProductAdapter.Accounts[position].Id;
            
            var intent = new Intent(this, typeof(AccountEditActivity));
            intent.PutExtra("Id", id);
            StartActivity(intent);
        }

        private void AddFab_Click(object sender, EventArgs e)
        {
            var intent = new Intent(this, typeof(AccountCreateActivity));
            StartActivity(intent);
        }

        protected override async void OnRestart()
        {
            base.OnRestart();

            var list = await Accounts();
            ProductAdapter.Accounts = list;
            ProductAdapter.NotifyDataSetChanged();
        }
    }
}

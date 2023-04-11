using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Google.Android.Material.FloatingActionButton;
using OnePass.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Path = System.IO.Path;

namespace OnePass.Droid.Activities
{
    [Activity(Theme = "@style/AppTheme")]
    public class MainActivity : Activity
    {
        private string Username { get; set; }

        private string Password { get; set; }

        private ProductAdapter ProductAdapter { get; set; }

        private RecyclerView RecyclerView { get; set; }

        private TextView EmptyListMessage { get; set; }

        private const int _activityResultCreated = 1;
        private const int _activityResultEdited = 2;
        private const int _activityResultSynced = 3;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            Username = Intent.GetStringExtra(nameof(Username));
            Password = Intent.GetStringExtra(nameof(Password));

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

            RecyclerView = FindViewById<RecyclerView>(Resource.Id.recycler_view);
            RecyclerView.SetLayoutManager(new LinearLayoutManager(this));
            RecyclerView.SetAdapter(ProductAdapter);
            RecyclerView.AddItemDecoration(new DividerItemDecoration(this, DividerItemDecoration.Vertical));

            EmptyListMessage = FindViewById<TextView>(Resource.Id.empty_list_message);

            TriggerComponentVisiblity(list, emptyMessage: "No results found");
        }

        private void TriggerComponentVisiblity(IList<OnePass.Models.Account> list, string emptyMessage)
        {
            if (list.Any())
            {
                EmptyListMessage.Visibility = ViewStates.Gone;
                RecyclerView.Visibility = ViewStates.Visible;
            }
            else
            {
                EmptyListMessage.Text = emptyMessage;
                EmptyListMessage.Visibility = ViewStates.Visible;
                RecyclerView.Visibility = ViewStates.Gone;
            }
        }

        private async Task<IList<OnePass.Models.Account>> Accounts()
        {
            var fileEncoder = new FileEncoder();

            var documentsPath = GetExternalFilesDir(Android.OS.Environment.DirectoryDocuments).AbsolutePath;
            var filename = $"{Username}.bin";
            var path = Path.Combine(documentsPath, filename);

            var data = await fileEncoder.LoadAsync(Username, Password, path);

            return data.Accounts
                .OrderByDescending(x => x.Favourite)
                .ThenBy(x => x.Name)
                .ToList();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.top_menus, menu);

            var item = menu.FindItem(Resource.Id.menu_search);
            var searchView = (Android.Support.V7.Widget.SearchView)item.ActionView;
            
            var id = searchView.Context.Resources.GetIdentifier("search_src_text", "id", PackageName);
            var searchEditText = searchView.FindViewById<EditText>(id);
            searchEditText.SetTextColor(Color.White);

            searchView.QueryTextChange += (s, e) =>
            {
                var filtered = ProductAdapter.OriginalAccounts.Where(x => x.Name.Contains(e.NewText, StringComparison.CurrentCultureIgnoreCase)).ToList();
                ProductAdapter.Accounts = filtered;
                ProductAdapter.NotifyDataSetChanged();

                TriggerComponentVisiblity(filtered, emptyMessage: "No results found");
            };

            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId ==  Resource.Id.menu_sync)
            {
                var intent = new Intent(this, typeof(SyncActivity));
                intent.PutExtra(nameof(Username), Username);
                intent.PutExtra(nameof(Password), Password);
                StartActivityForResult(intent, _activityResultSynced);
            }
            else
            {
                Toast.MakeText(this, "Action selected: " + item.TitleFormatted, ToastLength.Short).Show();
            }

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

            var guid = ProductAdapter.Accounts[position].Guid;

            var intent = new Intent(this, typeof(AccountEditActivity));
            intent.PutExtra("Guid", guid.ToString());
            intent.PutExtra(nameof(Username), Username);
            intent.PutExtra(nameof(Password), Password);
            StartActivityForResult(intent, _activityResultEdited);
        }

        private void AddFab_Click(object sender, EventArgs e)
        {
            var intent = new Intent(this, typeof(AccountCreateActivity));
            intent.PutExtra(nameof(Username), Username);
            intent.PutExtra(nameof(Password), Password);
            StartActivityForResult(intent, _activityResultCreated);
        }

        protected override async void OnRestart()
        {
            base.OnRestart();

            var list = await Accounts();
            ProductAdapter.Accounts = list;
            ProductAdapter.NotifyDataSetChanged();

            TriggerComponentVisiblity(list, emptyMessage: "No results found");
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (resultCode == Result.Ok)
            {
                var accountName = data.GetStringExtra("AccountName");

                var message = string.Empty;
                switch (requestCode)
                {
                    case _activityResultCreated:
                        message = $"Account {accountName} created";
                        break;

                    case _activityResultEdited:
                        message = $"Account {accountName} updated";
                        break;
                }

                Toast.MakeText(this, message, ToastLength.Short).Show();
            }
        }
    }
}

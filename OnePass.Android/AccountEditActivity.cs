using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using OnePass.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OnePass.Android
{
    [Activity(Theme = "@style/AppTheme")]
    public class AccountEditActivity : Activity
    {
        private EditText _accountPasswordEditText;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_account_create);

            // Set toolbar
            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetActionBar(toolbar);
            ActionBar.Title = "Edit Account";

            // Cache
            var submitButton = FindViewById<Button>(Resource.Id.submit_account_button);
            submitButton.Text = "Update Account";

            // Generate password
            var generatePasswordButton = FindViewById<Button>(Resource.Id.generate_password_button);
            generatePasswordButton.Click += GeneratePasswordButton_Click;

            _accountPasswordEditText = FindViewById<EditText>(Resource.Id.account_password);
        }

        private void GeneratePasswordButton_Click(object sender, EventArgs e)
        {
            var generator = new PasswordGenerator1();
            _accountPasswordEditText.Text = generator.Generate();
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
    }
}
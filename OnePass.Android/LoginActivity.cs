using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace OnePass.Android
{
    [Activity(Label = "OnePass", Theme = "@style/AppTheme", MainLauncher = true)]
    public class LoginActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_login);

            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);

            // Create your application here
            var loginButton = FindViewById<Button>(Resource.Id.login_button);
            loginButton.Click += LoginButton_Click;

            SetVersionNumber();
        }

        private void LoginButton_Click(object sender, EventArgs e)
        {
            var intent = new Intent(this, typeof(MainActivity));
            StartActivity(intent);
        }

        private void SetVersionNumber()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            var versionLabel = FindViewById<TextView>(Resource.Id.login_version);
            versionLabel.Text = $"v{version.ToString(3)}";
        }
    }
}
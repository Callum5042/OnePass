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
    public class LoginActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_login);

            // Create your application here
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            var versionLabel = FindViewById<TextView>(Resource.Id.login_version);
            versionLabel.Text = $"v{version.ToString(3)}";
        }
    }
}
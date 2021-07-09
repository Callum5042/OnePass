using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using OnePass.Services;
using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;

namespace OnePass.Droid.Activities
{
    [Activity(Label = "OnePass", Theme = "@style/AppTheme", MainLauncher = true)]
    public class LoginActivity : Activity
    {
        private EditText _usernameEditText;
        private EditText _passwordEditText;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_login);

            // Create your application here
            _usernameEditText = FindViewById<EditText>(Resource.Id.login_username);
            _passwordEditText = FindViewById<EditText>(Resource.Id.login_password);
            SetVersionNumber();

            var loginButton = FindViewById<Button>(Resource.Id.login_button);
            loginButton.Click += LoginButton_Click;

            var registerButton = FindViewById<Button>(Resource.Id.register_button);
            registerButton.Click += RegisterButton_Click;
        }

        private void RegisterButton_Click(object sender, EventArgs e)
        {
            var intent = new Intent(this, typeof(RegisterActivity));
            StartActivity(intent);
        }

        private async void LoginButton_Click(object sender, EventArgs e)
        {
            var encryptor = new FileEncryptor();

            var documentsPath = GetExternalFilesDir(Android.OS.Environment.DirectoryDocuments).AbsolutePath;
            var filename = $"{_usernameEditText.Text}.bin";
            var path = Path.Combine(documentsPath, filename);

            // Check if file exists
            if (!File.Exists(path))
            {
                Toast.MakeText(this, "Invalid username", ToastLength.Short).Show();
                return;
            }

            // Attempt to decrypt file with key
            try
            {
                using var file = File.OpenRead(path);
                using var memory = new MemoryStream();
                await encryptor.DecryptAsync(file, memory, _passwordEditText.Text);

                // Success
                var intent = new Intent(this, typeof(MainActivity));
                intent.PutExtra("Username", _usernameEditText.Text);
                intent.PutExtra("Password", _passwordEditText.Text);
                StartActivity(intent);
            }
            catch (CryptographicException)
            {
                Toast.MakeText(this, "Invalid password", ToastLength.Short).Show();
            }
        }

        private void SetVersionNumber()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            var versionLabel = FindViewById<TextView>(Resource.Id.login_version);
            versionLabel.Text = $"v{version.ToString(3)}";
        }
    }
}
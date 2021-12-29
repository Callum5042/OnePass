using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using OnePass.Droid.Models;
using OnePass.Services;
using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;

namespace OnePass.Droid.Activities
{
    [Activity(Label = "OnePass", Theme = "@style/AppTheme.Header", MainLauncher = true)]
    public class LoginActivity : Activity
    {
        private EditText _usernameEditText;
        private EditText _passwordEditText;
        private TextView _usernameValidationTextView;
        private TextView _passwordValidationTextView;
        private CheckBox _remember_usernameCheckbox;

        private const int activityResult = 1;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_login);

            // Create your application here
            _usernameEditText = FindViewById<EditText>(Resource.Id.login_username);
            _passwordEditText = FindViewById<EditText>(Resource.Id.login_password);
            _usernameValidationTextView = FindViewById<TextView>(Resource.Id.login_username_validation_message);
            _passwordValidationTextView = FindViewById<TextView>(Resource.Id.login_password_validation_message);
            _remember_usernameCheckbox = FindViewById<CheckBox>(Resource.Id.remember_username);
            SetVersionNumber();

            var loginButton = FindViewById<Button>(Resource.Id.login_button);
            loginButton.Click += LoginButton_Click;

            var registerButton = FindViewById<Button>(Resource.Id.register_button);
            registerButton.Click += RegisterButton_Click;

            // Read appsettings to see if remember username has a value
            var documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
            var filename = "appsettings.json";
            var path = Path.Combine(documentsPath, filename);

            if (File.Exists(path))
            {
                // Read appsettings
                using var file = File.OpenRead(path);
                using var reader = new StreamReader(file);
                var json = await reader.ReadToEndAsync();
                var options = JsonSerializer.Deserialize<AppOptions>(json);

                if (!string.IsNullOrEmpty(options.RememberUsername))
                {
                    _usernameEditText.Text = options.RememberUsername;
                    _remember_usernameCheckbox.Checked = true;
                }
            }
        }

        private void RegisterButton_Click(object sender, EventArgs e)
        {
            var intent = new Intent(this, typeof(RegisterActivity));
            StartActivityForResult(intent, activityResult);
        }

        private async void LoginButton_Click(object sender, EventArgs e)
        {
            var encryptor = new FileEncryptor();

            var documentsPath = GetExternalFilesDir(Android.OS.Environment.DirectoryDocuments).AbsolutePath;
            var filename = $"{_usernameEditText.Text}.bin";
            var path = Path.Combine(documentsPath, filename);

            _usernameValidationTextView.Visibility = ViewStates.Gone;
            _passwordValidationTextView.Visibility = ViewStates.Gone;

            // Validate fields
            var isValid = ValidateFields();
            if (!isValid)
            {
                return;
            }

            // Check if file exists
            if (!File.Exists(path))
            {
                _usernameValidationTextView.Text = "Invalid username";
                _usernameValidationTextView.Visibility = ViewStates.Visible;

                Toast.MakeText(this, "Invalid username", ToastLength.Short).Show();
                return;
            }

            // Attempt to decrypt file with key
            try
            {
                using var file = File.OpenRead(path);
                using var memory = new MemoryStream();
                await encryptor.DecryptAsync(file, memory, _passwordEditText.Text);

                // Write remember username to filesystem
                if (_remember_usernameCheckbox.Checked)
                {
                    SetRememberedUsername(_usernameEditText.Text);
                }
                else
                {
                    SetRememberedUsername(null);
                }

                // Success
                var intent = new Intent(this, typeof(MainActivity));
                intent.PutExtra("Username", _usernameEditText.Text);
                intent.PutExtra("Password", _passwordEditText.Text);
                StartActivity(intent);
            }
            catch (CryptographicException)
            {
                _passwordValidationTextView.Text = "Invalid password";
                _passwordValidationTextView.Visibility = ViewStates.Visible;

                Toast.MakeText(this, "Invalid password", ToastLength.Short).Show();
            }
        }

        private async void SetRememberedUsername(string username)
        {
            var documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
            var filename = "appsettings.json";
            var path = Path.Combine(documentsPath, filename);

            // Appsettings
            var option = new AppOptions
            {
                RememberUsername = username
            };

            var json = JsonSerializer.Serialize(option);

            // Read file
            using var file = File.Open(path, FileMode.OpenOrCreate, FileAccess.Write);
            file.SetLength(0);

            using var writer = new StreamWriter(file);
            await writer.WriteAsync(json);
        }

        private bool ValidateFields()
        {
            bool isValid = true;
            if (string.IsNullOrEmpty(_usernameEditText.Text))
            {
                isValid = false;
                _usernameValidationTextView.Text = "Username is required";
                _usernameValidationTextView.Visibility = ViewStates.Visible;
            }

            if (string.IsNullOrEmpty(_passwordEditText.Text))
            {
                isValid = false;
                _passwordValidationTextView.Text = "Password is required";
                _passwordValidationTextView.Visibility = ViewStates.Visible;
            }

            return isValid;
        }

        private void SetVersionNumber()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            var versionLabel = FindViewById<TextView>(Resource.Id.login_version);
            versionLabel.Text = $"v{version.ToString(3)}";
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (requestCode == activityResult)
            {
                if (resultCode == Result.Ok)
                {
                    var profileName = data.GetStringExtra("ProfileName");
                    Toast.MakeText(this, $"Profile {profileName} created", ToastLength.Short).Show();
                }
            }
        }
    }
}
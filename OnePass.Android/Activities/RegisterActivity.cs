using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using OnePass.Models;
using OnePass.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace OnePass.Droid.Activities
{
    [Activity(Theme = "@style/AppTheme")]
    public class RegisterActivity : Activity
    {
        private EditText _username;
        private EditText _password;
        private EditText _repeatPassword;

        private TextView _usernameTextView;
        private TextView _passwordTextView;
        private TextView _passwordRepeatTextView;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_register);

            // Cache controls
            _username = FindViewById<EditText>(Resource.Id.login_username);
            _password = FindViewById<EditText>(Resource.Id.login_password);
            _repeatPassword = FindViewById<EditText>(Resource.Id.login_password_repeat);

            _usernameTextView = FindViewById<TextView>(Resource.Id.login_username_validation_message);
            _passwordTextView = FindViewById<TextView>(Resource.Id.login_password_validation_message);
            _passwordRepeatTextView = FindViewById<TextView>(Resource.Id.login_password_repeat_validation_message);

            // Button
            var registerButton = FindViewById<Button>(Resource.Id.register_button);
            registerButton.Click += RegisterButton_Click;
        }

        private async void RegisterButton_Click(object sender, EventArgs e)
        {
            var encryptor = new FileEncryptor();

            var isValid = Validate();
            if (!isValid)
            {
                return;
            }

            // Create initial encrypted file
            var documentsPath = GetExternalFilesDir(Android.OS.Environment.DirectoryDocuments).AbsolutePath;
            var filename = $"{_username.Text}.bin";
            var path = Path.Combine(documentsPath, filename);

            if (File.Exists(path))
            {
                _usernameTextView.Text = "Username already exists";
                _usernameTextView.Visibility = ViewStates.Visible;

                Toast.MakeText(this, "Username already exists", ToastLength.Short).Show();
                return;
            }

            var json = JsonSerializer.Serialize(new List<Account>());
            var buffer = Encoding.UTF8.GetBytes(json);
            using var memory = new MemoryStream(buffer);
            using var file = File.OpenWrite(path);

            await encryptor.EncryptAsync(memory, file, _password.Text);

            // Finish
            Finish();
        }

        private bool Validate()
        {
            _usernameTextView.Visibility = ViewStates.Gone;
            _passwordTextView.Visibility = ViewStates.Gone;
            _passwordRepeatTextView.Visibility = ViewStates.Gone;

            var isValid = true;
            if (string.IsNullOrEmpty(_username.Text))
            {
                isValid = false;
                _usernameTextView.Text = "Username is required";
                _usernameTextView.Visibility = ViewStates.Visible;
            }

            if (string.IsNullOrEmpty(_password.Text))
            {
                isValid = false;
                _passwordTextView.Text = "Password is required";
                _passwordTextView.Visibility = ViewStates.Visible;
            }

            if (string.IsNullOrEmpty(_repeatPassword.Text))
            {
                isValid = false;
                _passwordRepeatTextView.Text = "Repeat Password is required";
                _passwordRepeatTextView.Visibility = ViewStates.Visible;
            }

            if (_password.Text != _repeatPassword.Text)
            {
                isValid = false;
                _passwordRepeatTextView.Text = "Passwords do not match";
                _passwordRepeatTextView.Visibility = ViewStates.Visible;
            }

            return isValid;
        }
    }
}
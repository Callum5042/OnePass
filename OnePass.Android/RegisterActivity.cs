using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using OnePass.Models;
using OnePass.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace OnePass.Droid
{
    [Activity(Theme = "@style/AppTheme")]
    public class RegisterActivity : Activity
    {
        private EditText _username;
        private EditText _password;
        private EditText _repeatPassword;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_register);

            // Cache controls
            _username = FindViewById<EditText>(Resource.Id.login_username);
            _password = FindViewById<EditText>(Resource.Id.login_password);
            _repeatPassword = FindViewById<EditText>(Resource.Id.login_password_repeat);

            // Button
            var registerButton = FindViewById<Button>(Resource.Id.register_button);
            registerButton.Click += RegisterButton_Click;
        }

        private async void RegisterButton_Click(object sender, EventArgs e)
        {
            var encryptor = new FileEncryptor();

            // Create initial encrypted file
            var json = JsonSerializer.Serialize(new AccountRoot());

            var documentsPath = GetExternalFilesDir(Android.OS.Environment.DirectoryDocuments).AbsolutePath;
            var filename = $"{_username.Text}.bin";

            var path = Path.Combine(documentsPath, filename);
            if (File.Exists(path))
            {
                using var input = File.OpenRead(path);
                using var output = new MemoryStream();
                await encryptor.DecryptAsync(input, output, _password.Text);

                output.Seek(0, SeekOrigin.Begin);
                using var reader = new StreamReader(output);
                var jsonOutput = await reader.ReadToEndAsync();
            }

            var buffer = Encoding.UTF8.GetBytes(json);
            using var memory = new MemoryStream(buffer);
            using var file = File.OpenWrite(path);

            await encryptor.EncryptAsync(memory, file, _password.Text);


            // Finish
            Finish();
        }
    }
}
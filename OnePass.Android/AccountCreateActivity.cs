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
    public class AccountCreateActivity : Activity
    {
        private EditText _accountUsernameEditText;
        private EditText _accountPasswordEditText;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_account_create);

            // Set toolbar
            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetActionBar(toolbar);
            ActionBar.Title = "Add Account";

            // Add account
            var submitButton = FindViewById<Button>(Resource.Id.submit_account_button);
            submitButton.Click += SubmitButton_Click;

            // Generate password
            var generatePasswordButton = FindViewById<Button>(Resource.Id.generate_password_button);
            generatePasswordButton.Click += GeneratePasswordButton_Click;

            _accountUsernameEditText = FindViewById<EditText>(Resource.Id.account_name);
            _accountPasswordEditText = FindViewById<EditText>(Resource.Id.account_password);
        }

        private async void SubmitButton_Click(object sender, EventArgs e)
        {
            var name = "Callum";
            var password = "SUPER";

            // File
            var documentsPath = GetExternalFilesDir(Android.OS.Environment.DirectoryDocuments).AbsolutePath;
            var filename = $"{name}.bin";
            var path = Path.Combine(documentsPath, filename);

            var encryptor = new FileEncryptor();

            // Decrypt file
            using var input = File.OpenRead(path);
            using var output = new MemoryStream();
            await encryptor.DecryptAsync(input, output, password);

            output.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(output);
            var jsonOutput = await reader.ReadToEndAsync();

            var accounts = JsonSerializer.Deserialize<ICollection<Account>>(jsonOutput);

            // Add data
            accounts.Add(new Account()
            {
                Username = _accountUsernameEditText.Text,
                Password = _accountPasswordEditText.Text
            });

            // Encrypt file
            var json = JsonSerializer.Serialize(accounts);
            var buffer = Encoding.UTF8.GetBytes(json);
            using var memory = new MemoryStream(buffer);
            using var file = File.OpenWrite(path);

            await encryptor.EncryptAsync(memory, file, password);

            // Finish
            Finish();
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
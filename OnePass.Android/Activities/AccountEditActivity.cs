using Android.App;
using Android.Content;
using Android.OS;
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

namespace OnePass.Droid.Activities
{
    [Activity(Theme = "@style/AppTheme")]
    public class AccountEditActivity : Activity
    {
        private int _accountId;
        private EditText _accountNameEditText;
        private EditText _accountLoginEditText;
        private EditText _accountPasswordEditText;

        private string Username { get; set; }

        private string Password { get; set; }

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_account_create);

            Username = Intent.GetStringExtra(nameof(Username));
            Password = Intent.GetStringExtra(nameof(Password));

            // Cache controls
            _accountNameEditText = FindViewById<EditText>(Resource.Id.account_name);
            _accountLoginEditText = FindViewById<EditText>(Resource.Id.account_login);
            _accountPasswordEditText = FindViewById<EditText>(Resource.Id.account_password);

            // Set toolbar
            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetActionBar(toolbar);
            ActionBar.Title = "Edit Account";

            // Submit button
            var submitButton = FindViewById<Button>(Resource.Id.submit_account_button);
            submitButton.Text = "Update Account";
            submitButton.Click += SubmitButton_Click;

            // Generate password
            var generatePasswordButton = FindViewById<Button>(Resource.Id.generate_password_button);
            generatePasswordButton.Click += GeneratePasswordButton_Click;

            // Populate data
            _accountId = Intent.GetIntExtra("Id", -1);

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
            var account = accounts.FirstOrDefault(x => x.Id == _accountId);

            _accountNameEditText.Text = account.Name;
            _accountLoginEditText.Text = account.Login;
            _accountPasswordEditText.Text = account.Password;
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

            var accounts = JsonSerializer.Deserialize<IList<Account>>(jsonOutput);

            // Add data
            var account = accounts.FirstOrDefault(x => x.Id == _accountId);
            account.Login = _accountNameEditText.Text;
            account.Name = _accountLoginEditText.Text;
            account.Password = _accountPasswordEditText.Text;

            // Encrypt file 
            var json = JsonSerializer.Serialize(accounts);
            var buffer = Encoding.UTF8.GetBytes(json);
            using var memory = new MemoryStream(buffer);
            using var file = File.OpenWrite(path);
            file.SetLength(0);
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
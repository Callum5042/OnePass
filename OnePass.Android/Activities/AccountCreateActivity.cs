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
using System.Text;
using System.Text.Json;

namespace OnePass.Droid.Activities
{
    [Activity(Theme = "@style/AppTheme")]
    public class AccountCreateActivity : Activity
    {
        private EditText _accountNameEditText;
        private EditText _accountLoginEditText;
        private EditText _accountPasswordEditText;

        private TextView _accountNameTextView;
        private TextView _accountLoginTextView;
        private TextView _accountPasswordTextView;

        private string Username { get; set; }

        private string Password { get; set; }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_account_create);

            Username = Intent.GetStringExtra(nameof(Username));
            Password = Intent.GetStringExtra(nameof(Password));

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

            _accountNameEditText = FindViewById<EditText>(Resource.Id.account_name);
            _accountLoginEditText = FindViewById<EditText>(Resource.Id.account_login);
            _accountPasswordEditText = FindViewById<EditText>(Resource.Id.account_password);

            _accountNameTextView = FindViewById<TextView>(Resource.Id.name_validation_message);
            _accountLoginTextView = FindViewById<TextView>(Resource.Id.login_validation_message);
            _accountPasswordTextView = FindViewById<TextView>(Resource.Id.password_validation_message);
        }

        private async void SubmitButton_Click(object sender, EventArgs e)
        {
            //var isValid = Validate();
            //if (!isValid)
            //{
            //    return;
            //}

            //// File
            //var documentsPath = GetExternalFilesDir(Android.OS.Environment.DirectoryDocuments).AbsolutePath;
            //var filename = $"{Username}.bin";
            //var path = Path.Combine(documentsPath, filename);

            //var encryptor = new FileEncryptor();

            //// Decrypt file
            //using var input = File.OpenRead(path);
            //using var output = new MemoryStream();
            //await encryptor.DecryptAsync(input, output, Password);

            //output.Seek(0, SeekOrigin.Begin);
            //using var reader = new StreamReader(output);
            //var jsonOutput = await reader.ReadToEndAsync();

            //var accounts = JsonSerializer.Deserialize<IList<Account>>(jsonOutput);

            //// Add data
            //accounts.Add(new Account()
            //{
            //    Name = _accountNameEditText.Text,
            //    Login = _accountLoginEditText.Text,
            //    Password = _accountPasswordEditText.Text,
            //    DateCreated = DateTime.Now,
            //    DateModified = DateTime.Now
            //});

            //for (int i = 0; i < accounts.Count; i++)
            //{
            //    accounts[i].Id = i;
            //}

            //// Encrypt file
            //var json = JsonSerializer.Serialize(accounts);
            //var buffer = Encoding.UTF8.GetBytes(json);
            //using var memory = new MemoryStream(buffer);
            //using var file = File.OpenWrite(path);

            //await encryptor.EncryptAsync(memory, file, Password);

            //var intent = new Intent();
            //intent.PutExtra("AccountName", _accountNameEditText.Text);
            //SetResult(Result.Ok, intent);

            //// Finish
            //Finish();
        }

        private bool Validate()
        {
            _accountNameTextView.Visibility = ViewStates.Gone;
            _accountLoginTextView.Visibility = ViewStates.Gone;
            _accountPasswordTextView.Visibility = ViewStates.Gone;

            var isValid = true;
            if (string.IsNullOrEmpty(_accountNameEditText.Text))
            {
                isValid = false;
                _accountNameTextView.Text = "Name is required";
                _accountNameTextView.Visibility = ViewStates.Visible;
            }

            if (string.IsNullOrEmpty(_accountLoginEditText.Text))
            {
                isValid = false;
                _accountLoginTextView.Text = "Password is required";
                _accountLoginTextView.Visibility = ViewStates.Visible;
            }

            if (string.IsNullOrEmpty(_accountPasswordEditText.Text))
            {
                isValid = false;
                _accountPasswordTextView.Text = "Repeat Password is required";
                _accountPasswordTextView.Visibility = ViewStates.Visible;
            }

            return isValid;
        }

        private void GeneratePasswordButton_Click(object sender, EventArgs e)
        {
            //var generator = new PasswordGenerator();
            //_accountPasswordEditText.Text = generator.Generate(new PasswordGeneratorOptions()
            //{
            //    MinLength = 10,
            //    MaxLength = 14,
            //    Uppercase = true,
            //    Lowercase = true,
            //    Numbers = true,
            //    Symbols = true,
            //    SymbolAmount = 1
            //});
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
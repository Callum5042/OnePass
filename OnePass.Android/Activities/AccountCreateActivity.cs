using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using OnePass.Models;
using OnePass.Services;
using System;
using System.IO;

namespace OnePass.Droid.Activities
{
    [Activity(Theme = "@style/AppTheme")]
    public class AccountCreateActivity : AccountBaseActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_account_edit);

            CreateCommon();

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
        }

        private async void SubmitButton_Click(object sender, EventArgs e)
        {
            var isValid = Validate();
            if (!isValid)
            {
                return;
            }

            // File
            var documentsPath = GetExternalFilesDir(Android.OS.Environment.DirectoryDocuments).AbsolutePath;
            var filename = $"{Username}.bin";
            var path = Path.Combine(documentsPath, filename);

            // Decrypt file
            var fileEncoder = new FileEncoder();
            var data = await fileEncoder.LoadAsync(Username, Password, path);

            var checkbox = FindViewById<CheckBox>(Resource.Id.favourite_checkbox);

            // Add data
            var account = new Account()
            {
                Guid = Guid.NewGuid(),
                Name = _accountNameEditText.Text,
                Username = _accountUsernameEditText.Text,
                EmailAddress = _accountEmailEditText.Text,
                Password = _accountPasswordEditText.Text,
                DateCreated = DateTime.Now,
                DateModified = DateTime.Now,
                Favourite = checkbox.Checked
            };

            if (OptionsInstance.Options.EnablePasswordHistory)
            {
                account.PasswordHistory.Add(new PasswordHistory()
                {
                    Guid = Guid.NewGuid(),
                    Password = _accountPasswordEditText.Text,
                    DateTime = DateTime.Now,
                });
            }

            data.Accounts.Add(account);

            // Save file
            await fileEncoder.SaveAsync(Username, Password, data, path);

            // Finish
            var intent = new Intent();
            intent.PutExtra("AccountName", _accountNameEditText.Text);
            SetResult(Result.Ok, intent);
            Finish();
        }

        private void GeneratePasswordButton_Click(object sender, EventArgs e)
        {
            var generator = new PasswordGenerator()
            {
                HasLowercase = OptionsInstance.Options.Lowercase,
                HasUppercase = OptionsInstance.Options.Uppercase,
                HasNumbers = OptionsInstance.Options.Numbers,
                HasSymbols = OptionsInstance.Options.Symbols,
                MinLength = OptionsInstance.Options.MinLength,
                MaxLength = OptionsInstance.Options.MaxLength
            };

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
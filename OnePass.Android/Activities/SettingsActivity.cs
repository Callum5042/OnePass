using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using OnePass.Droid.Models;
using System.IO;
using System.Text.Json;

namespace OnePass.Droid.Activities
{
    [Activity(Label = "Configuration", Theme = "@style/AppTheme.Header")]
    public class SettingsActivity : Activity
    {
        private CheckedTextView PasswordGeneratorLowercase { get; set; }

        private CheckedTextView PasswordGeneratorUppercase { get; set; }

        private CheckedTextView PasswordGeneratorNumbers { get; set; }

        private CheckedTextView PasswordGeneratorSymbols { get; set; }

        private EditText PasswordGeneratorMinLength { get; set; }

        private EditText PasswordGeneratorMaxLength { get; set; }

        private CheckedTextView PasswordHistoryEnable { get; set; }

        private AppOptions Options { get; set; }

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_settings);

            // Create your application here
            PasswordGeneratorLowercase = FindViewById<CheckedTextView>(Resource.Id.password_generator_lowercase);
            PasswordGeneratorUppercase = FindViewById<CheckedTextView>(Resource.Id.password_generator_uppercase);
            PasswordGeneratorNumbers = FindViewById<CheckedTextView>(Resource.Id.password_generator_numbers);
            PasswordGeneratorSymbols = FindViewById<CheckedTextView>(Resource.Id.password_generator_symbols);
            PasswordGeneratorMinLength = FindViewById<EditText>(Resource.Id.min_password_length);
            PasswordGeneratorMaxLength = FindViewById<EditText>(Resource.Id.max_password_length);

            PasswordHistoryEnable = FindViewById<CheckedTextView>(Resource.Id.password_history);

            // Add toggle click event
            PasswordGeneratorLowercase.Click += CheckedTextView_Toggle_Click;
            PasswordGeneratorUppercase.Click += CheckedTextView_Toggle_Click;
            PasswordGeneratorNumbers.Click += CheckedTextView_Toggle_Click;
            PasswordGeneratorSymbols.Click += CheckedTextView_Toggle_Click;
            PasswordHistoryEnable.Click += CheckedTextView_Toggle_Click;

            // Save button
            var saveButton = FindViewById<Button>(Resource.Id.save_options_button);
            saveButton.Click += SaveButton_Click;

            // Read appsettings to see if remember username has a value
            //var documentsPath = GetExternalFilesDir(Android.OS.Environment.DirectoryDocuments).AbsolutePath;
            var documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
            var filename = "appsettings.json";
            var path = Path.Combine(documentsPath, filename);

            if (File.Exists(path))
            {
                // Read appsettings
                using var file = File.OpenRead(path);
                using var reader = new StreamReader(file);
                var json = await reader.ReadToEndAsync();
                Options = JsonSerializer.Deserialize<AppOptions>(json);

                PasswordGeneratorLowercase.Checked = Options.Lowercase;
                PasswordGeneratorUppercase.Checked = Options.Uppercase;
                PasswordGeneratorNumbers.Checked = Options.Numbers;
                PasswordGeneratorSymbols.Checked = Options.Symbols;
                PasswordGeneratorMinLength.Text = Options.MinLength.ToString();
                PasswordGeneratorMaxLength.Text = Options.MaxLength.ToString();

                PasswordHistoryEnable.Checked = Options.EnablePasswordHistory;
            }
        }

        private void SaveButton_Click(object sender, System.EventArgs e)
        {
            var documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
            var filename = "appsettings.json";
            var path = Path.Combine(documentsPath, filename);

            // Appsettings
            Options.Lowercase = Options.Uppercase = true;

            Options.Lowercase = PasswordGeneratorLowercase.Checked;
            Options.Uppercase = PasswordGeneratorUppercase.Checked;
            Options.Numbers = PasswordGeneratorNumbers.Checked;
            Options.Symbols = PasswordGeneratorSymbols.Checked;

            Options.MinLength = int.Parse(PasswordGeneratorMinLength.Text);
            Options.MaxLength = int.Parse(PasswordGeneratorMaxLength.Text);

            Options.EnablePasswordHistory = PasswordHistoryEnable.Checked;

            OptionsInstance.Options = Options;

            // Read file
            var json = JsonSerializer.Serialize(Options);
            File.WriteAllText(path, json);

            // Redirect
            Finish();
        }

        private void CheckedTextView_Toggle_Click(object sender, System.EventArgs e)
        {
            var checkedTextView = sender as CheckedTextView;
            checkedTextView.Checked = !checkedTextView.Checked;
        }
    }
}
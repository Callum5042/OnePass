using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;
using Google.Android.Material.Tabs;
using System;
using System.Text.RegularExpressions;

namespace OnePass.Droid.Activities
{
    public abstract class AccountBaseActivity : Activity
    {
        protected Guid _accountId;
        protected EditText _accountNameEditText;
        protected EditText _accountUsernameEditText;
        protected EditText _accountEmailEditText;
        protected EditText _accountPasswordEditText;

        protected TextView _accountNameTextView;
        protected TextView _accountUsernameTextView;
        protected TextView _accountEmailTextView;
        protected TextView _accountPasswordTextView;
        protected CheckBox _accountFavouriteCheckbox;

        protected TabLayout _tabLayout;

        protected string Username { get; set; }

        protected string Password { get; set; }

        protected void CreateCommon()
        {
            Username = Intent.GetStringExtra(nameof(Username));
            Password = Intent.GetStringExtra(nameof(Password));

            // Cache controls
            _accountNameEditText = FindViewById<EditText>(Resource.Id.account_name);
            _accountUsernameEditText = FindViewById<EditText>(Resource.Id.account_username);
            _accountEmailEditText = FindViewById<EditText>(Resource.Id.account_email);
            _accountPasswordEditText = FindViewById<EditText>(Resource.Id.account_password);

            _accountNameTextView = FindViewById<TextView>(Resource.Id.name_validation_message);
            _accountUsernameTextView = FindViewById<TextView>(Resource.Id.username_validation_message);
            _accountEmailTextView = FindViewById<TextView>(Resource.Id.email_validation_message);
            _accountPasswordTextView = FindViewById<TextView>(Resource.Id.password_validation_message);

            // Tabs
            _tabLayout = FindViewById<TabLayout>(Resource.Id.tabLayout);
            _tabLayout.TabSelected += TabLayout_TabSelected;
        }

        private void TabLayout_TabSelected(object sender, TabLayout.TabSelectedEventArgs e)
        {
            var layoutDetails = FindViewById<LinearLayout>(Resource.Id.layout_details);
            var detailsHistory = FindViewById<LinearLayout>(Resource.Id.details_history);

            layoutDetails.Visibility = ViewStates.Gone;
            detailsHistory.Visibility = ViewStates.Gone;

            switch (e.Tab.Text)
            {
                case "Details":
                    layoutDetails.Visibility = ViewStates.Visible;
                    break;

                case "History":
                    detailsHistory.Visibility = ViewStates.Visible;
                    break;
            }
        }

        protected bool Validate()
        {
            _accountNameTextView.Visibility = ViewStates.Gone;
            _accountUsernameTextView.Visibility = ViewStates.Gone;
            _accountPasswordTextView.Visibility = ViewStates.Gone;

            var isValid = true;
            if (string.IsNullOrEmpty(_accountNameEditText.Text))
            {
                isValid = false;
                _accountNameTextView.Text = "Name is required";
                _accountNameTextView.Visibility = ViewStates.Visible;
            }

            var email = _accountEmailEditText.Text;
            if (!string.IsNullOrEmpty(email) && !ValidateEmail(email))
            {
                isValid = false;
                _accountEmailTextView.Text = "Email is invalid";
                _accountEmailTextView.Visibility = ViewStates.Visible;
            }

            return isValid;
        }

        private static bool ValidateEmail(string email)
        {
            var regex = new Regex(@"^.+@.+$");
            return regex.IsMatch(email);
        }
    }
}
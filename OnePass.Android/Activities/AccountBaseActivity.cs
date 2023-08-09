using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;
using Google.Android.Material.Tabs;
using System;

namespace OnePass.Droid.Activities
{
    public abstract class AccountBaseActivity : Activity
    {
        protected Guid _accountId;
        protected EditText _accountNameEditText;
        protected EditText _accountLoginEditText;
        protected EditText _accountPasswordEditText;

        protected TextView _accountNameTextView;
        protected TextView _accountLoginTextView;
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
            _accountLoginEditText = FindViewById<EditText>(Resource.Id.account_login);
            _accountPasswordEditText = FindViewById<EditText>(Resource.Id.account_password);

            _accountNameTextView = FindViewById<TextView>(Resource.Id.name_validation_message);
            _accountLoginTextView = FindViewById<TextView>(Resource.Id.login_validation_message);
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
    }
}
﻿using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace OnePass.WPF.Models
{
    public class LoginModel : ObservableValidator
    {
        public LoginModel()
        {
            LoginCommand = new RelayCommand(Login);
            RegisterCommand = new RelayCommand(Register);

            ErrorsChanged += LoginModel_ErrorsChanged;
        }

        private void LoginModel_ErrorsChanged(object sender, DataErrorsChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Username))
            {
                UsernameValidationMessage = GetErrors(e.PropertyName).Select(x => x.ErrorMessage).FirstOrDefault();
                UsernameLabelVisibility = GetErrors(e.PropertyName).Any() ? Visibility.Visible : Visibility.Collapsed;
            }

            if (e.PropertyName == nameof(Password))
            {
                PasswordValidationMessage = GetErrors(e.PropertyName).Select(x => x.ErrorMessage).FirstOrDefault();
                PasswordLabelVisibility = GetErrors(e.PropertyName).Any() ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public static string Version => $"v{Assembly.GetExecutingAssembly().GetName().Version.ToString(3)}";

        public ICommand LoginCommand { get; }

        public ICommand RegisterCommand { get; }

        private void Login()
        {
            ValidateAllProperties();
        }

        private void Register()
        {

        }

        private string _username;

        [Required]
        public string Username 
        {
            get => _username;
            set => SetProperty(ref _username, value, validate: true);
        }

        private string _password;

        [Required]
        [MinLength(10, ErrorMessage = $"{nameof(Password)} must be at least 10 characters")]
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value, validate: true);
        }

        private string _usernameValidationMessage;

        public string UsernameValidationMessage 
        {
            get => _usernameValidationMessage;
            set => SetProperty(ref _usernameValidationMessage, value);
        }

        private Visibility _visibilityUsernameLabel = Visibility.Collapsed;

        public Visibility UsernameLabelVisibility 
        {
            get => _visibilityUsernameLabel;
            set => SetProperty(ref _visibilityUsernameLabel, value);
        }

        private string _passwordValidationMessage;

        public string PasswordValidationMessage
        {
            get => _passwordValidationMessage;
            set => SetProperty(ref _passwordValidationMessage, value);
        }

        private Visibility _visibilitypasswordLabel = Visibility.Collapsed;

        public Visibility PasswordLabelVisibility
        {
            get => _visibilitypasswordLabel;
            set => SetProperty(ref _visibilitypasswordLabel, value);
        }
    }
}
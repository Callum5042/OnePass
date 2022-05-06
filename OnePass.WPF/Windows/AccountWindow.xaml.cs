using OnePass.Models;
using OnePass.WPF.Models;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Windows;

namespace OnePass.WPF.Windows
{
    /// <summary>
    /// Interaction logic for AddAccountWindow.xaml
    /// </summary>
    public partial class AccountWindow : Window
    {
        public AccountWindow()
        {
            InitializeComponent();
            DataContext = new AccountListModel();
        }

        private void Button_Click_AddAccount(object sender, RoutedEventArgs e)
        {
            // Read file
            var root = ReadFile();

            // Add new account
            root.Accounts.Add(new Account()
            {
                Guid = Guid.NewGuid(),
                Name = NameTextbox.Text,
                Username = UsernameTextbox.Text,
                EmailAddress = EmailAddressTextbox.Text,
                Password = PasswordTextbox.Text,
            });

            // Save file
            SaveFile(root);

            // Update thing
            var contentWindow = App.Current.Windows.OfType<ContentWindow>().FirstOrDefault();
            contentWindow.Accounts.Add(new AccountListModel()
            {
                Name = NameTextbox.Text,
                Username = UsernameTextbox.Text,
                EmailAddress= EmailAddressTextbox.Text,
                Password= PasswordTextbox.Text
            });

            Close();
        }

        private static RootAccount ReadFile()
        {
            var fileSignature = ".ONEPASS";
            var filename = App.Current.Filename;

            using var file = File.OpenRead(filename);
            using var reader = new BinaryReader(file);

            // Read signature
            var signature = reader.ReadBytes(Encoding.UTF8.GetByteCount(fileSignature));
            if (Encoding.UTF8.GetString(signature) != fileSignature)
            {
                throw new InvalidOperationException("Not a valid OnePass file");
            }

            // Read version
            var version = reader.ReadInt32();

            // Read password hash
            var passwordHashLength = reader.ReadInt32();
            var passwordHash = reader.ReadBytes(passwordHashLength);

            // Read salt
            var saltLength = reader.ReadInt32();
            var salt = reader.ReadBytes(saltLength);

            // Read IV
            var ivLength = reader.ReadInt32();
            var iv = reader.ReadBytes(ivLength);

            // Generate keys
            var rfc = new Rfc2898DeriveBytes(App.Current.Password, salt);
            using var aes = Aes.Create();
            aes.Key = rfc.GetBytes(16);
            aes.IV = iv;

            // Decrypt
            using var cryptoStream = new CryptoStream(file, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using var cryptoReader = new StreamReader(cryptoStream);
            var content = cryptoReader.ReadToEnd();

            var root = JsonSerializer.Deserialize<RootAccount>(content);
            return root;
        }

        private static void SaveFile(RootAccount rootAccount)
        {
            var fileSignature = ".ONEPASS";
            var fileVersion = 1;

            // Generate salt
            var salt = RandomNumberGenerator.GetBytes(8);

            // Generate keys
            var rfc = new Rfc2898DeriveBytes(App.Current.Password, salt);
            using var aes = Aes.Create();
            aes.Key = rfc.GetBytes(16);

            using var file = File.Create(App.Current.Filename);
            using var writer = new BinaryWriter(file);

            // Write signature
            writer.Write(Encoding.UTF8.GetBytes(fileSignature));

            // Write version
            writer.Write(fileVersion);

            // Write password hash
            using (var sha = SHA512.Create())
            {
                var passwordBytes = Encoding.UTF8.GetBytes(App.Current.Password);
                var bytes = passwordBytes.Concat(salt).ToArray();
                var passwordHash = sha.ComputeHash(bytes);

                writer.Write(passwordHash.Length);
                writer.Write(passwordHash);
            }

            // Write salt
            writer.Write(salt.Length);
            writer.Write(salt);

            // Write IV
            writer.Write(aes.IV.Length);
            writer.Write(aes.IV);

            // Encrypt
            using var cryptoStream = new CryptoStream(file, aes.CreateEncryptor(), CryptoStreamMode.Write);
            using var cryptoWriter = new StreamWriter(cryptoStream);

            var content = JsonSerializer.Serialize(rootAccount);
            cryptoWriter.Write(content);
        }
    }
}

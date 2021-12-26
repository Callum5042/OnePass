using OnePass.Models;
using OnePass.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace OnePass.Tests.Tests.Services
{
    public class AccountSyncerTests
    {
        [Fact]
        public void Sync_BothIsEmpty()
        {
            // Arrange
            var localAccounts = new List<Account>();
            var remoteAccounts = new List<Account>();

            // Act
            var accountSyncer = new AccountSyncer();
            var result = accountSyncer.Sync(localAccounts, remoteAccounts);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void Sync_BothHaveSameValue_ReturnsOne()
        {
            // Arrange
            var localAccounts = new List<Account>
            {
                new Account()
                {
                    Id = 1,
                    Name = "Product",
                    Login = "Login123",
                    Password = "Password123",
                    DateCreated = new DateTime(2010, 3, 10),
                    DateModified = new DateTime(2010, 3, 15, 12, 30, 15)
                }
            };

            var remoteAccounts = new List<Account>
            {
                new Account()
                {
                    Id = 1,
                    Name = "Product",
                    Login = "Login123",
                    Password = "Password123",
                    DateCreated = new DateTime(2010, 3, 10),
                    DateModified = new DateTime(2010, 3, 15, 12, 30, 15)
                }
            };

            // Act
            var accountSyncer = new AccountSyncer();
            var result = accountSyncer.Sync(localAccounts, remoteAccounts).ToList();

            // Assert
            Assert.Single(result);
            Assert.Equal(localAccounts[0].Password, result[0].Password);
            Assert.Equal(remoteAccounts[0].Password, result[0].Password);
        }

        [Fact]
        public void Sync_BothHaveSameValue_ReturnsOneWithUpdatedValue()
        {
            // Arrange
            var localAccounts = new List<Account>
            {
                new Account()
                {
                    Id = 1,
                    Name = "Product",
                    Login = "Login123",
                    Password = "Password123",
                    DateCreated = new DateTime(2010, 3, 10),
                    DateModified = new DateTime(2010, 3, 15, 12, 30, 15)
                }
            };

            var remoteAccounts = new List<Account>
            {
                new Account()
                {
                    Id = 1,
                    Name = "Product",
                    Login = "Login123",
                    Password = "UpdatedPassword123",
                    DateCreated = new DateTime(2010, 3, 10),
                    DateModified = new DateTime(2010, 3, 15, 13, 30, 15)
                }
            };

            // Act
            var accountSyncer = new AccountSyncer();
            var result = accountSyncer.Sync(localAccounts, remoteAccounts).ToList();

            // Assert
            Assert.Single(result);
            Assert.Equal(remoteAccounts[0].Password, result[0].Password);
        }

        [Fact]
        public void Sync_OnlyHasLocalAccount_ReturnsSingleAccount()
        {
            // Arrange
            var localAccounts = new List<Account>
            {
                new Account()
                {
                    Id = 1,
                    Name = "Product",
                    Login = "Login123",
                    Password = "Password123",
                    DateCreated = new DateTime(2010, 3, 10),
                    DateModified = new DateTime(2010, 3, 15, 12, 30, 15)
                }
            };

            var remoteAccounts = new List<Account>();

            // Act
            var accountSyncer = new AccountSyncer();
            var result = accountSyncer.Sync(localAccounts, remoteAccounts);

            // Assert
            Assert.Single(result);
        }

        [Fact]
        public void Sync_OnlyHasRemoteAccount_ReturnsSingleAccount()
        {
            // Arrange
            var localAccounts = new List<Account>();

            var remoteAccounts = new List<Account>
            {
                new Account()
                {
                    Id = 1,
                    Name = "Product",
                    Login = "Login123",
                    Password = "Password123",
                    DateCreated = new DateTime(2010, 3, 10),
                    DateModified = new DateTime(2010, 3, 15, 12, 30, 15)
                }
            };

            // Act
            var accountSyncer = new AccountSyncer();
            var result = accountSyncer.Sync(localAccounts, remoteAccounts);

            // Assert
            Assert.Single(result);
        }
    }
}

using OnePass.Models;
using OnePass.Services;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace OnePass.Tests.Tests.Services
{
    public class AccountSyncerTests
    {
        [Fact]
        public void Sync_ListsAreEmpty_ReturnsEmptyList()
        {
            // Arrange
            var data1 = new OnePassData();
            var data2 = new OnePassData();

            // Act
            var accountSyncer = new AccountSyncer();
            var result = accountSyncer.Sync(data1, data2);

            // Assert
            Assert.Empty(result.Accounts);
            Assert.Empty(result.DeletedAccounts);
        }

        [Fact]
        public void Sync_ListHasSingleValue_ReturnsListSingle()
        {
            // Arrange
            var data1 = new OnePassData()
            {
                Accounts = new List<Account>()
            };

            var data2 = new OnePassData()
            {
                Accounts = new List<Account>()
                {
                    new Account()
                    {
                        Guid = Guid.NewGuid(),
                        Name = "Test Account"
                    }
                }
            };

            // Act
            var accountSyncer = new AccountSyncer();
            var result = accountSyncer.Sync(data1, data2);

            // Assert
            Assert.NotEmpty(result.Accounts);
            Assert.Single(result.Accounts);
        }

        [Fact]
        public void Sync_ListHasDuplicateEntry_ReturnsListSingle()
        {
            // Arrange
            var data1 = new OnePassData()
            {
                Accounts = new List<Account>()
                {
                    new Account()
                    {
                        Guid = new Guid("F9F7B604-9E07-4147-8F89-317FF933AC52"),
                        Name = "Test Account"
                    }
                }
            };

            var data2 = new OnePassData()
            {
                Accounts = new List<Account>()
                {
                    new Account()
                    {
                        Guid = new Guid("F9F7B604-9E07-4147-8F89-317FF933AC52"),
                        Name = "Test Account"
                    }
                }
            };

            // Act
            var accountSyncer = new AccountSyncer();
            var result = accountSyncer.Sync(data1, data2);

            // Assert
            Assert.NotEmpty(result.Accounts);
            Assert.Single(result.Accounts);
        }

        [Fact]
        public void Sync_ListHasDuplicated_TakeLatestModifiedAccount()
        {
            // Arrange
            var data1 = new OnePassData
            {
                Accounts = new List<Account>()
                {
                    new Account()
                    {
                        Guid = new Guid("F9F7B604-9E07-4147-8F89-317FF933AC52"),
                        Name = "OLD Test Account",
                        DateModified = new DateTime(2000, 1, 1)
                    }
                }
            };

            var data2 = new OnePassData
            {
                Accounts = new List<Account>()
                {
                    new Account()
                    {
                        Guid = new Guid("F9F7B604-9E07-4147-8F89-317FF933AC52"),
                        Name = "NEW Test Account",
                        DateModified = new DateTime(2001, 10, 10)
                    }
                }
            };

            // Act
            var accountSyncer = new AccountSyncer();
            var result = accountSyncer.Sync(data1, data2);

            // Assert
            Assert.NotEmpty(result.Accounts);
            Assert.Single(result.Accounts);
            Assert.Contains(result.Accounts, x => x.Name == "NEW Test Account");
        }

        [Fact]
        public void Sync_ListHasDeletedAccount_EmptyList()
        {
            // Arrange
            var data1 = new OnePassData
            {
                Accounts = new List<Account>()
                {
                    new Account()
                    {
                        Guid = new Guid("F9F7B604-9E07-4147-8F89-317FF933AC52"),
                        Name = "OLD Test Account",
                        DateModified = new DateTime(2000, 1, 1)
                    }
                },
            };

            var data2 = new OnePassData
            {
                DeletedAccounts = new List<Guid>() { new Guid("F9F7B604-9E07-4147-8F89-317FF933AC52") }
            };

            // Act
            var accountSyncer = new AccountSyncer();
            var result = accountSyncer.Sync(data1, data2);

            // Assert
            Assert.Empty(result.Accounts);
            Assert.NotEmpty(result.DeletedAccounts);
        }

        [Fact]
        public void Sync_ListHasSame_MergePasswordHistory()
        {
            // Arrange
            var data1 = new OnePassData
            {
                Accounts = new List<Account>()
                {
                    new Account()
                    {
                        Guid = new Guid("F9F7B604-9E07-4147-8F89-317FF933AC52"),
                        Name = "OLD Test Account",
                        DateModified = new DateTime(2000, 1, 1),
                        Password = "password1",
                        PasswordHistory = new List<PasswordHistory>()
                        {
                            new PasswordHistory() { Guid = new Guid("F5F7B604-9E07-4147-8F89-317FF933AC52"), Password = "password", DateTime = DateTime.Now },
                            new PasswordHistory() { Guid = Guid.NewGuid(), Password = "password1", DateTime = DateTime.Now },
                        }
                    }
                },
            };

            var data2 = new OnePassData
            {
                Accounts = new List<Account>()
                {
                    new Account()
                    {
                        Guid = new Guid("F9F7B604-9E07-4147-8F89-317FF933AC52"),
                        Name = "NEW Test Account",
                        DateModified = new DateTime(2010, 1, 1),
                        Password = "password2",
                        PasswordHistory = new List<PasswordHistory>()
                        {
                            new PasswordHistory() { Guid = new Guid("F5F7B604-9E07-4147-8F89-317FF933AC52"), Password = "password", DateTime = DateTime.Now },
                            new PasswordHistory() { Guid = Guid.NewGuid(), Password = "password2", DateTime = DateTime.Now },
                        }
                    }
                },
            };

            // Act
            var accountSyncer = new AccountSyncer();
            var result = accountSyncer.Sync(data1, data2);

            // Assert
            Assert.Single(result.Accounts);

            var resultAccount = result.Accounts.First();
            Assert.Equal(3, resultAccount.PasswordHistory.Count);
        }
    }
}

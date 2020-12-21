﻿using System.Threading.Tasks;

namespace OnePass.Handlers
{
    public interface IChangePasswordHandler
    {
        Task<string> ChangePassword(string oldPassword, string newPassword);
    }
}
using System;

namespace OnePass.Services
{
    /// <summary>
    /// The vault could not be decrypted with the supplied password. For version 2
    /// files this is also what a damaged or tampered file looks like, because both
    /// fail the same authentication check.
    /// </summary>
    public class InvalidPasswordException : Exception
    {
        public InvalidPasswordException()
            : base("Password is incorrect.")
        {
        }
    }
}

using System;

namespace Mobile
{
    public class OnLoginEventArgs : EventArgs
    {
        public string Login { get; set; }
        public string Password { get; set; }

        public OnLoginEventArgs(string login, string password) : base()
        {
            Login = login;
            Password = password;
        }
    }
}
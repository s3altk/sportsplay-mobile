using System;

namespace Mobile
{
    public class OnRegisterEventArgs : EventArgs
    {
        public string Login { get; set; }
        public string Password { get; set; }

        public OnRegisterEventArgs(string login, string password) : base()
        {
            Login = login;
            Password = password;
        }
    }
}
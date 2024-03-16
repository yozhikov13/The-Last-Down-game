namespace Barebones.MasterServer
{
    public class PasswordChangeData
    {
        public string Email { get; set; }
        public string Code { get; set; }
        public string NewPassword { get; set; }
    }
}
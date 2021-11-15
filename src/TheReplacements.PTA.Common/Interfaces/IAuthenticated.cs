namespace TheReplacements.PTA.Common.Interfaces
{
    public interface IAuthenticated
    {
        public string PasswordHash { get; set; }
        public bool IsOnline { get; set; }
    }
}

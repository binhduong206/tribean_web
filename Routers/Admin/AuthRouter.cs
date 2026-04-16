namespace Tribean.Routers.Admin
{
    public static class AuthRouter
    {
        private const string Base = "Admin";
        public const string Login  = Base + "/Login";  // Cho cả GET và POST
        public const string Logout = Base + "/Logout"; // Cho GET
    }
}
// Routers/Admin/UserRouter.cs
namespace Tribean.Routers.Admin
{
    public static class UserRouter
    {
        private const string Base = "Admin/User";

        public const string Index  = Base;                  // GET  /Admin/User
        public const string Detail = Base + "/Detail/{id}"; // GET  /Admin/User/Detail/{id}
        public const string Create = Base + "/Create";      // POST /Admin/User/Create
        public const string Edit   = Base + "/Edit/{id}";   // GET  /Admin/User/Edit/{id}
        public const string Update = Base + "/Update/{id}"; // POST /Admin/User/Update/{id}
        public const string Delete = Base + "/Delete/{id}"; // POST /Admin/User/Delete/{id}
    }
}
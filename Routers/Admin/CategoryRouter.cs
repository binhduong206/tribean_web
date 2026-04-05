// Routers/Admin/CategoryRouter.cs
namespace Tribean.Routers.Admin
{
    public static class CategoryRouter
    {
        private const string Base = "Admin/Category";

        public const string Index  = Base;                  // GET  /Admin/Category
        public const string Create = Base + "/Create";      // POST /Admin/Category/Create
        public const string Edit   = Base + "/Edit/{id}";   // GET  /Admin/Category/Edit/{id}
        public const string Update = Base + "/Update/{id}"; // POST /Admin/Category/Update/{id}
        public const string Delete = Base + "/Delete/{id}"; // POST /Admin/Category/Delete/{id}
    }
}
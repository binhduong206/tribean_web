// Routers/Admin/ProductRouter.cs
namespace Tribean.Routers.Admin
{
    public static class ProductRouter
    {
        private const string Base = "Admin/Product";

        public const string Index  = Base;                 // GET  /Admin/Product
        public const string Add    = Base + "/Add";        // GET  /Admin/Product/Add
        public const string Create = Base + "/Create";     // POST /Admin/Product/Create
        public const string Edit   = Base + "/Edit/{id}";  // GET  /Admin/Product/Edit/{id}
        public const string Update = Base + "/Update/{id}";// POST /Admin/Product/Update/{id}
        public const string Delete = Base + "/Delete/{id}";// POST /Admin/Product/Delete/{id}
        public const string Detail = Base + "/Detail/{id}";// GET  /Admin/Product/Detail/{id}
    }
}
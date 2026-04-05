// Routers/Admin/OrderRouter.cs
namespace Tribean.Routers.Admin
{
    public static class OrderRouter
    {
        private const string Base = "Admin/Order";

        public const string Index        = Base;                          // GET  /Admin/Order
        public const string Detail       = Base + "/Detail/{id}";        // GET  /Admin/Order/Detail/{id}
        public const string UpdateStatus = Base + "/UpdateStatus/{id}";  // POST /Admin/Order/UpdateStatus/{id}
        public const string Cancel       = Base + "/Cancel/{id}";        // POST /Admin/Order/Cancel/{id}
    }
}
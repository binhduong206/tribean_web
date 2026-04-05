namespace Tribean.Routers.Admin
{
    public static class DiscountRouter
    {
        private const string Base = "Admin/Discount";

        public const string Index  = Base;                  // GET  /Admin/Discount
        public const string Create = Base + "/Create";      // GET & POST /Admin/Discount/Create
        public const string Edit   = Base + "/Edit/{id}";   // GET & POST /Admin/Discount/Edit/{id}
        public const string Toggle = Base + "/Toggle/{id}"; // POST /Admin/Discount/Toggle/{id}
        public const string Delete = Base + "/Delete/{id}"; // POST /Admin/Discount/Delete/{id}
    }
}
namespace Tribean.Routers.Admin
{
    public static class ReviewRouter
    {
        private const string Base = "Admin/Review";

        public const string Index  = Base;                  // GET  /Admin/Review
        public const string Delete = Base + "/Delete/{id}"; // POST /Admin/Review/Delete/{id}
        public const string Seed   = Base + "/SeedData";    // GET  /Admin/Review/SeedData (Dùng để test)
    }
}
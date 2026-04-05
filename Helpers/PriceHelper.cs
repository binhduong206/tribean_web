// Helpers/PriceHelper.cs
namespace Tribean.Helpers
{
    public static class PriceHelper
    {
        // Hàm tính giá sau khi giảm
        public static decimal CalculateFinalPrice(decimal originalPrice, int discountPercentage)
        {
            if (discountPercentage <= 0) return originalPrice;
            if (discountPercentage >= 100) return 0;
            
            // Tính toán giá trị và trả về (ép kiểu 100m để đảm bảo độ chính xác của decimal)
            return originalPrice - (originalPrice * discountPercentage / 100m);
        }
    }
}
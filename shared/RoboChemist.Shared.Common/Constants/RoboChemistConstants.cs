namespace RoboChemist.Shared.Common.Constants
{
    public static class RoboChemistConstants
    {
        /// <summary>
        /// Slide Request Status: Pending
        /// </summary>
        public const string SLIDEREQ_STATUS_PENDING = "Đang xử lý";

        /// <summary>
        /// Slide request Status: Completed
        /// </summary>
        public const string SLIDEREQ_STATUS_COMPLETED = "Hoàn thành";

        /// <summary>
        /// Slide request Status: Failed
        /// </summary>
        public const string SLIDEREQ_STATUS_FAILED = "Thất bại";

        /// <summary>
        /// Generated Slide Status: JSON Data Created
        /// </summary>
        public const string GENSLIDE_STATUS_JSON = "Đã tạo dữ liệu";
        
        /// <summary>
        /// Generated Slide Status: File Created
        /// </summary>
        public const string GENSLIDE_STATUS_FILE = "Đã tạo tệp";

        /// <summary>
        /// Generated Slide Status: Completed
        /// </summary>
        public const string GENSLIDE_STATUS_COMPLETED = "Hoàn thành";

        /// <summary>
        /// File format: PPTX
        /// </summary>
        public const string File_Format_PPTX = "pptx";

        /// <summary>
        /// Tăng số dư ví
        /// </summary>
        public const string UPDATE_BALANCE_TYPE_ADD = "A";

        /// <summary>
        /// Giảm số dư ví
        /// </summary>
        public const string UPDATE_BALANCE_TYPE_SUBTRACT = "S";

        /// <summary>
        /// Transaction Status: Pending
        /// </summary>
        public const string  TRANSACTION_STATUS_PENDING = "Đợi xử lí";

        /// <summary>
        /// Transaction Status: Completed
        /// </summary>
        public const string  TRANSACTION_STATUS_COMPLETED = "Hoàn thành";

        /// <summary>
        /// Transaction Status: Failed
        /// </summary>
        public const string  TRANSACTION_STATUS_FAILED = "Thất bại";

        /// <summary>
        /// Transacton through VNPay
        /// </summary>
        public const string TRANSACTION_METHOD_VNPAY = "VNPay";

        /// <summary>
        /// Transaction throuhg Wallet
        /// </summary>
        public const string TRANSACTION_METHOD_WALLET = "Ví";

        /// <summary>
        /// Transaction Type: Deposit
        /// </summary>
        public const string TRANSACTION_TYPE_DEPOSIT = "Nạp tiền";
        /// <summary>
        /// Transaction Type: Payment
        /// </summary>
        public const string TRANSACTION_TYPE_PAYMENT = "Thanh toán";
        /// <summary>
        /// Transaction Type: Refund
        /// </summary>
        public const string TRANSACTION_TYPE_REFUND = "Hoàn tiền";
    }
}

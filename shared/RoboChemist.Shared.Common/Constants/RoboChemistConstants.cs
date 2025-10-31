namespace RoboChemist.Shared.Common.Constants
{
    public static class RoboChemistConstants
    {
        #region Slide Service Constants
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
        #endregion

        #region Exam Service Constants
        /// <summary>
        /// Exam Request Status: Pending (Đang chờ xử lý)
        /// </summary>
        public const string EXAMREQ_STATUS_PENDING = "Pending";

        /// <summary>
        /// Exam Request Status: Processing (Đang tạo đề)
        /// </summary>
        public const string EXAMREQ_STATUS_PROCESSING = "Processing";

        /// <summary>
        /// Exam Request Status: Completed (Hoàn thành)
        /// </summary>
        public const string EXAMREQ_STATUS_COMPLETED = "Completed";

        /// <summary>
        /// Exam Request Status: Failed (Thất bại)
        /// </summary>
        public const string EXAMREQ_STATUS_FAILED = "Failed";

        /// <summary>
        /// Generated Exam Status: Draft (Bản nháp)
        /// </summary>
        public const string EXAM_STATUS_DRAFT = "Draft";

        /// <summary>
        /// Generated Exam Status: Published (Đã xuất bản)
        /// </summary>
        public const string EXAM_STATUS_PUBLISHED = "Published";

        /// <summary>
        /// Generated Exam Status: Archived (Đã lưu trữ)
        /// </summary>
        public const string EXAM_STATUS_ARCHIVED = "Archived";

        /// <summary>
        /// Question Type: Multiple Choice
        /// </summary>
        public const string QUESTION_TYPE_MULTIPLE_CHOICE = "MultipleChoice";

        /// <summary>
        /// Question Type: True/False
        /// </summary>
        public const string QUESTION_TYPE_TRUE_FALSE = "TrueFalse";

        /// <summary>
        /// Question Type: Fill in the Blank
        /// </summary>
        public const string QUESTION_TYPE_FILL_BLANK = "FillBlank";

        /// <summary>
        /// Question Type: Essay (Tự luận)
        /// </summary>
        public const string QUESTION_TYPE_ESSAY = "Essay";

        /// <summary>
        /// Exam Question Status: Active
        /// </summary>
        public const string EXAMQUESTION_STATUS_ACTIVE = "Active";

        /// <summary>
        /// Exam Question Status: Inactive
        /// </summary>
        public const string EXAMQUESTION_STATUS_INACTIVE = "Inactive";

        /// <summary>
        /// Status: Active (true)
        /// </summary>
        public const bool STATUS_ACTIVE = true;

        /// <summary>
        /// Status: Inactive (false)
        /// </summary>
        public const bool STATUS_INACTIVE = false;

        /// <summary>
        /// Minimum number of options for a question
        /// </summary>
        public const int MIN_OPTIONS_COUNT = 2;

        /// <summary>
        /// Maximum number of options for a question
        /// </summary>
        public const int MAX_OPTIONS_COUNT = 6;

        /// <summary>
        /// Question Status: Active (value = "1")
        /// </summary>
        public const string QUESTION_STATUS_ACTIVE = "1";

        /// <summary>
        /// Question Status: Inactive/Deleted (value = "0")
        /// </summary>
        public const string QUESTION_STATUS_INACTIVE = "0";
        #endregion

        #region User Roles
        /// <summary>
        /// Role: User (value = "1")
        /// </summary>
        public const string ROLE_USER = "User";

        /// <summary>
        /// Role: Admin (value = "2")
        /// </summary>
        public const string ROLE_ADMIN = "Admin";

        /// <summary>
        /// Role: Teacher (value = "3")
        /// </summary>
        //public const string ROLE_TEACHER = "3";
        #endregion

        #region Wallet Service Constants

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
        public const string TRANSACTION_STATUS_PENDING = "Đợi xử lí";

        /// <summary>
        /// Transaction Status: Completed
        /// </summary>
        public const string TRANSACTION_STATUS_COMPLETED = "Hoàn thành";

        /// <summary>
        /// Transaction Status: Failed
        /// </summary>
        public const string TRANSACTION_STATUS_FAILED = "Thất bại";

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
        #endregion

        #region Order Status

        /// <summary>
        /// Order Status: Pending - Order created but not yet paid
        /// </summary>
        public const string ORDER_STATUS_PENDING = "Chờ thanh toán";

        /// <summary>
        /// Order Status: Processing - Payment received, order being processed
        /// </summary>
        public const string ORDER_STATUS_PROCESSING = "Đang xử lý";

        /// <summary>
        /// Order Status: Completed - Order successfully completed and delivered
        /// </summary>
        public const string ORDER_STATUS_COMPLETED = "Hoàn thành";

        /// <summary>
        /// Order Status: Cancelled - Order cancelled by user or system
        /// </summary>
        public const string ORDER_STATUS_CANCELLED = "Đã hủy";

        /// <summary>
        /// Order Status: Failed - Order payment or processing failed
        /// </summary>
        public const string ORDER_STATUS_FAILED = "Thất bại";

        #endregion
    }
}

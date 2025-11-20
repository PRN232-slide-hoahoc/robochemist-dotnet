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
        /// Generated Exam Status: Pending (Đang tạo câu hỏi)
        /// </summary>
        public const string GENERATED_EXAM_STATUS_PENDING = "PENDING";

        /// <summary>
        /// Generated Exam Status: Ready (Sẵn sàng xuất đề)
        /// </summary>
        public const string GENERATED_EXAM_STATUS_READY = "READY";

        /// <summary>
        /// Generated Exam Status: Expired (Hết hạn)
        /// </summary>
        public const string GENERATED_EXAM_STATUS_EXPIRED = "EXPIRED";

        /// <summary>
        /// File format: DOCX (Word document)
        /// </summary>
        public const string FILE_FORMAT_DOCX = "docx";

        /// <summary>
        /// File format: PDF
        /// </summary>
        public const string FILE_FORMAT_PDF = "pdf";

        /// <summary>
        /// Generated Exam Type: Questions File (File đề thi)
        /// </summary>
        public const string GENERATED_EXAM_TYPE_QUESTIONS = "QUESTIONS";

        /// <summary>
        /// Generated Exam Type: Answers File (File đáp án)
        /// </summary>
        public const string GENERATED_EXAM_TYPE_ANSWERS = "ANSWERS";

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

        /// <summary>
        /// Question Level: Nhận biết (Biết)
        /// </summary>
        public const string QUESTION_LEVEL_NHAN_BIET = "NhanBiet";

        /// <summary>
        /// Question Level: Thông hiểu (Hiểu)
        /// </summary>
        public const string QUESTION_LEVEL_THONG_HIEU = "ThongHieu";

        /// <summary>
        /// Question Level: Vận dụng (Áp dụng)
        /// </summary>
        public const string QUESTION_LEVEL_VAN_DUNG = "VanDung";

        /// <summary>
        /// Question Level: Vận dụng cao (Tổng hợp, đánh giá)
        /// </summary>
        public const string QUESTION_LEVEL_VAN_DUNG_CAO = "VanDungCao";
        #endregion

        #region User Roles
        /// <summary>
        /// Role: User 
        /// </summary>
        public const string ROLE_USER = "User";

        /// <summary>
        /// Role: Admin 
        /// </summary>
        public const string ROLE_ADMIN = "Admin";

        /// <summary>
        /// Role: Staff
        /// </summary>
        public const string ROLE_STAFF = "Staff";
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
        /// Transaction through Wallet
        /// </summary>
        public const string TRANSACTION_METHOD_WALLET = "Ví";

        /// <summary>
        /// Use system to refund
        /// </summary>
        public const string TRANSACTION_METHOD_SYSTEM = "Hệ thống";

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

        public const string REFERENCE_TYPE_CREATE_SLIDE = "Tạo slide";
        public const string REFERENCE_TYPE_CREATE_EXAM = "Tạo đề thi";
        public const string REFERENCE_TYPE_BUY_TEMPLATE = "Mua template";
        #endregion

        #region Template Service Constants

        /// <summary>
        /// Template Status: Active - Template is available for purchase/use
        /// </summary>
        public const bool TEMPLATE_STATUS_ACTIVE = true;

        /// <summary>
        /// Template Status: Inactive - Template is hidden/disabled
        /// </summary>
        public const bool TEMPLATE_STATUS_INACTIVE = false;

        /// <summary>
        /// Template Type: Free - No payment required
        /// </summary>
        public const bool TEMPLATE_TYPE_FREE = false;

        /// <summary>
        /// Template Type: Premium - Payment required
        /// </summary>
        public const bool TEMPLATE_TYPE_PREMIUM = true;

        #endregion

        #region Order Status

        /// <summary>
        /// Order Status: Pending - Order created but not yet paid
        /// </summary>
        public const string ORDER_STATUS_PENDING = "Chờ thanh toán";

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

        #region Payment Reference Types

        /// <summary>
        /// Payment Reference Type: Template Purchase
        /// </summary>
        public const string PAYMENT_REF_TEMPLATE_PURCHASE = "MUA_TEMPLATE";

        #endregion
    }
}

-- ============================================
-- RoboChemist - ExamService Database Schema
-- PostgreSQL Script
-- ============================================
-- Thay đổi:
-- 1. GradeId và TopicId đổi từ INT sang UUID
-- 2. Xóa cột Prompt trong ExamRequest
-- ============================================

-- Drop existing tables if exist (careful in production!)
DROP TABLE IF EXISTS ExamQuestion CASCADE;
DROP TABLE IF EXISTS GeneratedExam CASCADE;
DROP TABLE IF EXISTS ExamRequest CASCADE;
DROP TABLE IF EXISTS Option CASCADE;
DROP TABLE IF EXISTS Question CASCADE;
DROP TABLE IF EXISTS MatrixDetail CASCADE;
DROP TABLE IF EXISTS Matrix CASCADE;

-- ============================================
-- 1. Matrix Table
-- ============================================
CREATE TABLE Matrix (
    MatrixId UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    Name VARCHAR(255) NOT NULL,
    TotalQuestion INT,
    CreatedBy UUID,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedBy UUID,
    UpdatedAt TIMESTAMP,
    IsActive BOOLEAN DEFAULT TRUE
);

COMMENT ON TABLE Matrix IS 'Ma trận đề - Template để tạo đề thi';
COMMENT ON COLUMN Matrix.MatrixId IS 'ID của ma trận (UUID)';
COMMENT ON COLUMN Matrix.Name IS 'Tên ma trận đề';
COMMENT ON COLUMN Matrix.TotalQuestion IS 'Tổng số câu hỏi trong đề';
COMMENT ON COLUMN Matrix.IsActive IS 'Trạng thái active/inactive';

-- ============================================
-- 2. MatrixDetail Table
-- ============================================
CREATE TABLE MatrixDetail (
    MatrixDetailsId UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    MatrixId UUID NOT NULL,
    TopicId UUID NOT NULL,  -- ĐÃ SỬA: từ INT sang UUID
    QuestionType VARCHAR(50) NOT NULL,  -- MultipleChoice, TrueFalse, FillBlank, Essay
    QuestionCount INT NOT NULL,
    CreatedBy UUID,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedBy UUID,
    UpdatedAt TIMESTAMP,
    IsActive BOOLEAN DEFAULT TRUE,
    CONSTRAINT FK_MatrixDetail_Matrix FOREIGN KEY (MatrixId) REFERENCES Matrix(MatrixId) ON DELETE CASCADE
);

COMMENT ON TABLE MatrixDetail IS 'Chi tiết ma trận - phân bổ câu hỏi theo Topic và Type';
COMMENT ON COLUMN MatrixDetail.TopicId IS 'ID của Topic (UUID) - tham chiếu đến SlidesService';
COMMENT ON COLUMN MatrixDetail.QuestionType IS 'Loại câu hỏi: MultipleChoice, TrueFalse, FillBlank, Essay';
COMMENT ON COLUMN MatrixDetail.QuestionCount IS 'Số lượng câu hỏi cần lấy cho Topic và Type này';

-- ============================================
-- 3. Question Table
-- ============================================
CREATE TABLE Question (
    QuestionId UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    TopicId UUID NOT NULL,  -- ĐÃ SỬA: từ INT sang UUID
    QuestionType VARCHAR(50) NOT NULL,  -- MultipleChoice, TrueFalse, FillBlank, Essay
    Question VARCHAR(1000) NOT NULL,  -- Nội dung câu hỏi (tên cột là Question1 trong model nhưng map thành Question)
    Explanation TEXT,
    CreatedBy UUID,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedBy UUID,
    UpdatedAt TIMESTAMP,
    IsActive BOOLEAN DEFAULT TRUE
);

COMMENT ON TABLE Question IS 'Ngân hàng câu hỏi';
COMMENT ON COLUMN Question.TopicId IS 'ID của Topic (UUID) - tham chiếu đến SlidesService';
COMMENT ON COLUMN Question.QuestionType IS 'Loại câu hỏi: MultipleChoice, TrueFalse, FillBlank, Essay';
COMMENT ON COLUMN Question.Question IS 'Nội dung câu hỏi';
COMMENT ON COLUMN Question.Explanation IS 'Giải thích đáp án';

-- ============================================
-- 4. Option Table
-- ============================================
CREATE TABLE Option (
    OptionId UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    QuestionId UUID NOT NULL,
    Answer VARCHAR(500) NOT NULL,
    IsCorrect BOOLEAN DEFAULT FALSE,
    CreatedBy UUID,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedBy UUID,
    UpdatedAt TIMESTAMP,
    CONSTRAINT FK_Option_Question FOREIGN KEY (QuestionId) REFERENCES Question(QuestionId) ON DELETE CASCADE
);

COMMENT ON TABLE Option IS 'Đáp án cho câu hỏi (MultipleChoice, TrueFalse, FillBlank)';
COMMENT ON COLUMN Option.Answer IS 'Nội dung đáp án';
COMMENT ON COLUMN Option.IsCorrect IS 'Đáp án đúng hay sai';

-- ============================================
-- 5. ExamRequest Table
-- ============================================
CREATE TABLE ExamRequest (
    ExamRequestId UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    UserId UUID NOT NULL,
    MatrixId UUID NOT NULL,
    GradeId UUID NOT NULL,  -- ĐÃ SỬA: từ INT sang UUID
    Status VARCHAR(50) NOT NULL,  -- Pending, Processing, Completed, Failed
    -- Prompt đã bị XÓA theo yêu cầu
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT FK_ExamRequest_Matrix FOREIGN KEY (MatrixId) REFERENCES Matrix(MatrixId) ON DELETE CASCADE
);

COMMENT ON TABLE ExamRequest IS 'Yêu cầu tạo đề thi từ user';
COMMENT ON COLUMN ExamRequest.UserId IS 'ID của user tạo request (UUID)';
COMMENT ON COLUMN ExamRequest.GradeId IS 'ID của Grade (UUID) - tham chiếu đến SlidesService';
COMMENT ON COLUMN ExamRequest.Status IS 'Trạng thái: Pending, Processing, Completed, Failed';

-- ============================================
-- 6. GeneratedExam Table
-- ============================================
CREATE TABLE GeneratedExam (
    GeneratedExamId UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    ExamRequestId UUID NOT NULL,
    Status VARCHAR(50) NOT NULL,  -- Draft, Published, Archived
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT FK_GeneratedExam_ExamRequest FOREIGN KEY (ExamRequestId) REFERENCES ExamRequest(ExamRequestId) ON DELETE CASCADE
);

COMMENT ON TABLE GeneratedExam IS 'Đề thi đã được tạo ra';
COMMENT ON COLUMN GeneratedExam.Status IS 'Trạng thái: Draft, Published, Archived';

-- ============================================
-- 7. ExamQuestion Table (Many-to-Many)
-- ============================================
CREATE TABLE ExamQuestion (
    ExamQuestionId UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    GeneratedExamId UUID NOT NULL,
    QuestionId UUID NOT NULL,
    Status VARCHAR(50) NOT NULL,  -- Active, Inactive
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT FK_ExamQuestion_GeneratedExam FOREIGN KEY (GeneratedExamId) REFERENCES GeneratedExam(GeneratedExamId) ON DELETE CASCADE,
    CONSTRAINT FK_ExamQuestion_Question FOREIGN KEY (QuestionId) REFERENCES Question(QuestionId) ON DELETE CASCADE,
    CONSTRAINT UQ_ExamQuestion UNIQUE (GeneratedExamId, QuestionId)  -- Tránh câu hỏi trùng trong 1 đề
);

COMMENT ON TABLE ExamQuestion IS 'Liên kết câu hỏi với đề thi';
COMMENT ON COLUMN ExamQuestion.Status IS 'Trạng thái câu hỏi trong đề';

-- ============================================
-- Indexes for Performance
-- ============================================

-- Matrix
CREATE INDEX idx_matrix_isactive ON Matrix(IsActive);
CREATE INDEX idx_matrix_createdby ON Matrix(CreatedBy);

-- MatrixDetail
CREATE INDEX idx_matrixdetail_matrixid ON MatrixDetail(MatrixId);
CREATE INDEX idx_matrixdetail_topicid ON MatrixDetail(TopicId);
CREATE INDEX idx_matrixdetail_questiontype ON MatrixDetail(QuestionType);

-- Question
CREATE INDEX idx_question_topicid ON Question(TopicId);
CREATE INDEX idx_question_type ON Question(QuestionType);
CREATE INDEX idx_question_isactive ON Question(IsActive);

-- Option
CREATE INDEX idx_option_questionid ON Option(QuestionId);
CREATE INDEX idx_option_iscorrect ON Option(IsCorrect);

-- ExamRequest
CREATE INDEX idx_examrequest_userid ON ExamRequest(UserId);
CREATE INDEX idx_examrequest_matrixid ON ExamRequest(MatrixId);
CREATE INDEX idx_examrequest_gradeid ON ExamRequest(GradeId);
CREATE INDEX idx_examrequest_status ON ExamRequest(Status);
CREATE INDEX idx_examrequest_createdat ON ExamRequest(CreatedAt);

-- GeneratedExam
CREATE INDEX idx_generatedexam_requestid ON GeneratedExam(ExamRequestId);
CREATE INDEX idx_generatedexam_status ON GeneratedExam(Status);

-- ExamQuestion
CREATE INDEX idx_examquestion_examid ON ExamQuestion(GeneratedExamId);
CREATE INDEX idx_examquestion_questionid ON ExamQuestion(QuestionId);

-- ============================================
-- Sample Data for Testing (Optional)
-- ============================================

-- Insert sample Matrix
INSERT INTO Matrix (MatrixId, Name, TotalQuestion, IsActive) VALUES
(gen_random_uuid(), 'Ma trận Hóa học lớp 10 - Học kỳ 1', 20, TRUE),
(gen_random_uuid(), 'Ma trận Hóa học lớp 11 - Học kỳ 2', 30, TRUE);

-- ============================================
-- Migration Notes
-- ============================================
-- QUAN TRỌNG: Sau khi chạy script này:
-- 1. Update các Model class (đã làm ở trên):
--    - Question.TopicId: int -> Guid ✓
--    - MatrixDetail.TopicId: int -> Guid
--    - ExamRequest.GradeId: int -> Guid
--    - ExamRequest.Prompt: xóa property
--
-- 2. Chạy EF Core commands:
--    dotnet ef dbcontext scaffold "YourConnectionString" Npgsql.EntityFrameworkCore.PostgreSQL 
--    --output-dir Models --context-dir Data --context AppDbContext --force
--
-- 3. Hoặc tạo migration mới nếu dùng Code First:
--    dotnet ef migrations add UpdateToGuidAndRemovePrompt
--    dotnet ef database update
-- ============================================

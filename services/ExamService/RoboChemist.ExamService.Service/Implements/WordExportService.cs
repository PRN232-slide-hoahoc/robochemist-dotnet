using RoboChemist.ExamService.Model.Models;
using RoboChemist.ExamService.Service.Interfaces;
using RoboChemist.Shared.Common.Helpers;
using Xceed.Document.NET;
using Xceed.Words.NET;

namespace RoboChemist.ExamService.Service.Implements
{
    /// <summary>
    /// Service for exporting exams to Word documents
    /// </summary>
    public class WordExportService : IWordExportService
    {
        /// <summary>
        /// Export exam to Word document with questions and answer key
        /// </summary>
        public async Task<byte[]> ExportExamToWordAsync(
            string matrixName,
            List<Question> questions,
            int totalQuestions,
            int? timeLimit = null)
        {
            return await Task.Run(() =>
            {
                using (var doc = DocX.Create(new MemoryStream()))
                {
                    // ===== HEADER SECTION =====
                    var title = doc.InsertParagraph("ĐỀ THI TRẮC NGHIỆM HÓA HỌC");
                    title.FontSize(16);
                    title.Bold();
                    title.SpacingAfter(10);
                    title.Alignment = Alignment.center;

                    var matrixInfo = doc.InsertParagraph($"Ma trận: {matrixName}");
                    matrixInfo.FontSize(12);
                    matrixInfo.SpacingAfter(5);
                    matrixInfo.Alignment = Alignment.center;

                    var questionCount = doc.InsertParagraph($"Tổng số câu: {totalQuestions} câu");
                    questionCount.FontSize(12);
                    questionCount.SpacingAfter(5);
                    questionCount.Alignment = Alignment.center;

                    if (timeLimit.HasValue)
                    {
                        var time = doc.InsertParagraph($"Thời gian: {timeLimit.Value} phút");
                        time.FontSize(12);
                        time.SpacingAfter(5);
                        time.Alignment = Alignment.center;
                    }

                    // Line break
                    doc.InsertParagraph("").SpacingAfter(10);

                    // ===== QUESTIONS SECTION =====
                    var labels = new[] { "A", "B", "C", "D", "E", "F" };

                    for (int i = 0; i < questions.Count; i++)
                    {
                        var question = questions[i];

                        // Question number and text - Bold with chemical formulas formatted
                        var questionPara = doc.InsertParagraph($"Câu {i + 1}: {ChemicalFormulaHelper.BeautifyChemicalFormulas(question.QuestionText)}");
                        questionPara.FontSize(11);
                        questionPara.Bold();
                        questionPara.SpacingAfter(5);

                        // Options (A, B, C, D, E, F) with chemical formulas formatted
                        var options = question.Options.OrderBy(o => o.Answer).ToList();
                        for (int j = 0; j < options.Count && j < labels.Length; j++)
                        {
                            var optionPara = doc.InsertParagraph($"  {labels[j]}. {ChemicalFormulaHelper.BeautifyChemicalFormulas(options[j].Answer)}");
                            optionPara.FontSize(11);
                            optionPara.SpacingAfter(3);
                        }

                        // Line break after each question
                        doc.InsertParagraph("").SpacingAfter(8);
                    }

                    // ===== ANSWER KEY SECTION =====
                    doc.InsertParagraph("").SpacingAfter(10);

                    // Separator line
                    var separator1 = doc.InsertParagraph(new string('═', 80));
                    separator1.FontSize(10);
                    separator1.SpacingAfter(5);

                    var answerTitle = doc.InsertParagraph("BẢNG ĐÁP ÁN");
                    answerTitle.FontSize(14);
                    answerTitle.Bold();
                    answerTitle.SpacingAfter(5);
                    answerTitle.Alignment = Alignment.center;

                    var separator2 = doc.InsertParagraph(new string('═', 80));
                    separator2.FontSize(10);
                    separator2.SpacingAfter(10);

                    // Answer key table - 5 columns
                    int rowCount = (int)Math.Ceiling(questions.Count / 5.0);

                    for (int row = 0; row < rowCount; row++)
                    {
                        var line = "";
                        for (int col = 0; col < 5; col++)
                        {
                            int index = row * 5 + col;
                            if (index >= questions.Count) break;

                            var q = questions[index];
                            var correctOption = q.Options.FirstOrDefault(o => o.IsCorrect == true);

                            string correctLabel = "?";
                            if (correctOption != null)
                            {
                                var options = q.Options.OrderBy(o => o.Answer).ToList();
                                int correctIndex = options.IndexOf(correctOption);
                                if (correctIndex >= 0 && correctIndex < labels.Length)
                                {
                                    correctLabel = labels[correctIndex];
                                }
                            }

                            line += $"Câu {index + 1}: {correctLabel}".PadRight(16);
                        }

                        var linePara = doc.InsertParagraph(line);
                        linePara.FontSize(11);
                        linePara.SpacingAfter(3);
                    }

                    // Save to MemoryStream
                    using (var ms = new MemoryStream())
                    {
                        doc.SaveAs(ms);
                        return ms.ToArray();
                    }
                }
            });
        }

        public async Task<byte[]> ExportExamQuestionsOnlyAsync(
            string matrixName,
            List<Question> questions,
            int totalQuestions,
            int? timeLimit = null)
        {
            return await Task.Run(() =>
            {
                using (var doc = DocX.Create(new MemoryStream()))
                {
                    var title = doc.InsertParagraph("ĐỀ THI TRẮC NGHIỆM HÓA HỌC");
                    title.FontSize(16);
                    title.Bold();
                    title.SpacingAfter(10);
                    title.Alignment = Alignment.center;

                    var matrixInfo = doc.InsertParagraph($"Ma trận: {matrixName}");
                    matrixInfo.FontSize(12);
                    matrixInfo.SpacingAfter(5);
                    matrixInfo.Alignment = Alignment.center;

                    var questionCount = doc.InsertParagraph($"Tổng số câu: {totalQuestions} câu");
                    questionCount.FontSize(12);
                    questionCount.SpacingAfter(5);
                    questionCount.Alignment = Alignment.center;

                    if (timeLimit.HasValue)
                    {
                        var time = doc.InsertParagraph($"Thời gian: {timeLimit.Value} phút");
                        time.FontSize(12);
                        time.SpacingAfter(5);
                        time.Alignment = Alignment.center;
                    }

                    doc.InsertParagraph("").SpacingAfter(10);

                    var labels = new[] { "A", "B", "C", "D", "E", "F" };

                    for (int i = 0; i < questions.Count; i++)
                    {
                        var question = questions[i];

                        var questionPara = doc.InsertParagraph($"Câu {i + 1}: {ChemicalFormulaHelper.BeautifyChemicalFormulas(question.QuestionText)}");
                        questionPara.FontSize(11);
                        questionPara.Bold();
                        questionPara.SpacingAfter(5);

                        var options = question.Options.OrderBy(o => o.Answer).ToList();
                        for (int j = 0; j < options.Count && j < labels.Length; j++)
                        {
                            var optionPara = doc.InsertParagraph($"  {labels[j]}. {ChemicalFormulaHelper.BeautifyChemicalFormulas(options[j].Answer)}");
                            optionPara.FontSize(11);
                            optionPara.SpacingAfter(3);
                        }

                        doc.InsertParagraph("").SpacingAfter(8);
                    }

                    using (var ms = new MemoryStream())
                    {
                        doc.SaveAs(ms);
                        return ms.ToArray();
                    }
                }
            });
        }

        public async Task<byte[]> ExportAnswerKeyOnlyAsync(
            string matrixName,
            List<Question> questions,
            int totalQuestions)
        {
            return await Task.Run(() =>
            {
                using (var doc = DocX.Create(new MemoryStream()))
                {
                    var title = doc.InsertParagraph("BẢNG ĐÁP ÁN VÀ GIẢI THÍCH");
                    title.FontSize(16);
                    title.Bold();
                    title.SpacingAfter(10);
                    title.Alignment = Alignment.center;

                    var matrixInfo = doc.InsertParagraph($"Ma trận: {matrixName}");
                    matrixInfo.FontSize(12);
                    matrixInfo.SpacingAfter(5);
                    matrixInfo.Alignment = Alignment.center;

                    var questionCount = doc.InsertParagraph($"Tổng số câu: {totalQuestions} câu");
                    questionCount.FontSize(12);
                    questionCount.SpacingAfter(15);
                    questionCount.Alignment = Alignment.center;

                    var labels = new[] { "A", "B", "C", "D", "E", "F" };

                    for (int i = 0; i < questions.Count; i++)
                    {
                        var q = questions[i];

                        // Câu hỏi
                        var questionPara = doc.InsertParagraph($"Câu {i + 1}: {ChemicalFormulaHelper.BeautifyChemicalFormulas(q.QuestionText)}");
                        questionPara.FontSize(11);
                        questionPara.Bold();
                        questionPara.SpacingAfter(5);

                        // Các đáp án
                        var options = q.Options.OrderBy(o => o.Answer).ToList();
                        for (int j = 0; j < options.Count && j < labels.Length; j++)
                        {
                            var opt = options[j];
                            var optionText = $"   {labels[j]}. {ChemicalFormulaHelper.BeautifyChemicalFormulas(opt.Answer)}";
                            
                            var optionPara = doc.InsertParagraph(optionText);
                            optionPara.FontSize(11);
                            optionPara.SpacingAfter(3);

                            if (opt.IsCorrect == true)
                            {
                                optionPara.Bold();
                                optionPara.Color(Xceed.Drawing.Color.Red);
                            }
                        }

                        // Đáp án đúng
                        var correctOption = options.FirstOrDefault(o => o.IsCorrect == true);
                        if (correctOption != null)
                        {
                            int correctIndex = options.IndexOf(correctOption);
                            string correctLabel = correctIndex >= 0 && correctIndex < labels.Length ? labels[correctIndex] : "?";
                            
                            var answerPara = doc.InsertParagraph($"Đáp án đúng: {correctLabel}");
                            answerPara.FontSize(11);
                            answerPara.Bold();
                            answerPara.Color(Xceed.Drawing.Color.Green);
                            answerPara.SpacingAfter(5);
                        }

                        // Giải thích
                        if (!string.IsNullOrWhiteSpace(q.Explanation))
                        {
                            var explanationPara = doc.InsertParagraph($"Giải thích: {ChemicalFormulaHelper.BeautifyChemicalFormulas(q.Explanation)}");
                            explanationPara.FontSize(11);
                            explanationPara.Italic();
                            explanationPara.SpacingAfter(10);
                        }
                        else
                        {
                            doc.InsertParagraph("").SpacingAfter(10);
                        }
                    }

                    using (var ms = new MemoryStream())
                    {
                        doc.SaveAs(ms);
                        return ms.ToArray();
                    }
                }
            });
        }

        // Chemical formula formatting methods have been moved to ChemicalFormulaHelper
        // in RoboChemist.Shared.Common.Helpers for reuse across services
    }
}

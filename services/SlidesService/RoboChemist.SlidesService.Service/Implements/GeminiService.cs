using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Google;
using RoboChemist.SlidesService.Service.Interfaces;
using System.Text.Json;
using static RoboChemist.Shared.DTOs.SlideDTOs.SlideRequestDTOs;
using static RoboChemist.Shared.DTOs.SlideDTOs.SlideResponseDTOs;

namespace RoboChemist.SlidesService.Service.Implements
{
    public class GeminiService : IGeminiService
    {

        private readonly Kernel _kernel;
        private readonly ILogger<GeminiService> _logger;

        public GeminiService(Kernel kernel, ILogger<GeminiService> logger)
        {
            _kernel = kernel;
            _logger = logger;
        }

        public async Task<ResponseGenerateDataDto?> GenerateSlidesAsync(DataForGenerateSlideRequest request)
        {
            var jsonStructure = """
                            {
                              "FirstSlide": {
                                "Title": "Tiêu đề bài học",
                                "Subtitle": "Phụ đề (lớp học, môn học)",
                                "Owner": "Tên giáo viên hoặc trường học"
                              },
                              "TableOfContentSlide": {
                                "Topics": ["Nội dung 1", "Nội dung 2", "Nội dung 3"]
                              },
                              "ContentSlides": [
                                {
                                  "Heading": "Tiêu đề slide nội dung",
                                  "BulletPoints": [
                                    {
                                      "Content": "Ý chính 1",
                                      "Level": 1,
                                      "Children": [
                                        {
                                          "Content": "Ý phụ 1.1",
                                          "Level": 2,
                                          "Children": [
                                            {
                                              "Content": "Ý con 1.1.1",
                                              "Level": 3,
                                              "Children": null
                                            }
                                          ]
                                        },
                                        {
                                          "Content": "Ý phụ 1.2",
                                          "Level": 2,
                                          "Children": null
                                        }
                                      ]
                                    },
                                    {
                                      "Content": "Ý chính 2",
                                      "Level": 1,
                                      "Children": null
                                    }
                                  ],
                                  "ImageDescription": "Mô tả hình ảnh minh họa (nếu cần)"
                                }
                              ]
                            }
                            """;

            var userPrompt = $"""
                            Bạn là một chuyên gia sư phạm và giáo viên Hóa học giàu kinh nghiệm, đồng thời là một chuyên gia thiết kế bài giảng trình chiếu. Nhiệm vụ của bạn là soạn thảo nội dung slide bài giảng chất lượng cao, hấp dẫn và chính xác về mặt khoa học.

                            Dưới đây là thông tin đầu vào cho bài học:
                            - Bài học: {request.Lesson}
                            - Chủ đề: {request.TopicName}
                            - Lớp: {request.GradeName}
                            - Mục tiêu bài học: {request.LearningObjectives}
                            - Dàn ý sơ bộ: {request.ContentOutline}
                            - Khái niệm/Từ khóa chính: {request.KeyConcepts}
                            - Số lượng slide mong muốn: {request.NumberOfSlides ?? 5}
                            - Yêu cầu bổ sung: {request.AiPrompt ?? "Tối ưu hóa cho việc giảng dạy trực quan"} 
                            Với tất cả những thông tin đầu vào trên nếu liên quan đến Hóa học, phương pháp dạy học hoặc cách trình bày thì hãy áp dụng nếu không thì hãy bỏ qua

                            ### NGUYÊN TẮC SOẠN THẢO NỘI DUNG (BẮT BUỘC):

                            1. **Đặc thù môn Hóa học:**
                               - Phương trình phản ứng phải cân bằng và có điều kiện phản ứng (nếu cần).
                               - Nội dung cần liên hệ thực tế hoặc thí nghiệm minh họa để bài học không khô khan.

                            2. **Cấu trúc Slide (Logic sư phạm):**
                               - Slide 1: Tiêu đề hấp dẫn + Tên bài học.
                               - Slide 2: Đặt vấn đề/Mục tiêu (Hook).
                               - Các slide nội dung: Triển khai kiến thức trọng tâm (chia nhỏ vấn đề).

                            3. **Quy tắc Bullet Points & Trình bày:**
                               - **Tuyệt đối ngắn gọn:** Mỗi ý (Content) KHÔNG QUÁ 15 từ. Dùng phong cách "điện báo" (telegraphic style), lược bỏ hư từ.
                               - **Phân cấp rõ ràng:**
                                 + Level 1: Luận điểm chính/Tên mục.
                                 + Level 2: Giải thích ngắn/Công thức/Ví dụ.
                                 + Level 3: Chi tiết nhỏ (hạn chế dùng, chỉ dùng khi liệt kê tính chất).
                               - Mỗi slide tối đa 3-5 ý chính (Level 1).
                               - Không sử dụng Markdown đậm nghiêng (**...**) trong nội dung JSON.

                            ### CẤU TRÚC DỮ LIỆU TRẢ VỀ:

                            Bạn PHẢI trả về duy nhất một chuỗi JSON hợp lệ theo cấu trúc sau (không kèm lời dẫn):

                            {jsonStructure}

                            ### VÍ DỤ VỀ CÁCH VIẾT NỘI DUNG (MÔN HÓA):

                            - Tốt: 
                              - Level 1: "Tính chất hóa học của Axit"
                                - Level 2: "Làm quỳ tím hóa đỏ"
                                - Level 2: "Tác dụng với kim loại (đứng trước H)"
                                  - Level 3: "2HCl + Fe → FeCl₂ + H₂"

                            - Xấu (Cần tránh):
                              - "Axit có tính chất là làm cho quỳ tím chuyển sang màu đỏ và tác dụng được với các kim loại..." (Quá dài dòng).
                            Hãy bắt đầu tạo nội dung slide ngay bây giờ dựa trên các nguyên tắc trên.
                            """;


            var settings = new GeminiPromptExecutionSettings
            {
                ResponseMimeType = "application/json"
            };

            try
            {
                var result = await _kernel.InvokePromptAsync(
                    userPrompt,
                    new(settings)
                );

                if (result is null || string.IsNullOrWhiteSpace(result.ToString()))
                {
                    _logger.LogWarning("AI không trả về kết quả hợp lệ (null hoặc rỗng).");
                    return null;
                }

                // Parse JSON response thành DTO
                var jsonResponse = result.ToString().Trim();
                _logger.LogInformation("AI Response: {Response}", jsonResponse);

                // Kiểm tra xem response có phải JSON hợp lệ không
                if (!jsonResponse.StartsWith('{') && !jsonResponse.StartsWith('}'))
                {
                    _logger.LogWarning("AI trả về không phải JSON. Response: {Response}", jsonResponse);
                    return null;
                }

                JsonSerializerOptions jsonSerializerOptions = new()
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    AllowTrailingCommas = true,
                    ReadCommentHandling = JsonCommentHandling.Skip
                };
                var jsonOptions = jsonSerializerOptions;

                var responseDto = JsonSerializer.Deserialize<ResponseGenerateDataDto>(
                    jsonResponse,
                    jsonOptions
                );

                if (responseDto is null)
                {
                    _logger.LogWarning("Không thể parse JSON từ AI thành ResponseGenerateDataDto. JSON: {Json}", jsonResponse);
                    return null;
                }

                if (!ValidateResponseDto(responseDto))
                {
                    _logger.LogWarning("Dữ liệu từ AI không hợp lệ (thiếu field bắt buộc).");
                    return null;
                }

                _logger.LogInformation("Tạo slide thành công cho: {Lesson}", request.Lesson);
                return responseDto;
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "Lỗi khi parse JSON từ Gemini response. Response nhận được có thể không đúng format.");
                throw new InvalidOperationException("Dữ liệu trả về từ AI không đúng định dạng JSON.", jsonEx);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gọi Semantic Kernel hoặc Gemini.");
                throw new InvalidOperationException("Lỗi từ máy chủ AI khi xử lý yêu cầu.", ex);
            }
        }

        /// <summary>
        /// Validate dữ liệu ResponseGenerateDataDto có đầy đủ các field bắt buộc
        /// </summary>
        private bool ValidateResponseDto(ResponseGenerateDataDto dto)
        {
            try
            {
                // Kiểm tra FirstSlide
                if (dto.FirstSlide is null || 
                    string.IsNullOrWhiteSpace(dto.FirstSlide.Title) ||
                    string.IsNullOrWhiteSpace(dto.FirstSlide.Subtitle))
                {
                    _logger.LogWarning("FirstSlide thiếu Title hoặc Subtitle");
                    return false;
                }

                // Kiểm tra TableOfContentSlide
                if (dto.TableOfContentSlide is null || 
                    dto.TableOfContentSlide.Topics is null || 
                    dto.TableOfContentSlide.Topics.Count == 0)
                {
                    _logger.LogWarning("TableOfContentSlide thiếu Topics hoặc Topics rỗng");
                    return false;
                }

                // Kiểm tra ContentSlides
                if (dto.ContentSlides is null || dto.ContentSlides.Count == 0)
                {
                    _logger.LogWarning("ContentSlides rỗng hoặc null");
                    return false;
                }

                // Kiểm tra từng ContentSlide
                foreach (var slide in dto.ContentSlides)
                {
                    if (string.IsNullOrWhiteSpace(slide.Heading) || 
                        slide.BulletPoints is null || 
                        slide.BulletPoints.Count == 0)
                    {
                        _logger.LogWarning("ContentSlide thiếu Heading hoặc BulletPoints rỗng");
                        return false;
                    }

                    // Validate hierarchical structure of bullet points
                    if (!ValidateBulletPoints(slide.BulletPoints))
                    {
                        _logger.LogWarning("ContentSlide có cấu trúc BulletPoints không hợp lệ");
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi validate ResponseDto");
                return false;
            }
        }

        /// <summary>
        /// Validate hierarchical bullet points structure recursively
        /// </summary>
        /// <param name="bulletPoints">List of bullet points to validate</param>
        /// <returns>True if structure is valid, false otherwise</returns>
        private bool ValidateBulletPoints(List<BulletPoint> bulletPoints)
        {
            if (bulletPoints == null || bulletPoints.Count == 0)
                return false;

            foreach (var bullet in bulletPoints)
            {
                // Validate Content field
                if (string.IsNullOrWhiteSpace(bullet.Content))
                {
                    _logger.LogWarning("BulletPoint có Content rỗng");
                    return false;
                }

                // Validate Level (should be >= 1)
                if (bullet.Level < 1)
                {
                    _logger.LogWarning("BulletPoint có Level không hợp lệ: {Level}", bullet.Level);
                    return false;
                }

                // Recursively validate children if they exist
                if (bullet.Children != null && bullet.Children.Count > 0)
                {
                    // Children should have level greater than parent
                    foreach (var child in bullet.Children)
                    {
                        if (child.Level <= bullet.Level)
                        {
                            _logger.LogWarning("Child BulletPoint có Level không lớn hơn parent. Parent Level: {ParentLevel}, Child Level: {ChildLevel}", 
                                bullet.Level, child.Level);
                            return false;
                        }
                    }

                    if (!ValidateBulletPoints(bullet.Children))
                        return false;
                }
            }

            return true;
        }
    }
}

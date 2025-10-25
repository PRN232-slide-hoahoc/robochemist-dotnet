using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Google;
using RoboChemist.SlidesService.Service.Interfaces;
using System.ComponentModel;
using System.Text.Json;
using static RoboChemist.Shared.DTOs.SlideDTOs.SlideRequestDTOs;
using static RoboChemist.Shared.DTOs.SlideDTOs.SlideResponseDTOs;

namespace RoboChemist.SlidesService.Service.Implements
{
    public class GeminiService : IGeminiService
    {

        private readonly Kernel _kernel;
        private readonly ILogger<GeminiService> _logger;

        // Kernel và Logger được tiêm (inject) vào qua Dependency Injection (DI)
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
                                  "BulletPoints": ["Điểm 1", "Điểm 2", "Điểm 3"],
                                  "ImageDescription": "Mô tả hình ảnh minh họa (nếu cần)"
                                }
                              ]
                            }
                            """;

            var userPrompt = $"""
                            Vui lòng tạo nội dung slide cho bài học sau:
                            - Bài học: {request.Lesson} (Chủ đề: {request.TopicName})
                            - Lớp: {request.GradeName}
                            - Mục tiêu: {request.LearningObjectives}
                            - Dàn ý: {request.ContentOutline}
                            - Khái niệm chính: {request.KeyConcepts}
                            - Số slide nội dung mong muốn: {request.NumberOfSlides ?? 5}
                            - Hướng dẫn thêm: {request.AiPrompt ?? "Không có"}
        
                            Bạn PHẢI trả về dữ liệu cấu trúc dưới dạng JSON với định dạng sau:
                            {jsonStructure}
                            """;

            var settings = new GeminiPromptExecutionSettings
            {
                ResponseMimeType = "application/json" // Yêu cầu Gemini trả về JSON
            };

            try
            {
                // Gọi Gemini và nhận response
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
                if (!jsonResponse.StartsWith("{") && !jsonResponse.StartsWith("["))
                {
                    _logger.LogWarning("AI trả về không phải JSON. Response: {Response}", jsonResponse);
                    return null;
                }

                var jsonOptions = new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    AllowTrailingCommas = true,
                    ReadCommentHandling = JsonCommentHandling.Skip
                };

                var responseDto = JsonSerializer.Deserialize<ResponseGenerateDataDto>(
                    jsonResponse,
                    jsonOptions
                );

                if (responseDto is null)
                {
                    _logger.LogWarning("Không thể parse JSON từ AI thành ResponseGenerateDataDto. JSON: {Json}", jsonResponse);
                    return null;
                }

                // Validate dữ liệu sau khi parse
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
                throw new ApplicationException("Dữ liệu trả về từ AI không đúng định dạng JSON.", jsonEx);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gọi Semantic Kernel hoặc Gemini.");
                throw new ApplicationException("Lỗi từ máy chủ AI khi xử lý yêu cầu.", ex);
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
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi validate ResponseDto");
                return false;
            }
        }
    }
}

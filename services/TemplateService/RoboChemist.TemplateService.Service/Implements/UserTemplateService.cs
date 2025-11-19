using Microsoft.Extensions.Logging;
using RoboChemist.Shared.Common.Constants;
using RoboChemist.Shared.DTOs.Common;
using RoboChemist.Shared.DTOs.UserDTOs;
using RoboChemist.TemplateService.Model.DTOs;
using RoboChemist.TemplateService.Model.Models;
using RoboChemist.TemplateService.Repository.Interfaces;
using RoboChemist.TemplateService.Service.HttpClients;
using RoboChemist.TemplateService.Service.Interfaces;
using static RoboChemist.Shared.DTOs.WalletServiceDTOs.WalletTransactionDTOs;

namespace RoboChemist.TemplateService.Service.Implements;

/// <summary>
/// UserTemplate service implementation
/// </summary>
public class UserTemplateService : IUserTemplateService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthServiceClient _authServiceClient;
    private readonly IStorageService _storageService;
    private readonly IWalletServiceClient _walletServiceClient;
    private readonly ILogger<UserTemplateService> _logger;

    public UserTemplateService(
        IUnitOfWork unitOfWork,
        IAuthServiceClient authServiceClient,
        IStorageService storageService,
        IWalletServiceClient walletServiceClient,
        ILogger<UserTemplateService> logger)
    {
        _unitOfWork = unitOfWork;
        _authServiceClient = authServiceClient;
        _storageService = storageService;
        _walletServiceClient = walletServiceClient;
        _logger = logger;
    }

    public async Task<ApiResponse<IEnumerable<UserTemplateResponse>>> GetMyTemplatesAsync()
    {
        // Get current user
        UserDto? user = await _authServiceClient.GetCurrentUserAsync();
        if (user == null)
        {
            return ApiResponse<IEnumerable<UserTemplateResponse>>.ErrorResult("User not authenticated");
        }

        // Get all active free templates
        var freeTemplates = await _unitOfWork.Templates.GetFreeTemplatesAsync();
        
        // Get premium templates that user owns
        var userPremiumTemplates = await _unitOfWork.UserTemplates.GetUserTemplatesByUserIdAsync(user.Id);
        
        // Combine both lists
        var allTemplates = new List<UserTemplateResponse>();
        
        // Add free templates
        allTemplates.AddRange(freeTemplates.Select(t => new UserTemplateResponse
        {
            TemplateId = t.TemplateId,
            ObjectKey = t.ObjectKey,
            TemplateName = t.TemplateName,
            Description = t.Description,
            SlideCount = t.SlideCount,
            IsPremium = t.IsPremium,
            Price = t.Price,
            DownloadCount = t.DownloadCount,
            CreatedAt = t.CreatedAt,
            UpdatedAt = t.UpdatedAt,
            CreatedBy = t.CreatedBy
        }));
        
        // Add user's premium templates
        allTemplates.AddRange(userPremiumTemplates.Select(ut => new UserTemplateResponse
        {
            TemplateId = ut.Template.TemplateId,
            ObjectKey = ut.Template.ObjectKey,
            TemplateName = ut.Template.TemplateName,
            Description = ut.Template.Description,
            SlideCount = ut.Template.SlideCount,
            IsPremium = ut.Template.IsPremium,
            Price = ut.Template.Price,
            DownloadCount = ut.Template.DownloadCount,
            CreatedAt = ut.Template.CreatedAt,
            UpdatedAt = ut.Template.UpdatedAt,
            CreatedBy = ut.Template.CreatedBy
        }));
        
        // Remove duplicates and sort by CreatedAt descending
        var distinctTemplates = allTemplates
            .GroupBy(t => t.TemplateId)
            .Select(g => g.First())
            .OrderByDescending(t => t.CreatedAt)
            .ToList();

        // Generate presigned URLs for thumbnails
        await GenerateThumbnailPresignedUrlsAsync(distinctTemplates);

        return ApiResponse<IEnumerable<UserTemplateResponse>>.SuccessResult(distinctTemplates, "Retrieved user templates successfully");
    }
    
    /// <summary>
    /// Generate presigned URLs for template thumbnails
    /// </summary>
    private async Task GenerateThumbnailPresignedUrlsAsync(IEnumerable<UserTemplateResponse> templates)
    {
        foreach (var template in templates.Where(t => !string.IsNullOrEmpty(t.ObjectKey)))
        {
            // Get the actual template entity to access ThumbnailKey
            var templateEntity = await _unitOfWork.Templates.GetByIdAsync(template.TemplateId);
            
            if (templateEntity != null && !string.IsNullOrEmpty(templateEntity.ThumbnailKey))
            {
                // Generate presigned URL from thumbnail key (7 days = 10,080 minutes)
                try
                {
                    template.ThumbnailUrl = await _storageService.GeneratePresignedUrlAsync(templateEntity.ThumbnailKey, 10080);
                }
                catch (Exception ex)
                {
                    // If generation fails, set to null and log
                    Console.WriteLine($"Failed to generate presigned URL for {template.TemplateId}: {ex.Message}");
                    template.ThumbnailUrl = null;
                }
            }
        }
    }

    public async Task<ApiResponse<bool>> CheckTemplateAccessAsync(Guid templateId)
    {
        // Get current user
        UserDto? user = await _authServiceClient.GetCurrentUserAsync();
        if (user == null)
        {
            return ApiResponse<bool>.ErrorResult("User not authenticated");
        }

        // Check if user has access
        bool hasAccess = await _unitOfWork.UserTemplates.UserHasTemplateAsync(user.Id, templateId);
        
        return ApiResponse<bool>.SuccessResult(hasAccess, hasAccess ? "User has access" : "User does not have access");
    }

    public async Task<ApiResponse<UserTemplateResponse>> GrantTemplateAccessAsync(GrantTemplateAccessRequest request)
    {
        // Get current user
        UserDto? user = await _authServiceClient.GetCurrentUserAsync();
        if (user == null)
        {
            return ApiResponse<UserTemplateResponse>.ErrorResult("User not authenticated");
        }

        // Validate template exists
        var template = await _unitOfWork.Templates.GetByIdAsync(request.TemplateId);
        if (template == null)
        {
            return ApiResponse<UserTemplateResponse>.ErrorResult("Template not found");
        }

        // Check if user already has this template
        bool alreadyHas = await _unitOfWork.UserTemplates.UserHasTemplateAsync(user.Id, request.TemplateId);
        if (alreadyHas)
        {
            return ApiResponse<UserTemplateResponse>.ErrorResult("User already has access to this template");
        }

        // Create user template
        var userTemplate = new UserTemplate
        {
            UserTemplateId = Guid.NewGuid(),
            UserId = user.Id,
            TemplateId = request.TemplateId
        };

        await _unitOfWork.UserTemplates.CreateAsync(userTemplate);

        var response = new UserTemplateResponse
        {
            TemplateId = template.TemplateId,
            ObjectKey = template.ObjectKey,
            TemplateName = template.TemplateName,
            Description = template.Description,
            SlideCount = template.SlideCount,
            IsPremium = template.IsPremium,
            Price = template.Price,
            DownloadCount = template.DownloadCount,
            CreatedAt = template.CreatedAt,
            UpdatedAt = template.UpdatedAt,
            CreatedBy = template.CreatedBy
        };

        return ApiResponse<UserTemplateResponse>.SuccessResult(response, "Template access granted successfully");
    }

    public async Task<ApiResponse<bool>> RevokeTemplateAccessAsync(Guid userTemplateId)
    {
        var userTemplate = await _unitOfWork.UserTemplates.GetByIdAsync(userTemplateId);
        if (userTemplate == null)
        {
            return ApiResponse<bool>.ErrorResult("User template not found");
        }

        await _unitOfWork.UserTemplates.RemoveAsync(userTemplate);

        return ApiResponse<bool>.SuccessResult(true, "Template access revoked successfully");
    }

    public async Task<ApiResponse<IEnumerable<UserTemplateResponse>>> GetUserTemplatesByUserIdAsync(Guid userId)
    {
        var userTemplates = await _unitOfWork.UserTemplates.GetUserTemplatesByUserIdAsync(userId);
        
        var response = userTemplates.Select(ut => new UserTemplateResponse
        {
            TemplateId = ut.Template.TemplateId,
            ObjectKey = ut.Template.ObjectKey,
            TemplateName = ut.Template.TemplateName,
            Description = ut.Template.Description,
            SlideCount = ut.Template.SlideCount,
            IsPremium = ut.Template.IsPremium,
            Price = ut.Template.Price,
            DownloadCount = ut.Template.DownloadCount,
            CreatedAt = ut.Template.CreatedAt,
            UpdatedAt = ut.Template.UpdatedAt,
            CreatedBy = ut.Template.CreatedBy
        });

        return ApiResponse<IEnumerable<UserTemplateResponse>>.SuccessResult(response, "Retrieved user templates successfully");
    }

    public async Task<ApiResponse<PurchaseTemplateResponse>> PurchaseTemplateAsync(PurchaseTemplateRequest request)
    {
        // Get current user
        UserDto? user = await _authServiceClient.GetCurrentUserAsync();
        if (user == null)
        {
            return ApiResponse<PurchaseTemplateResponse>.ErrorResult("User not authenticated");
        }

        // Validate template exists and is active
        var template = await _unitOfWork.Templates.GetByIdAsync(request.TemplateId);
        if (template == null)
        {
            return ApiResponse<PurchaseTemplateResponse>.ErrorResult("Template not found");
        }

        if (!template.IsActive)
        {
            return ApiResponse<PurchaseTemplateResponse>.ErrorResult("Template is not available for purchase");
        }

        if (!template.IsPremium)
        {
            return ApiResponse<PurchaseTemplateResponse>.ErrorResult("This template is free, no purchase needed");
        }

        // Check if user already owns this template
        bool alreadyOwns = await _unitOfWork.UserTemplates.UserHasTemplateAsync(user.Id, request.TemplateId);
        if (alreadyOwns)
        {
            return ApiResponse<PurchaseTemplateResponse>.ErrorResult("You already own this template");
        }

        // Step 1: Create Order (pending)
        var order = new Order
        {
            OrderId = Guid.NewGuid(),
            UserId = user.Id,
            OrderNumber = GenerateOrderNumber(),
            TotalAmount = template.Price,
            Status = RoboChemistConstants.ORDER_STATUS_PENDING,
            Notes = $"Purchase template: {template.TemplateName}",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var orderDetail = new OrderDetail
        {
            OrderDetailId = Guid.NewGuid(),
            OrderId = order.OrderId,
            TemplateId = request.TemplateId,
            Subtotal = template.Price,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Orders.CreateAsync(order);
        await _unitOfWork.OrderDetails.CreateAsync(orderDetail);

        // Step 2: Process payment via WalletService
        var paymentRequest = new CreatePaymentDto
        {
            UserId = user.Id,
            Amount = template.Price,
            ReferenceId = order.OrderId, // Use OrderId as ReferenceId
            ReferenceType = RoboChemistConstants.PAYMENT_REF_TEMPLATE_PURCHASE,
            Description = $"Order {order.OrderNumber} - Purchase template: {template.TemplateName}"
        };

        var paymentResult = await _walletServiceClient.CreatePaymentAsync(paymentRequest);
        
        if (paymentResult == null || !paymentResult.Success)
        {
            // Payment failed - cancel order
            order.Status = RoboChemistConstants.ORDER_STATUS_CANCELLED;
            order.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Orders.UpdateAsync(order);
            
            return ApiResponse<PurchaseTemplateResponse>.ErrorResult(
                paymentResult?.Message ?? "Payment failed");
        }

        // Step 3: Update order status and payment info
        order.Status = RoboChemistConstants.ORDER_STATUS_COMPLETED;
        order.PaymentTransactionId = paymentResult.Data.TransactionId.ToString();
        order.PaymentDate = paymentResult.Data.CreateAt.ToUniversalTime();
        order.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Orders.UpdateAsync(order);

        // Step 4: Grant template access
        try
        {
            var userTemplate = new UserTemplate
            {
                UserTemplateId = Guid.NewGuid(),
                UserId = user.Id,
                TemplateId = request.TemplateId
            };

            await _unitOfWork.UserTemplates.CreateAsync(userTemplate);

            // Step 5: Return success response
            var response = new PurchaseTemplateResponse
            {
                TransactionId = paymentResult.Data.TransactionId,
                TemplateId = template.TemplateId,
                TemplateName = template.TemplateName,
                Amount = paymentResult.Data.Amount,
                NewBalance = paymentResult.Data.NewBalance,
                Status = paymentResult.Data.Status,
                PurchasedAt = paymentResult.Data.CreateAt
            };

            return ApiResponse<PurchaseTemplateResponse>.SuccessResult(response, "Template purchased successfully");
        }
        catch (Exception ex)
        {
            // Failed to grant access - attempt automatic refund
            _logger.LogError(ex, $"Failed to grant template access for Order {order.OrderNumber}");
            
            try
            {
                var refundRequest = new RefundRequestDto
                {
                    ReferenceId = paymentResult.Data.TransactionId,
                    Reason = $"Auto-refund: Failed to grant template access - {ex.Message}"
                };

                var refundResult = await _walletServiceClient.RefundAsync(refundRequest);
                
                if (refundResult != null && refundResult.Success && refundResult.Data != null)
                {
                    // Refund successful - update order status
                    order.Status = RoboChemistConstants.ORDER_STATUS_CANCELLED;
                    order.Notes += $" | AUTO-REFUND: {refundResult.Data.RefundTransactionId} - {ex.Message}";
                    order.UpdatedAt = DateTime.UtcNow;
                    await _unitOfWork.Orders.UpdateAsync(order);
                    
                    return ApiResponse<PurchaseTemplateResponse>.ErrorResult(
                        $"Failed to grant template access. Payment has been refunded automatically. Refund ID: {refundResult.Data.RefundTransactionId}");
                }
                else
                {
                    // Refund failed - manual intervention required
                    order.Status = RoboChemistConstants.ORDER_STATUS_FAILED;
                    order.Notes += $" | REFUND FAILED - MANUAL INTERVENTION REQUIRED: {ex.Message}";
                    order.UpdatedAt = DateTime.UtcNow;
                    await _unitOfWork.Orders.UpdateAsync(order);
                    
                    return ApiResponse<PurchaseTemplateResponse>.ErrorResult(
                        $"Payment successful but failed to grant template access AND automatic refund failed. Please contact support immediately. Order: {order.OrderNumber}");
                }
            }
            catch (Exception refundEx)
            {
                _logger.LogError(refundEx, $"Refund attempt failed for Order {order.OrderNumber}");
                
                // Refund attempt crashed - mark for manual resolution
                order.Status = RoboChemistConstants.ORDER_STATUS_FAILED;
                order.Notes += $" | CRITICAL: Grant failed + Refund crashed - {ex.Message} | Refund error: {refundEx.Message}";
                order.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.Orders.UpdateAsync(order);
                
                return ApiResponse<PurchaseTemplateResponse>.ErrorResult(
                    $"Critical error: Payment completed but access grant failed and refund system error. Support ticket required. Order: {order.OrderNumber}");
            }
        }
    }

    /// <summary>
    /// Generate unique order number
    /// </summary>
    private string GenerateOrderNumber()
    {
        return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}";
    }
}

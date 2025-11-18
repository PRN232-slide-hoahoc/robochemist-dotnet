using Microsoft.AspNetCore.Mvc;
using RoboChemist.TemplateService.Model.DTOs;
using RoboChemist.TemplateService.Service.Interfaces;
using RoboChemist.Shared.DTOs.Common;

namespace RoboChemist.TemplateService.API.Controllers;

/// <summary>
/// Order management controller
/// </summary>
[ApiController]
[Route("api/v1/orders")]
[Produces("application/json")]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    /// <summary>
    /// Create a new order
    /// </summary>
    /// <param name="request">Order creation request</param>
    /// <returns>Created order details</returns>
    /// <response code="201">Order created successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="404">Template not found</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<OrderResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<OrderResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<OrderResponse>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<OrderResponse>>> CreateOrder([FromBody] CreateOrderRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResponse<OrderResponse>.ErrorResult("Dữ liệu xác thực không hợp lệ", errors));
            }

            var response = await _orderService.CreateOrderAsync(request);

            if (!response.Success)
                return BadRequest(response);

            return CreatedAtAction(
                nameof(GetOrderById),
                new { orderId = response.Data.OrderId },
                response);
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<OrderResponse>.ErrorResult("Lỗi hệ thống khi tạo order"));
        }
    }

    /// <summary>
    /// Get order by ID
    /// </summary>
    /// <param name="orderId">Order ID</param>
    /// <returns>Order details</returns>
    /// <response code="200">Order found</response>
    /// <response code="404">Order not found</response>
    [HttpGet("{orderId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<OrderResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<OrderResponse>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<OrderResponse>>> GetOrderById(Guid orderId)
    {
        try
        {
            var response = await _orderService.GetOrderByIdAsync(orderId);
            
            if (!response.Success)
                return NotFound(response);

            return Ok(response);
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<OrderResponse>.ErrorResult("Lỗi hệ thống khi lấy thông tin order"));
        }
    }

    /// <summary>
    /// Get order by order number
    /// </summary>
    /// <param name="orderNumber">Order number</param>
    /// <returns>Order details</returns>
    /// <response code="200">Order found</response>
    /// <response code="404">Order not found</response>
    [HttpGet("by-number/{orderNumber}")]
    [ProducesResponseType(typeof(ApiResponse<OrderResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<OrderResponse>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<OrderResponse>>> GetOrderByOrderNumber(string orderNumber)
    {
        try
        {
            var response = await _orderService.GetOrderByOrderNumberAsync(orderNumber);
            
            if (!response.Success)
                return NotFound(response);

            return Ok(response);
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<OrderResponse>.ErrorResult("Lỗi hệ thống khi lấy thông tin order"));
        }
    }

    /// <summary>
    /// Get all orders for a specific user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>List of user orders</returns>
    /// <response code="200">Orders retrieved successfully</response>
    [HttpGet("user/{userId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<OrderSummaryResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<OrderSummaryResponse>>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<IEnumerable<OrderSummaryResponse>>>> GetUserOrders(Guid userId)
    {
        try
        {
            var response = await _orderService.GetUserOrdersAsync(userId);
            
            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<IEnumerable<OrderSummaryResponse>>.ErrorResult("Lỗi hệ thống khi lấy danh sách order"));
        }
    }

    /// <summary>
    /// Get all orders (admin)
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10, max: 100)</param>
    /// <returns>Paginated list of orders</returns>
    /// <response code="200">Orders retrieved successfully</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<OrderSummaryResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<OrderSummaryResponse>>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<PagedResult<OrderSummaryResponse>>>> GetAllOrders(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var paginationParams = new PaginationParams
            {
                PageNumber = pageNumber,
                PageSize = Math.Min(pageSize, 100) // Max 100 items per page
            };

            var response = await _orderService.GetAllOrdersAsync(paginationParams);
            
            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<PagedResult<OrderSummaryResponse>>.ErrorResult("Lỗi hệ thống khi lấy danh sách order"));
        }
    }

    /// <summary>
    /// Update order status
    /// </summary>
    /// <param name="orderId">Order ID</param>
    /// <param name="request">Status update request</param>
    /// <returns>Updated order details</returns>
    /// <response code="200">Status updated successfully</response>
    /// <response code="400">Invalid status transition</response>
    /// <response code="404">Order not found</response>
    [HttpPatch("{orderId:guid}/status")]
    [ProducesResponseType(typeof(ApiResponse<OrderResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<OrderResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<OrderResponse>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<OrderResponse>>> UpdateOrderStatus(
        Guid orderId,
        [FromBody] UpdateOrderStatusRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResponse<OrderResponse>.ErrorResult("Dữ liệu xác thực không hợp lệ", errors));
            }

            var response = await _orderService.UpdateOrderStatusAsync(orderId, request);
            
            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<OrderResponse>.ErrorResult("Lỗi hệ thống khi cập nhật trạng thái order"));
        }
    }

    /// <summary>
    /// Cancel an order
    /// </summary>
    /// <param name="orderId">Order ID</param>
    /// <returns>Cancelled order details</returns>
    /// <response code="200">Order cancelled successfully</response>
    /// <response code="400">Cannot cancel order</response>
    /// <response code="404">Order not found</response>
    [HttpPost("{orderId:guid}/cancel")]
    [ProducesResponseType(typeof(ApiResponse<OrderResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<OrderResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<OrderResponse>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<OrderResponse>>> CancelOrder(Guid orderId)
    {
        try
        {
            var response = await _orderService.CancelOrderAsync(orderId);
            
            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<OrderResponse>.ErrorResult("Lỗi hệ thống khi hủy order"));
        }
    }

    /// <summary>
    /// Get order statistics for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>Order statistics</returns>
    /// <response code="200">Statistics retrieved successfully</response>
    [HttpGet("user/{userId:guid}/statistics")]
    [ProducesResponseType(typeof(ApiResponse<OrderStatistics>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<OrderStatistics>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<OrderStatistics>>> GetOrderStatistics(Guid userId)
    {
        try
        {
            var response = await _orderService.GetOrderStatisticsByUserAsync(userId);
            
            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<OrderStatistics>.ErrorResult("Lỗi hệ thống khi lấy thống kê order"));
        }
    }
}

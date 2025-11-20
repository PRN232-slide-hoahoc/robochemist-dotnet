using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoboChemist.TemplateService.Model.DTOs;
using RoboChemist.TemplateService.Service.Interfaces;
using RoboChemist.Shared.DTOs.Common;
using RoboChemist.Shared.Common.Constants;

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
    /// Get order by ID
    /// </summary>
    /// <param name="orderId">Order ID</param>
    /// <returns>Order details</returns>
    /// <response code="200">Order found</response>
    /// <response code="404">Order not found</response>
    [HttpGet("{orderId:guid}")]
    [Authorize(Roles = $"{RoboChemistConstants.ROLE_USER},{RoboChemistConstants.ROLE_STAFF}")]
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
    /// Get all orders for a specific user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>List of user orders</returns>
    /// <response code="200">Orders retrieved successfully</response>
    [HttpGet("user/{userId:guid}")]
    [Authorize(Roles = $"{RoboChemistConstants.ROLE_USER},{RoboChemistConstants.ROLE_STAFF}")]
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
    [Authorize(Roles = RoboChemistConstants.ROLE_STAFF)]
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
}

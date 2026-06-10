using System.Security.Claims;
using ArunVastra.Application.DTOs.SaleVouchers;
using ArunVastra.Application.Interfaces;
using ArunVastra.Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Swashbuckle.AspNetCore.Annotations;

namespace ArunVastra.Api.Controllers.SaleVouchers;

[ApiController]
[Authorize]
[Route("api/sale-vouchers")]
[Produces("application/json")]
[SwaggerTag("Sale voucher endpoints backed by dbo.SALEVOUCHERS, dbo.SALEVOUCHERDETAILS, and dbo.VOUCHERSTATUS.")]
public sealed class SaleVouchersController(ISaleVoucherService saleVoucherService) : ControllerBase
{
    private readonly ISaleVoucherService _saleVoucherService = saleVoucherService;

    [HttpGet]
    [SwaggerOperation(
        Summary = "List sale vouchers",
        Description = "Returns the first page of sale vouchers. Supplier users only see vouchers for their own companies.")]
    [ProducesResponseType(typeof(SaleVoucherListResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<SaleVoucherListResponse>> List(CancellationToken cancellationToken)
    {
        var vouchers = await _saleVoucherService.ListAsync(
            new SaleVoucherListRequest(),
            GetCurrentUser(),
            cancellationToken);

        return Ok(vouchers);
    }

    [HttpPost("search")]
    [Consumes("application/json")]
    [SwaggerOperation(
        Summary = "Search sale vouchers",
        Description = "Returns paged sale vouchers with filters, sorting, and paging supplied in the request body.")]
    [ProducesResponseType(typeof(SaleVoucherListResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<SaleVoucherListResponse>> Search(
        [FromBody] SaleVoucherListRequest request,
        CancellationToken cancellationToken)
    {
        var vouchers = await _saleVoucherService.ListAsync(request, GetCurrentUser(), cancellationToken);

        return Ok(vouchers);
    }

    [HttpGet("{saleVoucherId:int}")]
    [SwaggerOperation(
        Summary = "Get sale voucher",
        Description = "Returns sale voucher header and product lines by SVID.")]
    [ProducesResponseType(typeof(SaleVoucherResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SaleVoucherResponse>> GetById(
        int saleVoucherId,
        CancellationToken cancellationToken)
    {
        var voucher = await _saleVoucherService.GetByIdAsync(
            saleVoucherId,
            GetCurrentUser(),
            cancellationToken);

        if (voucher is null)
        {
            return NotFound();
        }

        return Ok(voucher);
    }

    [HttpGet("{saleVoucherId:int}/print")]
    [SwaggerOperation(
        Summary = "Get sale voucher print data",
        Description = "Returns the same voucher header, product lines, and totals used by duplicate print and sticker print screens.")]
    [ProducesResponseType(typeof(SaleVoucherResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SaleVoucherResponse>> GetPrintData(
        int saleVoucherId,
        CancellationToken cancellationToken)
    {
        var voucher = await _saleVoucherService.GetByIdAsync(
            saleVoucherId,
            GetCurrentUser(),
            cancellationToken);

        if (voucher is null)
        {
            return NotFound();
        }

        return Ok(voucher);
    }

    [HttpGet("{saleVoucherId:int}/status-history")]
    [SwaggerOperation(
        Summary = "Get sale voucher status history",
        Description = "Returns VOUCHERSTATUS rows for the sale voucher.")]
    [ProducesResponseType(typeof(IReadOnlyList<VoucherStatusHistoryResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<VoucherStatusHistoryResponse>>> GetStatusHistory(
        int saleVoucherId,
        CancellationToken cancellationToken)
    {
        var history = await _saleVoucherService.GetStatusHistoryAsync(
            saleVoucherId,
            GetCurrentUser(),
            cancellationToken);

        return Ok(history);
    }

    [HttpGet("floors")]
    [SwaggerOperation(
        Summary = "List active floors",
        Description = "Returns floors where STATUS = Y.")]
    [ProducesResponseType(typeof(IReadOnlyList<FloorResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<FloorResponse>>> ListFloors(CancellationToken cancellationToken)
    {
        var floors = await _saleVoucherService.ListFloorsAsync(cancellationToken);

        return Ok(floors);
    }

    [HttpGet("supplier-filter-options")]
    [SwaggerOperation(
        Summary = "List sale voucher supplier filter options",
        Description = "Returns distinct suppliers from sale vouchers using the same SALEVOUCHERS, COMPANIES, and USERS join path used by the list endpoint.")]
    [ProducesResponseType(typeof(IReadOnlyList<SaleVoucherSupplierFilterOptionResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<SaleVoucherSupplierFilterOptionResponse>>> ListSupplierFilterOptions(
        CancellationToken cancellationToken)
    {
        var suppliers = await _saleVoucherService.ListSupplierFilterOptionsAsync(
            GetCurrentUser(),
            cancellationToken);

        return Ok(suppliers);
    }

    [HttpGet("company-filter-options")]
    [SwaggerOperation(
        Summary = "List sale voucher company filter options",
        Description = "Returns distinct companies from sale vouchers using the same SALEVOUCHERS and COMPANIES join path used by the list endpoint.")]
    [ProducesResponseType(typeof(IReadOnlyList<SaleVoucherCompanyFilterOptionResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<SaleVoucherCompanyFilterOptionResponse>>> ListCompanyFilterOptions(
        CancellationToken cancellationToken)
    {
        var companies = await _saleVoucherService.ListCompanyFilterOptionsAsync(
            GetCurrentUser(),
            cancellationToken);

        return Ok(companies);
    }

    [HttpGet("floor-filter-options")]
    [SwaggerOperation(
        Summary = "List sale voucher floor filter options",
        Description = "Returns distinct floors from sale vouchers using the same SALEVOUCHERS.FLOORID to FLOORS join path used by the list endpoint.")]
    [ProducesResponseType(typeof(IReadOnlyList<SaleVoucherFloorFilterOptionResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<SaleVoucherFloorFilterOptionResponse>>> ListFloorFilterOptions(
        CancellationToken cancellationToken)
    {
        var floors = await _saleVoucherService.ListFloorFilterOptionsAsync(
            GetCurrentUser(),
            cancellationToken);

        return Ok(floors);
    }

    [HttpPost]
    [Consumes("application/json")]
    [SwaggerOperation(
        Summary = "Create sale voucher",
        Description = "Creates SALEVOUCHERS and SALEVOUCHERDETAILS rows, then records the initial VOUCHERSTATUS row. Supported statuses are Ready = 0, Enter = 6, Open = 7, and Cancel = 8.")]
    [ProducesResponseType(typeof(SaleVoucherResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SaleVoucherResponse>> Create(
        [FromBody] CreateSaleVoucherRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var voucher = await _saleVoucherService.CreateAsync(
                request,
                GetCurrentUser(),
                cancellationToken);

            return CreatedAtAction(nameof(GetById), new { saleVoucherId = voucher.SaleVoucherId }, voucher);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{saleVoucherId:int}")]
    [Consumes("application/json")]
    [SwaggerOperation(
        Summary = "Update sale voucher",
        Description = "Updates sale voucher header and upserts supplied product lines. Missing lines are not deleted by this endpoint.")]
    [ProducesResponseType(typeof(SaleVoucherResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SaleVoucherResponse>> Update(
        int saleVoucherId,
        [FromBody] UpdateSaleVoucherRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var voucher = await _saleVoucherService.UpdateAsync(
                saleVoucherId,
                request,
                GetCurrentUser(),
                cancellationToken);

            if (voucher is null)
            {
                return NotFound();
            }

            return Ok(voucher);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{saleVoucherId:int}/status")]
    [Consumes("application/json")]
    [SwaggerOperation(
        Summary = "Change sale voucher status",
        Description = "Updates SALEVOUCHERS.STATUS and inserts VOUCHERSTATUS history in one transaction. Supported statuses are Ready = 0, Enter = 6, Open = 7, and Cancel = 8.")]
    [ProducesResponseType(typeof(SaleVoucherResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SaleVoucherResponse>> ChangeStatus(
        int saleVoucherId,
        [FromBody] ChangeSaleVoucherStatusRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var voucher = await _saleVoucherService.ChangeStatusAsync(
                saleVoucherId,
                request,
                GetCurrentUser(),
                cancellationToken);

            if (voucher is null)
            {
                return NotFound();
            }

            return Ok(voucher);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{saleVoucherId:int}/cancel")]
    [Consumes("application/json")]
    [SwaggerOperation(
        Summary = "Cancel sale voucher",
        Description = "Sets SALEVOUCHERS.STATUS to Cancel = 8 and inserts VOUCHERSTATUS history in one transaction. The request body is optional.")]
    [ProducesResponseType(typeof(SaleVoucherResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SaleVoucherResponse>> Cancel(
        int saleVoucherId,
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] CancelSaleVoucherRequest? request,
        CancellationToken cancellationToken)
    {
        try
        {
            var voucher = await _saleVoucherService.CancelAsync(
                saleVoucherId,
                request,
                GetCurrentUser(),
                cancellationToken);

            if (voucher is null)
            {
                return NotFound();
            }

            return Ok(voucher);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{saleVoucherId:int}")]
    [SwaggerOperation(
        Summary = "Delete sale voucher",
        Description = "Deletes VOUCHERSTATUS history, SALEVOUCHERDETAILS rows, and the SALEVOUCHERS header in one transaction. Allowed for Internal and Admin users.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        int saleVoucherId,
        CancellationToken cancellationToken)
    {
        try
        {
            var deleted = await _saleVoucherService.DeleteAsync(
                saleVoucherId,
                GetCurrentUser(),
                cancellationToken);

            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private CurrentUserContext GetCurrentUser()
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var roleValue = User.FindFirstValue(ClaimTypes.Role);

        if (!int.TryParse(userIdValue, out var userId))
        {
            throw new InvalidOperationException("Current user id is missing from token.");
        }

        _ = int.TryParse(roleValue, out var role);

        return new CurrentUserContext
        {
            UserId = userId,
            Role = role
        };
    }
}

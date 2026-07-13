using ArunVastra.Api.Controllers;
using ArunVastra.Application.DTOs.Categories;
using ArunVastra.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ArunVastra.Api.Controllers.Categories;

[Route("api/categories")]
[SwaggerTag("Product category endpoints. These endpoints read and write dbo.PRODUCTS but expose Category naming.")]
public sealed class CategoriesController(ICategoryService categoryService) : ApiControllerBase
{
    private readonly ICategoryService _categoryService = categoryService;

    [HttpGet]
    [SwaggerOperation(
        Summary = "List categories",
        Description = "Returns all product categories from dbo.PRODUCTS ordered by product name.")]
    [ProducesResponseType(typeof(IReadOnlyList<CategoryResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CategoryResponse>>> List(
        CancellationToken cancellationToken)
    {
        var categories = await _categoryService.ListAsync(cancellationToken);

        return Ok(categories);
    }

    [HttpGet("{categoryId:int}")]
    [SwaggerOperation(
        Summary = "Get category",
        Description = "Returns one product category from dbo.PRODUCTS by PRODID.")]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoryResponse>> GetById(
        int categoryId,
        CancellationToken cancellationToken)
    {
        try
        {
            var category = await _categoryService.GetByIdAsync(categoryId, cancellationToken);

            if (category is null)
            {
                return NotFound();
            }

            return Ok(category);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost]
    [Consumes("application/json")]
    [SwaggerOperation(
        Summary = "Create category",
        Description = "Creates a product category in dbo.PRODUCTS. GST is stored in both GSTPER and GSTPERNEW.")]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CategoryResponse>> Create(
        [FromBody] CreateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var category = await _categoryService.CreateAsync(request, cancellationToken);

            return CreatedAtAction(
                nameof(GetById),
                new { categoryId = category.CategoryId },
                category);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{categoryId:int}")]
    [Consumes("application/json")]
    [SwaggerOperation(
        Summary = "Update category",
        Description = "Updates a product category in dbo.PRODUCTS. GST is stored in both GSTPER and GSTPERNEW.")]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoryResponse>> Update(
        int categoryId,
        [FromBody] UpdateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var category = await _categoryService.UpdateAsync(categoryId, request, cancellationToken);

            if (category is null)
            {
                return NotFound();
            }

            return Ok(category);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

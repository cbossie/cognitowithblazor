using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using webapi.Model;
using webapi.Service;


namespace webapi.Controllers;

[Route("[controller]")]
[ApiController]
public class DataItemController : ControllerBase
{
    IDataService Svc { get; init; }
    public DataItemController(IDataService svc)
    {
        Svc = svc;
    }


    [HttpGet]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    [Authorize]

    public async Task<ActionResult<List<DynamoDataItem>>> Get(string? id)
    {
        List<DynamoDataItem> items = new();

        var data = await Svc.GetDataItem(id ?? string.Empty);
        if(data is null)
        {
            return NotFound(id);
        }
        items.Add(data);
        return Ok(data);
    }

    [HttpPost]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [Authorize]
    public async Task<ActionResult<DynamoDataItem>> Post(DynamoDataItem? item)
    {
        if(item is null)
        {
            return BadRequest();
        }
        var result = await Svc.SaveDataItem(item);
        return Ok(result);
    }
}

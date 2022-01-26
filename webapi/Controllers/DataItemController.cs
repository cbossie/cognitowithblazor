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

    public async Task<ActionResult<List<DynamoDataItem>>> Get(string id)
    {
        List<DynamoDataItem> items = new();

        var data = await Svc.GetDataItem(id);
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
    [HttpGet]
    [ProducesResponseType(200)]

    public async Task<ActionResult<IEnumerable<DynamoDataItem>>> Get()
    {
        List<DynamoDataItem> items = new();

        var data = await Svc.ListDataItems();
        items.AddRange(data);
        return Ok(data);
    }



}

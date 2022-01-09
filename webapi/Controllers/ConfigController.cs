using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using webapi.Model;

namespace webapi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ConfigController : ControllerBase
    {
        CognitoSettings Settings { get; init; }

        public ConfigController(CognitoSettings settings)
        {
            Settings = settings;
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("cognito")]
        public CognitoSettings Get() => Settings;
    }
}

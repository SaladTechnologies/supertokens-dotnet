using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SuperTokens.AspNetCore
{
    [ApiController]
    [Authorize]
    [Route("api/values")]
    public sealed class ValuesController : ControllerBase
    {
        [HttpGet]
        public ActionResult<List<string>> Get() =>
            new List<string>
            {
                "One",
                "Two",
                "Three",
            };
    }
}

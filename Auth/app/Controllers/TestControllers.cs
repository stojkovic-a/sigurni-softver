using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
// using Neo4jClient;
// using StackExchange.Redis;

namespace Auth.app.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestControllers : Controller
{
    // private IConnectionMultiplexer _redis;
    // private IBoltGraphClient _neo4j;
    // public TestControllers(IBoltGraphClient neo4j, IConnectionMultiplexer redis)
    // {
    //     _neo4j = neo4j;
    //     _redis = redis;
    // }

    // [HttpGet("test")]
    // public async Task<ActionResult> Test()
    // {
    //     var db = _redis.GetDatabase();
    //     db.StringSet("test", "test");
    //     var newUser = new { Test = "test", broj = 45 };
    //     await _neo4j.Cypher
    //     .Create("(user:User $newUser)")
    //     .WithParam("newUser", newUser)
    //     .ExecuteWithoutResultsAsync();
    //     return Ok(_neo4j.IsConnected);
    // }
}

using Api.Data;
using Api.Models;
using Microsoft.AspNetCore.Mvc;
using Shared;
using Shared.Contracts;

namespace Api.Controllers;

[Route(Endpoints.Users)]
[ApiController]
public class UserController : ControllerBase
{
    private readonly AppDbContext _context;

    public UserController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Get()
    {
        var userDataList = _context.Users
            .Select(user => new UserData
            {
                Username = user.Name,
                CreatedAt = user.CreatedAt
            })
            .ToList();

        var userContract = new GetUsersContract { users = userDataList };

        return Ok(userContract);
    }

    [HttpGet]
    [Route("{id}")]
    public IActionResult Get(Guid id)
    {
        var user = _context.Users.Find(id);
        if (user == null)
        {
            return NotFound($"Could not find user {id}");
        }

        return Ok(user);
    }

    [HttpPost]
    public IActionResult Post([FromBody] UserContract userContract)
    {
        var baseUsername = userContract.Username;
        string preferredName;
        var attempts = 0;
        Random random = new Random();
        HashSet<string> triedUsernames = new HashSet<string>();

        do
        {
            var randomNumber = random.Next(0, 9999); 
            preferredName = $"{baseUsername}#{randomNumber:D4}";

            if (triedUsernames.Contains(preferredName))
            {
                continue;
            }

            attempts++;
            triedUsernames.Add(preferredName);

            if (attempts > 9999)
            {
                return BadRequest("Username already taken");
            }

        } while (_context.Users.Any(user => user.Name == preferredName));

        var user = new User { Name = preferredName };

        _context.Users.Add(user);
        _context.SaveChanges();

        NewUserContract newUserContract = new NewUserContract
        {
            Name = user.Name,
            Key = user.Key,
            CreatedAt = user.CreatedAt
        };

        return Ok(newUserContract);
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(Guid id)
    {
        var user = _context.Users.Find(id);
        if (user == null)
        {
            return NotFound($"Could not find user {id}");
        }

        _context.Users.Remove(user);
        return Ok($"Removed user {id}");
    }
}
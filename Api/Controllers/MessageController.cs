using Api.Data;
using Api.Models;
using Microsoft.AspNetCore.Mvc;
using Shared;
using Shared.Contracts;

namespace Api.Controllers
{
    [Route(Endpoints.Messages)]
    [ApiController]
    public class MessageController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MessageController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var messageDataList = _context.Messages
                .Select(x => new MessageData()
                {
                    Sender = x.Name,
                    Content = x.Content,
                    TimeStamp = x.TimeStamp,
                    Id = x.Id,
                })
                .ToList()
                .OrderBy(x => x.TimeStamp)
                .ToList();

            var messageContract = new GetMessagesContract
            {
                messages = messageDataList
            };

            return Ok(messageContract);
        }

        [HttpPost]
        public IActionResult Post([FromBody] MessageContract messageContract)
        {
            var key = messageContract.Key;

            if (key == Guid.Empty)
            {
                return BadRequest("Key is required");
            }

            var tes = _context.Users.ToList();

            var test = _context.Users.FirstOrDefault(x => x.Key == key);

            if (!_context.Users.Any(x => x.Key == key))
            {
                return BadRequest("Bad key");
            }

            var user = _context.Users.First(x => x.Key == key);

            var message = new Message
            {
                Content = messageContract.Content,
                Name = user.Name
            };

            if (message.Content.Length > 500 && message.Content.Length == 0)
            {
                return BadRequest("Message must be between 1 - 500 characters");
            }

            _context.Messages.Add(message);
            _context.SaveChanges();
            return Ok($"Created message {message.Id}");
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            var message = _context.Messages.Find(id);

            if (message == null) {
                return NotFound($"Could not find message {id}");
            }

            _context.Messages.Remove(message);
            return Ok($"Removed message {id}");
        }
    }
}
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using XR5_0TrainingRepo.Models;

namespace XR5_0TrainingRepo.Controllers
{
    [Route("/xr50/library_of_reality_altering_knowledge/[controller]")]
    [ApiController]
    public class user_managementController : ControllerBase
    {
        private readonly XR50RepoContext _context;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        public user_managementController(XR50RepoContext context, HttpClient httpClient, IConfiguration configuration)
        { 
            _context = context;
            _httpClient = httpClient;
            _configuration = configuration;     
        }

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {

            return await _context.Users.ToListAsync();
        }
        
        // GET: api/Users/5
        [HttpGet("{userName}")]
        public async Task<ActionResult<User>> GetUser(string userName)
        {
            var user = await _context.Users.FindAsync(userName);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        /*
        // PUT: api/Users/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{userName}")]
        public async Task<IActionResult> PutUser(string userName, User user)
        {
            if  (!user.UserName.Equals(userName))
            {
                return BadRequest();
            }

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(userName))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }
        */

        // POST: api/Users
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<User>> PostUser(User user)
        {
            
            var XR50App = await _context.Apps.FindAsync(user.AppName);
            if (XR50App == null)
            {
                return NotFound();
            }
            
            if (user.admin)
            {
                XR50App.AdminList.Add(user.UserName);
            }
            _context.Users.Add(user);
            _context.SaveChanges();

            var values = new List<KeyValuePair<string, string>>();
            values.Add(new KeyValuePair<string, string>("userid", user.UserName));
            values.Add(new KeyValuePair<string, string>("password", user.Password));
            values.Add(new KeyValuePair<string, string>("email", user.UserEmail));
            values.Add(new KeyValuePair<string, string>("display", user.FullName));
            values.Add(new KeyValuePair<string, string>("groups[]", XR50App.OwncloudGroup));
            FormUrlEncodedContent messageContent = new FormUrlEncodedContent(values);
            string username = _configuration.GetValue<string>("OwncloudSettings:Admin");
            string password = _configuration.GetValue<string>("OwncloudSettings:Password");
            string uri_base = _configuration.GetValue<string>("OwncloudSettings:BaseAPI");
            string uri_path = _configuration.GetValue<string>("OwncloudSettings:UserManagementPath");
            string authenticationString = $"{username}:{password}";
            var base64EncodedAuthenticationString = Convert.ToBase64String(Encoding.ASCII.GetBytes(authenticationString));

            var request = new HttpRequestMessage(HttpMethod.Post, uri_path)
            {
                Content = messageContent
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);
            _httpClient.BaseAddress = new Uri(uri_base);
            // _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Basic {base64EncodedAuthenticationString}");
            var result = _httpClient.SendAsync(request).Result;
            string resultContent = result.Content.ReadAsStringAsync().Result;
            Console.WriteLine($"Response content: {resultContent}");

            return CreatedAtAction("PostUser", new { id = user.UserName }, user);
        }

        // DELETE: api/Users/5
        [HttpDelete("{userName}")]
        public async Task<IActionResult> DeleteUser(string userName)
        {
            var user = await _context.Users.FindAsync(userName);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            var values = new List<KeyValuePair<string, string>>();
            values.Add(new KeyValuePair<string, string>("userid", user.UserName));
            values.Add(new KeyValuePair<string, string>("password", user.Password));
            values.Add(new KeyValuePair<string, string>("email", user.UserEmail));
            values.Add(new KeyValuePair<string, string>("display", user.FullName));
            FormUrlEncodedContent messageContent = new FormUrlEncodedContent(values);
            string username = _configuration.GetValue<string>("OwncloudSettings:Admin");
            string password = _configuration.GetValue<string>("OwncloudSettings:Password");
            string uri_base = _configuration.GetValue<string>("OwncloudSettings:BaseAPI");
            string uri_path = _configuration.GetValue<string>("OwncloudSettings:UserManagementPath");

            string authenticationString = $"{username}:{password}";
            var base64EncodedAuthenticationString = Convert.ToBase64String(Encoding.ASCII.GetBytes(authenticationString));

            var request = new HttpRequestMessage(HttpMethod.Delete, $"{uri_path}/{user.UserName}")
            {
                Content = messageContent
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);
            _httpClient.BaseAddress = new Uri(uri_base);
            // _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Basic {base64EncodedAuthenticationString}");
            var result = _httpClient.SendAsync(request).Result;
            string resultContent = result.Content.ReadAsStringAsync().Result;
            //Console.WriteLine($"Response content: {resultContent}");
            return NoContent();
        }

        private bool UserExists(string userName)
        {
            return _context.Users.Any(e => e.UserName.Equals(userName));
        }
    }
}

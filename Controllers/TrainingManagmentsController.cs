using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Policy;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using XR5_0TrainingRepo.Models;

namespace XR5_0TrainingRepo.Controllers
{
    [Route("/xr50/library_of_reality_altering_knowledge/[controller]")]
    [ApiController]
    public class training_managementController : ControllerBase
    {
        private readonly XR50RepoContext _context;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        public training_managementController(XR50RepoContext context, HttpClient httpClient, IConfiguration configuration)
        {
            _context = context;
            _httpClient = httpClient;
            _configuration = configuration; 
        }

        // GET: api/Training
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TrainingModule>>> GetTraining()
        {
            return await _context.Trainings.ToListAsync();
        }

        // GET: api/Training/5
        [HttpGet("{TrainingId}")]
        public async Task<ActionResult<TrainingModule>> GetTraining(string TrainingId)
        {
            var Training = await _context.Trainings.FindAsync(TrainingId);

            if (Training == null)
            {
                return NotFound();
            }
            return Training;
        }

        // PUT: api/Training/5 
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
       /* [HttpPut("{AppName}/{TrainingName}")]
        public async Task<IActionResult> PutTraining(string TrainingName, TrainingModule Training)
        {
            if (!TrainingName.Equals(Training.TrainingName))
            {
                return BadRequest();
            }

            _context.Entry(Training).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TrainingExists(TrainingName))
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
        
        // DELETE: api/Training/5
        [HttpDelete("{TrainingId}")]
        public async Task<IActionResult> DeleteTraining(string TrainingId)
        {
            var Training = await _context.Trainings.FindAsync(TrainingId);
            if (Training == null)
            {
                return NotFound();
            }
            var XR50App = await _context.Apps.FindAsync(Training.AppName);
            if (XR50App == null)
            {
                return NotFound();
            }
            var admin = await _context.Users.FindAsync(XR50App.OwnerName);
            if (admin == null)
            {
                return NotFound($"Admin user for {Training.AppName}");
            }
            string username = admin.UserName;
            string password = admin.Password;
            string uri_base = _configuration.GetValue<string>("OwncloudSettings:BaseAPI");
            string uri_path = _configuration.GetValue<string>("OwncloudSettings:GroupManagementPath");
            string webdav_base = _configuration.GetValue<string>("OwncloudSettings:BaseWebDAV");
            _context.Trainings.Remove(Training);
            XR50App.TrainingList.Remove(Training.TrainingId);
            await _context.SaveChangesAsync();
            // Remove root dir for the Training
	        string cmd= "curl";
            string Arg=  $"-X DELETE -u {username}:{password} \"{webdav_base}/{XR50App.OwncloudDirectory}/{Training.TrainingName}\"";
            Console.WriteLine("Executing command: " + cmd + " " + Arg);
            var startInfo = new ProcessStartInfo
            {                                                                                                                           FileName = cmd,
                Arguments = Arg,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            using (var process = Process.Start(startInfo))
            {
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();
                Console.WriteLine("Output: " + output);
                Console.WriteLine("Error: " + error);
            }
            return NoContent();
        }

        private bool TrainingExists(string TrainingName)
        {
            return _context.Trainings.Any(e => e.TrainingName.Equals(TrainingName));
        }
    }
}

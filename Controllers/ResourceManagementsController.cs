 using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
    public class resource_managementController : ControllerBase
    {
        private readonly XR50RepoContext _context;
        private readonly HttpClient _httpClient;
        IConfiguration _configuration;  
        public resource_managementController(XR50RepoContext context,HttpClient httpClient, IConfiguration configuration)
        {
            _context = context;
            _httpClient = httpClient;
            _configuration = configuration; 
        }

        // GET: api/ResourceManagements
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ResourceBundle>>> GetResource()
        {
            return await _context.Resources.ToListAsync();
        }

        // GET: api/ResourceManagements/5
        [HttpGet("{ResourceId}")]
        public async Task<ActionResult<ResourceBundle>> GetResourceManagement(string ResourceId)
        {
            var ResourceBundle = await _context.Resources.FindAsync(ResourceId);

            if (ResourceBundle == null)
            {
                return NotFound();
            }

            return ResourceBundle;
        }

       /* // PUT: api/ResourceManagements/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{ResourceId}")]
        public async Task<IActionResult> PutResourceManagement(string ResourceId, ResourceBundle ResourceBundle)
        {
            if (!ResourceId.Equals(ResourceBundle.ResourceId))
            {
                return BadRequest();
            }

            _context.Entry(ResourceBundle).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ResourceManagementExists(ResourceId))
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
        // DELETE: api/ResourceManagements/5
        [HttpDelete("{ResourceId}")]
        public async Task<IActionResult> DeleteResourceBundleById(string ResourceId)
        {
            var ResourceBundle = await _context.Resources.FindAsync(ResourceId);
            if (ResourceBundle == null)
            {
                return NotFound();
            }

            _context.Resources.Remove(ResourceBundle);
            await _context.SaveChangesAsync();

	        var Training = _context.Trainings.FirstOrDefault(t=> t.TrainingName.Equals(ResourceBundle.TrainingName) && t.AppName.Equals(ResourceBundle.AppName));
            if (Training == null)
            {
                return NotFound();
            }
	        Training.ResourceList.Remove(ResourceId);
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
            string webdav_base = _configuration.GetValue<string>("OwncloudSettings:BaseWebDAV");
            // Createe root dir for the Training
	        string cmd="curl";
            string Arg= $"-X DELETE -u {username}:{password} \"{webdav_base}/{XR50App.OwncloudDirectory}/{Training.TrainingName}/{ResourceBundle.OwncloudFileName}\"";
            // Create root dir for the App
            Console.WriteLine("Executing command:" + cmd + " " + Arg);
            var startInfo = new ProcessStartInfo
            {
                FileName = cmd,
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
        private bool ResourceManagementExists(string ResourceName)
        {
            return _context.Resources.Any(e => e.ResourceName.Equals(ResourceName));
        }
    }
}

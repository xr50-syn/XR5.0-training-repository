﻿using System;
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
    [Route("/xr50/training-repo/training-management/[controller]")]
    [ApiController]
    public class TrainingController : ControllerBase
    {
        private readonly TrainingContext _context;
        private readonly XR50AppContext _XR50AppContext;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        public TrainingController(TrainingContext context, XR50AppContext XR50AppContext, HttpClient httpClient, IConfiguration configuration)
        {
            _context = context;
            _XR50AppContext = XR50AppContext;
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
        [HttpGet("{TrainingName}")]
        public async Task<ActionResult<TrainingModule>> GetTraining(long TrainingName)
        {
            var Training = await _context.Trainings.FindAsync(TrainingName);

            if (Training == null)
            {
                return NotFound();
            }

            return Training;
        }

        // PUT: api/Training/5 
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{TrainingName}")]
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

        // POST: api/Training
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<TrainingModule>> PostTraining(TrainingModule Training)
        {
            var XR50App = await _XR50AppContext.Apps.FindAsync(Training.AppName);
            if (XR50App == null)
            {
                return NotFound();
            }

            _context.Trainings.Add(Training);
            await _context.SaveChangesAsync();

            string username = _configuration.GetValue<string>("OwncloudSettings:Admin");
            string password = _configuration.GetValue<string>("OwncloudSettings:Password");
            string webdav_base = _configuration.GetValue<string>("OwncloudSettings:BaseWebDAV");
            // Createe root dir for the Training
	    string cmd="curl";
            string Arg= $"-X MKCOL -u {username}:{password} \"{webdav_base}/{XR50App.OwncloudDirectory}/{Training.TrainingName}\"";
            Console.WriteLine("Ececuting command:" + cmd + " " + Arg);
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

            return CreatedAtAction("PostTraining", new { Training.TrainingName });
        }

        // DELETE: api/Training/5
        [HttpDelete("{TrainingName}")]
        public async Task<IActionResult> DeleteTraining(string TrainingName)
        {
            var Training = await _context.Trainings.FindAsync(TrainingName);
            if (Training == null)
            {
                return NotFound();
            }

            _context.Trainings.Remove(Training);
            await _context.SaveChangesAsync();

            var XR50App = await _XR50AppContext.Apps.FindAsync(Training.AppName);
            if (XR50App == null)
            {
                return NotFound();
            }

            string username = _configuration.GetValue<string>("OwncloudSettings:Admin");
            string password = _configuration.GetValue<string>("OwncloudSettings:Password");
            string uri_base = _configuration.GetValue<string>("OwncloudSettings:BaseAPI");
            string uri_path = _configuration.GetValue<string>("OwncloudSettings:GroupManagementPath");
            string webdav_base = _configuration.GetValue<string>("OwncloudSettings:BaseWebDAV");
            // Createe root dir for the Training
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

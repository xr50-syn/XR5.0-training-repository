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
using Microsoft.AspNetCore.Authorization;

namespace XR5_0TrainingRepo.Controllers
{
    [Route("/xr50/library_of_reality_altering_knowledge/[controller]")]
    [ApiController]
    
    public class app_managementController : ControllerBase
    {
        private readonly XR50RepoContext _context;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
     
        public app_managementController(XR50RepoContext context, HttpClient httpClient, IConfiguration configuration)
        {
            _context = context;
            _httpClient = httpClient;
            _configuration= configuration;
        }

        // GET: api/XR50App
        [HttpGet]
        public async Task<ActionResult<IEnumerable<XR50App>>> GetApps()
        {
            return await _context.Apps.ToListAsync();
        }
        
        // GET: api/XR50App/5
        [HttpGet("{appName}")]
        public async Task<ActionResult<XR50App>> GetXR50App(string appName)
        {
            var XR50App = await _context.Apps.FindAsync(appName);

            if (XR50App == null)
            {
                return NotFound();
            }

            return XR50App;
        }
        // GET: api/XR50App/5
        [HttpGet("{appName}/TrainingModules")]
        public async Task<ActionResult<IEnumerable<TrainingModule>>> GetAppTrainings(string appName)
        {
            return  _context.Trainings.Where(t=>t.AppName.Equals(appName)).ToList();
        }
        // GET: api/XR50App/5
        [HttpGet("{appName}/{trainingName}/ResourceBundles")]
        public async Task<ActionResult<IEnumerable<ResourceBundle>>> GetTrainingResources(string appName,string trainingName)
        {
            return  _context.Resources.Where(r=>r.AppName.Equals(appName) && r.TrainingName.Equals(trainingName)).ToList();
        }
        /*
        // PUT: api/XR50App/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutXR50App(long id, XR50App XR50App)
        {
            if (id != XR50App.AppId)
            {
                return BadRequest();
            }

            _context.Entry(XR50App).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!XR50AppExists(id))
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

        // POST: api/XR50App
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<XR50App>> PostXR50App(XR50App XR50App)
        {

            _context.Apps.Add(XR50App);

            var values = new List<KeyValuePair<string, string>>();
            values.Add(new KeyValuePair<string, string>("groupid", XR50App.OwncloudGroup));
            FormUrlEncodedContent messageContent = new FormUrlEncodedContent(values);
            string username = _configuration.GetValue<string>("OwncloudSettings:Admin");
            string password = _configuration.GetValue<string>("OwncloudSettings:Password");
            string uri_base = _configuration.GetValue<string>("OwncloudSettings:BaseAPI");
            string uri_path = _configuration.GetValue<string>("OwncloudSettings:GroupManagementPath");
            string webdav_base = _configuration.GetValue<string>("OwncloudSettings:BaseWebDAV");
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
            User adminUser = XR50App.Owner;
            XR50App.OwnerName = adminUser.UserName;
	        XR50App.AdminList.Add(adminUser.UserName);
            _context.Users.Add(adminUser);
            _context.SaveChanges();
            //Console.WriteLine($"Response content: {resultContent}");
            //Create the admin User
            var valuesAdmin = new List<KeyValuePair<string, string>>();
            valuesAdmin.Add(new KeyValuePair<string, string>("userid", adminUser.UserName));
            valuesAdmin.Add(new KeyValuePair<string, string>("password", adminUser.Password));
            valuesAdmin.Add(new KeyValuePair<string, string>("email", adminUser.UserEmail));
            valuesAdmin.Add(new KeyValuePair<string, string>("display", adminUser.FullName));
            valuesAdmin.Add(new KeyValuePair<string, string>("groups[]", XR50App.OwncloudGroup));
            //Target The User Interface
            uri_path = _configuration.GetValue<string>("OwncloudSettings:UserManagementPath");
            FormUrlEncodedContent messageContentAdmin = new FormUrlEncodedContent(valuesAdmin);
           
            var requestAdmin = new HttpRequestMessage(HttpMethod.Post, uri_path)
            {
                Content = messageContentAdmin
            };
            requestAdmin.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);
           
            // _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Basic {base64EncodedAuthenticationString}");
            var resultAdmin = _httpClient.SendAsync(requestAdmin).Result;
            string resultAdminContent = resultAdmin.Content.ReadAsStringAsync().Result;
            Console.WriteLine($"Response content: {resultAdminContent}");

            // Create root dir for the App, owned by Admin
	        string cmd="curl";
            string Arg= $"-X MKCOL -u {adminUser.UserName}:{adminUser.Password} \"{webdav_base}/{XR50App.OwncloudDirectory}/\"";
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
            _context.SaveChanges();
            return CreatedAtAction("PostXR50App", XR50App);
        }
        // POST: api/XR50App/
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("/xr50/library_of_reality_altering_knowledge/[controller]/training-management/{AppName}")]
        public async Task<ActionResult<TrainingModule>> PostTraining(string AppName,TrainingModule Training)
        {
	        if (!AppName.Equals(Training.AppName)) {
		        return NotFound($"App {Training.AppName} is not our parent");
	        }
            var XR50App = await _context.Apps.FindAsync(Training.AppName);
            if (XR50App == null)
            {
                return NotFound($"App {Training.AppName}");
            }
            var admin = await _context.Users.FindAsync(XR50App.OwnerName);
            if (admin ==null) 
            {
                return NotFound($"Admin user for {Training.AppName}");
            }
            Training.TrainingId= Guid.NewGuid().ToString();; 
                
            XR50App.TrainingList.Add(Training.TrainingId); 
            _context.Trainings.Add(Training);
            await _context.SaveChangesAsync();

            string username = admin.UserName;
            string password = admin.Password;
            string webdav_base = _configuration.GetValue<string>("OwncloudSettings:BaseWebDAV");
            // Createe root dir for the Training
	    string cmd="curl";
            string Arg= $"-X MKCOL -u {username}:{password} \"{webdav_base}/{XR50App.OwncloudDirectory}/{Training.TrainingName}\"";
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
            return CreatedAtAction("PostTraining", Training);
        }
        // POST: api/XR50App
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("/xr50/library_of_reality_altering_knowledge/[controller]/resource-management/{AppName}/{TrainingName}")]
        public async Task<ActionResult<ResourceBundle>> PostResourceManagement(string AppName, string TrainingName, ResourceBundle ResourceBundle)
        {

            var XR50App = await _context.Apps.FindAsync(ResourceBundle.AppName);
            if (XR50App == null)
            {
                return NotFound($"App {ResourceBundle.AppName}");
            }
            var admin = await _context.Users.FindAsync(XR50App.OwnerName);
            if (admin == null)
            {
                return NotFound($"Admin user for {ResourceBundle.AppName}");
            }
            var Training = _context.Trainings.FirstOrDefault(t=> t.TrainingName.Equals(TrainingName) && t.AppName.Equals(AppName));
            if (Training == null)
            {
                return NotFound($"Training for {ResourceBundle.TrainingName}");
            }
            ResourceBundle.ResourceId = Guid.NewGuid().ToString();
            Training.ResourceList.Add(ResourceBundle.ResourceId);
            _context.Resources.Add(ResourceBundle);
            await _context.SaveChangesAsync();
           
            string username = admin.UserName;
            string password = admin.Password;
            string webdav_base = _configuration.GetValue<string>("OwncloudSettings:BaseWebDAV");
            // Createe root dir for the Training
            string cmd="curl";
            string Arg= $"-X MKCOL -u {username}:{password} \"{webdav_base}/{XR50App.OwncloudDirectory}/{Training.TrainingName}/{ResourceBundle.OwncloudFileName}\"";
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
            
            _context.SaveChanges();
            return CreatedAtAction("PostResourceManagement", ResourceBundle);
        }

        // DELETE: api/XR50App/5
        [HttpDelete("{appName}")]
        public async Task<IActionResult> DeleteXR50App(string appName)
        {
            var XR50App = await _context.Apps.FindAsync(appName);
            if (XR50App == null)
            {
                Console.WriteLine($"Did not find XR app with id: {appName}");
                return NotFound();
            }
            var adminUser = await _context.Users.FindAsync(XR50App.OwnerName); 
	        if (adminUser == null) {
		        Console.WriteLine($"Did not find Owner: {XR50App.OwnerName}");
                return NotFound();
            }
            _context.Apps.Remove(XR50App);
	        _context.Users.Remove(adminUser);
            await _context.SaveChangesAsync();

            var values = new List<KeyValuePair<string, string>>();
            FormUrlEncodedContent messageContent = new FormUrlEncodedContent(values);
            string username = _configuration.GetValue<string>("OwncloudSettings:Admin");
            string password = _configuration.GetValue<string>("OwncloudSettings:Password");
            string uri_base = _configuration.GetValue<string>("OwncloudSettings:BaseAPI");
            string uri_group = _configuration.GetValue<string>("OwncloudSettings:GroupManagementPath");
	        string uri_user = _configuration.GetValue<string>("OwncloudSettings:UserManagementPath");
            string webdav_base = _configuration.GetValue<string>("OwncloudSettings:BaseWebDAV");

            string authenticationString = $"{username}:{password}";
            var base64EncodedAuthenticationString = Convert.ToBase64String(Encoding.ASCII.GetBytes(authenticationString));
            var request = new HttpRequestMessage(HttpMethod.Delete, $"{uri_group}/{XR50App.OwncloudGroup}")
            {
                Content = messageContent
            };
            Console.WriteLine(XR50App.OwncloudGroup);
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);
            _httpClient.BaseAddress = new Uri(uri_base);
            //_httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Basic {base64EncodedAuthenticationString}");
            var result = _httpClient.SendAsync(request).Result;
            string resultContent = result.Content.ReadAsStringAsync().Result;
            // Delete root dir for the App
	        string cmd= "curl";
            string Arg=  $"-X DELETE -u {adminUser.UserName}:{adminUser.Password} \"{webdav_base}/{XR50App.OwncloudDirectory}/\"";
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
            request = new HttpRequestMessage(HttpMethod.Delete, $"{uri_user}/{XR50App.OwnerName}")
            {
                Content = messageContent
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);
            // _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Basic {base64EncodedAuthenticationString}");
	        result = _httpClient.SendAsync(request).Result;
            resultContent = result.Content.ReadAsStringAsync().Result;    
            //Console.WriteLine($"Response content: {resultContent}");
            return NoContent();
        }
        [HttpDelete("/xr50/library_of_reality_altering_knowledge/[controller]/training-management/{AppName}/{TrainingName}")]
        public async Task<IActionResult> DeleteTraining(string AppName,string TrainingName)
        {
            var Training = _context.Trainings.FirstOrDefault(t=> t.TrainingName.Equals(TrainingName) && t.AppName.Equals(AppName));
            if (Training == null)           
            {
                return NotFound($"Did not find training {TrainingName}");
            }

            var XR50App = await _context.Apps.FindAsync(AppName);
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


        // DELETE: api/XR50App/
        [HttpDelete("/xr50/library_of_reality_altering_knowledge/[controller]/resource-management/{AppName}/{TrainingName}/{ResourceName}")]
        public async Task<IActionResult> DeleteResourceBundle(string AppName, string TrainingName, string ResourceName)
        {
            var ResourceBundle = _context.Resources.FirstOrDefault( r=> r.ResourceName.Equals(ResourceName) && r.TrainingName.Equals(TrainingName) && r.AppName.Equals(AppName));
            if (ResourceBundle == null)
            {
                return NotFound();
            }

            _context.Resources.Remove(ResourceBundle);
            await _context.SaveChangesAsync();

	        var Training = _context.Trainings.FirstOrDefault(t=> t.TrainingName.Equals(TrainingName) && t.AppName.Equals(AppName));
            if (Training == null)
            {
                return NotFound();
            }
	        Training.ResourceList.Remove(ResourceBundle.ResourceId);
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
        private bool XR50AppExists(string appName)
        {
            return _context.Apps.Any(e => e.AppName.Equals(appName));
        }
    }
}

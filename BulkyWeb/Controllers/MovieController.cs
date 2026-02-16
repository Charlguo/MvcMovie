using Bulky.DataAccess.Data;
using Bulky.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace BulkyWeb.Controllers
{
    public class MovieController : Controller
    {
        private readonly ApplicationDbContext _db;

        public MovieController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            // Fetch all movies from the database
            List<Movie> objMovieList = _db.Movies.ToList();
            return View(objMovieList);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Movie obj)
        {
            // Custom Validation: Prevent creating a movie with the same title and description
            if (obj.Title != null && obj.Title.ToLower() == obj.Description?.ToLower())
            {
                ModelState.AddModelError("title", "The Description cannot exactly match the Title.");
            }

            if (ModelState.IsValid)
            {
                _db.Movies.Add(obj);
                _db.SaveChanges();
                TempData["success"] = "Movie created successfully";
                return RedirectToAction("Index");
            }

            return View(obj);
        }

        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0) return NotFound();

            Movie? movieFromDb = _db.Movies.Find(id);

            if (movieFromDb == null) return NotFound();

            return View(movieFromDb);
        }

        [HttpPost]
        public IActionResult Edit(Movie obj)
        {
            if (ModelState.IsValid)
            {
                _db.Movies.Update(obj);
                _db.SaveChanges();
                TempData["success"] = "Movie updated successfully";
                return RedirectToAction("Index");
            }

            return View(obj);
        }

        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0) return NotFound();

            Movie? movieFromDb = _db.Movies.Find(id);

            if (movieFromDb == null) return NotFound();

            return View(movieFromDb);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePOST(int? id)
        {
            Movie? obj = _db.Movies.Find(id);
            if (obj == null) return NotFound();

            _db.Movies.Remove(obj);
            _db.SaveChanges();
            TempData["success"] = "Movie deleted successfully";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> GenerateAiScript([FromBody] ScriptRequest request)
        {
            // 1. Setup the "Hollywood" Prompt
            string systemPrompt = @"You are an expert Hollywood Screenwriter. 
    Strictly follow standard screenplay formatting:
    - Start with a SLUGLINE (e.g., INT. LOCATION - DAY).
    - Character names in CAPS.
    - Clear, concise Action Lines.
    - Realistic dialogue.
    - No conversational filler.";

            string userPrompt = $"Write a scene for a {request.Genre} movie titled '{request.Title}'. " +
                                $"Scenario: {request.Prompt}";

            // 2. Configure JSON to match Groq's requirements (camelCase)
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };

            // 3. Create the Payload
            var payload = new
            {
                // FIX #1: Use a valid, non-deprecated model
                model = "llama-3.3-70b-versatile",

                messages = new[]
                {
            new { role = "system", content = systemPrompt },
            new { role = "user", content = userPrompt }
        },
                temperature = 0.7,
                max_tokens = 1000 // approx 2 pages of script
            };

            // Serialize with the options 
            string jsonPayload = JsonSerializer.Serialize(payload, jsonOptions);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            // 4. Send the Request
            // Replace this with your console.groq API key
            var apiKey = "your-api-key";

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            try
            {
                var response = await client.PostAsync("https://api.groq.com/openai/v1/chat/completions", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(responseString);

                    string script = doc.RootElement
                        .GetProperty("choices")[0]
                        .GetProperty("message")
                        .GetProperty("content")
                        .GetString();

                    return Json(new { success = true, script = script });
                }
                else
                {
                    // Debugging: Read the error message to know exactly what failed
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    return Json(new { success = false, message = $"Groq Error ({response.StatusCode}): {errorMsg}" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Server Error: " + ex.Message });
            }
        }

        // Helper class for the data coming from JavaScript
        public class ScriptRequest
        {
            public string Prompt { get; set; }
            public string Title { get; set; }
            public string Genre { get; set; }
        }
    }
}
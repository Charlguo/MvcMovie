using Bulky.DataAccess.Data;
using Bulky.Models;
using Microsoft.AspNetCore.Mvc;

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
    }
}
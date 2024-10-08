using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MIS_521_assignment3_nowiggins.Data;
using MIS_521_assignment3_nowiggins.Models;
using VaderSharp2;

namespace MIS_521_assignment3_nowiggins.Controllers
{
    public class MoviesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MoviesController(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> GetMoviePoster(int MovieId)
        {
            var movie = await _context.Movie
                .FirstOrDefaultAsync(m => m.MovieId == MovieId);

            if (movie == null)
            {
                return NotFound();
            }

            var imageData = movie.MoviePoster;

            return File(imageData, "image/jpg");
        }
        // GET: Movies
        public async Task<IActionResult> Index()
        {
            return View(await _context.Movie.ToListAsync());
        }

        // GET: Movies/Details/5
        //public async Task<IActionResult> Details(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var movie = await _context.Movie
        //        .FirstOrDefaultAsync(m => m.MovieId == id);
        //    if (movie == null)
        //    {
        //        return NotFound();
        //    }
        //    MovieDetailsVM movieDetailsVM = new MovieDetailsVM();
        //    movieDetailsVM.movie = movie;
        //    movieDetailsVM.Sentiment = await SearchRedditAsync(movie.MovieTitle);
        //    return View(movieDetailsVM);
        //}
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movie.FirstOrDefaultAsync(m => m.MovieId == id);
            if (movie == null)
            {
                return NotFound();
            }

            MovieDetailsVM movieDetailsVM = new MovieDetailsVM();
            movieDetailsVM.movie = movie;

            // Get list of text from Reddit
            List<string> redditTexts = await SearchRedditAsync(movie.MovieTitle);

            var sentimentResults = new List<SentimentAnalysisResult>();
            var analyzer = new SentimentIntensityAnalyzer(); // VaderSharp analyzer

            double totalScore = 0;
            int scoreCount = 0;

            foreach (var text in redditTexts)
            {
                // Analyze the sentiment of each text
                var sentimentScores = analyzer.PolarityScores(text);
                double sentimentScore = sentimentScores.Compound;  // Get the compound sentiment score

                // Categorize the sentiment score
                var category = CategorizeSentiment(sentimentScore);

                // Add the text, score, and category to the results list
                sentimentResults.Add(new SentimentAnalysisResult
                {
                    Text = text,
                    Score = sentimentScore,
                    Category = category
                });

                // Calculate the total score for combined sentiment
                totalScore += sentimentScore;
                scoreCount++;
            }

            // Calculate the average sentiment score and round to 4 decimal places
            if (scoreCount > 0)
            {
                movieDetailsVM.CombinedSentimentScore = Math.Round(totalScore / scoreCount, 4); // Round to 4 decimals
            }
            else
            {
                movieDetailsVM.CombinedSentimentScore = 0; // Handle case with no results
            }

            movieDetailsVM.SentimentAnalysisResults = sentimentResults;

            return View(movieDetailsVM);
        }

        // GET: Movies/Create
        public IActionResult Create()
        {
            return View();
        }
        //Get the text from Reddit related to the actor name
        //List<string> textToExamine = await SearchRedditAsync(movie.MovieTitle);
//Do the sentiment stuff here (watch class video)
//HINT: DO NOT count values where the compound score is ZERO. Likewise, do not count the item in the total number of items if it is zero.

        public static async Task<List<string>> SearchRedditAsync(string searchQuery)
        {
            var returnList = new List<string>();
            var json = "";
            using (HttpClient client = new HttpClient())
            {
                //fake like you are a "real" web browser
                client.DefaultRequestHeaders.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
                json = await client.GetStringAsync("https://www.reddit.com/search.json?limit=100&q=" + HttpUtility.UrlEncode(searchQuery));
            }
            var textToExamine = new List<string>();
            JsonDocument doc = JsonDocument.Parse(json);
            // Navigate to the "data" object
            JsonElement dataElement = doc.RootElement.GetProperty("data");
            // Navigate to the "children" array
            JsonElement childrenElement = dataElement.GetProperty("children");
            foreach (JsonElement child in childrenElement.EnumerateArray())
            {
                if (child.TryGetProperty("data", out JsonElement data))
                {
                    if (data.TryGetProperty("selftext", out JsonElement selftext))
                    {
                        string selftextValue = selftext.GetString();
                        if (!string.IsNullOrEmpty(selftextValue)) { returnList.Add(selftextValue); }
                        else if (data.TryGetProperty("title", out JsonElement title)) //use title if text is empty
                        {
                            string titleValue = title.GetString();
                            if (!string.IsNullOrEmpty(titleValue)) { returnList.Add(titleValue); }
                        }
                    }
                }
            }
            var analyzer = new SentimentIntensityAnalyzer();
            int validResults = 0;
            double resultsTotal = 0;
            foreach (string textValue in textToExamine)
            {
                var results = analyzer.PolarityScores(textValue);
                if (results.Compound != 0)
                {
                    resultsTotal += results.Compound;
                    validResults++;
                }
            }

            double avgResult = Math.Round(resultsTotal / validResults, 2);
            returnList.Add(avgResult.ToString() + ", " + CategorizeSentiment(avgResult));
            return returnList;
        }

        public static string CategorizeSentiment(double sentiment)
        {
            if (sentiment >= -1 && sentiment < -0.6)
                return "Extremely Negative";
            else if (sentiment >= -0.6 && sentiment <= -0.2)
                return "Very Negative";
            else if (sentiment >= -0.2 && sentiment < 0)
                return "Slightly Negative";
            else if (sentiment >= 0 && sentiment < 0.2)
                return "Slightly Positive";
            else if (sentiment >= 0.2 && sentiment < 0.6)
                return "Very Positive";
            else if (sentiment >= 0.6 && sentiment <= 1)
                return "Highly Positive";
            else
                return "Invalid sentiment value";
        }

        // POST: Movies/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MovieId,MovieTitle,MovieGenre,YearOfRelease,MovieIMDBHyperlink")] Movie movie, IFormFile MoviePoster)
        {
            ModelState.Remove(nameof(movie.MoviePoster));

            if (ModelState.IsValid)
            {
                if (MoviePoster != null && MoviePoster.Length > 0)
                {
                    var memoryStream = new MemoryStream();
                    await MoviePoster.CopyToAsync(memoryStream);
                    movie.MoviePoster = memoryStream.ToArray();
                }
                else
                {
                    movie.MoviePoster = new byte[0];
                }

                _context.Add(movie);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(movie);
        }

        // GET: Movies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movie.FindAsync(id);
            if (movie == null)
            {
                return NotFound();
            }
            return View(movie);
        }

        // POST: Movies/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MovieId,MovieTitle,MovieGenre,YearOfRelease,MovieIMDBHyperlink")] Movie movie, IFormFile MoviePoster)
        {
            if (id != movie.MovieId)
            {
                return NotFound();
            }

            ModelState.Remove(nameof(movie.MoviePoster));

            Movie existingMovie = _context.Movie.AsNoTracking().FirstOrDefault(m => m.MovieId == id);
            if (MoviePoster != null && MoviePoster.Length > 0)
            {
                var memoryStream = new MemoryStream();
                await MoviePoster.CopyToAsync(memoryStream);
                movie.MoviePoster = memoryStream.ToArray();
            }
            else if (existingMovie != null)
            {
                movie.MoviePoster = existingMovie.MoviePoster;
            }
            else
            {
                movie.MoviePoster = new byte[0];
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(movie);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MovieExists(movie.MovieId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(movie);
        }

        // GET: Movies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movie
                .FirstOrDefaultAsync(m => m.MovieId == id);
            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        // POST: Movies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movie = await _context.Movie.FindAsync(id);
            if (movie != null)
            {
                _context.Movie.Remove(movie);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MovieExists(int id)
        {
            return _context.Movie.Any(e => e.MovieId == id);
        }
    }
}

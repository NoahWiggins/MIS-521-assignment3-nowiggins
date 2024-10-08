using System;
using System.Collections.Generic;
using System.Linq;
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
    public class ActorsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ActorsController(ApplicationDbContext context)
        {
            _context = context;
        }     
        public async Task<IActionResult> GetActorPhoto(int ActorId)
        {
            var actor = await _context.Actor
                .FirstOrDefaultAsync(m => m.ActorId == ActorId);

            if (actor == null)
            {
                return NotFound();
            }

            var imageData = actor.ActorPhoto;

            return File(imageData, "image/jpg");
        }

        // GET: Actors
        public async Task<IActionResult> Index()
        {
            return View(await _context.Actor.ToListAsync());
        }

        // GET: Actors/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var actor = await _context.Actor.FirstOrDefaultAsync(m => m.ActorId == id);
            if (actor == null)
            {
                return NotFound();
            }

            ActorDetailsVM actorDetailsVM = new ActorDetailsVM();
            actorDetailsVM.actor = actor;

            // Get list of text from Reddit
            List<string> redditTexts = await SearchRedditAsync(actor.ActorName);

            var sentimentResults = new List<ActorSentimentAnalysisResult>();
            var analyzer = new SentimentIntensityAnalyzer(); // VaderSharp analyzer

            double totalScore = 0;
            int scoreCount = 0;

            foreach (var text in redditTexts)
            {
                // Analyze the sentiment of each text
                var sentimentScores = analyzer.PolarityScores(text);
                double sentimentScore = sentimentScores.Compound;  // Get the compound sentiment score

                // Skip scores that are exactly zero
                if (sentimentScore == 0)
                {
                    continue;  // Skip to the next iteration if score is 0
                }

                // Categorize the sentiment score
                var category = CategorizeSentiment(sentimentScore);

                // Add the text, score, and category to the results list
                sentimentResults.Add(new ActorSentimentAnalysisResult
                {
                    Text = text,
                    Score = sentimentScore,
                    Category = category
                });

                // Calculate the total score for combined sentiment
                totalScore += sentimentScore;
                scoreCount++;
            }

            // Calculate the average sentiment score and round it to 4 decimal places
            if (scoreCount > 0)
            {
                actorDetailsVM.CombinedSentimentScore = Math.Round(totalScore / scoreCount, 4); // Round to 4 decimal places
            }
            else
            {
                actorDetailsVM.CombinedSentimentScore = 0; // Handle case with no results
            }

            actorDetailsVM.ActorSentimentAnalysisResults = sentimentResults;

            return View(actorDetailsVM);
        }

        // GET: Actors/Create
        public IActionResult Create()
        {
            return View();
        }
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


        // POST: Actors/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ActorId,ActorName,ActorGender,ActorAge,ActorIMDBHyperlink")] Actor actor, IFormFile ActorPhoto)
        {
            ModelState.Remove(nameof(actor.ActorPhoto));

            if (ModelState.IsValid)
            {
                if (ActorPhoto != null && ActorPhoto.Length > 0)
                {
                    Console.WriteLine($"Uploaded file: {ActorPhoto.FileName}, Size: {ActorPhoto.Length}");
                    var memoryStream = new MemoryStream();
                    await ActorPhoto.CopyToAsync(memoryStream);
                    actor.ActorPhoto = memoryStream.ToArray();
                }
                else
                {
                    actor.ActorPhoto = new byte[0];
                }

                _context.Add(actor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(actor);
        }

        // GET: Actors/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var actor = await _context.Actor.FindAsync(id);
            if (actor == null)
            {
                return NotFound();
            }
            return View(actor);
        }

        // POST: Actors/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ActorId,ActorName,ActorGender,ActorAge,ActorIMDBHyperlink")] Actor actor, IFormFile ActorPhoto)
        {
            if (id != actor.ActorId)
            {
                return NotFound();
            }

            ModelState.Remove(nameof(actor.ActorPhoto));

            Actor existingActor = _context.Actor.AsNoTracking().FirstOrDefault(m => m.ActorId == id);
            if (ActorPhoto != null && ActorPhoto.Length > 0)
            {
                var memoryStream = new MemoryStream();
                await ActorPhoto.CopyToAsync(memoryStream);
                actor.ActorPhoto = memoryStream.ToArray();
            }
            else if (existingActor != null)
            {
                actor.ActorPhoto = existingActor.ActorPhoto;
            }
            else
            {
                actor.ActorPhoto = new byte[0];
            }
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(actor);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ActorExists(actor.ActorId))
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
            return View(actor);
        }

        // GET: Actors/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var actor = await _context.Actor
                .FirstOrDefaultAsync(m => m.ActorId == id);
            if (actor == null)
            {
                return NotFound();
            }

            return View(actor);
        }

        // POST: Actors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var actor = await _context.Actor.FindAsync(id);
            if (actor != null)
            {
                _context.Actor.Remove(actor);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ActorExists(int id)
        {
            return _context.Actor.Any(e => e.ActorId == id);
        }
    }
}

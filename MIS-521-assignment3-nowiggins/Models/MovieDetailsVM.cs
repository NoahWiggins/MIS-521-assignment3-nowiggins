namespace MIS_521_assignment3_nowiggins.Models
{
    public class MovieDetailsVM
    {
        public Movie movie { get; set; }
        public List<Actor> actors { get; set; }
        public List<SentimentAnalysisResult> SentimentAnalysisResults { get; set; }
        public double CombinedSentimentScore { get; set; }

    }
    public class SentimentAnalysisResult
    {
        public string Text { get; set; }
        public double Score { get; set; }          // Store the sentiment score
        public string Category { get; set; }       // Store the sentiment category
    }

}

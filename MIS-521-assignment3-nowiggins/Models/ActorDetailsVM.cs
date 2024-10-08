namespace MIS_521_assignment3_nowiggins.Models
{
    public class ActorDetailsVM
    {
        public Actor actor { get; set; }
        public List<Movie> movies { get; set; }
        public List<ActorSentimentAnalysisResult> ActorSentimentAnalysisResults { get; set; }
        public double CombinedSentimentScore { get; set; }

    }
    public class ActorSentimentAnalysisResult
    {
        public string Text { get; set; }
        public double Score { get; set; }          // Store the sentiment score
        public string Category { get; set; }       // Store the sentiment category
    }
}


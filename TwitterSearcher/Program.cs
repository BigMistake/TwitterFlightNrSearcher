using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Tweetinvi;
using Stream = Tweetinvi.Stream;
namespace TwitterSearcher
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var consumerKey = "consumerKeyHere";
                var consumerSecret = "consumerSecretHere";
                var accessToken = "accessTokenHere";
                var accessTokenSecret = "accessTokenSecretHere";

                var applicationCredentials = Auth.SetUserCredentials(consumerKey, consumerSecret, accessToken, accessTokenSecret);

                TweetinviConfig.CurrentThreadSettings.TweetMode = TweetMode.Extended;
                RateLimit.RateLimitTrackerMode = RateLimitTrackerMode.TrackAndAwait;
                RateLimit.QueryAwaitingForRateLimit += (sender, arguments) =>
                {
                    Console.WriteLine($"Query : {arguments.Query} is awaiting for rate limits!");
                };

                var airlines = new List<string> { "KLM", "British_Airways", "TUINederland", "TUIflyBelgium", "vueling", "eurowings", "Aeroflot_World", "easyJet", "lufthansa", "Ryanair", "SunExpress_De", "SAS", "flybe", "AmericanAir", "AerLingus", "Iberia_en", "airfrance", "Fly_Norwegian", "wizzair", "Alitalia", "tapairportugal", "aegeanairlines", "Finnair", "AirEuropa" };

                var stream = Stream.CreateFilteredStream();
                foreach (var airline in airlines)
                {
                    var userId = User.GetUserFromScreenName(airline.ToString());
                    stream.AddFollow(userId);
                }

                stream.MatchingTweetReceived += (sender, arguments) =>
                {
                    var content = arguments.Tweet.FullText.Trim();
                    var match = Regex.Match(content, @"\s\w{2,3}?\d{2,4}\s");
                    
                    if (match.Success)
                    {
                        var date = arguments.Tweet.CreatedAt.ToString();
                        var id = arguments.Tweet.Id;
                        var customer = "";
                        var airline = "";
                        var direction = "";

                        switch (arguments.MatchOn)
                        {
                            case Tweetinvi.Streaming.MatchOn.FollowerInReplyTo:
                                customer = arguments.Tweet.CreatedBy.ScreenName;
                                airline = arguments.Tweet.InReplyToScreenName;
                                direction = "from";
                                break;
                            case Tweetinvi.Streaming.MatchOn.Follower:
                                customer = arguments.Tweet.InReplyToScreenName;
                                airline = arguments.Tweet.CreatedBy.ScreenName;
                                direction = "to";
                                break;
                            default:
                                break;
                        }

                        if (arguments.Tweet.Language.ToString() != "English")
                        {
                            Console.WriteLine($"{date} - {airline} {direction} {customer}: {content}. ID #{id}, TRANSLATE");
                        }
                        else
                        {
                            Console.WriteLine($"{date} - {airline} {direction} {customer}: {content}. ID #{id}");
                        }
                    }
                };
                stream.StartStreamMatchingAllConditions();
            }
            catch( Exception exception )
            {
                Console.WriteLine( exception.Message );
            }
        }
    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenGraph_Net;

namespace UnitTest
{
    [TestClass]
    public class TwitterCardTest
    {
        protected OpenGraph ParseUrl(string u)
        {
            return OpenGraph.ParseUrl(u);
        }

        [TestMethod]
        public void ParseUrl_TwitterCard_PlayerBandCamp()
        {
            var ogs = ParseUrl("https://rednoisefactory.bandcamp.com/album/ataraxia");
            Assert.AreEqual(ogs.TwitterCards.Card, "player");
            Assert.AreEqual(ogs.TwitterCards.Player, "https://bandcamp.com/EmbeddedPlayer/v=2/album=3274937510/size=large/linkcol=0084B4/notracklist=true/twittercard=true/");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RelicMatcher.Shared;

namespace RelicMatcher.Server.Service
{
    public class RelicListService
    {
        private List<RelicType> Relics = new List<RelicType>();

        public RelicListService(ILogger<RelicListService> logger)
        {
            using var webClient = new System.Net.WebClient();
            var json = webClient.DownloadString("https://github.com/WFCD/warframe-items/blob/development/data/json/Relics.json?raw=true");
            dynamic result = JsonConvert.DeserializeObject(json);
            foreach (dynamic relic in result)
            {
                var name = (string) relic.name;
                if (name.EndsWith("Intact"))
                {
                    var relicType = new RelicType
                    {
                        BaseName = name.Remove(name.Length - 7),
                        UniqueName = relic.uniqueName,
                        Vaulted = relic.drops == null
                    };
                    Relics.Add(relicType);
                }
            }
            // Now parse with JSON.Net
        }

        public List<RelicType> GetRelics()
        {
            return Relics;
        }

        public RelicType GetRelicFromUniqueName(string uniqueName)
        {
            return Relics.First(x => x.UniqueName == uniqueName);
        }
    }
}

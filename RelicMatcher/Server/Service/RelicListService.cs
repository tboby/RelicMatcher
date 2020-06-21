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

            Relics = LoadRelics();
            EnrichRelicDropData(Relics);
            // Now parse with JSON.Net
        }

        private List<RelicType> LoadRelics()
        {
            using var webClient = new System.Net.WebClient();
            var json = webClient.DownloadString("https://github.com/WFCD/warframe-items/blob/development/data/json/Relics.json?raw=true");
            dynamic result = JsonConvert.DeserializeObject(json);
            var list = new List<RelicType>();
            foreach (dynamic relic in result)
            {
                var name = (string)relic.name;
                if (name.EndsWith("Intact"))
                {
                    var relicType = new RelicType
                    {
                        BaseName = name.Remove(name.Length - 7),
                        UniqueName = relic.uniqueName,
                        Vaulted = relic.drops == null
                    };
                    list.Add(relicType);
                }
            }

            return list;
        }

        private void EnrichRelicDropData(List<RelicType> input)
        {
            using var webClient = new System.Net.WebClient();
            var json = webClient.DownloadString("https://raw.githubusercontent.com/WFCD/warframe-drop-data/gh-pages/data/relics.json");
            dynamic result = JsonConvert.DeserializeObject(json);
            foreach (dynamic relic in result.relics)
            {
                var tier = (string) relic.tier;
                var name = (string) relic.relicName;
                var state = (string) relic.state;
                var baseName = tier + " " + name;
                if (state == "Intact")
                {
                    var rewards = new List<RelicDrop>();
                    foreach (dynamic reward in relic.rewards)
                    {
                        rewards.Add(new RelicDrop()
                        {
                            Name = reward.itemName,
                            Rarity = reward.chance.ToString() switch
                            {
                                "2" => "Rare",
                                "11" => "Uncommon",
                                "25.33" => "Common",
                                _ => "Unknown"
                            }
                        });
                    }
                    var existing = input.Find(r => String.Equals(r.BaseName,baseName, StringComparison.InvariantCultureIgnoreCase));
                    existing.Drops = rewards;
                }
            }

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

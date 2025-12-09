using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MDPro3
{
    [CreateAssetMenu(fileName = "Characters", menuName = "Scriptable Objects/Characters")]
    public class Characters : ScriptableObject
    {
        [Serializable]
        public struct SeriesCharacter
        {
            public string id;
            public string originalId;
            public bool notReady;

            public readonly string GetOriginalId()
            {
                return string.IsNullOrEmpty(originalId) ? id : originalId;
            }
        }

        public List<SeriesCharacter> dm;
        public List<SeriesCharacter> gx;
        public List<SeriesCharacter> _5ds;
        public List<SeriesCharacter> dsod;
        public List<SeriesCharacter> zexal;
        public List<SeriesCharacter> arcv;
        public List<SeriesCharacter> vrains;
        public List<SeriesCharacter> sevens;
        public List<SeriesCharacter> npc;
        public List<SeriesCharacter> gorush;

        NPC_Names names;
        NPC_Profiles profiles;

        public string language = "zh-CN";
        private static Characters instance;

        public string GetCharacterSeries(string charaID)
        {
            foreach (var c in dm)
                if (c.id == charaID)
                    return "00";
            foreach (var c in gx)
                if (c.id == charaID)
                    return "01";
            foreach (var c in _5ds)
                if (c.id == charaID)
                    return "02";
            foreach (var c in dsod)
                if (c.id == charaID)
                    return "03";
            foreach (var c in zexal)
                if (c.id == charaID)
                    return "04";
            foreach (var c in arcv)
                if (c.id == charaID)
                    return "05";
            foreach (var c in vrains)
                if (c.id == charaID)
                    return "06";
            foreach (var c in sevens)
                if (c.id == charaID)
                    return "07";
            foreach (var c in npc)
                if (c.id == charaID)
                    return "08";
            foreach (var c in gorush)
                if (c.id == charaID)
                    return "09";
            return "00";
        }

        public List<SeriesCharacter> GetSeriesCharacters(string serial)
        {
            return serial switch
            {
                "00" => dm,
                "01" => gx,
                "02" => _5ds,
                "03" => dsod,
                "04" => zexal,
                "05" => arcv,
                "06" => vrains,
                "07" => sevens,
                "08" => npc,
                "09" => gorush,
                _ => dm,
            };
        }

        public string GetCharacterOriginalId(string charaID)
        {
            foreach (var c in dm)
                if (c.id == charaID)
                    return c.GetOriginalId();
            foreach (var c in gx)
                if (c.id == charaID)
                    return c.GetOriginalId();
            foreach (var c in _5ds)
                if (c.id == charaID)
                    return c.GetOriginalId();
            foreach (var c in dsod)
                if (c.id == charaID)
                    return c.GetOriginalId();
            foreach (var c in zexal)
                if (c.id == charaID)
                    return c.GetOriginalId();
            foreach (var c in arcv)
                if (c.id == charaID)
                    return c.GetOriginalId();
            foreach (var c in vrains)
                if (c.id == charaID)
                    return c.GetOriginalId();
            foreach (var c in sevens)
                if (c.id == charaID)
                    return c.GetOriginalId();
            foreach (var c in npc)
                if (c.id == charaID)
                    return c.GetOriginalId();
            foreach (var c in gorush)
                if (c.id == charaID)
                    return c.GetOriginalId();
            return charaID;
        }

        static bool initialized = false;

        public void Initialize()
        {
            if(initialized) 
                return;

            var path = Program.PATH_DATA + "DuelLinks_NPC_NAME.json";
            names = JsonConvert.DeserializeObject<NPC_Names>(File.ReadAllText(path));
            path = Program.PATH_DATA + "DuelLinks_Profile.json";
            profiles = JsonConvert.DeserializeObject<NPC_Profiles>(File.ReadAllText(path));
            initialized = true;
            instance = this;
        }

        public void ChangeLanguage(string language)
        {
            this.language = language;

        }

        public string GetName(string id)
        {
            if(!initialized)
                Initialize();

            if (names.NPC_NAME.TryGetValue("NAME_ID" + id, out var data))
            {
                return language switch
                {
                    "ja-JP" => data.japanese,
                    "en-US" => data.english,
                    "fr-FR" => data.french,
                    "it-IT" => data.italian,
                    "de-DE" => data.german,
                    "es-ES" => data.spanish,
                    "pt-BR" => data.portuguese,
                    "ru-RU" => data.russian,
                    "ko-KR" => data.korean,
                    "zh-TW" => data.tChinese,
                    "zh-CN" => data.sChinese,
                    _ => data.english,
                };
            }
            else
                return string.Empty;
        }
        public string GetProfile(string id)
        {
            if (!initialized)
                Initialize();

            if (profiles.PROFILE.TryGetValue("ID" + id, out var data))
            {
                return language switch
                {
                    "ja-JP" => data.japanese,
                    "en-US" => data.english,
                    "fr-FR" => data.french,
                    "it-IT" => data.italian,
                    "de-DE" => data.german,
                    "es-ES" => data.spanish,
                    "pt-BR" => data.portuguese,
                    "ru-RU" => data.russian,
                    "ko-KR" => data.korean,
                    "zh-TW" => data.tChinese,
                    "zh-CN" => data.sChinese,
                    _ => data.english,
                };
            }
            else
                return string.Empty;
        }
    }

    [Serializable]
    public class NPC_Data
    {
        public string id;
        public string note;
        [JsonProperty("ja-JP")]
        public string japanese;
        [JsonProperty("en-US")]
        public string english;
        [JsonProperty("fr-FR")]
        public string french;
        [JsonProperty("it-IT")]
        public string italian;
        [JsonProperty("de-DE")]
        public string german;
        [JsonProperty("es-ES")]
        public string spanish;
        [JsonProperty("pt-BR")]
        public string portuguese;
        [JsonProperty("ru-RU")]
        public string russian;
        [JsonProperty("ko-KR")]
        public string korean;
        [JsonProperty("zh-TW")]
        public string tChinese;
        [JsonProperty("zh-CN")]
        public string sChinese;
        public string date;
    }

    public class NPC_Names
    {
        public Dictionary<string, NPC_Data> NPC_NAME;
    }

    public class NPC_Profiles
    {
        public Dictionary<string, NPC_Data> PROFILE;
    }
}

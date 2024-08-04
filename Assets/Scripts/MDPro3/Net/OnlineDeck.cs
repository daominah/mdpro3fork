using MDPro3.Net;
using MDPro3.YGOSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace MDPro3
{
    public static class OnlineDeck
    {
        const string url = "http://rarnu.xyz:38383";
        const string uploadAPI = "/api/mdpro3/deck/upload";
        const string updateAPI = "/api/mdpro3/deck/update";
        const string getAPI = "/api/mdpro3/deck/";
        const string listAPI = "/api/mdpro3/deck/list";
        const string liteAPI = "/api/mdpro3/deck/list/lite";
        const string likeAPI = "/api/mdpro3/deck/like/";

        const string getAllAPI = "/api/mdpro3/sync/";
        const string syncAllAPI = "/api/mdpro3/sync/multi";
        const string getIdsAPI = "/api/mdpro3/deck/deckIds?count=";
        const string getIdAPI = "/api/mdpro3/deck/deckId";
        const string syncSigleAPI = "/api/mdpro3/sync/single";
        const string publicAPI = "/api/mdpro3/deck/public";


        const string reqHeader = "ReqSource";
        const string reqValue = "MDPro3";
        const string contentTypeHeader = "Content-Type";
        const string jsonHeader = "application/json";
        const string tokenHeader = "token";

        public static OnlineDeckData[] decks;

        #region Old Online
        public static async void FetchDeckList(int page = 1, int pageSize = 20,  string keyWord = "", string contributor = "")
        {
            string apiUrl = url + listAPI + $"?page={page}&size={pageSize}&keyWord={keyWord}&contributor={contributor}";
            using UnityWebRequest request = UnityWebRequest.Get(apiUrl);
            request.SetRequestHeader(reqHeader, reqValue);

            AsyncOperation sendRequestOperation = request.SendWebRequest();
            while (!sendRequestOperation.isDone)
                await Task.Yield();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonResult = request.downloadHandler.text;
                Debug.Log(jsonResult);
            }
            else
            {

            }
        }

        public static async Task<OnlineDeckData[]> FetchSimpleDeckList(int size, string keyWord = "", string contributor = "", bool sortLike = true)
        {
            string apiUrl = url + liteAPI + $"?size={size}&keyWord={keyWord}&contributor={contributor}&sortLike={sortLike}";
            using UnityWebRequest request = UnityWebRequest.Get(apiUrl);
            request.SetRequestHeader(reqHeader, reqValue);

            try
            {
                await request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    string jsonResult = request.downloadHandler.text;
                    var responseData = JsonUtility.FromJson<ResponseMultiSimpleData>(request.downloadHandler.text);
                    return responseData.data;
                }
                else
                {
                    MessageManager.Cast("FetchSimpleDeckList Error : " + request.error);
                    return null;
                }
            }
            catch(Exception e)
            {
                Debug.Log("FetchSimpleDeckList Error: " + e);
                return null;
            }
            finally
            {
                request.Dispose();
                if(request.downloadHandler != null)
                    request.downloadHandler.Dispose();
            }
        }

        public static async void UploadDeck(OnlineDeckData deck)
        {
            string apiUrl = url + uploadAPI;

            string jsonData = JsonUtility.ToJson(deck);
            using UnityWebRequest request = UnityWebRequest.Post(apiUrl, jsonData, jsonHeader);

            request.SetRequestHeader(reqHeader, reqValue);
            request.SetRequestHeader(contentTypeHeader, jsonHeader);

            request.downloadHandler = new DownloadHandlerBuffer();

            AsyncOperation sendRequestOperation = request.SendWebRequest();
            while (!sendRequestOperation.isDone)
                await Task.Yield();

            if (request.result == UnityWebRequest.Result.Success)
            {
                var responseData = JsonUtility.FromJson<ResponseSingleData>(request.downloadHandler.text);
                if(responseData.code == 0)
                {
                    Program.I().editDeck.ChangeCurrentDeckAuthor(responseData.data.deckId);
                    MessageManager.Cast(InterString.Get("上传卡组「[?]」成功。", responseData.data.deckName));
                }
                else
                {
                    MessageManager.Cast(InterString.Get("上传卡组「[?]」失败：", responseData.data.deckName) + InterString.Get(responseData.message, responseData.messageValue));
                }
            }
            else
                MessageManager.Cast(InterString.Get("上传卡组失败：") + request.error);
        }

        public static async void UpdateDeck(OnlineDeckData deck)
        {
            string apiUrl = url + updateAPI;

            string jsonData = JsonUtility.ToJson(deck);
            byte[] dataRaw = Encoding.UTF8.GetBytes(jsonData);
            using UnityWebRequest request = UnityWebRequest.Put(apiUrl, dataRaw);

            request.SetRequestHeader(reqHeader, reqValue);
            request.SetRequestHeader(contentTypeHeader, jsonHeader);

            request.downloadHandler = new DownloadHandlerBuffer();

            AsyncOperation sendRequestOperation = request.SendWebRequest();
            while (!sendRequestOperation.isDone)
                await Task.Yield();

            if (request.result == UnityWebRequest.Result.Success)
            {
                var responseData = JsonUtility.FromJson<ResponseSingleData>(request.downloadHandler.text);
                if (responseData.code == 0)
                {
                    Program.I().editDeck.ChangeCurrentDeckAuthor(responseData.data.deckId);
                    MessageManager.Cast(InterString.Get("更新卡组「[?]」成功。", responseData.data.deckName));
                }
                else
                {
                    MessageManager.Cast(InterString.Get("更新卡组「[?]」失败：", responseData.data.deckName) + InterString.Get(responseData.message, responseData.messageValue));
                }
            }
            else
                MessageManager.Cast(InterString.Get("更新卡组失败：") + request.error);
        }

        public static async Task<OnlineDeckData> GetDeck(string deckID)
        {
            string apiUrl = url + getAPI + deckID;
            using UnityWebRequest request = UnityWebRequest.Get(apiUrl);
            request.SetRequestHeader(reqHeader, reqValue);

            try
            {
                AsyncOperation sendRequestOperation = request.SendWebRequest();
                await sendRequestOperation;

                if (request.result == UnityWebRequest.Result.Success)
                {
                    return JsonUtility.FromJson<ResponseSingleData>(request.downloadHandler.text).data;
                }
                else
                {
                    MessageManager.Cast("FetchSimpleDeckList Error : " + request.error);
                    return null;
                }
            }
            catch (Exception e)
            {
                Debug.Log("FetchSimpleDeckList Error: " + e);
                return null;
            }
            finally
            {
                request.Dispose();
                if (request.downloadHandler != null)
                    request.downloadHandler.Dispose();
            }

        }

        public static async void LikeDeck(string deckId)
        {
            string apiUrl = url + likeAPI + deckId;

            using UnityWebRequest request = UnityWebRequest.PostWwwForm(apiUrl, jsonHeader);

            request.SetRequestHeader(reqHeader, reqValue);
            request.SetRequestHeader(contentTypeHeader, jsonHeader);

            await request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                var responseData = JsonUtility.FromJson<ResponseSingleData>(request.downloadHandler.text);
                if (responseData.code == 0)
                    MessageManager.Cast(InterString.Get("点赞卡组成功。"));
                else
                    MessageManager.Cast(InterString.Get("点赞卡组失败：") + InterString.Get(responseData.message, responseData.messageValue));
            }
            else
                MessageManager.Cast(InterString.Get("点赞卡组失败：") + request.error);
        }

        #endregion

        public static async Task<OnlineDeckData[]> GetAllDecks()
        {
            if (MyCard.account == null)
                return null;

            int userId = MyCard.account.user.id;
            string token = MyCard.account.token;

            string apiUrl = url + getAllAPI + userId;

            using UnityWebRequest request = UnityWebRequest.Get(apiUrl);
            request.SetRequestHeader(reqHeader, reqValue);
            request.SetRequestHeader(tokenHeader, token);

            await request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                decks = JsonUtility.FromJson<ResponseMultiSimpleData>(request.downloadHandler.text).data;
                return decks;
            }
            else
            {
                MessageManager.Cast("获取MyCard卡组失败：" + request.error);
                return null;
            }
        }
        public static OnlineDeckData GetOnlineDeckByName(string deckName)
        {
            if (decks == null)
                return null;
            foreach (var deck in decks)
                if (deck.deckName == deckName)
                    return deck;
            return null;
        }
        public static OnlineDeckData GetOnlineDeckByID(string deckId)
        {
            if (decks == null)
                return null;
            foreach (var deck in decks)
                if (deck.deckId == deckId)
                    return deck;
            return null;
        }
        public static Deck GetDeckByName(string deckName)
        {
            if (decks == null)
                return null;
            var onlineDeck = GetOnlineDeckByName(deckName);
            if (onlineDeck == null)
                return null;
            return new Deck(onlineDeck.deckYdk, onlineDeck.deckId, onlineDeck.deckId, onlineDeck.userid.ToString());
        }
        public static bool GetDeckPublicState(string deckId)
        {
            if (decks == null)
                return false;
            foreach(var deck in decks)
                if(deck.deckId == deckId)
                    return deck.isPublic;
            return false;
        }
        public static DateTime GetDeckLastEditTime()
        {
            if (decks == null)
                return DateTime.MinValue;
            var returnValue = DateTime.MinValue;
            string format = "";
            foreach (var deck in decks)
            {
                try
                {
                    var time = DateTime.Parse(deck.deckUpdateDate);
                    if (time > returnValue)
                        returnValue = time;
                }
                catch { }
            }
            return returnValue;
        }
        public static bool StringIsIdFormat(string deckId)
        {
            if (deckId.Length != 10)
                return false;
            if (!Tools.StringIsLowerAlphaNumeric(deckId))
                return false;
            return true;
        }



        public static async Task<bool> SyncDecks(List<Deck> decks, List<string> deckNames)
        {
            string apiUrl = url + getIdsAPI + decks.Count;
            using var getIDs = UnityWebRequest.Get(apiUrl);
            getIDs.SetRequestHeader(reqHeader, reqValue);

            await getIDs.SendWebRequest();
            string[] ids;
            if (getIDs.result == UnityWebRequest.Result.Success)
            {
                var responseData = JsonUtility.FromJson<ResponseDeckIDs>(getIDs.downloadHandler.text);
                ids = responseData.data;
            }
            else
            {
                MessageManager.Cast("上传卡组失败：" + getIDs.error);
                return false;
            }


            apiUrl = url + syncAllAPI;
            var body = new PostAllDecksBody();
            body.deckContributor = MyCard.account.user.username;
            body.userId = MyCard.account.user.id;
            body.decks = new PostDeck[decks.Count];
            for(int i = 0;  i < decks.Count; i++)
            {
                body.decks[i] = new PostDeck();
                body.decks[i].deckId = ids[i];
                body.decks[i].deckName = deckNames[i];
                body.decks[i].deckCoverCard1 = decks[i].Pickup.Count > 0 ? decks[i].Pickup[0] : 0;
                body.decks[i].deckCoverCard2 = decks[i].Pickup.Count > 1 ? decks[i].Pickup[1] : 0;
                body.decks[i].deckCoverCard3 = decks[i].Pickup.Count > 2 ? decks[i].Pickup[2] : 0;
                body.decks[i].deckCase = decks[i].Case[0];
                body.decks[i].deckProtector = decks[i].Protector[0];
                body.decks[i].isDelete = false;

                var deck = new Deck(Program.deckPath + body.decks[i].deckName + Program.ydkExpansion);
                deck.userId = body.userId.ToString();
                deck.deckId = body.decks[i].deckId;
                var ydk = EditDeck.FromDeckToYDK(deck);
                body.decks[i].deckYdk = ydk;
                File.WriteAllText(Program.deckPath + body.decks[i].deckName + Program.ydkExpansion, ydk);
            }

            var json = JsonUtility.ToJson(body);
            using UnityWebRequest request = UnityWebRequest.Post(apiUrl, json, jsonHeader);

            request.SetRequestHeader(reqHeader, reqValue);
            request.SetRequestHeader(contentTypeHeader, jsonHeader);
            request.SetRequestHeader(tokenHeader, MyCard.account.token);

            await request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                var responseData = JsonUtility.FromJson<ResponseSingleData>(request.downloadHandler.text);
                return true;
            }
            else
            {
                MessageManager.Cast("上传卡组失败：" + request.error);
                return false;
            }
        }

        public static async Task<bool> SyncDeck(string deckId, string deckName, string ydk, bool showHint = true)
        {
            var deck = GetOnlineDeckByID(deckId);

            if (deck == null)
            {
                string api = url + getIdAPI;
                using var re = UnityWebRequest.Get(api);
                re.SetRequestHeader(reqHeader, reqValue);
                await re.SendWebRequest();
                if (re.result == UnityWebRequest.Result.Success)
                {
                    var response = JsonUtility.FromJson<ResponseDeckID>(re.downloadHandler.text);
                    var ygoDeck = new Deck(ydk, Deck.defaultDeckAuthor);
                    ygoDeck.deckId = response.data;
                    ygoDeck.userId = MyCard.account.user.id.ToString();
                    File.WriteAllText(Program.deckPath + deckName + Program.ydkExpansion, EditDeck.FromDeckToYDK(ygoDeck));
                    deck = new OnlineDeckData(ygoDeck)
                    {
                        deckName = deckName,
                        deckContributor = MyCard.account.user.username
                    };
                }
                else
                {
                    MessageManager.Cast(InterString.Get("云端卡组同步失败：") + re.error);
                    return false;
                }
            }
            else
            {
                deck.deckName = deckName;
            }

            string apiUrl = url + syncSigleAPI;
            var body = new PostDeckBody
            {
                userId = MyCard.account.user.id,
                deckContributor = MyCard.account.user.username,
                deck = new PostDeck(deck)
            };
            body.deck.deckName = deckName;
            body.deck.deckYdk = ydk;

            var json = JsonUtility.ToJson(body);
            using var request = UnityWebRequest.Post(apiUrl, json, jsonHeader);
            request.SetRequestHeader(reqHeader, reqValue);
            request.SetRequestHeader(contentTypeHeader, jsonHeader);
            request.SetRequestHeader(tokenHeader, MyCard.account.token);

            await request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                if(showHint)
                    MessageManager.Cast(InterString.Get("云端卡组「[?]」已同步。", deckName));
                return true;
            }
            else
            {
                MessageManager.Cast(InterString.Get("云端卡组同步失败：") + request.error);
                return false;
            }
        }

        public static async Task<bool> UpdatePublicState(string deckId, bool isPublic)
        {
            var apiUrl = url + publicAPI;
            var body = new PostPublicBody();
            body.deckId = deckId;
            body.isPublic = isPublic;
            body.userId = MyCard.account.user.id;

            var json = JsonUtility.ToJson(body);
            using var request = UnityWebRequest.Post(apiUrl, json, jsonHeader);
            request.SetRequestHeader(reqHeader, reqValue);
            request.SetRequestHeader(contentTypeHeader, jsonHeader);
            request.SetRequestHeader(tokenHeader, MyCard.account.token);

            await request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("UpdatePublicState Success: " + isPublic);
                return true;
            }
            else
            {
                Debug.Log("UpdatePublicState Failed: " + request.error);
                return false;
            }
        }

        public static async Task<bool> DeleteDecks(List<string> ids)
        {
            var toDelete = new List<string>();
            foreach(var id in ids)
                foreach (var deck in decks)
                    if (deck.deckId == id)
                        toDelete.Add(id);

            var apiUrl = url + syncAllAPI;
            var body = new PostAllDecksBody();
            body.deckContributor = MyCard.account.user.username;
            body.userId = MyCard.account.user.id;
            body.decks = new PostDeck[toDelete.Count];
            for (int i = 0; i < toDelete.Count; i++)
            {
                body.decks[i] = new PostDeck();
                body.decks[i].deckId = toDelete[i];
                body.decks[i].isDelete = true;
            }

            var json = JsonUtility.ToJson(body);
            using UnityWebRequest request = UnityWebRequest.Post(apiUrl, json, jsonHeader);

            request.SetRequestHeader(reqHeader, reqValue);
            request.SetRequestHeader(contentTypeHeader, jsonHeader);
            request.SetRequestHeader(tokenHeader, MyCard.account.token);

            await request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                MessageManager.Cast(InterString.Get("云端卡组删除成功"));
                var responseData = JsonUtility.FromJson<ResponseSingleData>(request.downloadHandler.text);
                return true;
            }
            else
            {
                MessageManager.Cast(InterString.Get("删除云端卡组失败：") + request.error);
                return false;
            }

        }



        [Serializable]
        public class OnlineDeckData
        {
            public string deckId;
            public string deckContributor;
            public string deckName;
            public int deckRank = 0;
            public int deckLike = 0;
            public string deckUploadDate;
            public string deckUpdateDate;
            public int deckCoverCard1 = 0;
            public int deckCoverCard2 = 0;
            public int deckCoverCard3 = 0;
            public int deckCase = 0;
            public int deckProtector = 0;
            public string lastDate;
            public string deckYdk;
            public string deckMainSerial;
            public int userid;
            public bool isPublic;
            public string description;
            public bool isDelete;

            public OnlineDeckData() { }

            public OnlineDeckData(Deck deck)
            {
                deckId = deck.deckId;
                deckCoverCard1 = deck.Pickup.Count > 0 ? deck.Pickup[0] : 0;
                deckCoverCard2 = deck.Pickup.Count > 1 ? deck.Pickup[1] : 0;
                deckCoverCard3 = deck.Pickup.Count > 2 ? deck.Pickup[2] : 0;
                deckCase = deck.Case[0];
                deckProtector = deck.Protector[0];
                deckYdk = EditDeck.FromDeckToYDK(deck);
                userid = int.Parse(deck.userId);
            }
        }

        [Serializable]
        public class ResponseSingleData
        {
            public int code = 0;
            public string message;
            public string messageValue;
            public OnlineDeckData data;
        }

        [Serializable]
        public class ResponseMultiData
        {
            public int code = 0;
            public string message;
            public string messageValue;
            public ResponseRecords data;
        }

        [Serializable]
        public class ResponseMultiSimpleData
        {
            public int code = 0;
            public string message;
            public string messageValue;
            public OnlineDeckData[] data;
        }

        [Serializable]
        public class ResponseRecords
        {
            public int current;
            public int size;
            public int total;
            public int pages;
            public OnlineDeckData[] records;
        }
        [Serializable]
        public class ResponseDeckID
        {
            public int code;
            public int message;
            public string data;
        }
        [Serializable]
        public class ResponseDeckIDs
        {
            public int code;
            public int message;
            public string[] data;
        }

        [Serializable]
        public class PostAllDecksBody
        {
            public string deckContributor;
            public int userId;
            public PostDeck[] decks;
        }

        [Serializable]
        public class PostDeckBody
        {
            public string deckContributor;
            public int userId;
            public PostDeck deck;
        }

        [Serializable]
        public class PostDeck
        {
            public string deckId;
            public string deckName;
            public int deckCoverCard1;
            public int deckCoverCard2;
            public int deckCoverCard3;
            public int deckCase;
            public int deckProtector;
            public string deckYdk;
            public bool isDelete;

            public PostDeck()
            {

            }

            public PostDeck(OnlineDeckData data)
            {
                deckId = data.deckId;
                deckName = data.deckName;
                deckCoverCard1 = data.deckCoverCard1;
                deckCoverCard2 = data.deckCoverCard2;
                deckCoverCard3 = data.deckCoverCard3;
                deckCase = data.deckCase;
                deckProtector = data.deckProtector;
                deckYdk = data.deckYdk;
                isDelete = false;
            }
        }

        [Serializable]
        public class PostPublicBody
        {
            public int userId;
            public string deckId;
            public bool isPublic;
        }

    }
}

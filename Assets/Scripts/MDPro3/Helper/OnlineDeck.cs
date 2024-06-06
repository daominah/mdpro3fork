using System;
using System.Collections.Generic;
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

        const string reqHeader = "ReqSource";
        const string reqValue = "MDPro3";
        const string contentTypeHeader = "Content-Type";
        const string jsonHeader = "application/json";

        public static async void FetchDeckList(int page = 1, int pageSize = 20,  string keyWord = "", string contributor = "")
        {
            string apiUrl = url + listAPI + $"?page={page}&size={pageSize}&keyWord={keyWord}&contributor={contributor}";
            UnityWebRequest request = UnityWebRequest.Get(apiUrl);
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
            UnityWebRequest request = UnityWebRequest.Get(apiUrl);
            request.SetRequestHeader(reqHeader, reqValue);

            try
            {
                AsyncOperation sendRequestOperation = request.SendWebRequest();
                await sendRequestOperation;

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
            UnityWebRequest request = UnityWebRequest.Post(apiUrl, jsonData, jsonHeader);

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
            UnityWebRequest request = UnityWebRequest.Put(apiUrl, dataRaw);

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
            UnityWebRequest request = UnityWebRequest.Get(apiUrl);
            request.SetRequestHeader(reqHeader, reqValue);

            try
            {
                AsyncOperation sendRequestOperation = request.SendWebRequest();
                await sendRequestOperation;

                if (request.result == UnityWebRequest.Result.Success)
                {
                    string jsonResult = request.downloadHandler.text;
                    var responseData = JsonUtility.FromJson<ResponseSingleData>(request.downloadHandler.text);
                    return responseData.data;
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

            UnityWebRequest request = UnityWebRequest.PostWwwForm(apiUrl, jsonHeader);

            request.SetRequestHeader(reqHeader, reqValue);
            request.SetRequestHeader(contentTypeHeader, jsonHeader);

            request.downloadHandler = new DownloadHandlerBuffer();

            AsyncOperation sendRequestOperation = request.SendWebRequest();
            await sendRequestOperation;

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


        public static bool StringIsIdFormat(string deckId)
        {
            if(deckId.Length != 10)
                return false;
            if(!Tools.IsLowerAlphaNumeric(deckId))
                return false;
            return true;
        }

        [Serializable]
        public class OnlineDeckData
        {
            public string deckId = string.Empty;
            public string deckContributor;
            public string deckName;
            public int deckRank = 0;
            public int deckLike = 0;
            public string deckUploadDate = string.Empty;
            public string deckUpdateDate = string.Empty;
            public int deckCoverCard1 = 0;
            public int deckCoverCard2 = 0;
            public int deckCoverCard3 = 0;
            public int deckCase = 0;
            public int deckProtector = 0;
            public string lastDate;
            public string deckYdk;
            public string deckMainSerial;
        }

        [Serializable]
        public class ResponseSingleData
        {
            public int code = 0;
            public string message = string.Empty;
            public string messageValue = string.Empty;
            public OnlineDeckData data;
        }

        [Serializable]
        public class ResponseMultiData
        {
            public int code = 0;
            public string message = string.Empty;
            public string messageValue = string.Empty;
            public ResponseRecords data;
        }

        [Serializable]
        public class ResponseMultiSimpleData
        {
            public int code = 0;
            public string message = string.Empty;
            public string messageValue = string.Empty;
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

    }
}

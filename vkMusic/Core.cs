using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;


namespace vkMusic
{
    class Core
    {
        const string id = "4594925";
        const string scope = "friends,audio";
        const string display = "mobile";

        public static string authUrl() {

            string url = "https://oauth.vk.com/authorize";            
            string redirect = "redirect_uri=https://oauth.vk.com/blank.html";
            string ver = "v=5.25";
            string response = "response_type=token";

            return String.Format("{0}?client_id={1}&scope={2}&{3}&display={4}&{5}&{6}", url, id, scope, redirect, display, ver, response);

        
        }

        public static bool browserParser(string url, out string token, out string userid) {

            if (url.Contains("#"))
            {
                url = url.Split('#')[1];

                token = url.Split('&')[0].Split('=')[1];               
                userid = url.Split('&')[2].Split('=')[1];

                return true;

            }
            else if (url.Contains("?"))
            {//error
                token = null;
                userid = null;
                return false;
            }
            else
            {
                token = null;
                userid = null;
                return false;
            }

        }



       
        public static string GET_http(string method, string param, string token)
        {
            string url = string.Format("https://api.vk.com/method/{0}?{1}&access_token={2}", method, param, token);

            System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            System.Net.WebRequest reqGET = System.Net.WebRequest.Create(url);
            System.Net.WebResponse resp = reqGET.GetResponse();
            System.IO.Stream stream = resp.GetResponseStream();
            System.IO.StreamReader sr = new System.IO.StreamReader(stream);
            string html = sr.ReadToEnd();
            return html;
        }

        public static string nameById(string id) {
            string temp=GET_http("users.get.xml", "user_ids=" + id+"&fields=name", data.Default.token);
                
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(temp);
            
           return string.Format("{0} {1}",doc.SelectSingleNode("/response/user/first_name").InnerText,doc.SelectSingleNode("/response/user/last_name").InnerText);
            
        }


        public static List<Song> getMusic(string id, int offset, int count,out string all)
        {
           //возвращает определенное количество песен в лист типа Song для конкретного пользователя, с указанной позиции 
                string res = GET_http("audio.get.xml", "owner_id=" + id + "&offset=" + offset + "&count=" + count, data.Default.token);
                data.Default.currentId = id;
                data.Default.currentOffset = offset;
                if (res.Contains("<error_msg>")) { all = "0"; return null; }
                
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(res);
                XmlNode x = doc.SelectSingleNode("/response/count");
                all = x.InnerText;
                XmlNodeList xnList = doc.SelectNodes("/response/audio");
                List <Song> songs= new List<Song>();
                foreach (XmlNode xn in xnList)
                {
                    string artist = xn["artist"].InnerText;
                    string title = xn["title"].InnerText;
                    string link = xn["url"].InnerText.Split('?')[0];
                    
                    songs.Add(new Song(artist, title, link));

                }
                return songs;

            
        }

        public static List<Friend> getFriendList(string id)
        {
            List<Friend> friends = new List<Friend>();

            string res = GET_http("friends.get.xml", "user_id=" + id+ "&order=name&fields=domain", data.Default.token);

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(res);
            XmlNodeList xnList = doc.SelectNodes("/response/user");

            foreach (XmlNode xn in xnList)
            {
                string _name = xn["first_name"].InnerText;
                string _lastname = xn["last_name"].InnerText;
                string _uid = xn["uid"].InnerText;
                string _domain = xn["domain"].InnerText;

                Friend fr = new Friend { name = _name, surname = _lastname, id = _uid, domain = _domain };
                friends.Add(fr);

               
            }
            return friends;
        }


    }

    
}

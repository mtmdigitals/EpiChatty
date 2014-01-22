using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using agsXMPP.protocol.client;
using agsXMPP.protocol;
using agsXMPP;
using agsXMPP.protocol.x.muc;
using System.Xml;
using System.Text.RegularExpressions;

namespace EPiChatty
{
    class Program
    {
        #region GLOBAL VARIABLES
        
        /// <summary>
        /// what is pattern constant
        /// </summary>
        public const string WHAT_IS = "what is";
        /// <summary>
        /// what are pattern constant
        /// </summary>
        public const string WHAT_ARE = "what are";
        /// <summary>
        /// how to pattern constant
        /// </summary>
        public const string HOW_TO = "how to";
        /// <summary>
        /// how does pattern constant
        /// </summary>
        public const string HOW_DOES = "how does";
        /// <summary>
        /// default generic message to return, if the asked question is not in knowledge base XML
        /// </summary>
        public const string DEFAULT_MESSAGE = "I am sorry, I could not understand what are you saying.";
            
        /// <summary>
        /// what is root path within the XML
        /// </summary>
        public const string WHAT_IS_ROOT = "knowledge_base/what_is/questions";
        /// <summary>
        /// how to root path within the XML
        /// </summary>
        public const string HOW_TO_ROOT = "knowledge_base/how_to/questions";
        /// <summary>
        /// generic questions root path
        /// </summary>
        public const string GENERIC_ROOT = "knowledge_base/generic/questions";
        
        /// <summary>
        /// XML path for the knowledge base, place it in bin/debug or use app.config settings for different location
        /// </summary>
        public const string XML_DOC_PATH = "KnowledgeBase.xml"; 

        #endregion


        #region MAIN FUNCTION
        /// <summary>
        /// Starting point of the code
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            
            //xmpp object
            XmppClientConnection xmpp = new XmppClientConnection();

            //create the bot
            xmpp.Server = "googlemail.com"; //server name part of the email address
            xmpp.ConnectServer = "talk.google.com";//server where Google's XMPP server is running
            xmpp.Port = 5222;//default port
            xmpp.Username = "epichatty";//your username only
            xmpp.Password = "**********";//your password only
            xmpp.AutoResolveConnectServer = false;


            //register handlers
            xmpp.OnReadXml += new XmlHandler(xmpp_OnReadXml);
            xmpp.OnWriteXml += new XmlHandler(xmpp_OnWriteXml);
            xmpp.OnError += new ErrorHandler(xmpp_OnError);
            xmpp.OnMessage += new MessageHandler(xmpp_OnMessage);
            xmpp.OnLogin += new ObjectHandler(xmpp_OnLogin);
            xmpp.OnPresence += new PresenceHandler(xmpp_OnPresence);


            //log in          
            xmpp.Open();

            //wait for 5 sec to allow login 
            System.Threading.Thread.Sleep(5000);
            //write the connection state
            Console.WriteLine(xmpp.XmppConnectionState);
            //write the authentication state
            Console.WriteLine(xmpp.Authenticated);
            //wait for any key to be pressed before shutting down  
            Console.ReadLine();
            //close connections and log ogg
            xmpp.Close();
        }
        #endregion
          

        #region ON LOG IN

        static void xmpp_OnLogin(object sender)
        {
            Console.WriteLine("Logged In");

        }

        #endregion

        #region WHEN ANY MESSAGE IS RECEIVED
        
        /// <summary>
        /// this is called every time a message is received
        /// </summary>
        /// <param name="sender">xmpp object</param>
        /// <param name="msg">msg in XML</param>
        static void xmpp_OnMessage(object sender, Message msg)
        {
            if (msg.Body == null)
                return;

            if (msg.Type == MessageType.groupchat)
                return;

            if (msg.Type == MessageType.error)
                return;
          
                
            

            //splits the xml 
            string[] chatMessage = msg.From.ToString().Split('/');
            //creates the return recepient
            agsXMPP.Jid jid = new agsXMPP.Jid(chatMessage[0]);
            //creates the return message
            Message autoReply = new Message(jid, MessageType.chat, reply(msg.Body));
            //creates the sender object
            XmppClientConnection objXmpp = (XmppClientConnection)sender;
            //returns back to the sender
            objXmpp.Send(autoReply);

        }

        #endregion

        #region EXTRACT AUTO REPLY FROM XML
        /// <summary>
        /// This method loops through the XML and extracts the relevant answer if found
        /// </summary>
        /// <param name="keyword">message body</param>
        /// <returns>appropriate answer</returns>
        public static string reply(string keyword)
        {
            //load XML document
            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            doc.Load(XML_DOC_PATH);
            //retrun answer variable
            string answer = string.Empty;
            //modify message variable, this is use to remove what is or what are kind pattern before searching in XML
            string modified_keyword = string.Empty;

            //looks for 'what is' or 'what are' kind of statements
            if (Regex.IsMatch(keyword, WHAT_IS, RegexOptions.IgnoreCase) || Regex.IsMatch(keyword, WHAT_ARE, RegexOptions.IgnoreCase))
            {
                //found what is or what are, reomve them before search
                modified_keyword = keyword.Replace(WHAT_IS, "").Replace(WHAT_ARE, "").Trim().Normalize();

                //get the root of what is type of questions              
                XmlNodeList what_is_questions  = doc.SelectNodes(WHAT_IS_ROOT);
               
                //loop through the list and try to find answer
                foreach (XmlNode node in what_is_questions[0].ChildNodes)
                {
                    //if a match is found for the modified key word, return the answer
                    if ( Regex.IsMatch(node.FirstChild.InnerText,modified_keyword,RegexOptions.IgnoreCase))
                    {
                        answer = node.LastChild.InnerText.ToString();
                        break;
                    }                
   
                }
            }
            //looks for 'how to' or 'how does' kind of statements
            else if (Regex.IsMatch(keyword, HOW_TO, RegexOptions.IgnoreCase) || Regex.IsMatch(keyword, HOW_DOES, RegexOptions.IgnoreCase))
            {
                //found how to or how does pattern, remove them before searching
                modified_keyword = keyword.Replace(HOW_TO, "").Replace(HOW_DOES, "").Trim().Normalize();

                //get the root of how to questions
                XmlNodeList how_to_questions = doc.SelectNodes(HOW_TO_ROOT);

                //loop through the XML to find answer
                foreach (XmlNode node in how_to_questions[0].ChildNodes)
                {
                    //if a match is found for the modified key word, return the answer
                    if (Regex.IsMatch(node.FirstChild.InnerText, modified_keyword, RegexOptions.IgnoreCase))
                    {
                        answer = node.LastChild.InnerText.ToString();
                        break;
                    }
                }

            }
            //looks inside generic questions like 'how are you'
            else
            {
                XmlNodeList generic_questions = doc.SelectNodes(GENERIC_ROOT);

                foreach (XmlNode node in generic_questions[0].ChildNodes)
                {
                    if (node.FirstChild.InnerText.ToLower() == keyword.ToLower())
                    {
                        answer = node.LastChild.InnerText.ToString();
                        break;
                    }
                }
            
            }
            //more pattern can be added and the if statments can be extended or converted to switch using enum, upto to you


            //if nothing is found, return a generic reply
            if (String.IsNullOrEmpty(answer))
            {
                answer = DEFAULT_MESSAGE;
            }


            //return the answer
            return answer;

        }

        #endregion

        #region AUTO SUBSCRIBE
        /// <summary>
        /// auto subscribe to the requests
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="pres"></param>
        static void xmpp_OnPresence(object sender, Presence pres)
        {

            if (pres.Type == PresenceType.subscribe)
            {
                XmppClientConnection objXmpp = (XmppClientConnection)sender;
                objXmpp.PresenceManager.ApproveSubscriptionRequest(pres.From);
            }

        }
        #endregion


        #region SOME DEBUGGING METHODS
        //debugging on error
        static void xmpp_OnError(object sender, Exception ex)
        {
            Console.WriteLine("Error!!!!" + ex.Message.ToString());
        }

        //debuggin on write
        static void xmpp_OnWriteXml(object sender, string xml)
        {
            Console.WriteLine("SEND XML: " + xml);
        }

        //debuggin on read
        static void xmpp_OnReadXml(object sender, string xml)
        {
            Console.WriteLine("REC XML: " + xml);
        }
        #endregion
    
    }
}

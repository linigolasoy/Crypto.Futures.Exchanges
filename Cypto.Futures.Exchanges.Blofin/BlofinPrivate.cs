using Crypto.Futures.Exchanges.Model;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Blofin
{
    internal class BlofinPrivate
    {
        private const string ACCESS_KEY = "ACCESS-KEY";
        private const string ACCESS_SIGN = "ACCESS-SIGN";
        private const string ACCESS_TIMESTAMP = "ACCESS-TIMESTAMP";
        private const string ACCESS_NONCE = "ACCESS-NONCE";
        private const string ACCESS_PASSPHRASE = "ACCESS-PASSPHRASE";
        public static void PutHeaders( HttpRequestMessage oRequest, IApiKey oApiKey  )
        {
            Dictionary<string, string> aHeaders = new Dictionary<string, string>();

            Guid oGuid = Guid.NewGuid();    
            long nTimestamp = Util.ToUnixTimestamp(DateTime.Now, true);
            aHeaders.Add(ACCESS_KEY, oApiKey.ApiKey );
            aHeaders.Add(ACCESS_TIMESTAMP, nTimestamp.ToString());
            aHeaders.Add(ACCESS_NONCE, oGuid.ToString());
            aHeaders.Add(ACCESS_PASSPHRASE, "Cotton12$$");

            string strPath = oRequest.RequestUri!.LocalPath;
            string strMethod = oRequest.Method.ToString();
            string strTimestamp = nTimestamp.ToString();
            string strNonce = oGuid.ToString();

            string strPreHash = $"{strPath}{strMethod}{strTimestamp}{strNonce}";

            string strSignature = Util.EncodePayLoadHmac(strPreHash, oApiKey, false);


            aHeaders.Add(ACCESS_SIGN, strSignature);

            foreach( var oValue in aHeaders )
            {
                oRequest.Headers.Add(oValue.Key, oValue.Value );    
            }
            /*
body = ""  # Empty for GET requests

prehash = f"{path}{method}{timestamp}{nonce}{body}"
hex_signature = hmac.new(
    secret_key.encode(),
    prehash.encode(),
    hashlib.sha256
).hexdigest().encode()

signature = base64.b64encode(hex_signature).decode()             */
            /*
ACCESS-KEY The API Key as a String.


ACCESS-TIMESTAMP The UTC timestamp of your request .e.g : 1597026383085

ACCESS-NONCE The client’s random string generation algorithm must not produce duplicates within the time difference range allowed by the server, such as UUID, Snowflake algorithm, etc.

ACCESS-PASSPHRASE The passphrase you specified when creating the APIKey.
ACCESS-SIGN The Base64-encoded signature (see Signing Messages subsection for details).
            */
        }
    }
}

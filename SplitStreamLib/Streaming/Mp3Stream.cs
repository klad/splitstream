using NAudio.Wave;
using SplitStreamLib.Streaming.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SplitStreamLib.Streaming
{
    public class Mp3Stream
    {
        private readonly string _uri;

        public Mp3Stream(string uri)
        {
            _uri = uri;
        }

        public IEnumerable<Mp3Frame> GetFrames()
        {
            var request = (HttpWebRequest)WebRequest.Create(_uri);
            var response = request.GetResponse();

            using (var s = response.GetResponseStream())
            using (var brs = new BufferedReadStream(s))
            {
                Mp3Frame frame;
                do
                {
                    frame = Mp3Frame.LoadFromStream(brs);

                    yield return frame;

                } while (frame != null);
            }
        
        }
    }
}

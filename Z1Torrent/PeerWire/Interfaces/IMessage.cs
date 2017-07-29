using System;

namespace Z1Torrent.PeerWire.Interfaces {

    /// <summary>
    /// Exception for invalid message structure
    /// </summary>
    public class InvalidMessageException : Exception {
        public InvalidMessageException(string msg) : base(msg) { }
        public InvalidMessageException(string msg, Exception innerException) : base(msg, innerException) { }
    }

    public interface IMessage {

        /// <summary>
        /// Message ID
        /// </summary>
        int Id { get; }

        /// <summary>
        /// This method should return the payload bytes of the message.
        /// <para>Can be an empty array.</para>
        /// </summary>
        /// <returns>Payload data</returns>
        byte[] Pack();

        /// <summary>
        /// This method should read the given message and populate the instance with data.
        /// </summary>
        /// <param name="data">Data to parse</param>
        void Unpack(byte[] data);
    }

}

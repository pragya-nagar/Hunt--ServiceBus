using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Synergy.Common.FileStorage.Abstraction;
using Synergy.ServiceBus.Abstracts;

namespace Synergy.ServiceBus.Extensions.Serialization
{
    public class ExternalStorageSerializer : IMessageSerializer
    {
        private const string StoragePath = "message_store/";
        private const string ExternalMarker = "-->ExternalStorage:";
        private const int MaxMsgBytes = 200 * 1024;

        private readonly IFileStorage _fileStorage;
        private readonly JsonMessageSerializer _jsonMessageSerializer;

        public ExternalStorageSerializer(IFileStorage fileStorage, JsonMessageSerializer jsonMessageSerializer)
        {
            this._fileStorage = fileStorage ?? throw new ArgumentNullException(nameof(fileStorage));
            this._jsonMessageSerializer = jsonMessageSerializer ?? throw new ArgumentNullException(nameof(jsonMessageSerializer));
        }

        public async Task<string> SerializeMessageAsync(IMessage source, CancellationToken cancellationToken)
        {
            var msg = await _jsonMessageSerializer.SerializeMessageAsync(source, cancellationToken).ConfigureAwait(false);

            if (Encoding.UTF8.GetByteCount(msg) > MaxMsgBytes)
            {
                var objectName = StoragePath + Guid.NewGuid();

                await this._fileStorage.SaveAsync(Encoding.UTF8.GetBytes(msg), objectName, cancellationToken).ConfigureAwait(false);

                return ExternalMarker + objectName;
            }

            return msg;
        }

        public async Task<object> DeserializeMessageAsync(string source, Type type, CancellationToken cancellationToken)
        {
            string objectContent;

            if (source.StartsWith(ExternalMarker, StringComparison.InvariantCulture))
            {
                var objectName = source.Substring(ExternalMarker.Length);

                var msgBytes = await this._fileStorage.GetAsync(objectName, cancellationToken).ConfigureAwait(false);

                objectContent = Encoding.UTF8.GetString(msgBytes);
            }
            else
            {
                objectContent = source;
            }

            return await _jsonMessageSerializer.DeserializeMessageAsync(objectContent, type, cancellationToken).ConfigureAwait(false);
        }
    }
}

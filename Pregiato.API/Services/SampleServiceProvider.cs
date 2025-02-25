using SmtpServer.Storage;

namespace Pregiato.API.Services
{
    public class SampleServiceProvider : IServiceProvider
    {
        private readonly IMessageStore _messageStore;

        public SampleServiceProvider(IMessageStore messageStore)
        {
            _messageStore = messageStore;
        }

        public object GetService(Type serviceType)
        {
            if (serviceType == typeof(IMessageStore))
            {
                return _messageStore;
            }

            return null!;
        }
    }
}

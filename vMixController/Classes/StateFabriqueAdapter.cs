using System;
using vMixAPI;

namespace vMixController.Classes
{
    public interface IStateFactory
    {
        void Configure(string ip = "127.0.0.1", string port = "8088", string login = "admin", string password = "");
        void SetConnectionOptions(ConnectionOptions options);
        bool IsUrlValid(string ip, string port);
        string GetUrl(string ip, string port);
        string GetCredentials(string login, string password);
    }

    public interface IStateSyncService
    {
        event EventHandler OnStateCreated;
        void Start();
        void Stop();
        void CreateAsync();
    }

    public sealed class StateFabriqueAdapter : IStateFactory, IStateSyncService
    {
        public static StateFabriqueAdapter Instance { get; } = new StateFabriqueAdapter();
        private readonly object _sync = new object();
        private bool _isStarted;

        public event EventHandler OnStateCreated
        {
            add => StateFabrique.OnStateCreated += value;
            remove => StateFabrique.OnStateCreated -= value;
        }

        public void Start()
        {
            lock (_sync)
            {
                _isStarted = true;
            }
        }

        public void Stop()
        {
            lock (_sync)
            {
                _isStarted = false;
            }
        }

        public void Configure(string ip = "127.0.0.1", string port = "8088", string login = "admin", string password = "")
        {
            StateFabrique.Configure(ip, port, login, password);
        }

        public void SetConnectionOptions(ConnectionOptions options)
        {
            StateFabrique.SetConnectionOptions(options);
        }

        public bool IsUrlValid(string ip, string port)
        {
            return StateFabrique.IsUrlValid(ip, port);
        }

        public string GetUrl(string ip, string port)
        {
            return StateFabrique.GetUrl(ip, port);
        }

        public string GetCredentials(string login, string password)
        {
            return StateFabrique.GetCredentials(login, password);
        }

        public void CreateAsync()
        {
            lock (_sync)
            {
                if (!_isStarted)
                    return;
            }
            StateFabrique.CreateAsync();
        }
    }
}

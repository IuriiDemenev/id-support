using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;
using ID.SupportDatabase.Annotations;
using ID.SupportDatabase.Models;
using ID.SupportDatabase.Services;
using Microsoft.Extensions.Logging;

namespace ID.SupportDatabase.Views.Connections
{
    public class ConnectionDialogViewModel : INotifyPropertyChanged
    {
        private const string StorageFileName = "id-support-connections";
        private readonly ILogger<ConnectionDialogViewModel> _logger;
        private readonly IStorageService _storageService;

        #region Properies

        private Connection _currentConnection;
        private ObservableCollection<Connection> _connections;
        private string _error;
        private bool _checkInProgress;

        public bool CheckInProgress
        {
            get => _checkInProgress;
            set
            {
                if (value == _checkInProgress) return;
                _checkInProgress = value;
                OnPropertyChanged();
            }
        }

        public string Error
        {
            get => _error;
            set
            {
                if (value == _error) return;
                _error = value;
                OnPropertyChanged();
            }
        }

        public Connection CurrentConnection
        {
            get => _currentConnection ?? (_currentConnection = new Connection());
            set
            {
                if (Equals(value, _currentConnection)) return;
                _currentConnection = (Connection)value.Clone();
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Connection> Connections
        {
            get => _connections;
            set
            {
                if (Equals(value, _connections)) return;
                _connections = value;
                OnPropertyChanged();
            }
        }

        #endregion
        
        public ICommand ConnectCommand { get; set; }
        public ICommand DeleteCommand { get; set; }

        public ConnectionDialogViewModel(ILogger<ConnectionDialogViewModel> logger, IStorageService storageService)
        {
            _logger = logger;
            _storageService = storageService;

            // Load connections from storage
            var connections = _storageService.Load<ObservableCollection<Connection>>(StorageFileName);
            Connections = connections ?? new ObservableCollection<Connection>();

            ConnectCommand = new RelayCommand<dynamic>(ExecuteConnectCommand, CanExecuteConnectCommand);

            DeleteCommand = new RelayCommand<Connection>(ExecuteDeleteCommand);
        }

        private bool CanExecuteConnectCommand(dynamic form)
        {
            return !string.IsNullOrEmpty(CurrentConnection.DataSource)
                   && !string.IsNullOrEmpty(CurrentConnection.Catalog)
                   && !string.IsNullOrEmpty(CurrentConnection.UserId)
                   && !CheckInProgress;
        }

        private async void ExecuteConnectCommand(dynamic form)
        {
            // Update password
            if (!string.IsNullOrEmpty(form.password.Password))
                CurrentConnection.Password = form.password.Password;

            // Check connection
            if (!await CheckConnection(CurrentConnection))
                return;

            // Update connection list
            if (!_connections.Contains(CurrentConnection))
                _connections.Add(CurrentConnection);
            else
            {
                // if connection exist, then check and update, if needed, password
                var replaceConnection = _connections.First();
                if (!CurrentConnection.Password.Equals(replaceConnection.Password))
                    replaceConnection.Password = CurrentConnection.Password;
            }

            // Save connections in storage
            _storageService.Save(StorageFileName, _connections);

            form.window.DialogResult = true;
            form.window.Close();
        }

        private void ExecuteDeleteCommand(Connection connection)
        {
            Connections.Remove(connection);
            OnPropertyChanged(nameof(Connections));

            _logger.LogInformation($"Connection {BuildConnectionString(connection)} delete");

            _storageService.Save(StorageFileName, _connections);
        }

        private async Task<bool> CheckConnection(Connection connection)
        {
            if (string.IsNullOrEmpty(connection.Password))
            {
                Error = "Password required";
                return false;
            }

            Error = string.Empty;
            CheckInProgress = true;

            var connectionBuilder = BuildConnectionString(connection);

            try
            {
                using (var sqlconnection = new SqlConnection(connectionBuilder))
                {
                    await sqlconnection.OpenAsync();

                    sqlconnection.Close();
                }
                _logger.LogInformation($"Check connection {connectionBuilder} success");
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Check connection {connectionBuilder} error");
                _logger.LogError(ex, ex.Message);
                Error = ex.Message;
                return false;
            }
            finally
            {
                CheckInProgress = false;
            }

            return true;
        }

        private static string BuildConnectionString(Connection connection)
        {
            return new SqlConnectionStringBuilder
            {
                DataSource = connection.DataSource,
                InitialCatalog = connection.Catalog,
                UserID = connection.UserId,
                Password = connection.Password
            }.ToString();
        }
        
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

using System.IO;
using System.IO.IsolatedStorage;
using System.Runtime.Serialization.Formatters.Binary;

namespace ID.SupportDatabase.Services
{
    public interface IStorageService
    {
        void Save(string name, object value);
        T Load<T>(string name);
    }

    public class IsolatedStorageService : IStorageService
    {
        public void Save(string name, object value)
        {
            var isoStorage = GetStorage();

            using (var stream = new IsolatedStorageFileStream(name, FileMode.Create, FileAccess.Write, isoStorage))
            {
                var formatter = new BinaryFormatter();

                formatter.Serialize(stream, value);

                stream.Close();
            }
        }

        public T Load<T>(string name)
        {
            var isoStorage = GetStorage();

            if (!isoStorage.FileExists(name))
                return default;

            T result;

            using (var stream = new IsolatedStorageFileStream(name, FileMode.Open, isoStorage))
            {
                if (stream.Length == 0)
                {
                    stream.Close();
                    return default;
                }

                var formatter = new BinaryFormatter();

                result = (T)formatter.Deserialize(stream);

                stream.Close();
            }

            return result;
        }

        private IsolatedStorageFile GetStorage() => IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);
    }
}

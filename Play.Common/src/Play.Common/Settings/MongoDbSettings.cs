namespace Play.Common.Settings
{
    public class MongoDbSettings
    {

        //since we are not going to change the values after the microservice loads, change set to init.
        public string Host { get; init; }
        public int Port { get; init; }
        public string ConnectionString => $"mongodb://{Host}:{Port}";
    }
}
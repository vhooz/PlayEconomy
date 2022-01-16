using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace Play.Common.MongoDB
{

    public class MongoRepository<T> : IRepository<T> where T : IEntity
    {
        private readonly IMongoCollection<T> dbCollection; //DeCal instance. represents the actual mongodb collection.
        private readonly FilterDefinitionBuilder<T> filterBuilder = Builders<T>.Filter; //filter builder used to build the filters to query fot Ts in MongoDB. 

        //constructure of the repository: 
        public MongoRepository(IMongoDatabase database, string collectionName)
        {
            //use the mongo client class to connect to the acutal DB.
            // var mongoClient = new MongoClient("mongodb://localhost:27017"); //connection string.
            // var database = mongoClient.GetDatabase("Catalog"); // catalog microservice uses the catalog db.
            dbCollection = database.GetCollection<T>(collectionName); //instance object for the dbCollection
        }

        /**
        For public methods, use the ASYNCHRONOUS PROGRAMMING MODEL.
        Enhances the overall responsiveness of our service. Avoids performance bottleneckes.

        All methos will become asynchronous by returning async task and by using the await keyboard anytime they interact.
        use the async suffix on all methods.

        **/

        public async Task<IReadOnlyCollection<T>> GetAllAsync()
        {
            return await dbCollection.Find(filterBuilder.Empty).ToListAsync(); //no need to filter anything, we want it all!
        }

        public async Task<IReadOnlyCollection<T>> GetAllAsync(Expression<Func<T, bool>> filter)
        {
            //throw new NotImplementedException();
            return await dbCollection.Find(filter).ToListAsync();
        }

        public async Task<T> GetAsync(Guid id)
        {
            FilterDefinition<T> filter = filterBuilder.Eq(entity => entity.Id, id); //this filter will look for an equality in the id.
            return await dbCollection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<T> GetAsync(Expression<Func<T, bool>> filter)
        {
            return await dbCollection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }
            //to create the T :
            await dbCollection.InsertOneAsync(entity);


        }

        public async Task UpdateAsync(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }
            FilterDefinition<T> filter = filterBuilder.Eq(currentEntity => currentEntity.Id, entity.Id); //this filter will look for an equality in the id.

            await dbCollection.ReplaceOneAsync(filter, entity);
        }

        public async Task RemoveAsync(Guid id)
        {
            FilterDefinition<T> filter = filterBuilder.Eq(entity => entity.Id, id); //this filter will look for an equality in the id.
            await dbCollection.DeleteOneAsync(filter);
        }


    }
}
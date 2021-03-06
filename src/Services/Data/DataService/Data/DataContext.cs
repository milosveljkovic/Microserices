﻿using DataService.Entities;
using DataService.Settings;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataService.Data
{
    public class DataContext : IDataContext
    {
        public DataContext(ISensorDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            Sensors = database.GetCollection<Sensor>(settings.CollectionName);
            DataContextSeed.SeedData(Sensors);
        }
        public IMongoCollection<Sensor> Sensors { get; }
    }
}

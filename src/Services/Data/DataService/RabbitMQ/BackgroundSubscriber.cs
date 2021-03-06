﻿using Microsoft.Extensions.Hosting;
using MongoDB.Bson.IO;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using DataService.Entities;
using DataService.Repository;
using Microsoft.Extensions.Logging;

namespace DataService.RabbitMQ
{
    public class BackgroundSubscriber : IHostedService
    {
        private ConnectionFactory _factory;
        private Publisher _publisher;
        private IConnection _connection;
        private IModel _channel;
        private string queueName;
        private EventingBasicConsumer consumer;
        private readonly ISensorRepository _repository;
        private readonly ILogger<BackgroundSubscriber> _logger;

        public BackgroundSubscriber(ISensorRepository repository, ILogger<BackgroundSubscriber> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _publisher = new Publisher();
        }

        public void InitBackgroundDataSubscriber()
        {
            //this._factory = new ConnectionFactory() { HostName = "localhost" }; //{ HostName = "rabbitmq", Port = 5672  };
            this._factory = new ConnectionFactory()
            {
                HostName = "rabbitmq",
                UserName = "user",
                Password = "password",
                Port = 5672
              };
            this._connection = this._factory.CreateConnection();
            this._channel = this._connection.CreateModel();
            this._channel.ExchangeDeclare(exchange: "device-data", type: ExchangeType.Fanout);

            var queueName = this._channel.QueueDeclare().QueueName;
            this._channel.QueueBind(queue: queueName,
                              exchange: "device-data",
                              routingKey: "");

            var consumer = new EventingBasicConsumer(this._channel);
            consumer.Received += (model, ea) =>
            {
                var json = Encoding.Default.GetString(ea.Body.ToArray());
                var message = Newtonsoft.Json.JsonConvert.DeserializeObject<Sensor>(json);
                _repository.Create(message);
                _publisher.SendMessage(message);

            };
            this._channel.BasicConsume(queue: queueName,
                                 autoAck: true,
                                 consumer: consumer);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            InitBackgroundDataSubscriber();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            this._channel.Dispose();
            this._connection.Dispose();
            return Task.CompletedTask;
        }
    }
}

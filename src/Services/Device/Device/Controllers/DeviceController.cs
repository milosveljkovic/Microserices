﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Device.Entities;
using Device.RabbitMQ;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RabbitMQ.Client;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Device.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeviceController : ControllerBase
    {

        private readonly ISensor _mySensor;
        private readonly IPublisher _publisher;

        public DeviceController(ISensor mySensor, IPublisher publisher)
        {
            _mySensor = mySensor;
            _publisher = publisher;
        }

        // POST api/device/setSensorSendPeriod
        [HttpPost("setSensorSendPeriod", Name = "setSensorSendPeriod")]
        public ActionResult setSensorSendPeriod([FromBody]Period _period)
        {
            if ((_period.periodValue > 0) && (_period.periodValue > _mySensor.getReadPeriod()))
            {
                _mySensor.setSendPeriod(_period.periodValue);
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }

        // POST api/device/setSensorReadPeriod
        [HttpPost("setSensorReadPeriod", Name = "setSensorReadPeriod")]
        public ActionResult setSensorReadPeriod([FromBody]Period _period)
        {
            if ((_period.periodValue > 0) && (_period.periodValue < _mySensor.getSendPeriod()))
            {
                _mySensor.setReadPeriod(_period.periodValue);
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }

        // POST api/device/turnOnOff
        [HttpPost("turnOnOff", Name = "turnOnOff")]
        public ActionResult turnOnOff([FromBody]int mode)
        {
            if (mode == 1 || mode == 0)
            {
                _mySensor.turnOnOff(mode);
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }

        // POST api/device/setTreshold
        [HttpPost("setTreshold", Name = "setTreshold")]
        public ActionResult setTreshold([FromBody]int tresholdValue)
        {
            if (tresholdValue >0 && tresholdValue< 50)
            {
                _mySensor.setTreshold(tresholdValue);
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }

        // POST api/device/turnOnOffMiAirPurifier
        [HttpPost("turnOnOffMiAirPurifier", Name = "turnOnOffMiAirPurifier")]
        public ActionResult turnOnOffMiAirPurifier([FromBody]int isOn)
        {
            if (isOn==1)
            {
                _mySensor.turnOnOffMiAirPurifier(true);
                return Ok();
            }
            else if(isOn==0)
            {
                _mySensor.turnOnOffMiAirPurifier(false);
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }

        // POST api/device/setMiAirPurfierCleaningStrength
        [HttpPost("setMiAirPurfierCleaningStrength", Name = "setMiAirPurfierCleaningStrength")]
        public ActionResult setMiAirPurfierCleaningStrength([FromBody]int cleaningStrength)
        {
            if (cleaningStrength>=10 && cleaningStrength<=50)
            {
                _mySensor.setMiAirPurfierCleaningStrength(cleaningStrength);
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpGet]
        public ActionResult Get()
        {
            Settings _settings = new Settings();
            _settings.readPeriod = _mySensor.getReadPeriod();
            _settings.sendPeriod = _mySensor.getSendPeriod();
            _settings.isOnSensor = _mySensor.getIsOnSensor();
            _settings.isOnMiAirPurfier = _mySensor.getIsMiAirPurfierOn();
            _settings.cleaningStrengthMiAirPurfier = _mySensor.getMiAirPurfierCleaningStrength();
            _settings.treshold = _mySensor.getTreshold();
            return Ok(_settings);
        }
    }
}

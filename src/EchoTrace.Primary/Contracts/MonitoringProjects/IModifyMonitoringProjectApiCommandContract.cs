﻿using System.Net;
using AutoMapper;
using EchoTrace.Infrastructure.DataPersistence.EfCore.Entities.MonitoringProjects;
using EchoTrace.Infrastructure.DataPersistence.EfCore.Entities.MonitoringProjects.Enums;
using EchoTrace.Primary.Bases;
using EchoTrace.Primary.Contracts.Bases;
using Mediator.Net.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace EchoTrace.Primary.Contracts.MonitoringProjects;

public interface IModifyMonitoringProjectApiCommandContract : ICommandContract<ModifyMonitoringProjectApiCommand>
{
}

public class ModifyMonitoringProjectApiCommand : ICommand
{
    [FromRoute]
    public Guid MonitoringProjectId { get; set; }
    
    [FromRoute]
    public Guid MonitoringProjectApiId { get; set; }
    
    [FromBody]
    public ModifyMonitoringProjectApiDto ModifyMonitoringProjectApi { get; set; }
}

public class ModifyMonitoringProjectApiDto : IMapFrom<MonitoringProjectApi>
{
    /// <summary>
    ///  0：OneMinute， 10：FiveMinutes， 20：TenMinutes， 30：ThirtyMinutes， 40：OneHour， 50：ThreeHours， 60：SixHours， 70：TwelveHours， 80：OneDay
    /// </summary>
    public MonitorInterval MonitorInterval { get; set; }
    
    public HttpRequestMethod HttpRequestMethod { get; set; }
    
    public bool IsDeactivate { get; set; }

    public HttpStatusCode ExpectationCode { get; set; }
    
    public string? BodyJson { get; set; }

    public void ConfigureMapper(IMapperConfigurationExpression cfg, MonitoringProjectApi? source)
    {
        this.CreateMapperConfiguration(cfg, source)
            .ForMember(des=>des.BodyJson, opt=>opt.Ignore())
            .AfterMap((src, des) =>
            {
                if (des.BodyJson != src.BodyJson)
                {
                    src.BodyJson = des.BodyJson;
                }
            }).ReverseMap();
    }
}
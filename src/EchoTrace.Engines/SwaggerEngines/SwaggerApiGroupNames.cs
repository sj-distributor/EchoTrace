namespace EchoTrace.Engines.SwaggerEngines;

public enum SwaggerApiGroupNames
{
    [SwaggerGroupInfo(Title = "Web端接口文档", Description = "Web端", Version = "v1", MatchRule = "api/web")]
    Web,

    [SwaggerGroupInfo(Title = "App端接口文档", Description = "App端", Version = "v1", MatchRule = "api/app")]
    App
}
FROM registry-vpc.cn-hongkong.aliyuncs.com/wiltechs/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM registry-vpc.cn-hongkong.aliyuncs.com/wiltechs/sdk:8.0 AS publish
WORKDIR /src
COPY ./src  .
WORKDIR "/src/EchoTrace"
RUN dotnet restore "EchoTrace.csproj" --configfile ../NuGet.Config
RUN dotnet publish "EchoTrace.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
RUN sed -i 's/\[openssl_init\]/# [openssl_init]/' /etc/ssl/openssl.cnf
RUN printf "\n\n[openssl_init]\nssl_conf = ssl_sect" >> /etc/ssl/openssl.cnf
RUN printf "\n\n[ssl_sect]\nsystem_default = ssl_default_sect" >> /etc/ssl/openssl.cnf
RUN printf "\n\n[ssl_default_sect]\nMinProtocol = TLSv1\nCipherString = DEFAULT@SECLEVEL=0\n" >> /etc/ssl/openssl.cnf

WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EchoTrace.dll"]
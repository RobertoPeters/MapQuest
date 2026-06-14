FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
EXPOSE 3080
ENV ASPNETCORE_URLS="http://+:3080"
ENV DOTNET_RUNNING_IN_CONTAINER=true
COPY src/MapQuest/DeployLinux/ .
RUN mkdir Settings
USER root
USER $APP_UID
ENTRYPOINT ["dotnet", "MapQuest.dll"]
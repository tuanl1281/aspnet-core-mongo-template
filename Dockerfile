FROM mcr.microsoft.com/dotnet/aspnet:6.0-bullseye-slim AS base
RUN apt-get update && apt-get install -y libgdiplus
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0-bullseye-slim AS build
WORKDIR /src
COPY ["src/Api.Template.Application/Api.Template.Application.csproj", "Api.Template.Application/"]
COPY ["src/Api.Template.Data/Api.Template.Data.csproj", "Api.Template.Data/"]
COPY ["src/Api.Template.Service/Api.Template.Service.csproj", "Api.Template.Service/"]
COPY ["src/Api.Template.ViewModel/Api.Template.ViewModel.csproj", "Api.Template.ViewModel/"]
RUN dotnet restore "Api.Template.Application/Api.Template.Application.csproj"
COPY ./src .
RUN ls
WORKDIR "/src/Api.Template.Application"
RUN dotnet build "Api.Template.Application.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Api.Template.Application.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=build /src/Api.Template.Application/Resources ./Resources
ENTRYPOINT ["dotnet", "Api.Template.Application.dll"]

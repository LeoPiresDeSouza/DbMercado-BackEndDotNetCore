dotnet run --project DbMercado.Api --launch-profile http

dotnet ef database update --project DbMercado.Infrastructure --startup-project DbMercado.Api

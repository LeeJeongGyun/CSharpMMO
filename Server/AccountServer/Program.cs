namespace AccountServer
{
    using AccountServer.DB;
    using Microsoft.EntityFrameworkCore;

    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            // 웹 패킷 응답 시 옵션 설정, 세팅 안해줄 경우 자동으로 camelcase로 변경하기 때문에 Unity에서 Deserialize 오류 발생
            builder.Services.AddControllers().AddJsonOptions(option =>
            {
                option.JsonSerializerOptions.PropertyNamingPolicy = null;
                option.JsonSerializerOptions.DictionaryKeyPolicy = null;
            });

            // 웹 서버에서는 DI로 AppDbContext를 생성
            builder.Services.AddDbContext<AppDbContext>(option
                => option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}

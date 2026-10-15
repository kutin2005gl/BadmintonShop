using System.Configuration;
using BadmintonShop.Core.DataAccess;

namespace BadmintonShop.Desktop;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();

        // Chuoi ket noi: uu tien bien moi truong ConnectionStrings__BadmintonShop (giong ben Web),
        // khong co thi lay trong App.config -> moi may tu dat, khong can sua file commit.
        DbHelper.ConnectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__BadmintonShop")
            ?? ConfigurationManager.ConnectionStrings["BadmintonShop"].ConnectionString;

        Application.Run(new Form1());
    }
}

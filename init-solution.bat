@echo off
REM =====================================================================
REM  BadmintonShop - khoi tao solution + git (SV1 chay MOT LAN)
REM  Can: .NET 8 SDK, Git (da git config user.name/user.email), co mang de tai NuGet
REM  Tuy chon: GitHub CLI (gh) da "gh auth login" -> tu tao repo tren GitHub
REM =====================================================================
setlocal
cd /d "%~dp0"

where dotnet >nul 2>nul
if errorlevel 1 (
  echo [LOI] Chua cai .NET 8 SDK. Cai Visual Studio 2022 hoac https://dotnet.microsoft.com/download
  goto loi
)
where git >nul 2>nul
if errorlevel 1 (
  echo [LOI] Chua cai Git. Tai tai https://git-scm.com
  goto loi
)

if exist BadmintonShop.sln (
  echo [BO QUA] Da co BadmintonShop.sln, khong tao lai solution.
  goto git
)

echo.
echo === 1/4 Tao solution va 3 project ===
dotnet new sln -n BadmintonShop
if errorlevel 1 goto loi
dotnet new classlib -n BadmintonShop.Core -o BadmintonShop.Core -f net8.0
if errorlevel 1 goto loi
if exist BadmintonShop.Core\Class1.cs del /q BadmintonShop.Core\Class1.cs
dotnet new mvc -n BadmintonShop.Web -o BadmintonShop.Web -f net8.0
if errorlevel 1 goto loi
dotnet new winforms -n BadmintonShop.Desktop -o BadmintonShop.Desktop -f net8.0
if errorlevel 1 goto loi

dotnet sln BadmintonShop.sln add BadmintonShop.Core\BadmintonShop.Core.csproj BadmintonShop.Web\BadmintonShop.Web.csproj BadmintonShop.Desktop\BadmintonShop.Desktop.csproj
if errorlevel 1 goto loi
dotnet add BadmintonShop.Web\BadmintonShop.Web.csproj reference BadmintonShop.Core\BadmintonShop.Core.csproj
dotnet add BadmintonShop.Desktop\BadmintonShop.Desktop.csproj reference BadmintonShop.Core\BadmintonShop.Core.csproj

echo.
echo === 2/4 Cai thu vien NuGet ===
dotnet add BadmintonShop.Core\BadmintonShop.Core.csproj package Microsoft.Data.SqlClient
if errorlevel 1 goto loi
dotnet add BadmintonShop.Core\BadmintonShop.Core.csproj package BCrypt.Net-Next
if errorlevel 1 goto loi
dotnet add BadmintonShop.Desktop\BadmintonShop.Desktop.csproj package System.Configuration.ConfigurationManager
if errorlevel 1 goto loi

REM Them chuoi ket noi vao appsettings.json cua Web
powershell -NoProfile -ExecutionPolicy Bypass -Command "$p='BadmintonShop.Web\appsettings.json'; $j=Get-Content $p -Raw | ConvertFrom-Json; $cs=[pscustomobject]@{BadmintonShop='Server=localhost;Database=BadmintonShop;Trusted_Connection=True;TrustServerCertificate=True;'}; $j | Add-Member -NotePropertyName ConnectionStrings -NotePropertyValue $cs -Force; $j | ConvertTo-Json -Depth 10 | Set-Content $p -Encoding UTF8"

echo.
echo === 3/4 Build + tao chuoi bam mat khau cho du lieu mau ===
dotnet build BadmintonShop.sln
if errorlevel 1 goto loi
dotnet run --project tools\HashTool\HashTool.csproj -- --patch database\03_SampleData.sql
if errorlevel 1 goto loi

:git
echo.
echo === 4/4 Git ===
if exist .git (
  echo [BO QUA] Da co repo git.
  goto remote
)
git config user.name >nul 2>nul
if errorlevel 1 (
  echo [LOI] Chua khai bao ten Git. Chay:
  echo   git config --global user.name "Ten Ban"
  echo   git config --global user.email "email@github"
  goto loi
)
git init -b main
git add .
git commit -m "chore(sv1): khoi tao solution, CSDL va thu vien Core"
if errorlevel 1 goto loi
git branch develop

:remote
git remote get-url origin >nul 2>nul
if not errorlevel 1 (
  echo [BO QUA] Da co remote origin.
  goto xong
)
where gh >nul 2>nul
if errorlevel 1 goto taytay
gh auth status >nul 2>nul
if errorlevel 1 goto taytay
gh repo create BadmintonShop --private --source . --remote origin --push
if errorlevel 1 goto taytay
git push -u origin develop
goto xong

:taytay
echo.
echo ---------------------------------------------------------------
echo  Chua tu tao duoc repo tren GitHub. Lam tay:
echo   1. Vao https://github.com/new  - ten repo: BadmintonShop - Private
echo      KHONG tick "Add a README" / .gitignore / license
echo   2. Chay 3 lenh (thay ten-tai-khoan):
echo      git remote add origin https://github.com/ten-tai-khoan/BadmintonShop.git
echo      git push -u origin main
echo      git push -u origin develop
echo ---------------------------------------------------------------
goto xong

:xong
echo.
echo HOAN TAT. Mo BadmintonShop.sln bang Visual Studio 2022.
pause
exit /b 0

:loi
echo.
echo Co loi - doc thong bao o tren, sua roi chay lai file nay.
pause
exit /b 1

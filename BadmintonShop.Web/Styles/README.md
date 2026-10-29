# Giao dien Web (Tailwind)

Giao dien SmashPro (thiet ke React goc da duoc chuyen sang Razor + Tailwind). CSS duoc build san vao `wwwroot/css/smashpro.css` (da commit),
nen chay Web **khong can Node**. Chi khi sua class Tailwind trong `Views/**/*.cshtml` moi can build lai:

    cd BadmintonShop.Web
    npx tailwindcss@3 -c Styles/tailwind.config.js -i Styles/input.css -o wwwroot/css/smashpro.css --minify

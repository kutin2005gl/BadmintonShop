/** Cau hinh Tailwind lay tu giao dien smashpro-badminton; build ra wwwroot/css/smashpro.css (xem Styles/README.md) */
module.exports = {
  content: ["./Views/**/*.cshtml"],
  theme: {
    extend: {
      colors: { primary: '#0057FF', secondary: '#00C853', grayLight: '#F5F5F5' },
      fontFamily: { sans: ['Inter', 'sans-serif'] },
    },
  },
  plugins: [],
};
